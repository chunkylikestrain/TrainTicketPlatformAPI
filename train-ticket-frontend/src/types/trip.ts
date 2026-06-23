export type TripSearchParams = {
  departureStation: string;
  arrivalStation: string;
  date: string;
};

export type TripSearchResult = {
  tripId: number;
  trainId: number;
  trainName: string;
  departureStationId: number;
  departureStationCode: string;
  departureStationName: string;
  arrivalStationId: number;
  arrivalStationCode: string;
  arrivalStationName: string;
  departureStopOrder: number;
  arrivalStopOrder: number;
  departureTime: string;
  arrivalTime: string;
  status: string;
  lowestFare: number | null;
  currency: string;
};

export type Fare = {
  classType: string;
  price: number;
  currency: string;
};

export type TripDetails = TripSearchResult & {
  distanceKm: number;
  fares: Fare[];
};

export type TripSeatAvailability = {
  seatId: number;
  coach: string;
  number: string;
  classType: string;
  isAvailable: boolean;
  carriagePosition: number;
  carriageClass: string;
  layoutType: string;
  vehicleType: string;
  hasBikeSpace: boolean;
  hasAccessibleSpace: boolean;
  hasFamilyCompartment: boolean;
  hasDiningSection: boolean;
  carriageNotes: string;
};
