import argparse
import datetime as dt
import json
import os
import urllib.parse
import urllib.request
from pathlib import Path


USER_SECRETS_ID = "56689f1a-4b52-4c21-a008-06d0177db705"
DEFAULT_OPERATING_DATE = "2026-07-03"
BASE_URL = "https://pdp-api.plk-sa.pl"


def read_api_key():
    appdata = os.environ.get("APPDATA")
    if not appdata:
        raise SystemExit("APPDATA is not set; cannot locate ASP.NET user secrets.")

    secrets_path = (
        Path(appdata)
        / "Microsoft"
        / "UserSecrets"
        / USER_SECRETS_ID
        / "secrets.json"
    )
    if not secrets_path.exists():
        raise SystemExit(f"User secrets file was not found: {secrets_path}")

    secrets = json.loads(secrets_path.read_text(encoding="utf-8-sig"))
    api_key = secrets.get("OpenRailway:ApiKey") or secrets.get("PlkOpenRailway:ApiKey")
    if not api_key:
        raise SystemExit("OpenRailway:ApiKey is not configured in user secrets.")

    return api_key


def load_local_station_entries(snapshot_dir):
    entries = {}
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
            if station_id is None or not name:
                continue

            entries[int(station_id)] = {
                "externalStationId": int(station_id),
                "name": name,
                "source": "local-station-artifact",
                "code": station.get("code") or "",
                "city": station.get("city") or station.get("localityName") or "",
                "countryCode": station.get("countryCode") or "",
                "regionCode": station.get("regionCode") or "",
            }

    return entries


def fetch_station_dictionary(api_key, page_size):
    query = urllib.parse.urlencode({"page": 1, "pageSize": page_size})
    request = urllib.request.Request(
        f"{BASE_URL}/api/v1/dictionaries/stations?{query}",
        headers={"X-API-Key": api_key, "Accept": "application/json"},
    )

    with urllib.request.urlopen(request, timeout=60) as response:
        payload = json.loads(response.read().decode("utf-8-sig"))

    stations = payload.get("stations") or []
    return payload, {
        int(station["id"]): {
            "externalStationId": int(station["id"]),
            "name": station.get("name") or f"#{station['id']}",
            "source": "openrailway-station-dictionary",
        }
        for station in stations
        if station.get("id") is not None
    }


def collect_route_station_ids(snapshot_dir, operating_date):
    detail_dir = snapshot_dir / f"pkpic-route-details-{operating_date}"
    station_ids = set()

    for path in detail_dir.glob("*.json"):
        if path.name.endswith(".error.json"):
            continue
        try:
            payload = json.loads(path.read_text(encoding="utf-8-sig"))
        except (OSError, json.JSONDecodeError):
            continue
        for stop in payload.get("stations") or []:
            if stop.get("stationId") is not None:
                station_ids.add(int(stop["stationId"]))

    return station_ids


def main():
    parser = argparse.ArgumentParser(description="Build a PLK station-id map for cached PKPIC routes.")
    parser.add_argument("--operating-date", default=DEFAULT_OPERATING_DATE)
    parser.add_argument("--page-size", type=int, default=10000)
    parser.add_argument("--local-only", action="store_true")
    args = parser.parse_args()

    repo_root = Path(__file__).resolve().parents[2]
    snapshot_dir = repo_root / "TrainTicketPlatformAPI" / "App_Data" / "SeedSnapshots"
    output_path = snapshot_dir / f"plk-station-id-map-{args.operating_date}.json"

    route_station_ids = collect_route_station_ids(snapshot_dir, args.operating_date)
    entries = load_local_station_entries(snapshot_dir)
    dictionary_meta = None
    api_calls_spent = 0

    if not args.local_only:
        payload, dictionary_entries = fetch_station_dictionary(read_api_key(), args.page_size)
        api_calls_spent = 1
        dictionary_meta = {
            "generatedAt": payload.get("generatedAt"),
            "totalCount": payload.get("totalCount"),
            "returnedCount": payload.get("returnedCount"),
            "page": payload.get("page"),
            "pageSize": payload.get("pageSize"),
            "totalPages": payload.get("totalPages"),
        }
        entries.update(dictionary_entries)

    unknown_route_station_ids = sorted(station_id for station_id in route_station_ids if station_id not in entries)
    used_entries = [
        entries[station_id]
        for station_id in sorted(route_station_ids)
        if station_id in entries
    ]

    result = {
        "generatedAtUtc": dt.datetime.utcnow().replace(microsecond=0).isoformat() + "Z",
        "phase": "phase-2-station-id-map",
        "operatingDate": args.operating_date,
        "source": "PLK OpenRailway station dictionary plus local station artifacts",
        "apiCallsSpent": api_calls_spent,
        "dictionaryMeta": dictionary_meta,
        "routeStationIdCount": len(route_station_ids),
        "mappedRouteStationIdCount": len(used_entries),
        "unknownRouteStationIdCount": len(unknown_route_station_ids),
        "unknownRouteStationIds": unknown_route_station_ids,
        "stations": used_entries,
        "allKnownStationIdCount": len(entries),
    }
    output_path.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")

    print(
        json.dumps(
            {
                "outputPath": str(output_path.relative_to(repo_root)).replace("\\", "/"),
                "apiCallsSpent": api_calls_spent,
                "routeStationIdCount": result["routeStationIdCount"],
                "mappedRouteStationIdCount": result["mappedRouteStationIdCount"],
                "unknownRouteStationIdCount": result["unknownRouteStationIdCount"],
                "dictionaryMeta": dictionary_meta,
            },
            ensure_ascii=False,
            indent=2,
        )
    )


if __name__ == "__main__":
    main()
