import type { Booking } from "../types/booking";

export type TicketGroup = {
  key: string;
  orderId: number | null;
  tickets: Booking[];
  totalAmount: number;
  currency: string;
  isOrder: boolean;
};

export function groupTicketsByOrder(tickets: Booking[]): TicketGroup[] {
  const groups = new Map<string, Booking[]>();

  for (const ticket of tickets) {
    const key = ticket.bookingOrderId ? `order-${ticket.bookingOrderId}` : `ticket-${ticket.id}`;
    groups.set(key, [...(groups.get(key) ?? []), ticket]);
  }

  return [...groups.entries()].map(([key, groupTickets]) => {
    const orderedTickets = [...groupTickets].sort((first, second) => first.id - second.id);
    const orderId = orderedTickets[0]?.bookingOrderId ?? null;

    return {
      key,
      orderId,
      tickets: orderedTickets,
      totalAmount: orderedTickets.reduce((sum, ticket) => sum + ticket.amount, 0),
      currency: orderedTickets[0]?.currency || "PLN",
      isOrder: Boolean(orderId && orderedTickets.length > 1),
    };
  });
}

export function isReturnedTicket(ticket: Booking) {
  return ticket.bookingStatus === "Refunded" || ticket.paymentStatus === "Refunded" || ticket.isCancelled;
}

export function isPastTicket(ticket: Booking) {
  const effectiveArrival = ticket.arrivalTime ?? ticket.travelDate;
  return Boolean(effectiveArrival && new Date(effectiveArrival).getTime() < Date.now());
}
