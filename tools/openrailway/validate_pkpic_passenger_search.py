import argparse
import datetime as dt
import json
import sys
import unicodedata
from pathlib import Path


DEFAULT_OPERATING_DATE = "2026-07-03"


def load_json(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def normalize(value):
    text = (value or "").strip().lower()
    text = unicodedata.normalize("NFKD", text)
    text = "".join(ch for ch in text if unicodedata.category(ch) != "Mn")
    return text.replace("ł", "l")


def parse_dt(value):
    return dt.datetime.fromisoformat(value)


def build_station_indexes(snapshot):
    by_id = {}
    by_code = {}
    for station in snapshot["stations"]:
        by_id[station["externalStationId"]] = station
        by_code[station["code"]] = station
    return by_id, by_code


def ordered_route_station_ids(route):
    stops = sorted(route["stops"], key=lambda stop: stop["stopOrder"])
    if stops and stops[0].get("externalStationId") == route.get("departureExternalStationId"):
        stops = stops[1:]
    if stops and stops[-1].get("externalStationId") == route.get("arrivalExternalStationId"):
        stops = stops[:-1]

    return [
        route["departureExternalStationId"],
        *[stop["externalStationId"] for stop in stops],
        route["arrivalExternalStationId"],
    ]


def stop_time(trip, route, station_index, station_count):
    if station_index == 0:
        return parse_dt(trip["departureTime"])
    if station_index == station_count - 1:
        return parse_dt(trip["arrivalTime"])

    stops = sorted(route["stops"], key=lambda stop: stop["stopOrder"])
    if stops and stops[0].get("externalStationId") == route.get("departureExternalStationId"):
        stops = stops[1:]
    if stops and stops[-1].get("externalStationId") == route.get("arrivalExternalStationId"):
        stops = stops[:-1]

    stop = stops[station_index - 1]
    offset = stop.get("departureOffsetMinutes") or stop.get("arrivalOffsetMinutes")
    if offset is None:
        total_minutes = (parse_dt(trip["arrivalTime"]) - parse_dt(trip["departureTime"])).total_seconds() / 60
        offset = round(total_minutes * station_index / max(1, station_count - 1))

    return parse_dt(trip["departureTime"]) + dt.timedelta(minutes=offset)


def search(snapshot, from_name, to_name, date_value):
    station_by_id, _ = build_station_indexes(snapshot)
    routes = {route["code"]: route for route in snapshot["routes"]}
    trains = {train["code"]: train for train in snapshot["trains"]}
    target_date = dt.date.fromisoformat(date_value)
    results = []

    for trip in snapshot["trips"]:
        route = routes[trip["routeCode"]]
        station_ids = ordered_route_station_ids(route)
        names = [station_by_id[station_id]["name"] for station_id in station_ids]
        normalized_names = [normalize(name) for name in names]
        try:
            from_index = normalized_names.index(normalize(from_name))
            to_index = normalized_names.index(normalize(to_name))
        except ValueError:
            continue
        if from_index >= to_index:
            continue

        departure_time = stop_time(trip, route, from_index, len(station_ids))
        arrival_time = stop_time(trip, route, to_index, len(station_ids))
        if departure_time.date() != target_date:
            continue

        train = trains[trip["trainCode"]]
        results.append(
            {
                "train": train,
                "trip": trip,
                "route": route,
                "departureTime": departure_time,
                "arrivalTime": arrival_time,
                "stationIds": station_ids,
            }
        )

    return sorted(results, key=lambda item: (item["departureTime"], item["train"]["code"]))


def assert_search(snapshot, from_name, to_name, date_value, expected_prefix=None):
    results = search(snapshot, from_name, to_name, date_value)
    if not results:
        raise AssertionError(f"No passenger search results for {from_name} -> {to_name} on {date_value}")

    if expected_prefix and not any(result["train"]["name"].startswith(expected_prefix) for result in results):
        names = [result["train"]["name"] for result in results[:10]]
        raise AssertionError(
            f"No result for {from_name} -> {to_name} starts with {expected_prefix}. Sample: {names}"
        )

    first = results[0]
    if first["arrivalTime"] <= first["departureTime"]:
        raise AssertionError(f"Invalid time order for {from_name} -> {to_name}")
    if not first["trip"].get("fares"):
        raise AssertionError(f"Missing fares for {first['train']['name']}")
    if not first["train"].get("carriages"):
        raise AssertionError(f"Missing carriages for {first['train']['name']}")
    if len(first["stationIds"]) != len(set((index, station_id) for index, station_id in enumerate(first["stationIds"]))):
        raise AssertionError(f"Invalid station sequence for {first['route']['code']}")

    return {
        "from": from_name,
        "to": to_name,
        "date": date_value,
        "resultCount": len(results),
        "firstTrain": first["train"]["name"],
        "firstDeparture": first["departureTime"].isoformat(),
        "firstArrival": first["arrivalTime"].isoformat(),
    }


def find_public_category_check(snapshot, date_value, prefix):
    station_by_id, _ = build_station_indexes(snapshot)
    routes = {route["code"]: route for route in snapshot["routes"]}

    for trip in snapshot["trips"]:
        train = next(train for train in snapshot["trains"] if train["code"] == trip["trainCode"])
        if not train["name"].startswith(prefix):
            continue

        route = routes[trip["routeCode"]]
        station_ids = ordered_route_station_ids(route)
        for from_index in range(len(station_ids) - 1):
            departure_time = stop_time(trip, route, from_index, len(station_ids))
            if departure_time.date().isoformat() != date_value:
                continue

            to_index = min(len(station_ids) - 1, from_index + 1)
            from_name = station_by_id[station_ids[from_index]]["name"]
            to_name = station_by_id[station_ids[to_index]]["name"]
            return assert_search(snapshot, from_name, to_name, date_value, prefix)

    raise AssertionError(f"Could not find a {prefix} passenger segment on {date_value}")


def main():
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")

    parser = argparse.ArgumentParser(description="Validate passenger searches against the exported PKPIC seed snapshot.")
    parser.add_argument("--operating-date", default=DEFAULT_OPERATING_DATE)
    args = parser.parse_args()

    repo_root = Path(__file__).resolve().parents[2]
    path = repo_root / "TrainTicketPlatformAPI" / "App_Data" / "SeedSnapshots" / f"railbook-pkpic-{args.operating_date}.seed-snapshot.json"
    snapshot = load_json(path)

    routes = {route["code"] for route in snapshot["routes"]}
    trains = {train["code"] for train in snapshot["trains"]}
    missing_route_refs = [trip["routeCode"] for trip in snapshot["trips"] if trip["routeCode"] not in routes]
    missing_train_refs = [trip["trainCode"] for trip in snapshot["trips"] if trip["trainCode"] not in trains]
    if missing_route_refs or missing_train_refs:
        raise AssertionError(
            f"Missing references: routes={len(missing_route_refs)}, trains={len(missing_train_refs)}"
        )

    checks = [
        assert_search(snapshot, "Kraków Główny", "Gdynia Główna", args.operating_date, "EIP"),
        assert_search(snapshot, "Warszawa Centralna", "Kraków Główny", args.operating_date),
        assert_search(snapshot, "Warszawa Wschodnia", "Berlin Gesundbrunnen", args.operating_date, "EC/EIC"),
        find_public_category_check(snapshot, args.operating_date, "EN/IC"),
    ]

    result = {
        "snapshot": str(path.relative_to(repo_root)).replace("\\", "/"),
        "stationCount": len(snapshot["stations"]),
        "routeCount": len(snapshot["routes"]),
        "trainCount": len(snapshot["trains"]),
        "tripCount": len(snapshot["trips"]),
        "checks": checks,
    }
    print(json.dumps(result, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
