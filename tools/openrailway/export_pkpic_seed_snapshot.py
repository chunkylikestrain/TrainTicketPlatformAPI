import argparse
import datetime as dt
import json
from pathlib import Path


DEFAULT_OPERATING_DATE = "2026-07-03"


def load_json(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def validate_snapshot(snapshot):
    required = ["externalSource", "stations", "trains", "routes", "trips"]
    missing = [key for key in required if key not in snapshot]
    if missing:
        raise SystemExit(f"Snapshot is missing required keys: {', '.join(missing)}")

    counts = {
        "stationCount": len(snapshot["stations"]),
        "routeCount": len(snapshot["routes"]),
        "trainCount": len(snapshot["trains"]),
        "tripCount": len(snapshot["trips"]),
    }
    if any(value == 0 for value in counts.values()):
        raise SystemExit(f"Snapshot is not exportable because it has empty sections: {counts}")

    route_codes = {route["code"] for route in snapshot["routes"]}
    train_codes = {train["code"] for train in snapshot["trains"]}
    missing_route_refs = [
        trip["routeCode"]
        for trip in snapshot["trips"]
        if trip.get("routeCode") not in route_codes
    ]
    missing_train_refs = [
        trip["trainCode"]
        for trip in snapshot["trips"]
        if trip.get("trainCode") not in train_codes
    ]
    trains_without_carriages = [
        train["code"]
        for train in snapshot["trains"]
        if not train.get("carriages")
    ]

    if missing_route_refs or missing_train_refs or trains_without_carriages:
        raise SystemExit(
            "Snapshot failed integrity checks: "
            f"missingRouteRefs={len(missing_route_refs)}, "
            f"missingTrainRefs={len(missing_train_refs)}, "
            f"trainsWithoutCarriages={len(trains_without_carriages)}"
        )

    return counts


def main():
    parser = argparse.ArgumentParser(description="Export the generated PKPIC data as a RailBook startup seed snapshot.")
    parser.add_argument("--operating-date", default=DEFAULT_OPERATING_DATE)
    parser.add_argument("--name", default="railbook-pkpic")
    args = parser.parse_args()

    repo_root = Path(__file__).resolve().parents[2]
    snapshot_dir = repo_root / "TrainTicketPlatformAPI" / "App_Data" / "SeedSnapshots"
    input_path = snapshot_dir / f"pkpic-train-trip-seed-snapshot-{args.operating_date}.json"
    output_path = snapshot_dir / f"{args.name}-{args.operating_date}.seed-snapshot.json"
    manifest_path = snapshot_dir / f"{args.name}-{args.operating_date}.seed-snapshot-manifest.json"

    if not input_path.exists():
        raise SystemExit(f"Generated train/trip snapshot does not exist: {input_path}")

    snapshot = load_json(input_path)
    snapshot["exportedAtUtc"] = dt.datetime.utcnow().replace(microsecond=0).isoformat() + "Z"
    counts = validate_snapshot(snapshot)

    output_path.write_text(json.dumps(snapshot, ensure_ascii=False, indent=2), encoding="utf-8")

    manifest = {
        "generatedAtUtc": snapshot["exportedAtUtc"],
        "phase": "phase-7-export-seed-snapshot",
        "sourcePath": str(input_path.relative_to(repo_root)).replace("\\", "/"),
        "outputPath": str(output_path.relative_to(repo_root)).replace("\\", "/"),
        "operatingDate": args.operating_date,
        **counts,
    }
    manifest_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")

    print(json.dumps(manifest, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
