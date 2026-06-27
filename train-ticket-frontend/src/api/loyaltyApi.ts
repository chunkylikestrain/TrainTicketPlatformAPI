import apiClient from "./apiClient";
import type { LoyaltyAccount, LoyaltyTransaction } from "../types/loyalty";

export async function getMyLoyaltyAccount() {
  const response = await apiClient.get<LoyaltyAccount>("/Loyalty/me");
  return response.data;
}

export async function getMyLoyaltyTransactions() {
  const response = await apiClient.get<LoyaltyTransaction[]>("/Loyalty/me/transactions");
  return response.data;
}
