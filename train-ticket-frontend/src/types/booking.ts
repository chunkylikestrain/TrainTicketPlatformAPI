export type Booking = {
  id: number;
  userId: number | null;
  trainId: number;
  tripId: number | null;
  seatId: number;
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
  confirmedAtUtc: string | null;
  refundedAtUtc: string | null;
};

export type CreateBookingRequest = {
  trainId: number;
  tripId: number;
  seatId: number;
  travelDate: string;
  guestEmail?: string;
  passengerName?: string;
};

export type UpdateGuestBookingDataRequest = {
  guestEmail: string;
  passengerName: string;
  acceptedTerms: boolean;
  acceptedMarketing: boolean;
};
