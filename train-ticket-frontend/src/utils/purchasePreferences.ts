export type DiscountCode =
  | "normal"
  | "student51"
  | "student37"
  | "child37"
  | "senior30"
  | "senior37"
  | "bigFamily30"
  | "family30"
  | "largeFamily50";

export type PassengerCounts = {
  adults: number;
  children: number;
};

export const discountOptions: Array<{
  code: DiscountCode;
  label: string;
  percent: number;
  appliesTo: "all" | "adult" | "child";
  documentHint: string;
}> = [
  {
    code: "normal",
    label: "Normal Ticket",
    percent: 0,
    appliesTo: "adult",
    documentHint: "No discount document required",
  },
  {
    code: "student51",
    label: "Student 51%",
    percent: 51,
    appliesTo: "adult",
    documentHint: "Student ID checked during travel",
  },
  {
    code: "student37",
    label: "Student/Doctoral 37%",
    percent: 37,
    appliesTo: "adult",
    documentHint: "Student or doctoral entitlement document checked during travel",
  },
  {
    code: "child37",
    label: "Child 37%",
    percent: 37,
    appliesTo: "child",
    documentHint: "Age or school document checked during travel",
  },
  {
    code: "senior30",
    label: "Senior 30%",
    percent: 30,
    appliesTo: "adult",
    documentHint: "Identity document checked during travel",
  },
  {
    code: "senior37",
    label: "Senior statutory 37%",
    percent: 37,
    appliesTo: "adult",
    documentHint: "Pensioner or retiree entitlement document checked during travel",
  },
  {
    code: "bigFamily30",
    label: "Big Family 30%",
    percent: 30,
    appliesTo: "all",
    documentHint: "Big Family Card checked during travel",
  },
  {
    code: "family30",
    label: "Family Ticket 30%",
    percent: 30,
    appliesTo: "all",
    documentHint: "Child age document checked during travel",
  },
];

export type FilterOption = {
  code: string;
  label: string;
  section: "seat" | "car" | "changes" | "other" | "transport";
};

export const filterOptions: FilterOption[] = [
  { code: "quiet-zone", label: "in the Quiet Zone", section: "seat" },
  { code: "sleeper", label: "with couchettes or sleepers", section: "seat" },
  { code: "family-compartment", label: "family car with a compartment for children", section: "car" },
  { code: "accessible-platform", label: "carriage with a platform for persons with disabilities", section: "car" },
  {
    code: "wheelchair-lift",
    label: "carriage with a platform/lift for people in a wheelchair, with couchettes or sleepers",
    section: "car",
  },
  { code: "braille", label: "car with Braille signage", section: "car" },
  { code: "bicycle", label: "seat with an area for a bicycle", section: "car" },
  { code: "direct", label: "direct connections only", section: "changes" },
  { code: "long-interchange", label: "minimum time for an interchange", section: "changes" },
  { code: "dining", label: "dining area", section: "other" },
  { code: "snacks", label: "minibar trolley or vending machine with snacks and drinks", section: "other" },
  { code: "air-conditioning", label: "with air conditioning", section: "other" },
  { code: "wifi", label: "with Wi-Fi", section: "other" },
  { code: "eip", label: "EIP", section: "transport" },
  { code: "eic", label: "EIC", section: "transport" },
  { code: "ic", label: "IC", section: "transport" },
  { code: "icn", label: "ICN", section: "transport" },
  { code: "tlk", label: "TLK", section: "transport" },
];

const maxTravelers = 6;

export function getPassengerCounts(params: URLSearchParams): PassengerCounts {
  const adults = clampCount(Number(params.get("adults") ?? "1"), 1, maxTravelers);

  return {
    adults,
    children: clampCount(Number(params.get("children") ?? "0"), 0, maxTravelers - adults),
  };
}

export function getPassengerTotal(counts: PassengerCounts) {
  return Math.max(1, Math.min(maxTravelers, counts.adults + counts.children));
}

export function getDiscountCodes(params: URLSearchParams, counts: PassengerCounts): DiscountCode[] {
  const total = getPassengerTotal(counts);
  const rawCodes = (params.get("discounts") ?? "")
    .split(",")
    .map((code) => code.trim())
    .filter(Boolean);

  return Array.from({ length: total }, (_, index) => {
    const passengerType = index < counts.adults ? "adult" : "child";
    const rawCode = rawCodes[index] as DiscountCode | undefined;
    const option = discountOptions.find((discount) => discount.code === rawCode);

    if (option && (option.appliesTo === "all" || option.appliesTo === passengerType)) {
      return option.code;
    }

    return passengerType === "child" ? "child37" : "normal";
  });
}

export function getDiscountOption(code: DiscountCode) {
  return discountOptions.find((discount) => discount.code === code) ?? discountOptions[0];
}

export function formatPassengerSummary(counts: PassengerCounts) {
  const parts = [];

  if (counts.adults > 0) {
    parts.push(`${counts.adults}x ${counts.adults === 1 ? "Adult" : "Adults"}`);
  }

  if (counts.children > 0) {
    parts.push(`${counts.children}x ${counts.children === 1 ? "Child" : "Children"}`);
  }

  return parts.join(", ");
}

export function formatDiscountSummary(codes: DiscountCode[]) {
  const grouped = codes.reduce<Record<string, number>>((result, code) => {
    const label = getDiscountOption(code).label;
    result[label] = (result[label] ?? 0) + 1;
    return result;
  }, {});

  return Object.entries(grouped)
    .map(([label, count]) => `${count}x ${label}`)
    .join(", ");
}

export function writePurchasePreferenceParams(
  params: URLSearchParams,
  counts: PassengerCounts,
  discounts: DiscountCode[],
  filters?: string[],
) {
  const total = getPassengerTotal(counts);

  params.set("adults", String(counts.adults));
  params.set("children", String(counts.children));
  params.set("discounts", discounts.slice(0, total).join(","));

  if (filters && filters.length > 0) {
    params.set("filters", filters.join(","));
  } else if (filters) {
    params.delete("filters");
  }

  return params;
}

export function copyPurchasePreferenceParams(from: URLSearchParams, to = new URLSearchParams()) {
  const counts = getPassengerCounts(from);
  const discounts = getDiscountCodes(from, counts);
  const filters = getFilterCodes(from);
  return writePurchasePreferenceParams(to, counts, discounts, filters);
}

export function getFilterCodes(params: URLSearchParams) {
  return (params.get("filters") ?? "")
    .split(",")
    .map((filter) => filter.trim())
    .filter(Boolean);
}

export function buildDiscountSelectionUrl(returnTo: string, params: URLSearchParams) {
  const discountParams = copyPurchasePreferenceParams(params);
  discountParams.set("returnTo", returnTo);
  return `/discounts?${discountParams.toString()}`;
}

export function buildFilterSelectionUrl(returnTo: string, params: URLSearchParams) {
  const filterParams = copyPurchasePreferenceParams(params);
  filterParams.set("returnTo", returnTo);
  return `/filters?${filterParams.toString()}`;
}

function clampCount(value: number, min: number, max: number) {
  if (Number.isNaN(value)) {
    return min;
  }

  return Math.max(min, Math.min(max, Math.trunc(value)));
}
