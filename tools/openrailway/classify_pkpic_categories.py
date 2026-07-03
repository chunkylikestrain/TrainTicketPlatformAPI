import argparse
import datetime as dt
import json
from collections import Counter, defaultdict
from pathlib import Path


DEFAULT_OPERATING_DATE = "2026-07-03"


CATEGORY_RULES = {
    "EIP": {
        "railBookCategoryCode": "EIP",
        "railBookCategoryName": "Express InterCity Premium",
        "passengerDisplayCategory": "EIP",
        "trainTemplate": "ED250 Pendolino",
        "needsReview": False,
        "notes": "Passenger-facing premium ED250 category.",
    },
    "EIC": {
        "railBookCategoryCode": "EIC",
        "railBookCategoryName": "Express InterCity",
        "passengerDisplayCategory": "EIC",
        "trainTemplate": "Locomotive-hauled express coaches",
        "needsReview": False,
        "notes": "Passenger-facing express InterCity category.",
    },
    "IC": {
        "railBookCategoryCode": "IC",
        "railBookCategoryName": "InterCity",
        "passengerDisplayCategory": "IC",
        "trainTemplate": "Long-distance InterCity coaches",
        "needsReview": False,
        "notes": "Standard passenger-facing InterCity category.",
    },
    "IC+": {
        "railBookCategoryCode": "IC",
        "railBookCategoryName": "InterCity",
        "passengerDisplayCategory": "IC",
        "trainTemplate": "Long-distance InterCity coaches",
        "needsReview": False,
        "notes": "Treat IC+ as a higher-standard InterCity service for passenger display.",
    },
    "ICN": {
        "railBookCategoryCode": "IC",
        "railBookCategoryName": "InterCity",
        "passengerDisplayCategory": "IC",
        "trainTemplate": "Long-distance InterCity coaches",
        "needsReview": False,
        "notes": "Collapsed into InterCity for RailBook passenger display.",
    },
    "TLK": {
        "railBookCategoryCode": "TLK",
        "railBookCategoryName": "Twoje Linie Kolejowe",
        "passengerDisplayCategory": "TLK",
        "trainTemplate": "Long-distance TLK coaches",
        "needsReview": False,
        "notes": "Passenger-facing TLK category.",
    },
    "EC": {
        "railBookCategoryCode": "IC",
        "railBookCategoryName": "InterCity",
        "passengerDisplayCategory": "IC",
        "trainTemplate": "International long-distance coaches",
        "needsReview": False,
        "notes": "Raw EC category collapsed into InterCity for RailBook passenger display.",
    },
    "EC/IC": {
        "railBookCategoryCode": "IC",
        "railBookCategoryName": "InterCity",
        "passengerDisplayCategory": "IC",
        "trainTemplate": "International long-distance coaches",
        "needsReview": False,
        "notes": "Mixed raw category collapsed into InterCity for RailBook passenger display.",
    },
    "EC/EIC": {
        "railBookCategoryCode": "IC",
        "railBookCategoryName": "InterCity",
        "passengerDisplayCategory": "IC",
        "trainTemplate": "International express coaches",
        "needsReview": False,
        "notes": "Mixed raw EC/EIC category collapsed into InterCity for RailBook passenger display.",
    },
    "EN/IC": {
        "railBookCategoryCode": "IC",
        "railBookCategoryName": "InterCity",
        "passengerDisplayCategory": "IC",
        "trainTemplate": "International night train coaches",
        "needsReview": False,
        "notes": "Raw EN/IC category collapsed into InterCity for RailBook passenger display.",
    },
    "EC/EN/IC": {
        "railBookCategoryCode": "IC",
        "railBookCategoryName": "InterCity",
        "passengerDisplayCategory": "IC",
        "trainTemplate": "International night train coaches",
        "needsReview": False,
        "notes": "Mixed raw EC/EN/IC category collapsed into InterCity for RailBook passenger display.",
    },
    "IC/MP": {
        "railBookCategoryCode": "IC",
        "railBookCategoryName": "InterCity",
        "passengerDisplayCategory": "IC",
        "trainTemplate": "International long-distance coaches",
        "needsReview": True,
        "notes": "Mixed passenger-facing IC and internal MP category; display IC, preserve raw category for admin review.",
    },
    "MP": {
        "railBookCategoryCode": "IC",
        "railBookCategoryName": "InterCity",
        "passengerDisplayCategory": "IC",
        "trainTemplate": "Long-distance InterCity coaches",
        "needsReview": False,
        "notes": "PLK/internal MP category collapsed into InterCity for RailBook passenger display.",
    },
}


