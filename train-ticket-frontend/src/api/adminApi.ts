import apiClient from "./apiClient";
import type {
  AdminBooking,
  AdminDiscount,
  AdminRevenueReport,
  AdminRoute,
  AdminSchedule,
  AdminTrain,
  AdminUser,
  PagedResponse,
  Station,
} from "../types/admin";

type UnknownPagedResponse<T> = Partial<PagedResponse<T>> & {
  Items?: T[];
  TotalCount?: number;
  TotalPages?: number;
  Page?: number;
  PageSize?: number;
};

function normalizePagedResponse<T>(data: UnknownPagedResponse<T> | T[]): PagedResponse<T> {
  if (Array.isArray(data)) {
    return {
      items: data,
      page: 1,
      pageSize: data.length,
      totalCount: data.length,
      totalPages: data.length > 0 ? 1 : 0,
    };
  }

  const items = data.items ?? data.Items ?? [];
  const totalCount = data.totalCount ?? data.TotalCount ?? items.length;
  const pageSize = data.pageSize ?? data.PageSize ?? items.length;

  return {
    items,
    page: data.page ?? data.Page ?? 1,
    pageSize,
    totalCount,
    totalPages: data.totalPages ?? data.TotalPages ?? (pageSize > 0 ? Math.ceil(totalCount / pageSize) : 0),
  };
}

export async function getAdminTrains() {
  const response = await apiClient.get<AdminTrain[]>("/admin/trains");
  return response.data;
}

export async function createAdminTrain(train: Omit<AdminTrain, "id">) {
  const response = await apiClient.post<AdminTrain>("/admin/trains", train);
  return response.data;
}

export async function deleteAdminTrain(id: number) {
  await apiClient.delete(`/admin/trains/${id}`);
}

export async function getAdminRoutes() {
  const response = await apiClient.get<AdminRoute[]>("/admin/routes");
  return response.data;
}

export async function getAdminRoute(id: number) {
  const response = await apiClient.get<AdminRoute>(`/admin/routes/${id}`);
  return response.data;
}

export async function createAdminRoute(route: Omit<AdminRoute, "id" | "departureStationName" | "arrivalStationName">) {
  const response = await apiClient.post<AdminRoute>("/admin/routes", route);
  return response.data;
}

export async function updateAdminRoute(route: AdminRoute) {
  const response = await apiClient.put<AdminRoute>(`/admin/routes/${route.id}`, route);
  return response.data;
}

export async function deleteAdminRoute(id: number) {
  await apiClient.delete(`/admin/routes/${id}`);
}

export async function getAdminSchedules() {
  const response = await apiClient.get<AdminSchedule[]>("/admin/schedules");
  return response.data;
}

export async function createAdminSchedule(schedule: Omit<AdminSchedule, "id" | "trainCode" | "routeCode" | "route">) {
  const response = await apiClient.post<AdminSchedule>("/admin/schedules", schedule);
  return response.data;
}

export async function updateAdminSchedule(schedule: AdminSchedule) {
  const response = await apiClient.put<AdminSchedule>(`/admin/schedules/${schedule.id}`, schedule);
  return response.data;
}

export async function deleteAdminSchedule(id: number) {
  await apiClient.delete(`/admin/schedules/${id}`);
}

export async function getAdminDiscounts() {
  const response = await apiClient.get<AdminDiscount[]>("/admin/discounts");
  return response.data;
}

export async function createAdminDiscount(discount: Omit<AdminDiscount, "id">) {
  const response = await apiClient.post<AdminDiscount>("/admin/discounts", discount);
  return response.data;
}

export async function updateAdminDiscount(discount: AdminDiscount) {
  const response = await apiClient.put<AdminDiscount>(`/admin/discounts/${discount.id}`, discount);
  return response.data;
}

export async function deleteAdminDiscount(id: number) {
  await apiClient.delete(`/admin/discounts/${id}`);
}

export async function getAdminUsers(search = "") {
  const response = await apiClient.get<UnknownPagedResponse<AdminUser> | AdminUser[]>("/admin/users", {
    params: { search, pageSize: 100 },
  });
  const page = normalizePagedResponse(response.data);

  if (page.items.length > 0 || search.trim()) {
    return page.items;
  }

  const legacyResponse = await apiClient.get<AdminUser[]>("/Users");
  return legacyResponse.data;
}

export async function getAdminUsersPage(pageSize = 10) {
  const response = await apiClient.get<UnknownPagedResponse<AdminUser> | AdminUser[]>("/admin/users", {
    params: { pageSize },
  });
  const page = normalizePagedResponse(response.data);

  if (page.totalCount > 0) {
    return page;
  }

  const legacyResponse = await apiClient.get<AdminUser[]>("/Users");
  return normalizePagedResponse(legacyResponse.data);
}

export async function updateAdminUser(user: AdminUser) {
  const response = await apiClient.put<AdminUser>(`/admin/users/${user.id}`, user);
  return response.data;
}

export async function deleteAdminUser(id: number) {
  await apiClient.delete(`/admin/users/${id}`);
}

export async function getAdminBookings(search = "") {
  const response = await apiClient.get<PagedResponse<AdminBooking>>("/admin/bookings", {
    params: { pageSize: 100 },
  });

  const normalizedSearch = search.trim().toLowerCase();
  if (!normalizedSearch) return response.data.items;

  return response.data.items.filter((booking) =>
    `${booking.bookingReference} ${booking.ticketNumber} ${booking.passengerName ?? ""} ${booking.guestEmail ?? ""} ${booking.route} ${booking.trainName}`
      .toLowerCase()
      .includes(normalizedSearch));
}

export async function getAdminBookingsPage(pageSize = 10) {
  const response = await apiClient.get<PagedResponse<AdminBooking>>("/admin/bookings", {
    params: { pageSize },
  });
  return response.data;
}

export async function adminCancelAndRefundBooking(id: number, reason: string) {
  const response = await apiClient.post<AdminBooking>(`/admin/bookings/${id}/cancel-refund`, { reason });
  return response.data;
}

export async function getAdminRevenueReport(from: string, to: string) {
  const response = await apiClient.get<AdminRevenueReport>("/admin/reports/revenue", {
    params: { from, to },
  });
  return response.data;
}

export async function getStations() {
  const response = await apiClient.get<Station[]>("/Stations");
  return response.data;
}
