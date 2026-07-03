import argparse
import datetime as dt
import hashlib
import json
import re
from collections import Counter
from pathlib import Path


DEFAULT_OPERATING_DATE = "2026-07-03"


CATEGORY_ALIASES = {
    "EIP": ("EIP", "Express InterCity Premium"),
    "EIC": ("EIC", "Express InterCity"),
    "TLK": ("TLK", "Twoje Linie Kolejowe"),
    "IC": ("IC", "InterCity"),
    "IC+": ("IC", "InterCity"),
    "ICN": ("IC", "InterCity"),
    "MP": ("IC", "InterCity"),
    "EC": ("IC", "InterCity"),
    "EN": ("IC", "InterCity"),
}


def load_json(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def parse_time(value, day_offset, operating_date):
    if not value:
        return None

    hours, minutes, seconds, *_ = [int(part) for part in str(value).split(":")]
    return dt.datetime.combine(
        operating_date + dt.timedelta(days=day_offset or 0),
        dt.time(hours, minutes, seconds),
    )


def iso_datetime(value):
    return value.replace(microsecond=0).isoformat()


def clean_train_number(value):
    if not value:
        return ""

    digits = re.sub(r"[^0-9A-Za-z]", "", str(value))
    if digits.startswith("0") and len(digits) > 1:
        trimmed = digits.lstrip("0")
        return trimmed or digits
    return digits


def public_train_number(payload, stops):
    values = [
        payload.get("nationalNumber"),
        payload.get("internationalDepartureNumber"),
        payload.get("internationalArrivalNumber"),
    ]
    for stop in stops:
        values.append(stop.get("departureTrainNumber"))
        values.append(stop.get("arrivalTrainNumber"))

    for value in values:
        number = clean_train_number(value)
        if number:
            return number

    return str(payload.get("orderId") or payload.get("trainOrderId") or "0")


def split_raw_categories(raw_category):
    return [
        part.strip().upper()
        for part in re.split(r"[/,+ ]+", raw_category or "")
        if part.strip()
    ]


def classify_category(raw_category):
    normalized = (raw_category or "").strip().upper()
    if normalized == "EIP":
        return CATEGORY_ALIASES["EIP"]
    if normalized == "EIC":
        return CATEGORY_ALIASES["EIC"]
    if normalized == "TLK":
        return CATEGORY_ALIASES["TLK"]

    return CATEGORY_ALIASES["IC"]


def public_category_label(raw_category, category_code):
    normalized = (raw_category or "").strip().upper()
    if not normalized:
        return category_code

    parts = split_raw_categories(normalized)
    visible_parts = [part for part in parts if part in {"EC", "EN", "EIP", "EIC", "IC", "ICN", "TLK"}]
    if visible_parts:
        return "/".join(dict.fromkeys(visible_parts))

    return category_code


def route_fingerprint(station_ids):
    return "PLK:" + "|".join(str(station_id) for station_id in station_ids)


def shape_hash(station_ids):
    key = "|".join(str(station_id) for station_id in station_ids)
    return hashlib.sha256(key.encode("utf-8")).hexdigest()[:16]


def load_route_shape_snapshot(snapshot_dir, operating_date):
    path = snapshot_dir / f"pkpic-route-shape-seed-snapshot-{operating_date}.json"
    if not path.exists():
        raise SystemExit(f"Route-shape seed snapshot does not exist: {path}")

    return load_json(path)


def load_route_code_by_fingerprint(snapshot_dir, operating_date):
    snapshot = load_route_shape_snapshot(snapshot_dir, operating_date)
    return {
        route["routeFingerprint"]: route["code"]
        for route in snapshot.get("routes", [])
        if route.get("routeFingerprint") and route.get("code")
    }


def load_station_names(snapshot_dir, operating_date):
    path = snapshot_dir / f"plk-station-id-map-{operating_date}.json"
    if not path.exists():
        raise SystemExit(f"Station map does not exist: {path}")

    payload = load_json(path)
    return {
        int(station["externalStationId"]): station["name"]
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
        if len(station_ids) < 2:
            continue

        details.append(
            {
                "path": path.name,
                "payload": payload,
                "stops": stops,
                "stationIds": station_ids,
            }
        )

    return details, errors


def service_start_end(stops, operating_date):
    departure = None
    arrival = None

    for stop in stops:
        departure = parse_time(stop.get("departureTime"), stop.get("departureDay"), operating_date)
        if departure:
            break
        departure = parse_time(stop.get("arrivalTime"), stop.get("arrivalDay"), operating_date)
        if departure:
            break

    for stop in reversed(stops):
        arrival = parse_time(stop.get("arrivalTime"), stop.get("arrivalDay"), operating_date)
        if arrival:
            break
        arrival = parse_time(stop.get("departureTime"), stop.get("departureDay"), operating_date)
        if arrival:
            break

    if not departure or not arrival:
        return None, None

    while arrival < departure:
        arrival += dt.timedelta(days=1)
    return departure, arrival


def first_platform_track(stops):
    first = stops[0] if stops else {}
    return (
        first.get("departurePlatform") or first.get("arrivalPlatform") or "",
        first.get("departureTrack") or first.get("arrivalTrack") or "",
    )


def is_night_or_sleeper_service(raw_category, train_name):
    normalized_category = (raw_category or "").upper()
    normalized_name = (train_name or "").upper()
    return (
        "EN" in split_raw_categories(normalized_category)
        or "ICN" in split_raw_categories(normalized_category)
        or "NIGHT" in normalized_name
        or "CHOPIN" in normalized_name
        or "CARPATIA" in normalized_name
        or "ADRIATIC" in normalized_name
        or "KYIV EXPRESS" in normalized_name
    )


def carriage_template(category_code, raw_category="", train_name=""):
    if category_code == "EIP":
        return [
            carriage("1", 1, "Class 1", "OpenFirst", "ED250-1 first class cab unit", 54, notes="Fixed first-class Pendolino cab unit."),
            carriage("2", 2, "Class 2", "EmuSecondFamilyOpen", "ED250-2 family and open second class", 98, family=True, notes="Second-class unit with family compartment and open-space seating."),
            carriage("3", 3, "Class 2", "EmuDiningAccessible", "ED250-3 accessible dining unit", 12, accessible=True, dining=True, notes="Accessible WARS dining unit with wheelchair spaces."),
            carriage("4", 4, "Class 2", "EmuSecondOpen", "ED250-4 second class open unit", 88, notes="Fixed second-class open-space unit."),
            carriage("5", 5, "Class 2", "EmuSecondOpen", "ED250-5 second class open unit", 88, notes="Fixed second-class open-space unit."),
            carriage("6", 6, "Class 2", "EmuSecondOpen", "ED250-6 second class open unit", 88, notes="Fixed second-class open-space unit."),
            carriage("7", 7, "Class 2", "EmuSecondQuiet", "ED250-7 quiet second class cab unit", 88, notes="Dedicated quiet second-class end unit, not an accessible coach."),
        ]

    if category_code == "EIC":
        return [
            carriage("1", 1, "Class 1", "FirstCompartment", "A9nouz first-class compartment", 54),
            carriage("2", 2, "Dining", "Restaurant", "WRnouz WARS restaurant", 0, dining=True),
            carriage("3", 3, "Class 2", "SecondCompartment", "B10nouz second-class compartment", 66),
            carriage("4", 4, "Class 2", "OpenSecondAccessible", "B8bnopuz accessible open coach", 82, accessible=True),
            carriage("5", 5, "Class 2", "OpenSecondBike", "B7nopuvz bicycle open coach", 72, bike=True),
            carriage("6", 6, "Class 2", "OpenSecond", "B9nopuvz second-class open coach", 88),
            carriage("7", 7, "Class 2", "SecondCompartment", "B10nouz second-class compartment", 66),
        ]

    if is_night_or_sleeper_service(raw_category, train_name):
        return [
            carriage("1", 1, "Sleeper", "InternationalSleeper", "WLAB international sleeper coach", 30, notes="Sleeper coach for overnight international service."),
            carriage("2", 2, "Couchette", "Couchette", "Bc couchette coach", 30, accessible=True, notes="Four-berth couchette coach with accessible compartment."),
            carriage("3", 3, "Class 1/2", "ComboFirstSecond", "AB9nouz first/second combo", 54),
            carriage("4", 4, "Class 2", "SecondCompartment", "B10nouz second-class compartment", 66),
            carriage("5", 5, "Class 2", "OpenSecondBike", "B7nopuvz bicycle open coach", 72, bike=True),
            carriage("6", 6, "Class 2", "OpenSecond", "B9nopuvz second-class open coach", 88),
        ]

    return [
        carriage("1", 1, "Class 1/2", "ComboFirstSecond", "AB9nouz first/second combo", 54),
        carriage("2", 2, "Class 2", "SecondCompartment", "B10nouz second-class compartment", 66),
        carriage("3", 3, "Class 2", "SecondFamilyCompartment", "Bmnopux family compartment", 66, family=True),
        carriage("4", 4, "Class 2", "OpenSecondBike", "B7nopuvz bicycle open coach", 72, bike=True),
        carriage("5", 5, "Class 2", "OpenSecondAccessible", "B8bnopuz accessible open coach", 82, accessible=True),
        carriage("6", 6, "Class 2", "OpenSecond", "B9nopuvz second-class open coach", 88),
    ]


def carriage(coach, position, class_type, layout_type, vehicle_type, seat_count, bike=False, accessible=False, family=False, dining=False, notes=""):
    return {
        "coach": coach,
        "position": position,
        "classType": class_type,
        "layoutType": layout_type,
        "vehicleType": vehicle_type,
        "seatCount": seat_count,
        "hasBikeSpace": bike,
        "hasAccessibleSpace": accessible,
        "hasFamilyCompartment": family,
        "hasDiningSection": dining,
        "notes": notes,
    }


def estimate_fares(category_code, duration_minutes):
    route_factor = min(max(duration_minutes / 600.0, 0), 1)
    if category_code == "EIP":
        return 350, 200
    if category_code == "EIC":
        return 250, 170
    if category_code == "TLK":
        return round(60 + 40 * route_factor), round(20 + 50 * route_factor)
    return round(75 + 55 * route_factor), round(25 + 25 * route_factor)


def main():
    parser = argparse.ArgumentParser(description="Build a train/trip seed snapshot from cached PKPIC route details.")
    parser.add_argument("--operating-date", default=DEFAULT_OPERATING_DATE)
    args = parser.parse_args()

    repo_root = Path(__file__).resolve().parents[2]
    snapshot_dir = repo_root / "TrainTicketPlatformAPI" / "App_Data" / "SeedSnapshots"
    detail_dir = snapshot_dir / f"pkpic-route-details-{args.operating_date}"
    output_path = snapshot_dir / f"pkpic-train-trip-seed-snapshot-{args.operating_date}.json"
    manifest_path = snapshot_dir / f"pkpic-train-trip-seed-manifest-{args.operating_date}.json"

    route_shape_snapshot = load_route_shape_snapshot(snapshot_dir, args.operating_date)
    route_codes = {
        route["routeFingerprint"]: route["code"]
        for route in route_shape_snapshot.get("routes", [])
        if route.get("routeFingerprint") and route.get("code")
    }
    station_names = load_station_names(snapshot_dir, args.operating_date)
    details, errors = load_route_details(detail_dir)
    operating_date = dt.date.fromisoformat(args.operating_date)

    trains_by_code = {}
    trips = []
    skipped = []
    category_counts = Counter()

    for detail in details:
        payload = detail["payload"]
        stops = detail["stops"]
        station_ids = detail["stationIds"]
        fingerprint = route_fingerprint(station_ids)
        route_code = route_codes.get(fingerprint)
        if not route_code:
            skipped.append({"path": detail["path"], "reason": "missing-route-shape", "shapeHash": shape_hash(station_ids)})
            continue

        departure_time, arrival_time = service_start_end(stops, operating_date)
        if not departure_time or not arrival_time:
            skipped.append({"path": detail["path"], "reason": "missing-service-times", "orderId": payload.get("orderId")})
            continue

        raw_category = payload.get("commercialCategorySymbol") or ""
        category_code, category_name = classify_category(raw_category)
        display_category = public_category_label(raw_category, category_code)
        train_number = public_train_number(payload, stops)
        train_code = f"{category_code}-{train_number}"[:32]
        train_name = (payload.get("name") or "").strip()
        origin = station_names.get(station_ids[0], str(station_ids[0]))
        destination = station_names.get(station_ids[-1], str(station_ids[-1]))
        platform, track = first_platform_track(stops)
        duration_minutes = max(1, int((arrival_time - departure_time).total_seconds() // 60))
        class1, class2 = estimate_fares(category_code, duration_minutes)
        carriages = carriage_template(category_code, raw_category, train_name)
        seats_per_carriage = max((item["seatCount"] for item in carriages), default=0)

        if train_code not in trains_by_code:
            trains_by_code[train_code] = {
                "code": train_code,
                "name": " ".join(part for part in [display_category, train_number, train_name] if part).strip(),
                "type": category_name,
                "carriageCount": len(carriages),
                "seatsPerCarriage": seats_per_carriage,
                "status": "Active",
                "departureStation": origin,
                "arrivalStation": destination,
                "departureTime": iso_datetime(departure_time),
                "arrivalTime": iso_datetime(arrival_time),
                "externalCarrierCode": payload.get("carrierCode") or "PKPIC",
                "externalCommercialCategorySymbol": category_code,
                "externalNationalNumber": train_number,
                "externalInternationalArrivalNumber": "",
                "externalInternationalDepartureNumber": "",
                "carriages": carriages,
            }

        fares = [
            {"classType": "Class 1", "price": class1, "currency": "PLN"},
            {"classType": "Class 2", "price": class2, "currency": "PLN"},
        ]
        trips.append(
            {
                "trainCode": train_code,
                "routeCode": route_code,
                "departureTime": iso_datetime(departure_time),
                "arrivalTime": iso_datetime(arrival_time),
                "platform": platform,
                "track": track,
                "status": "Scheduled",
                "delayMinutes": 0,
                "cancellationReason": "",
                "originalPlatform": platform,
                "originalTrack": track,
                "disruptionMessage": "",
                "disruptionSeverity": "",
                "externalScheduleId": payload.get("scheduleId"),
                "externalOrderId": payload.get("orderId"),
                "externalTrainOrderId": payload.get("trainOrderId"),
                "externalOperatingDate": operating_date.isoformat(),
                "externalRawVersion": json.dumps(
                    {
                        "rawCategory": raw_category,
                        "displayCategory": category_code,
                        "publicCategory": display_category,
                        "name": payload.get("name"),
                        "number": train_number,
                        "sourceFile": detail["path"],
                    },
                    ensure_ascii=False,
                    separators=(",", ":"),
                ),
                "fares": fares,
            }
        )
        category_counts[category_code] += 1

    snapshot = {
        "externalSource": "PLK",
        "exportedAtUtc": dt.datetime.utcnow().replace(microsecond=0).isoformat() + "Z",
        "stations": route_shape_snapshot.get("stations", []),
        "trains": sorted(trains_by_code.values(), key=lambda train: train["code"]),
        "routes": route_shape_snapshot.get("routes", []),
        "trips": sorted(trips, key=lambda trip: (trip["departureTime"], trip["trainCode"])),
    }
    output_path.write_text(json.dumps(snapshot, ensure_ascii=False, indent=2), encoding="utf-8")

    manifest = {
        "generatedAtUtc": snapshot["exportedAtUtc"],
        "phase": "phase-5-train-trip-seed-snapshot",
        "operatingDate": args.operating_date,
        "usableServiceRecordCount": len(details),
        "errorRecordCount": len(errors),
        "stationCount": len(snapshot["stations"]),
        "routeShapeCount": len(snapshot["routes"]),
        "trainCount": len(snapshot["trains"]),
        "tripCount": len(snapshot["trips"]),
        "skippedServiceRecordCount": len(skipped),
        "skipped": skipped,
        "categoryCounts": dict(sorted(category_counts.items())),
    }
    manifest_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")

    print(
        json.dumps(
            {
                "outputPath": str(output_path.relative_to(repo_root)).replace("\\", "/"),
                "manifestPath": str(manifest_path.relative_to(repo_root)).replace("\\", "/"),
                "trainCount": len(snapshot["trains"]),
                "stationCount": len(snapshot["stations"]),
                "routeShapeCount": len(snapshot["routes"]),
                "tripCount": len(snapshot["trips"]),
                "skippedServiceRecordCount": len(skipped),
                "categoryCounts": dict(sorted(category_counts.items())),
            },
            ensure_ascii=False,
            indent=2,
        )
    )


if __name__ == "__main__":
    main()
