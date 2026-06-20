import type { CurrentUser, LoginResponse } from "../types/auth";

const AUTH_TOKEN_KEY = "authToken";
const USER_EMAIL_KEY = "userEmail";
const USER_ID_KEY = "userId";
const USER_ROLE_KEY = "userRole";

export function hasAuthToken() {
  return Boolean(localStorage.getItem(AUTH_TOKEN_KEY));
}

export function getUserRole() {
  return localStorage.getItem(USER_ROLE_KEY);
}

export function getUserEmail() {
  return localStorage.getItem(USER_EMAIL_KEY);
}

export function saveLoginSession(response: LoginResponse, email: string) {
  localStorage.setItem(AUTH_TOKEN_KEY, response.token);
  localStorage.setItem(USER_ROLE_KEY, response.role);
  localStorage.setItem(USER_ID_KEY, String(response.userId));
  localStorage.setItem(USER_EMAIL_KEY, email.trim());
}

export function saveCurrentUser(user: CurrentUser) {
  localStorage.setItem(USER_EMAIL_KEY, user.email);
  localStorage.setItem(USER_ROLE_KEY, user.role);
  localStorage.setItem(USER_ID_KEY, String(user.id));
}

export function clearAuthSession() {
  localStorage.removeItem(AUTH_TOKEN_KEY);
  localStorage.removeItem(USER_EMAIL_KEY);
  localStorage.removeItem(USER_ROLE_KEY);
  localStorage.removeItem(USER_ID_KEY);
}

export function saveProfileDisplayName(email: string, displayName: string) {
  localStorage.setItem(`profileName:${email.trim().toLowerCase()}`, displayName.trim());
}

export function getProfileDisplayName(email: string) {
  return localStorage.getItem(`profileName:${email.trim().toLowerCase()}`) ?? email;
}
