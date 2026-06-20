import apiClient from "./apiClient";
import type { Station } from "../types/station";

export async function getStations(query?: string) {
  const response = await apiClient.get<Station[]>("/Stations", {
    params: query?.trim() ? { query } : undefined,
  });

  return response.data;
}
