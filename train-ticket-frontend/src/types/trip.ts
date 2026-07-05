export type TripSearchParams = {
  departureStation: string;
  arrivalStation: string;
  date: string;
  time?: string;
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
  platform: string;
  track: string;
  status: string;
  delayMinutes: number;
  cancellationReason: string;
  originalPlatform: string;
  originalTrack: string;
  disruptionMessage: string;
  disruptionSeverity: string;
  hasPlatformChange: boolean;
  hasDisruption: boolean;
  lowestFare: number | null;
  currency: string;
  callingPattern: TripCallingPatternStop[];
};

export type TripItinerarySearchResult = {
  itineraryId: string;
  transferCount: number;
  departureTime: string;
  arrivalTime: string;
  totalDurationMinutes: number;
  totalTransferMinutes: number;
  lowestFare: number | null;
  currency: string;
  segments: TripItinerarySegment[];
};

export type TripItinerarySegment = {
  segmentIndex: number;
  tripId: number;
  trainId: number;
  trainName: string;
  serviceType: string;
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
  durationMinutes: number;
  transferAfterMinutes: number;
  platform: string;
  track: string;
  status: string;
  delayMinutes: number;
  hasDisruption: boolean;
  lowestFare: number | null;
  currency: string;
  callingPattern: TripCallingPatternStop[];
};

export type TripCallingPatternStop = {
  stationId: number;
  stationCode: string;
  stationName: string;
  stopOrder: number;
  arrivalTime: string | null;
  departureTime: string | null;
  arrivalOffsetMinutes: number | null;
  departureOffsetMinutes: number | null;
  dwellMinutes: number;
  platform: string;
  track: string;
  stopType: string;
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
