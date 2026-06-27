import apiClient from "./apiClient";
import type { Payment, PaymentIntent } from "../types/payment";

export async function createPaymentIntent(bookingId: number | string, redeemLoyaltyPoints = 0) {
  const response = await apiClient.post<PaymentIntent>("/Payments/intent", {
    bookingId: Number(bookingId),
    redeemLoyaltyPoints,
  });

  return response.data;
}

export async function createOrderPaymentIntent(orderId: number | string, redeemLoyaltyPoints = 0) {
  const response = await apiClient.post<PaymentIntent>("/Payments/intent", {
    bookingOrderId: Number(orderId),
    redeemLoyaltyPoints,
  });

  return response.data;
}

export async function confirmPayment(paymentIntentId: string, paymentMethodToken = "tok_success") {
  const response = await apiClient.post<Payment>("/Payments/confirm", {
    paymentIntentId,
    paymentMethodToken,
  });

  return response.data;
}
