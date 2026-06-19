import apiClient from "./apiClient";
import type { TripDetails, TripSearchParams, TripSearchResult, TripSeatAvailability } from "../types/trip";

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

export async function getTripById(tripId: number | string) {
  const response = await apiClient.get<TripDetails>(`/Trips/${tripId}`);
  return response.data;
}

export async function getTripSeats(tripId: number | string) {
  const response = await apiClient.get<TripSeatAvailability[]>(`/Trips/${tripId}/seats`);
  return response.data;
}
