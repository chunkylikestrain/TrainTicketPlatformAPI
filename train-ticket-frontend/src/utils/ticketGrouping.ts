import type { Booking } from "../types/booking";

export type TicketGroup = {
  key: string;
  orderId: number | null;
  tickets: Booking[];
  totalAmount: number;
  currency: string;
  isOrder: boolean;
};

export type TicketJourneyGroup = {
  direction: string;
  tickets: Booking[];
};

export function groupTicketsByOrder(tickets: Booking[]): TicketGroup[] {
  const groups = new Map<string, Booking[]>();

  for (const ticket of tickets) {
    const key = ticket.bookingOrderId ? `order-${ticket.bookingOrderId}` : `ticket-${ticket.id}`;
    groups.set(key, [...(groups.get(key) ?? []), ticket]);
  }

  return [...groups.entries()].map(([key, groupTickets]) => {
    const orderedTickets = sortTicketsByJourney(groupTickets);
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

export function groupTicketsByJourney(tickets: Booking[]): TicketJourneyGroup[] {
  const groups = new Map<string, Booking[]>();

  for (const ticket of sortTicketsByJourney(tickets)) {
    const direction = normalizeJourneyDirection(ticket.journeyDirection);
    groups.set(direction, [...(groups.get(direction) ?? []), ticket]);
  }

  return [...groups.entries()]
    .sort(([first], [second]) => journeyDirectionRank(first) - journeyDirectionRank(second))
    .map(([direction, journeyTickets]) => ({
      direction,
      tickets: journeyTickets,
    }));
}

export function isReturnedTicket(ticket: Booking) {
  return ticket.bookingStatus === "Refunded" || ticket.paymentStatus === "Refunded" || ticket.isCancelled;
}

export function isPastTicket(ticket: Booking) {
  const effectiveArrival = ticket.arrivalTime ?? ticket.travelDate;
  return Boolean(effectiveArrival && new Date(effectiveArrival).getTime() < Date.now());
}

function sortTicketsByJourney(tickets: Booking[]) {
  return [...tickets].sort((first, second) => {
    const directionDifference = journeyDirectionRank(first.journeyDirection) - journeyDirectionRank(second.journeyDirection);
    if (directionDifference !== 0) {
      return directionDifference;
    }

    const segmentDifference = first.journeySegmentIndex - second.journeySegmentIndex;
    if (segmentDifference !== 0) {
      return segmentDifference;
    }

    const firstTime = new Date(first.departureTime ?? first.travelDate).getTime();
    const secondTime = new Date(second.departureTime ?? second.travelDate).getTime();
    if (firstTime !== secondTime) {
      return firstTime - secondTime;
    }

    return first.id - second.id;
  });
}

function normalizeJourneyDirection(direction?: string | null) {
  return direction?.toLowerCase() === "return" ? "Return" : "Outbound";
}

function journeyDirectionRank(direction?: string | null) {
  return direction?.toLowerCase() === "return" ? 1 : 0;
}
