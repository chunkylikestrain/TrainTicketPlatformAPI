export type TicketArtifact = {
  bookingId: number;
  bookingReference: string;
  ticketNumber: string;
  passengerName: string;
  recipientEmail: string;
  trainName: string;
  route: string;
  seatLabel: string;
  journeyDirection: string;
  journeySegmentIndex: number;
  travelDate: string;
  departureTime: string | null;
  arrivalTime: string | null;
  issuedAtUtc: string;
  qrPayload: string;
  qrSvgUrl: string;
  pdfUrl: string;
  emailDeliveryStatus: string;
  emailSentAtUtc: string | null;
};

export type TicketEmailDelivery = {
  id: number;
  bookingId: number;
  recipientEmail: string;
  status: string;
  requestedAtUtc: string;
  sentAtUtc: string | null;
  providerMessageId: string;
  errorMessage: string;
};

export type BookingOrderTickets = {
  bookingOrderId: number;
  orderReference: string;
  ticketCount: number;
  tickets: TicketArtifact[];
};

export type BookingOrderEmailDelivery = {
  bookingOrderId: number;
  orderReference: string;
  requestedCount: number;
  sentCount: number;
  deliveries: TicketEmailDelivery[];
};
