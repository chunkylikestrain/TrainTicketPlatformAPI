import argparse
import datetime as dt
import hashlib
import json
from collections import Counter, defaultdict
from pathlib import Path


DEFAULT_OPERATING_DATE = "2026-07-03"


def load_category_classifications(snapshot_dir):
    classification_files = sorted(snapshot_dir.glob("pkpic-category-classification-*.json"), reverse=True)
    for path in classification_files:
        try:
            payload = json.loads(path.read_text(encoding="utf-8-sig"))
        except (OSError, json.JSONDecodeError):
            continue

        mappings = {}
        for mapping in payload.get("categoryMappings") or []:
            raw = mapping.get("rawCategory")
            if raw:
                mappings[raw] = {
                    "railBookCategoryCode": mapping.get("railBookCategoryCode") or "UNKNOWN",
                    "railBookCategoryName": mapping.get("railBookCategoryName") or "Unknown",
                    "passengerDisplayCategory": mapping.get("passengerDisplayCategory") or "InterCity",
                    "trainTemplate": mapping.get("trainTemplate") or "Long-distance InterCity coaches",
                    "needsReview": bool(mapping.get("needsReview")),
                }
        if mappings:
            return mappings

    return {}


def parse_time(value):
    if not value:
        return None
    return str(value)[:8]


def load_station_names(snapshot_dir):
    station_names = {}
    station_map_files = sorted(snapshot_dir.glob("plk-station-id-map-*.json"), reverse=True)
    for path in station_map_files:
        try:
            payload = json.loads(path.read_text(encoding="utf-8-sig"))
        except (OSError, json.JSONDecodeError):
            continue

        for station in payload.get("stations") or []:
            station_id = station.get("externalStationId")
            name = station.get("name")
            if station_id is not None and name:
                station_names[int(station_id)] = name

    candidate_files = [
        "ic-station-candidates-locality-enriched-2026-07-03.json",
        "ic-station-candidates-filtered-2026-07-03.json",
        "ic-station-candidates-classified-2026-07-03.json",
        "ic-station-candidates-2026-07-03.json",
    ]

    for filename in candidate_files:
        path = snapshot_dir / filename
        if not path.exists():
            continue

        try:
            payload = json.loads(path.read_text(encoding="utf-8-sig"))
        except (OSError, json.JSONDecodeError):
            continue

        candidates = payload.get("stations") or payload.get("candidates") or payload
        if isinstance(candidates, dict):
            candidates = candidates.get("items") or []
        if not isinstance(candidates, list):
            continue

        for station in candidates:
            if not isinstance(station, dict):
                continue
            station_id = (
                station.get("externalStationId")
                or station.get("stationId")
                or station.get("id")
            )
            name = station.get("stationName") or station.get("name")
            if station_id is not None and name and int(station_id) not in station_names:
                station_names[int(station_id)] = name

    return station_names


def stop_time(stop, kind):
    if kind == "departure":
        return parse_time(stop.get("departureTime"))
    return parse_time(stop.get("arrivalTime"))


def stop_day(stop, kind):
    if kind == "departure":
        return stop.get("departureDay")
    return stop.get("arrivalDay")


def service_departure_time(stops):
    for stop in stops:
        value = stop_time(stop, "departure")
        if value:
            return value
    return None


def service_arrival_time(stops):
    for stop in reversed(stops):
        value = stop_time(stop, "arrival")
        if value:
            return value
    return None


def normalize_route_detail(payload):
    stops = sorted(payload.get("stations") or [], key=lambda stop: stop.get("orderNumber") or 0)
    station_ids = [
        int(stop["stationId"])
        for stop in stops
        if stop.get("stationId") is not None
    ]
    if not station_ids:
        return None

    shape_key = "|".join(str(station_id) for station_id in station_ids)
    reverse_shape_key = "|".join(str(station_id) for station_id in reversed(station_ids))
    undirected_shape_key = min(shape_key, reverse_shape_key)

    return {
        "scheduleId": payload.get("scheduleId"),
        "orderId": payload.get("orderId"),
        "trainOrderId": payload.get("trainOrderId"),
        "name": (payload.get("name") or "").strip() or "(unnamed)",
        "carrierCode": payload.get("carrierCode") or "",
        "nationalNumber": payload.get("nationalNumber") or "",
        "internationalArrivalNumber": payload.get("internationalArrivalNumber") or "",
        "internationalDepartureNumber": payload.get("internationalDepartureNumber") or "",
        "commercialCategorySymbol": payload.get("commercialCategorySymbol") or "(blank)",
        "operatingDates": payload.get("operatingDates") or [],
        "stationIds": station_ids,
        "stops": stops,
        "departureTime": service_departure_time(stops),
        "arrivalTime": service_arrival_time(stops),
        "routeFingerprint": hashlib.sha256(shape_key.encode("utf-8")).hexdigest()[:16],
        "undirectedRouteFingerprint": hashlib.sha256(undirected_shape_key.encode("utf-8")).hexdigest()[:16],
    }


