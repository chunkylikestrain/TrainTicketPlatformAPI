export type PaymentIntent = {
  paymentIntentId: string;
  bookingId: number;
  amount: number;
  currency: string;
  status: string;
  expiresAtUtc: string | null;
  testPaymentMethodTokens: string[];
};

export type Payment = {
  id: number;
  bookingId: number;
  paymentIntentId: string;
  paymentDate: string;
  status: string;
  amount: number;
};
