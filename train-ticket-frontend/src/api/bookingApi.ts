import apiClient from "./apiClient";
import type {
  Booking,
  BookingOrder,
  CreateBookingOrderRequest,
  CreateBookingRequest,
  UpdateBookingExtrasRequest,
  UpdateGuestBookingDataRequest,
} from "../types/booking";

export async function createBookingHold(request: CreateBookingRequest) {
  const response = await apiClient.post<Booking>("/Bookings", request);
  return response.data;
}

export async function createBookingOrderHold(request: CreateBookingOrderRequest) {
  const response = await apiClient.post<BookingOrder>("/Bookings/orders", request, {
    timeout: 30000,
  });
  return response.data;
}

export async function getBookingOrder(orderId: number | string) {
  const response = await apiClient.get<BookingOrder>(`/Bookings/orders/${orderId}`);
  return response.data;
}

export async function getBookingById(bookingId: number | string) {
  const response = await apiClient.get<Booking>(`/Bookings/${bookingId}`);
  return response.data;
}

export async function updateGuestBookingData(bookingId: number | string, request: UpdateGuestBookingDataRequest) {
  const response = await apiClient.put<Booking>(`/Bookings/${bookingId}/guest-data`, request);
  return response.data;
}

export async function updateBookingExtras(bookingId: number | string, request: UpdateBookingExtrasRequest) {
  const response = await apiClient.put<Booking>(`/Bookings/${bookingId}/extras`, request);
  return response.data;
}

export async function updateBookingOrderExtras(orderId: number | string, request: UpdateBookingExtrasRequest) {
  const response = await apiClient.put<BookingOrder>(`/Bookings/orders/${orderId}/extras`, request);
  return response.data;
}

export async function getGuestTickets(email: string) {
  const response = await apiClient.get<Booking[]>("/Bookings/guest", {
    params: { email },
  });

  return response.data;
}

export async function getMyTickets(section = "tickets") {
  const response = await apiClient.get<Booking[]>("/Bookings/me", {
    params: { section },
  });
  return response.data;
}

export async function refundGuestTicket(ticketNumber: string, email: string) {
  const response = await apiClient.post<Booking>(`/Bookings/tickets/${ticketNumber}/refund`, {
    email,
  });

  return response.data;
}

export async function refundMyTicket(bookingId: number | string, reason = "Passenger requested return") {
  const response = await apiClient.post<Booking>(`/Bookings/${bookingId}/refund`, {
    reason,
  });

  return response.data;
}