def build_shape_summary(fingerprint, services, station_names, category_classifications):
    representative = services[0]
    station_ids = representative["stationIds"]
    station_name_list = [station_names.get(station_id, f"#{station_id}") for station_id in station_ids]
    unknown_station_ids = [station_id for station_id in station_ids if station_id not in station_names]
    categories = Counter(service["commercialCategorySymbol"] for service in services)
    railbook_categories = Counter(
        category_classifications.get(service["commercialCategorySymbol"], {}).get("railBookCategoryCode", "UNKNOWN")
        for service in services
    )
    passenger_categories = Counter(
        category_classifications.get(service["commercialCategorySymbol"], {}).get("passengerDisplayCategory", "InterCity")
        for service in services
    )
    needs_category_review = any(
        category_classifications.get(service["commercialCategorySymbol"], {}).get("needsReview", True)
        for service in services
    )
    train_names = Counter(service["name"] for service in services)
    train_numbers = Counter(
        service["nationalNumber"]
        for service in services
        if service["nationalNumber"]
    )
    departures = sorted(
        time for time in (service["departureTime"] for service in services) if time
    )
    arrivals = sorted(
        time for time in (service["arrivalTime"] for service in services) if time
    )

    return {
        "routeFingerprint": fingerprint,
        "undirectedRouteFingerprint": representative["undirectedRouteFingerprint"],
        "serviceRecordCount": len(services),
        "departureExternalStationId": station_ids[0],
        "arrivalExternalStationId": station_ids[-1],
        "departureName": station_name_list[0],
        "arrivalName": station_name_list[-1],
        "stationCount": len(station_ids),
        "knownStationNameCount": len(station_ids) - len(unknown_station_ids),
        "unknownStationIds": unknown_station_ids,
        "commercialCategories": dict(categories.most_common()),
        "railBookCategories": dict(railbook_categories.most_common()),
        "passengerDisplayCategories": dict(passenger_categories.most_common()),
        "needsCategoryReview": needs_category_review,
        "sampleTrainNames": [name for name, _ in train_names.most_common(8)],
        "sampleTrainNumbers": [number for number, _ in train_numbers.most_common(8)],
        "firstDepartureTime": departures[0] if departures else None,
        "lastDepartureTime": departures[-1] if departures else None,
        "firstArrivalTime": arrivals[0] if arrivals else None,
        "lastArrivalTime": arrivals[-1] if arrivals else None,
        "stationIds": station_ids,
        "stationNames": station_name_list,
        "sampleServices": [
            {
                "scheduleId": service["scheduleId"],
                "orderId": service["orderId"],
                "trainOrderId": service["trainOrderId"],
                "name": service["name"],
                "nationalNumber": service["nationalNumber"],
                "category": service["commercialCategorySymbol"],
                "departureTime": service["departureTime"],
                "arrivalTime": service["arrivalTime"],
            }
            for service in services[:8]
        ],
    }


