export type Booking = {
  id: number;
  userId: number | null;
  trainId: number;
  tripId: number | null;
  seatId: number;
  segmentDepartureStationId: number | null;
  segmentArrivalStationId: number | null;
  segmentDepartureOrder: number | null;
  segmentArrivalOrder: number | null;
  segmentDepartureTime: string | null;
  segmentArrivalTime: string | null;
  bookingReference: string;
  ticketNumber: string;
  guestEmail: string | null;
  passengerName: string | null;
  bookingDate: string;
  travelDate: string;
  expiresAtUtc: string | null;
  bookingStatus: string;
  paymentStatus: string;
  isCancelled: boolean;
  cancellationDate: string | null;
  cancellationReason: string | null;
  confirmedAtUtc: string | null;
  refundedAtUtc: string | null;
  ticketIssuedAtUtc: string | null;
  hasTicketArtifact: boolean;
  ticketEmailStatus: string;
  ticketEmailSentAtUtc: string | null;
  ticketEmailRecipient: string;
  trainName: string;
  route: string;
  seatLabel: string;
  departureTime: string | null;
  arrivalTime: string | null;
  platform: string;
  track: string;
  delayMinutes: number;
  tripCancellationReason: string;
  originalPlatform: string;
  originalTrack: string;
  disruptionMessage: string;
  disruptionSeverity: string;
  hasPlatformChange: boolean;
  hasDisruption: boolean;
  amount: number;
};

export type BookingOrder = {
  id: number;
  userId: number | null;
  orderReference: string;
  guestEmail: string | null;
  createdAtUtc: string;
  expiresAtUtc: string | null;
  bookingStatus: string;
  paymentStatus: string;
  confirmedAtUtc: string | null;
  amount: number;
  ticketCount: number;
  hasTicketArtifacts: boolean;
  bookings: Booking[];
};

export type CreateBookingRequest = {
  trainId: number;
  tripId: number;
  seatId: number;
  segmentDepartureStationId?: number;
  segmentArrivalStationId?: number;
  travelDate: string;
  guestEmail?: string;
  passengerName?: string;
};

export type CreateBookingOrderRequest = {
  trainId: number;
  tripId?: number;
  segmentDepartureStationId?: number;
  segmentArrivalStationId?: number;
  travelDate: string;
  guestEmail?: string;
  passengers: Array<{
    seatId: number;
    passengerName?: string;
  }>;
};

export type UpdateGuestBookingDataRequest = {
  guestEmail: string;
  passengerName: string;
  acceptedTerms: boolean;
  acceptedMarketing: boolean;
};
