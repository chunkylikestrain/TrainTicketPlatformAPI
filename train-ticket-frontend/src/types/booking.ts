export type Booking = {
  id: number;
  userId: number | null;
  trainId: number;
  tripId: number | null;
  bookingOrderId: number | null;
  seatId: number;
  segmentDepartureStationId: number | null;
  segmentArrivalStationId: number | null;
  segmentDepartureOrder: number | null;
  segmentArrivalOrder: number | null;
  segmentDepartureTime: string | null;
  segmentArrivalTime: string | null;
  journeyDirection: string;
  journeySegmentIndex: number;
  bookingReference: string;
  ticketNumber: string;
  guestEmail: string | null;
  passengerName: string | null;
  passengerType: string;
  discountCode: string;
  discountName: string;
  discountPercent: number;
  baseAmount: number;
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
  loyaltyPointsRedeemed: number;
  loyaltyDiscountAmount: number;
  currency: string;
};

export type BookingOrder = {
  id: number;
  userId: number | null;
  orderReference: string;
  tripType: string;
  itineraryId: string | null;
  isItinerary: boolean;
  segmentCount: number;
  journeyDepartureStationId: number | null;
  journeyArrivalStationId: number | null;
  journeyDepartureTime: string | null;
  journeyArrivalTime: string | null;
  guestEmail: string | null;
  createdAtUtc: string;
  expiresAtUtc: string | null;
  bookingStatus: string;
  paymentStatus: string;
  confirmedAtUtc: string | null;
  amount: number;
  loyaltyPointsRedeemed: number;
  loyaltyDiscountAmount: number;
  ticketCount: number;
  hasTicketArtifacts: boolean;
  segments: BookingOrderSegment[];
  bookings: Booking[];
};

export type BookingOrderSegment = {
  segmentIndex: number;
  journeyDirection: string;
  journeySegmentIndex: number;
  tripId: number | null;
  trainId: number;
  trainName: string;
  departureStationId: number | null;
  arrivalStationId: number | null;
  route: string;
  departureTime: string | null;
  arrivalTime: string | null;
  tickets: Booking[];
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
  passengerType?: string;
  discountCode?: string;
};

export type CreateBookingOrderRequest = {
  trainId: number;
  tripId?: number;
  segmentDepartureStationId?: number;
  segmentArrivalStationId?: number;
  travelDate: string;
  guestEmail?: string;
  tripType?: string;
  itineraryId?: string;
  journeys?: CreateBookingOrderJourneyRequest[];
  segments?: CreateBookingOrderSegmentRequest[];
  passengers: Array<{
    seatId: number;
    passengerName?: string;
    passengerType?: string;
    discountCode?: string;
  }>;
};

export type CreateBookingOrderJourneyRequest = {
  direction: "Outbound" | "Return";
  segments: CreateBookingOrderSegmentRequest[];
};

export type CreateBookingOrderSegmentRequest = {
  segmentIndex: number;
  trainId: number;
  tripId: number;
  segmentDepartureStationId?: number;
  segmentArrivalStationId?: number;
  travelDate?: string;
  passengers: Array<{
    seatId: number;
    passengerName?: string;
    passengerType?: string;
    discountCode?: string;
  }>;
};

export type UpdateGuestBookingDataRequest = {
  guestEmail: string;
  passengerName: string;
  acceptedTerms: boolean;
  acceptedMarketing: boolean;
};
