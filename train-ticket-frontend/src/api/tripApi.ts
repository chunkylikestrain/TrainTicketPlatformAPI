import apiClient from "./apiClient";
import type { TripSearchParams, TripSearchResult } from "../types/trip";

export async function searchTrips(params: TripSearchParams) {
  const response = await apiClient.get<TripSearchResult[]>("/Trips/search", {
    params: {
      from: params.departureStation,
      to: params.arrivalStation,
      date: params.date,
    },
  });

  return response.data;
}
