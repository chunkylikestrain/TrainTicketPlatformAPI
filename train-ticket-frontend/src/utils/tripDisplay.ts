import type { TripDetails } from "../types/trip";

export function formatTripTime(value?: string) {
  if (!value) {
    return "--:--";
  }

  return new Intl.DateTimeFormat("en", {
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

export function formatTripDate(value?: string) {
  if (!value) {
    return "Selected date";
  }

  return new Intl.DateTimeFormat("en", {
    weekday: "short",
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(new Date(value));
}

export function formatTripLongDate(value?: string) {
  if (!value) {
    return "Selected date";
  }

  return new Intl.DateTimeFormat("en", {
    weekday: "long",
    day: "numeric",
    month: "long",
  }).format(new Date(value));
}

export function formatTripPrice(value?: number | null, currency = "PLN") {
  if (value == null) {
    return "TBA";
  }

  return `${value.toFixed(2).replace(".", ",")} ${currency || "PLN"}`;
}

export function getFareForClass(trip: TripDetails | null, selectedClass: string) {
  const desiredClass = selectedClass === "2" ? "Class 2" : "Class 1";
  return trip?.fares.find((fare) => fare.classType === desiredClass) ?? null;
}

export function getTripPriceLabel(trip: TripDetails | null, selectedClass: string) {
  const fare = getFareForClass(trip, selectedClass);
  return formatTripPrice(fare?.price, fare?.currency ?? trip?.currency ?? "PLN");
}

export function getTripVatLabel(trip: TripDetails | null, selectedClass: string) {
  const fare = getFareForClass(trip, selectedClass);
  if (!fare) {
    return "TBA";
  }

  return formatTripPrice(fare.price * 0.08, fare.currency);
}
