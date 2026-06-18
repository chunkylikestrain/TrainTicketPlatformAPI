import apiClient from "./apiClient";
import type { LoginRequest, LoginResponse, RegisterRequest } from "../types/auth";

export async function login(request: LoginRequest) {
  const response = await apiClient.post<LoginResponse>("/Auth/login", request);
  return response.data;
}

export async function register(request: RegisterRequest) {
  await apiClient.post("/Auth/register", request);
}
