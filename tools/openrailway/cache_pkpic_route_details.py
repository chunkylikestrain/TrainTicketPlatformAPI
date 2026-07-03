import argparse
import datetime as dt
import hashlib
import json
import os
import time
import urllib.error
import urllib.request
from collections import Counter, defaultdict
from pathlib import Path


USER_SECRETS_ID = "56689f1a-4b52-4c21-a008-06d0177db705"
DEFAULT_OPERATING_DATE = "2026-07-03"
DEFAULT_MAX_NEW_CALLS = 56
BASE_URL = "https://pdp-api.plk-sa.pl"


def train_name_sort_key(value):
    return (value.startswith("(unnamed:"), value)


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


def load_station_names(snapshot_dir):
    station_names = {}
    candidate_files = [
        "ic-station-candidates-locality-enriched-2026-07-03.json",
        "ic-station-candidates-filtered-2026-07-03.json",
        "ic-station-candidates-2026-07-03.json",
    ]

    for filename in candidate_files:
        path = snapshot_dir / filename
        if not path.exists():
            continue

        try:
            payload = json.loads(path.read_text(encoding="utf-8-sig"))
            candidates = payload.get("stations") or payload.get("candidates") or payload
            if isinstance(candidates, dict):
                candidates = candidates.get("items") or []

            for station in candidates:
                station_id = (
                    station.get("externalStationId")
                    or station.get("stationId")
                    or station.get("id")
                )
                name = station.get("stationName") or station.get("name")
                if station_id is not None and name:
                    station_names[int(station_id)] = name
        except (OSError, json.JSONDecodeError):
            continue

    return station_names


def build_fetch_order(routes):
    groups = defaultdict(list)
    for route in routes:
        key = (route.get("name") or "").strip() or f"(unnamed:{route.get('orderId')})"
        groups[key].append(route)

    for items in groups.values():
        items.sort(key=lambda item: (item.get("scheduleId", 0), item.get("orderId", 0)))

    ordered = []
    while any(groups.values()):
        for key in sorted(groups.keys(), key=train_name_sort_key):
            if groups[key]:
                ordered.append(groups[key].pop(0))

    return ordered


def cache_route_details(routes, cache_dir, api_key, max_new_calls, retry_errors):
    new_calls = 0
    status_counts = Counter()
    errors = []

    for route in build_fetch_order(routes):
        schedule_id = int(route["scheduleId"])
        order_id = int(route["orderId"])
        detail_path = cache_dir / f"{schedule_id}-{order_id}.json"
        error_path = cache_dir / f"{schedule_id}-{order_id}.error.json"

        if detail_path.exists():
            continue
        if error_path.exists() and not retry_errors:
            continue
        if new_calls >= max_new_calls:
            break

        request = urllib.request.Request(
            f"{BASE_URL}/api/v1/schedules/route/{schedule_id}/{order_id}",
            headers={"X-API-Key": api_key, "Accept": "application/json"},
        )

        try:
            with urllib.request.urlopen(request, timeout=30) as response:
                data = json.loads(response.read().decode("utf-8-sig"))
                status_counts[str(response.status)] += 1
                detail_path.write_text(
                    json.dumps(data, ensure_ascii=False, indent=2),
                    encoding="utf-8",
                )
                if error_path.exists():
                    error_path.unlink()
                new_calls += 1
                if new_calls % 10 == 0:
                    print(f"cached {new_calls} new route details...")
                time.sleep(0.18)
        except urllib.error.HTTPError as ex:
            status_counts[str(ex.code)] += 1
            body = ex.read().decode("utf-8", errors="replace")
            error = {
                "scheduleId": schedule_id,
                "orderId": order_id,
                "status": ex.code,
                "reason": ex.reason,
                "body": body[:1000],
            }
            error_path.write_text(
                json.dumps(error, ensure_ascii=False, indent=2),
                encoding="utf-8",
            )
            errors.append(error)

            if ex.code == 404:
                print(f"skipped missing route detail {schedule_id}/{order_id} (404)")
                continue

            print(f"stopped on HTTP {ex.code} after {new_calls} new detail calls")
            if ex.code in (401, 403, 429):
                break
        except Exception as ex:
            error = {
                "scheduleId": schedule_id,
                "orderId": order_id,
                "error": type(ex).__name__,
                "message": str(ex),
            }
            error_path.write_text(
                json.dumps(error, ensure_ascii=False, indent=2),
                encoding="utf-8",
            )
            errors.append(error)
            print(f"non-http error after {new_calls} calls: {type(ex).__name__}: {ex}")
            break

    return new_calls, status_counts, errors