def build_review(snapshot_dir, operating_date):
    detail_dir = snapshot_dir / f"pkpic-route-details-{operating_date}"
    index_path = snapshot_dir / f"pkpic-route-index-{operating_date}.json"

    if not detail_dir.exists():
        raise SystemExit(f"Route detail cache directory was not found: {detail_dir}")
    if not index_path.exists():
        raise SystemExit(f"Route index was not found: {index_path}")

    station_names = load_station_names(snapshot_dir)
    category_classifications = load_category_classifications(snapshot_dir)
    index = json.loads(index_path.read_text(encoding="utf-8"))
    pkpic_records = [
        route
        for route in index.get("routes", [])
        if (route.get("carrierCode") or "").upper() == "IC"
    ]

    services = []
    errors = []
    for path in sorted(detail_dir.glob("*.json")):
        try:
            payload = json.loads(path.read_text(encoding="utf-8-sig"))
        except (OSError, json.JSONDecodeError) as ex:
            errors.append({"file": path.name, "error": type(ex).__name__, "message": str(ex)})
            continue

        if path.name.endswith(".error.json"):
            errors.append(
                {
                    "file": path.name,
                    "scheduleId": payload.get("scheduleId"),
                    "orderId": payload.get("orderId"),
                    "status": payload.get("status"),
                    "reason": payload.get("reason"),
                }
            )
            continue

        service = normalize_route_detail(payload)
        if service is None:
            errors.append(
                {
                    "file": path.name,
                    "scheduleId": payload.get("scheduleId"),
                    "orderId": payload.get("orderId"),
                    "error": "NoStations",
                }
            )
            continue

        services.append(service)

    by_shape = defaultdict(list)
    by_undirected = defaultdict(list)
    for service in services:
        by_shape[service["routeFingerprint"]].append(service)
        by_undirected[service["undirectedRouteFingerprint"]].append(service)

    shape_summaries = [
        build_shape_summary(
            fingerprint,
            sorted(items, key=lambda item: (item["departureTime"] or "", item["orderId"] or 0)),
            station_names,
            category_classifications,
        )
        for fingerprint, items in by_shape.items()
    ]
    shape_summaries.sort(
        key=lambda shape: (
            -shape["serviceRecordCount"],
            shape["departureName"],
            shape["arrivalName"],
            shape["routeFingerprint"],
        )
    )

    category_counts = Counter(service["commercialCategorySymbol"] for service in services)
    unknown_station_ids = sorted(
        {
            station_id
            for service in services
            for station_id in service["stationIds"]
            if station_id not in station_names
        }
    )

    return {
        "generatedAtUtc": dt.datetime.utcnow().replace(microsecond=0).isoformat() + "Z",
        "phase": "phase-1-route-shape-review",
        "source": "PLK OpenRailway cached route details",
        "operatingDate": operating_date,
        "sourceRouteIndexFile": str(index_path).replace("\\", "/"),
        "sourceRouteDetailDirectory": str(detail_dir).replace("\\", "/"),
        "pkpicRouteRecordCount": len(pkpic_records),
        "usableServiceRecordCount": len(services),
        "errorRecordCount": len(errors),
        "directedRouteShapeCount": len(by_shape),
        "undirectedRouteShapeCount": len(by_undirected),
        "knownStationIdCount": len(station_names),
        "unknownStationIdCountInUsableServices": len(unknown_station_ids),
        "unknownStationIdsInUsableServices": unknown_station_ids,
        "commercialCategoryCounts": dict(category_counts.most_common()),
        "categoryClassificationLoaded": bool(category_classifications),
        "topRouteShapes": shape_summaries[:30],
        "routeShapes": shape_summaries,
        "errorRecords": errors,
    }


def main():
    parser = argparse.ArgumentParser(description="Build a human-reviewable route-shape artifact from cached PKPIC route details.")
    parser.add_argument("--operating-date", default=DEFAULT_OPERATING_DATE)
    args = parser.parse_args()

    repo_root = Path(__file__).resolve().parents[2]
    snapshot_dir = repo_root / "TrainTicketPlatformAPI" / "App_Data" / "SeedSnapshots"
    output_path = snapshot_dir / f"pkpic-route-shape-review-{args.operating_date}.json"

    review = build_review(snapshot_dir, args.operating_date)
    output_path.write_text(json.dumps(review, ensure_ascii=False, indent=2), encoding="utf-8")

    print(
        json.dumps(
            {
                "outputPath": str(output_path.relative_to(repo_root)).replace("\\", "/"),
                "usableServiceRecordCount": review["usableServiceRecordCount"],
                "errorRecordCount": review["errorRecordCount"],
                "directedRouteShapeCount": review["directedRouteShapeCount"],
                "undirectedRouteShapeCount": review["undirectedRouteShapeCount"],
                "unknownStationIdCountInUsableServices": review["unknownStationIdCountInUsableServices"],
                "commercialCategoryCounts": review["commercialCategoryCounts"],
            },
            ensure_ascii=False,
            indent=2,
        )
    )


if __name__ == "__main__":
    main()
