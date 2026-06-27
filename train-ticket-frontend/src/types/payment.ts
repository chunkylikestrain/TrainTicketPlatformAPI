export type PaymentIntent = {
  paymentIntentId: string;
  bookingId: number | null;
  bookingOrderId: number | null;
  bookingIds: number[];
  originalAmount: number;
  amount: number;
  loyaltyPointsRedeemed: number;
  loyaltyDiscountAmount: number;
  currency: string;
  status: string;
  expiresAtUtc: string | null;
  testPaymentMethodTokens: string[];
};

export type Payment = {
  id: number;
  bookingId: number | null;
  bookingOrderId: number | null;
  bookingIds: number[];
  paymentIntentId: string;
  paymentDate: string;
  status: string;
  amount: number;
  loyaltyPointsRedeemed: number;
  loyaltyDiscountAmount: number;
};
