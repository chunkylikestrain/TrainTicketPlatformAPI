export type LoyaltyAccount = {
  userId: number;
  redeemablePoints: number;
  pendingPoints: number;
  expiringPoints: number;
  redeemableValuePln: number;
  earnRatePointsPerPln: number;
  redeemRatePointsPerPln: number;
  updatedAtUtc: string;
};

export type LoyaltyTransaction = {
  id: number;
  type: string;
  status: string;
  points: number;
  sourceAmount: number;
  currency: string;
  reference: string;
  description: string;
  transactionDateUtc: string;
  validFromUtc: string;
  expiresAtUtc: string | null;
  bookingId: number | null;
  bookingOrderId: number | null;
};