def read_cached_details(cache_dir, station_names):
    details = []
    for path in sorted(cache_dir.glob("*.json")):
        if path.name.endswith(".error.json"):
            continue

        try:
            payload = json.loads(path.read_text(encoding="utf-8-sig"))
        except (OSError, json.JSONDecodeError):
            continue

        stops = sorted(
            payload.get("stations") or [],
            key=lambda stop: stop.get("orderNumber") or 0,
        )
        station_ids = [
            int(stop["stationId"])
            for stop in stops
            if stop.get("stationId") is not None
        ]
        if not station_ids:
            continue

        shape_key = "|".join(map(str, station_ids))
        undirected_key = min(shape_key, "|".join(map(str, reversed(station_ids))))
        details.append(
            {
                "scheduleId": payload.get("scheduleId"),
                "orderId": payload.get("orderId"),
                "trainOrderId": payload.get("trainOrderId"),
                "name": (payload.get("name") or "").strip(),
                "carrierCode": payload.get("carrierCode"),
                "nationalNumber": payload.get("nationalNumber"),
                "commercialCategorySymbol": payload.get("commercialCategorySymbol"),
                "stationCount": len(station_ids),
                "departureExternalStationId": station_ids[0],
                "arrivalExternalStationId": station_ids[-1],
                "departureName": station_names.get(station_ids[0], f"#{station_ids[0]}"),
                "arrivalName": station_names.get(station_ids[-1], f"#{station_ids[-1]}"),
                "stationIds": station_ids,
                "stationNames": [
                    station_names.get(station_id, f"#{station_id}")
                    for station_id in station_ids
                ],
                "routeFingerprint": hashlib.sha256(
                    shape_key.encode("utf-8")
                ).hexdigest()[:16],
                "undirectedKeyHash": hashlib.sha256(
                    undirected_key.encode("utf-8")
                ).hexdigest()[:16],
            }
        )

    return details


def build_summary(index, routes, details, run):
    by_shape = defaultdict(list)
    by_undirected = defaultdict(list)
    for detail in details:
        by_shape[detail["routeFingerprint"]].append(detail)
        by_undirected[detail["undirectedKeyHash"]].append(detail)

    shape_summaries = []
    for fingerprint, items in by_shape.items():
        representative = items[0]
        names = Counter((item.get("name") or "(unnamed)") for item in items)
        categories = Counter(
            (item.get("commercialCategorySymbol") or "(blank)") for item in items
        )
        shape_summaries.append(
            {
                "routeFingerprint": fingerprint,
                "serviceRecordCount": len(items),
                "departureExternalStationId": representative[
                    "departureExternalStationId"
                ],
                "arrivalExternalStationId": representative["arrivalExternalStationId"],
                "departureName": representative["departureName"],
                "arrivalName": representative["arrivalName"],
                "stationCount": representative["stationCount"],
                "sampleTrainNames": [name for name, _ in names.most_common(5)],
                "commercialCategories": dict(categories),
                "stationNames": representative["stationNames"],
                "stationIds": representative["stationIds"],
                "sampleKeys": [
                    {
                        "scheduleId": item["scheduleId"],
                        "orderId": item["orderId"],
                        "name": item["name"],
                    }
                    for item in items[:5]
                ],
            }
        )

    shape_summaries.sort(
        key=lambda shape: (
            -shape["serviceRecordCount"],
            shape["departureName"],
            shape["arrivalName"],
        )
    )

    category_counts = Counter(
        (detail.get("commercialCategorySymbol") or "(blank)") for detail in details
    )
    return {
        "generatedAtUtc": dt.datetime.utcnow().replace(microsecond=0).isoformat()
        + "Z",
        "lastRunStartedAtUtc": run["startedAtUtc"],
        "sourceRouteIndexFile": run["sourceRouteIndexFile"],
        "operatingDate": index.get("operatingDate"),
        "phase": "phase-3-route-detail-cache-continuation",
        "apiCallsSpentThisRun": run["newCalls"],
        "maxNewCallsThisRun": run["maxNewCalls"],
        "httpStatusCountsThisRun": dict(run["statusCounts"]),
        "errorsThisRun": run["errors"][:10],
        "pkpicRouteRecordCount": len(routes),
        "cachedRouteDetailCount": len(details),
        "remainingUncachedRouteDetailCount": len(routes) - len(details),
        "cachedRouteDetailPercent": round(len(details) * 100 / len(routes), 2),
        "uniqueDirectedRouteShapeCountInCache": len(by_shape),
        "uniqueUndirectedRouteShapeCountInCache": len(by_undirected),
        "cachedCommercialCategoryCounts": dict(category_counts),
        "cachedServiceRecordsPerShapeTop": shape_summaries[:20],
        "allCachedRouteShapes": shape_summaries,
    }