def classify(raw_category):
    raw = raw_category or "(blank)"
    if raw in CATEGORY_RULES:
        return {"rawCategory": raw, **CATEGORY_RULES[raw]}

    return {
        "rawCategory": raw,
        "railBookCategoryCode": "UNKNOWN",
        "railBookCategoryName": "Unknown",
        "passengerDisplayCategory": "InterCity",
        "trainTemplate": "Long-distance InterCity coaches",
        "needsReview": True,
        "notes": "No explicit mapping yet. Preserve raw category and review before import.",
    }


def load_services(snapshot_dir, operating_date):
    detail_dir = snapshot_dir / f"pkpic-route-details-{operating_date}"
    services = []

    for path in sorted(detail_dir.glob("*.json")):
        if path.name.endswith(".error.json"):
            continue
        payload = json.loads(path.read_text(encoding="utf-8-sig"))
        services.append(
            {
                "scheduleId": payload.get("scheduleId"),
                "orderId": payload.get("orderId"),
                "trainOrderId": payload.get("trainOrderId"),
                "name": (payload.get("name") or "").strip() or "(unnamed)",
                "nationalNumber": payload.get("nationalNumber") or "",
                "rawCategory": payload.get("commercialCategorySymbol") or "(blank)",
            }
        )

    return services


def main():
    parser = argparse.ArgumentParser(description="Classify raw PLK commercial categories into RailBook import categories.")
    parser.add_argument("--operating-date", default=DEFAULT_OPERATING_DATE)
    args = parser.parse_args()

    repo_root = Path(__file__).resolve().parents[2]
    snapshot_dir = repo_root / "TrainTicketPlatformAPI" / "App_Data" / "SeedSnapshots"
    output_path = snapshot_dir / f"pkpic-category-classification-{args.operating_date}.json"

    services = load_services(snapshot_dir, args.operating_date)
    raw_counts = Counter(service["rawCategory"] for service in services)
    service_classifications = []
    grouped_examples = defaultdict(list)
    needs_review_count = 0

    for service in services:
        classification = classify(service["rawCategory"])
        if classification["needsReview"]:
            needs_review_count += 1
        service_classifications.append({**service, **classification})
        grouped_examples[service["rawCategory"]].append(service)

    category_summaries = []
    for raw_category, count in raw_counts.most_common():
        classification = classify(raw_category)
        examples = grouped_examples[raw_category][:10]
        category_summaries.append(
            {
                **classification,
                "serviceRecordCount": count,
                "sampleServices": examples,
            }
        )

    result = {
        "generatedAtUtc": dt.datetime.utcnow().replace(microsecond=0).isoformat() + "Z",
        "phase": "phase-3-category-classification",
        "operatingDate": args.operating_date,
        "usableServiceRecordCount": len(services),
        "rawCategoryCounts": dict(raw_counts.most_common()),
        "needsReviewServiceRecordCount": needs_review_count,
        "categoryMappings": category_summaries,
        "serviceClassifications": service_classifications,
    }
    output_path.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")

    print(
        json.dumps(
            {
                "outputPath": str(output_path.relative_to(repo_root)).replace("\\", "/"),
                "usableServiceRecordCount": result["usableServiceRecordCount"],
                "needsReviewServiceRecordCount": result["needsReviewServiceRecordCount"],
                "rawCategoryCounts": result["rawCategoryCounts"],
            },
            ensure_ascii=False,
            indent=2,
        )
    )


if __name__ == "__main__":
    main()
