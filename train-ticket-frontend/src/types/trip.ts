export type TripSearchParams = {
  departureStation: string;
  arrivalStation: string;
  date: string;
};

export type TripSearchResult = {
  tripId: number;
  trainId: number;
  trainName: string;
  departureStationCode: string;
  departureStationName: string;
  arrivalStationCode: string;
  arrivalStationName: string;
  departureTime: string;
  arrivalTime: string;
  status: string;
  lowestFare: number | null;
  currency: string;
};
