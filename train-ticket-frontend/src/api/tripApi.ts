import apiClient from "./apiClient";
import type {
  TripDetails,
  TripItinerarySearchResult,
  TripSearchParams,
  TripSearchResult,
  TripSeatAvailability,
} from "../types/trip";

export async function searchTrips(params: TripSearchParams) {
  const response = await apiClient.get<TripSearchResult[]>("/Trips/search", {
    params: {
      from: params.departureStation,
      to: params.arrivalStation,
      date: params.date,
      time: params.time || undefined,
    },
  });

  return response.data;
}

export async function searchItineraries(params: TripSearchParams) {
  const response = await apiClient.get<TripItinerarySearchResult[]>("/Trips/itineraries", {
    params: {
      from: params.departureStation,
      to: params.arrivalStation,
      date: params.date,
      time: params.time || undefined,
    },
  });

  return response.data;
}

export async function getTripById(tripId: number | string) {
  const response = await apiClient.get<TripDetails>(`/Trips/${tripId}`);
  return response.data;
}

export async function getTripSeats(
  tripId: number | string,
  segment?: { fromStationId?: number | string | null; toStationId?: number | string | null },
) {
  const response = await apiClient.get<TripSeatAvailability[]>(`/Trips/${tripId}/seats`, {
    params: {
      fromStationId: segment?.fromStationId || undefined,
      toStationId: segment?.toStationId || undefined,
    },
  });
  return response.data;
}