def main():
    parser = argparse.ArgumentParser(
        description="Cache PKPIC OpenRailway route-detail payloads without refetching saved records."
    )
    parser.add_argument("--operating-date", default=DEFAULT_OPERATING_DATE)
    parser.add_argument("--max-new", type=int, default=DEFAULT_MAX_NEW_CALLS)
    parser.add_argument("--retry-errors", action="store_true")
    parser.add_argument("--summarize-only", action="store_true")
    args = parser.parse_args()

    repo_root = Path(__file__).resolve().parents[2]
    snapshot_dir = repo_root / "TrainTicketPlatformAPI" / "App_Data" / "SeedSnapshots"
    index_path = snapshot_dir / f"pkpic-route-index-{args.operating_date}.json"
    cache_dir = snapshot_dir / f"pkpic-route-details-{args.operating_date}"
    summary_path = snapshot_dir / f"pkpic-route-detail-summary-{args.operating_date}.json"

    if not index_path.exists():
        raise SystemExit(f"Route index file was not found: {index_path}")

    cache_dir.mkdir(exist_ok=True)
    index = json.loads(index_path.read_text(encoding="utf-8"))
    routes = [
        route
        for route in index["routes"]
        if (route.get("carrierCode") or "").upper() == "IC"
    ]

    started_at_utc = dt.datetime.utcnow().replace(microsecond=0).isoformat() + "Z"
    if args.summarize_only:
        new_calls = 0
        status_counts = Counter()
        errors = []
    else:
        api_key = read_api_key()
        new_calls, status_counts, errors = cache_route_details(
            routes,
            cache_dir,
            api_key,
            args.max_new,
            args.retry_errors,
        )

    details = read_cached_details(cache_dir, load_station_names(snapshot_dir))
    summary = build_summary(
        index,
        routes,
        details,
        {
            "startedAtUtc": started_at_utc,
            "sourceRouteIndexFile": str(index_path.relative_to(repo_root)).replace(
                "\\", "/"
            ),
            "newCalls": new_calls,
            "maxNewCalls": args.max_new,
            "statusCounts": status_counts,
            "errors": errors,
        },
    )
    summary_path.write_text(
        json.dumps(summary, ensure_ascii=False, indent=2),
        encoding="utf-8",
    )
    print(
        json.dumps(
            {
                "apiCallsSpentThisRun": new_calls,
                "statusCounts": dict(status_counts),
                "cachedRouteDetailCount": summary["cachedRouteDetailCount"],
                "remainingUncachedRouteDetailCount": summary[
                    "remainingUncachedRouteDetailCount"
                ],
                "cachedRouteDetailPercent": summary["cachedRouteDetailPercent"],
                "uniqueDirectedRouteShapeCountInCache": summary[
                    "uniqueDirectedRouteShapeCountInCache"
                ],
                "uniqueUndirectedRouteShapeCountInCache": summary[
                    "uniqueUndirectedRouteShapeCountInCache"
                ],
                "cachedCommercialCategoryCounts": summary[
                    "cachedCommercialCategoryCounts"
                ],
                "summaryPath": str(summary_path.relative_to(repo_root)).replace(
                    "\\", "/"
                ),
            },
            ensure_ascii=False,
            indent=2,
        )
    )


if __name__ == "__main__":
    main()
