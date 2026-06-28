import apiClient from "./apiClient";
import type { CreateInvoiceRequest, Invoice } from "../types/invoice";

export async function generateBookingInvoice(bookingId: number | string, request: CreateInvoiceRequest) {
  const response = await apiClient.post<Invoice>(`/Invoices/bookings/${bookingId}`, request);
  return response.data;
}

export async function generateOrderInvoice(orderId: number | string, request: CreateInvoiceRequest) {
  const response = await apiClient.post<Invoice>(`/Invoices/orders/${orderId}`, request);
  return response.data;
}

export async function getMyInvoices() {
  const response = await apiClient.get<Invoice[]>("/Invoices/me");
  return response.data;
}

export async function getInvoicePdfBlob(invoiceId: number | string) {
  const response = await apiClient.get<Blob>(`/Invoices/${invoiceId}/pdf`, {
    responseType: "blob",
  });

  return response.data;
}

export async function downloadInvoicePdf(invoice: Invoice) {
  const pdf = await getInvoicePdfBlob(invoice.id);
  const url = window.URL.createObjectURL(pdf);
  const link = document.createElement("a");
  link.href = url;
  link.download = `invoice-${invoice.invoiceNumber.replaceAll("/", "-")}.pdf`;
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(url);
}
