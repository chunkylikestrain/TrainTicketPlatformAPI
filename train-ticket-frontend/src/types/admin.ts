export type PagedResponse<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type AdminTrain = {
  id: number;
  code: string;
  name: string;
  type: string;
  carriageCount: number;
  seatsPerCarriage: number;
  status: string;
  departureStation: string;
  arrivalStation: string;
  departureTime: string;
  arrivalTime: string;
};

export type AdminRoute = {
  id: number;
  code: string;
  name: string;
  departureStationId: number;
  arrivalStationId: number;
  departureStationName: string;
  arrivalStationName: string;
  distanceKm: number;
  estimatedDurationMinutes: number;
  operatingDays: string;
  intermediateStops: string;
  intermediateStopStationIds: number[];
  stops: AdminRouteStop[];
  isActive: boolean;
};

export type AdminRouteStop = {
  stationId: number;
  stationCode: string;
  stationName: string;
  stopOrder: number;
};

export type AdminSchedule = {
  id: number;
  trainId: number;
  trainCode: string;
  trainRouteId: number;
  routeCode: string;
  route: string;
  departureTime: string;
  arrivalTime: string;
  platform: string;
  track: string;
  status: string;
  class1Price: number;
  class2Price: number;
};

export type AdminDiscount = {
  id: number;
  name: string;
  percent: number;
  eligibleClass: string;
  documentHint: string;
  status: string;
};

export type AdminUser = {
  id: number;
  email: string;
  phone: string;
  role: string;
  displayName: string;
  status: string;
};

export type AdminBooking = {
  id: number;
  userId: number | null;
  trainId: number;
  tripId: number | null;
  seatId: number;
  bookingReference: string;
  ticketNumber: string;
  guestEmail: string | null;
  passengerName: string | null;
  bookingDate: string;
  travelDate: string;
  bookingStatus: string;
  paymentStatus: string;
  cancellationReason: string | null;
  trainName: string;
  route: string;
  seatLabel: string;
  amount: number;
};

export type AdminRevenueDailyPoint = {
  date: string;
  revenue: number;
  refunds: number;
  bookings: number;
};

export type AdminRevenueRoute = {
  route: string;
  revenue: number;
  paidBookings: number;
};

export type AdminRevenueActivity = {
  bookingReference: string;
  ticketNumber: string;
  passengerName: string;
  route: string;
  date: string;
  amount: number;
  status: string;
};

export type AdminRevenueReport = {
  from: string;
  to: string;
  grossRevenue: number;
  refunds: number;
  netRevenue: number;
  totalBookings: number;
  paidBookings: number;
  refundedBookings: number;
  averageOrderValue: number;
  dailyRevenue: AdminRevenueDailyPoint[];
  routeBreakdown: AdminRevenueRoute[];
  recentActivity: AdminRevenueActivity[];
};

export type Station = {
  id: number;
  code: string;
  name: string;
  city: string;
};
