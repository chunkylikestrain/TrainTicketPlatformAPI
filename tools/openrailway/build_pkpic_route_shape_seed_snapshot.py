import argparse
import datetime as dt
import hashlib
import json
import re
import unicodedata
from collections import defaultdict
from pathlib import Path


DEFAULT_OPERATING_DATE = "2026-07-03"


def load_json(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def parse_time_to_minutes(value, day_offset):
    if not value:
        return None

    hours, minutes, *_ = [int(part) for part in str(value).split(":")]
    return ((day_offset or 0) * 1440) + (hours * 60) + minutes


def offset_from_base(value, day_offset, base_minutes):
    absolute = parse_time_to_minutes(value, day_offset)
    if absolute is None or base_minutes is None:
        return None

    offset = absolute - base_minutes
    while offset < 0:
        offset += 1440
    return offset


def normalize_ascii(value):
    text = unicodedata.normalize("NFKD", value)
    text = text.encode("ascii", "ignore").decode("ascii")
    return re.sub(r"[^A-Za-z0-9]", "", text).upper()


def build_station_code(station, used_codes):
    explicit_code = (station.get("code") or "").strip().upper()
    if explicit_code and explicit_code not in used_codes and len(explicit_code) <= 32:
        used_codes.add(explicit_code)
        return explicit_code

    name_part = normalize_ascii(station.get("name") or "")
    prefix = name_part[:8] if name_part else "PLK"
    station_id = int(station["externalStationId"])
    code = f"{prefix}{station_id}"
    if len(code) > 32:
        code = f"PLK{station_id}"

    while code in used_codes:
        code = f"PLK{station_id}"
        if code not in used_codes:
            break
        code = f"PLK{station_id}{len(used_codes) % 1000}"

    used_codes.add(code)
    return code


def build_route_code(station_ids, fingerprint):
    return f"PLK-{station_ids[0]}-{station_ids[-1]}-{fingerprint[:8].upper()}"[:32]


def build_route_fingerprint(station_ids):
    return "PLK:" + "|".join(str(station_id) for station_id in station_ids)


def build_admin_display_name(station_names):
    if len(station_names) <= 2:
        return f"{station_names[0]} to {station_names[-1]}"

    via = ", ".join(station_names[1:-1][:3])
    return f"{station_names[0]} to {station_names[-1]} via {via}"


def load_station_map(snapshot_dir, operating_date):
    path = snapshot_dir / f"plk-station-id-map-{operating_date}.json"
    if not path.exists():
        raise SystemExit(f"Station map does not exist: {path}")

    payload = load_json(path)
    return {
        int(station["externalStationId"]): station
        for station in payload.get("stations", [])
        if station.get("externalStationId") is not None
    }


def load_route_details(detail_dir):
    details = []
    errors = []
    for path in sorted(detail_dir.glob("*.json")):
        payload = load_json(path)
        if path.name.endswith(".error.json"):
            errors.append(payload)
            continue

        stops = sorted(payload.get("stations") or [], key=lambda stop: stop.get("orderNumber") or 0)
        station_ids = [
            int(stop["stationId"])
            for stop in stops
            if stop.get("stationId") is not None
        ]
        if not station_ids:
            continue

        key = "|".join(str(station_id) for station_id in station_ids)
        details.append(
            {
                "path": path.name,
                "payload": payload,
                "stops": stops,
                "stationIds": station_ids,
                "fingerprint": hashlib.sha256(key.encode("utf-8")).hexdigest()[:16],
            }
        )

    return details, errors


def service_duration_minutes(stops):
    first = None
    last = None

    for stop in stops:
        first = parse_time_to_minutes(stop.get("departureTime"), stop.get("departureDay"))
        if first is not None:
            break
        first = parse_time_to_minutes(stop.get("arrivalTime"), stop.get("arrivalDay"))
        if first is not None:
            break

    for stop in reversed(stops):
        last = parse_time_to_minutes(stop.get("arrivalTime"), stop.get("arrivalDay"))
        if last is not None:
            break
        last = parse_time_to_minutes(stop.get("departureTime"), stop.get("departureDay"))
        if last is not None:
            break

    if first is None or last is None:
        return None

    while last < first:
        last += 1440
    return max(1, last - first)


def choose_representative(services):
    return max(
        services,
        key=lambda service: (
            sum(1 for stop in service["stops"] if stop.get("arrivalTime") or stop.get("departureTime")),
            -int(service["payload"].get("orderId") or 0),
        ),
    )


def main():
    parser = argparse.ArgumentParser(description="Build an importable route-shape seed snapshot from cached PKPIC route details.")
    parser.add_argument("--operating-date", default=DEFAULT_OPERATING_DATE)
    args = parser.parse_args()

    repo_root = Path(__file__).resolve().parents[2]
    snapshot_dir = repo_root / "TrainTicketPlatformAPI" / "App_Data" / "SeedSnapshots"
    detail_dir = snapshot_dir / f"pkpic-route-details-{args.operating_date}"
    output_path = snapshot_dir / f"pkpic-route-shape-seed-snapshot-{args.operating_date}.json"
    manifest_path = snapshot_dir / f"pkpic-route-shape-seed-manifest-{args.operating_date}.json"

    station_map = load_station_map(snapshot_dir, args.operating_date)
    details, errors = load_route_details(detail_dir)

    services_by_shape = defaultdict(list)
    for detail in details:
        services_by_shape[detail["fingerprint"]].append(detail)

    used_station_codes = set()
    station_codes = {}
    route_station_ids = sorted({station_id for detail in details for station_id in detail["stationIds"]})
    stations = []
    for station_id in route_station_ids:
        station = station_map[station_id]
        code = build_station_code(station, used_station_codes)
        station_codes[station_id] = code
        stations.append(
            {
                "code": code,
                "name": station["name"],
                "city": station.get("city") or station["name"],
                "externalStationId": station_id,
            }
        )

    routes = []
    route_manifest = []
    operating_date = dt.date.fromisoformat(args.operating_date)
    for fingerprint, services in sorted(services_by_shape.items(), key=lambda item: item[0]):
        representative = choose_representative(services)
        payload = representative["payload"]
        stops = representative["stops"]
        station_ids = representative["stationIds"]
        station_names = [station_map[station_id]["name"] for station_id in station_ids]
        duration = service_duration_minutes(stops) or max(1, (len(station_ids) - 1) * 18)
        base_minutes = None
        for stop in stops:
            base_minutes = parse_time_to_minutes(stop.get("departureTime"), stop.get("departureDay"))
            if base_minutes is not None:
                break
            base_minutes = parse_time_to_minutes(stop.get("arrivalTime"), stop.get("arrivalDay"))
            if base_minutes is not None:
                break

        route_code = build_route_code(station_ids, fingerprint)
        route_stops = []
        for index, stop in enumerate(stops):
            station_id = int(stop["stationId"])
            route_stops.append(
                {
                    "externalStationId": station_id,
                    "stationCode": station_codes[station_id],
                    "stopOrder": index,
                    "arrivalOffsetMinutes": offset_from_base(stop.get("arrivalTime"), stop.get("arrivalDay"), base_minutes),
                    "departureOffsetMinutes": offset_from_base(stop.get("departureTime"), stop.get("departureDay"), base_minutes),
                    "platform": stop.get("arrivalPlatform") or stop.get("departurePlatform") or "",
                    "track": stop.get("arrivalTrack") or stop.get("departureTrack") or "",
                    "stopType": stop.get("stopTypeName") or "",
                    "externalStopTypeId": stop.get("stopTypeId"),
                    "externalStopTypeName": stop.get("stopTypeName") or "",
                    "externalArrivalTrainNumber": stop.get("arrivalTrainNumber") or "",
                    "externalDepartureTrainNumber": stop.get("departureTrainNumber") or "",
                    "arrivalDayOffset": stop.get("arrivalDay"),
                    "departureDayOffset": stop.get("departureDay"),
                }
            )

        routes.append(
            {
                "code": route_code,
                "name": f"{station_names[0]} to {station_names[-1]}",
                "adminDisplayName": build_admin_display_name(station_names),
                "routeFingerprint": build_route_fingerprint(station_ids),
                "distanceKm": round(max(1, duration * 1.35), 2),
                "estimatedDurationMinutes": duration,
                "operatingDays": "Imported",
                "intermediateStops": ", ".join(station_names[1:-1])[:1000],
                "isActive": True,
                "externalScheduleId": payload.get("scheduleId"),
                "externalOrderId": payload.get("orderId"),
                "externalTrainOrderId": payload.get("trainOrderId"),
                "externalOperatingDate": operating_date.isoformat(),
                "departureExternalStationId": station_ids[0],
                "arrivalExternalStationId": station_ids[-1],
                "stops": route_stops,
            }
        )
        route_manifest.append(
            {
                "routeCode": route_code,
                "routeFingerprint": build_route_fingerprint(station_ids),
                "serviceRecordCount": len(services),
                "representativeOrderId": payload.get("orderId"),
                "departureName": station_names[0],
                "arrivalName": station_names[-1],
                "stationCount": len(station_ids),
                "estimatedDurationMinutes": duration,
            }
        )

    snapshot = {
        "externalSource": "PLK",
        "exportedAtUtc": dt.datetime.utcnow().replace(microsecond=0).isoformat() + "Z",
        "stations": stations,
        "trains": [],
        "routes": routes,
        "trips": [],
    }
    output_path.write_text(json.dumps(snapshot, ensure_ascii=False, indent=2), encoding="utf-8")

    manifest = {
        "generatedAtUtc": snapshot["exportedAtUtc"],
        "phase": "phase-4-route-shape-seed-snapshot",
        "operatingDate": args.operating_date,
        "usableServiceRecordCount": len(details),
        "errorRecordCount": len(errors),
        "stationCount": len(stations),
        "routeShapeCount": len(routes),
        "manifest": route_manifest,
    }
    manifest_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")

    print(
        json.dumps(
            {
                "outputPath": str(output_path.relative_to(repo_root)).replace("\\", "/"),
                "manifestPath": str(manifest_path.relative_to(repo_root)).replace("\\", "/"),
                "stationCount": len(stations),
                "routeShapeCount": len(routes),
                "usableServiceRecordCount": len(details),
                "errorRecordCount": len(errors),
            },
            ensure_ascii=False,
            indent=2,
        )
    )


if __name__ == "__main__":
    main()
