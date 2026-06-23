import apiClient from "./apiClient";
import type { BookingOrderEmailDelivery, BookingOrderTickets, TicketArtifact, TicketEmailDelivery } from "../types/ticket";

function guestParams(email?: string) {
  return email ? { email } : undefined;
}

export async function getTicketArtifact(bookingId: number | string, email?: string) {
  const response = await apiClient.get<TicketArtifact>(`/Bookings/${bookingId}/ticket`, {
    params: guestParams(email),
  });

  return response.data;
}

export async function getTicketQrSvgBlob(bookingId: number | string, email?: string) {
  const response = await apiClient.get<Blob>(`/Bookings/${bookingId}/ticket/qr`, {
    params: guestParams(email),
    responseType: "blob",
  });

  return response.data;
}

export async function getTicketPdfBlob(bookingId: number | string, email?: string) {
  const response = await apiClient.get<Blob>(`/Bookings/${bookingId}/ticket/pdf`, {
    params: guestParams(email),
    responseType: "blob",
  });

  return response.data;
}

export async function sendTicketEmail(bookingId: number | string, email?: string) {
  const response = await apiClient.post<TicketEmailDelivery>(`/Bookings/${bookingId}/ticket/email`, {
    email,
  });

  return response.data;
}

export async function getOrderTickets(orderId: number | string, email?: string) {
  const response = await apiClient.get<BookingOrderTickets>(`/Bookings/orders/${orderId}/tickets`, {
    params: guestParams(email),
  });

  return response.data;
}

export async function sendOrderTicketsEmail(orderId: number | string, email?: string) {
  const response = await apiClient.post<BookingOrderEmailDelivery>(`/Bookings/orders/${orderId}/tickets/email`, {
    email,
  });

  return response.data;
}

export async function downloadTicketPdf(bookingId: number | string, email?: string, ticketNumber?: string) {
  const pdf = await getTicketPdfBlob(bookingId, email);
  const url = window.URL.createObjectURL(pdf);
  const link = document.createElement("a");
  link.href = url;
  link.download = `ticket-${ticketNumber || bookingId}.pdf`;
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(url);
}
