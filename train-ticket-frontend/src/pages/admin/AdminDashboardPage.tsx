import { CalendarOutlined, DollarOutlined, ReloadOutlined, TeamOutlined, TransactionOutlined } from "@ant-design/icons";
import { useEffect, useMemo, useState } from "react";
import {
  getAdminBookingsPage,
  getAdminRevenueReport,
  getAdminSchedules,
  getAdminUsersPage,
} from "../../api/adminApi";
import AdminLayout from "../../components/AdminLayout";
import type { AdminBooking, AdminRevenueReport, AdminSchedule } from "../../types/admin";

type DashboardData = {
  monthRevenue: AdminRevenueReport;
  weekRevenue: AdminRevenueReport;
  totalBookings: number;
  totalUsers: number;
  schedulesThisMonth: number;
  recentBookings: AdminBooking[];
};

const emptyRevenueReport: AdminRevenueReport = {
  from: "",
  to: "",
  grossRevenue: 0,
  refunds: 0,
  netRevenue: 0,
  totalBookings: 0,
  paidBookings: 0,
  refundedBookings: 0,
  averageOrderValue: 0,
  dailyRevenue: [],
  routeBreakdown: [],
  recentActivity: [],
};

function toInputDate(date: Date) {
  return date.toISOString().slice(0, 10);
}

function startOfMonth() {
  const date = new Date();
  return new Date(date.getFullYear(), date.getMonth(), 1);
}

function startOfLastSevenDays() {
  const date = new Date();
  date.setDate(date.getDate() - 6);
  return date;
}

function formatMoney(value: number) {
  return `${value.toLocaleString("pl-PL", { minimumFractionDigits: 2, maximumFractionDigits: 2 })} PLN`;
}

function formatChartDate(value: string) {
  return new Intl.DateTimeFormat("en", { month: "2-digit", day: "2-digit" }).format(new Date(value));
}

function statusClass(status: string) {
  const normalized = status.toLowerCase();
  if (normalized.includes("cancel") || normalized.includes("refund")) return "status-pill status-danger";
  if (normalized.includes("pending")) return "status-pill status-warning";
  return "status-pill status-active";
}

function countSchedulesThisMonth(schedules: AdminSchedule[]) {
  const from = startOfMonth();
  const to = new Date(from.getFullYear(), from.getMonth() + 1, 1);

  return schedules.filter((schedule) => {
    const departure = new Date(schedule.departureTime);
    return departure >= from && departure < to;
  }).length;
}

function makeEmptyWeeklyReport(from: string) {
  const dailyRevenue = Array.from({ length: 7 }, (_, index) => {
    const date = new Date(from);
    date.setDate(date.getDate() + index);
    return {
      date: date.toISOString(),
      revenue: 0,
      refunds: 0,
      bookings: 0,
    };
  });

  return { ...emptyRevenueReport, from, to: toInputDate(new Date()), dailyRevenue };
}

function AdminDashboardPage() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");

  const maxWeeklyRevenue = useMemo(() => {
    if (!data?.weekRevenue.dailyRevenue.length) return 1;
    return Math.max(...data.weekRevenue.dailyRevenue.map((point) => point.revenue), 1);
  }, [data]);

  async function loadDashboard() {
    setIsLoading(true);
    setError("");

    const today = toInputDate(new Date());
    const monthFrom = toInputDate(startOfMonth());
    const weekFrom = toInputDate(startOfLastSevenDays());

    try {
      const [monthRevenueResult, weekRevenueResult, bookingsResult, usersResult, schedulesResult] = await Promise.allSettled([
        getAdminRevenueReport(monthFrom, today),
        getAdminRevenueReport(weekFrom, today),
        getAdminBookingsPage(5),
        getAdminUsersPage(1),
        getAdminSchedules(),
      ]);

      const monthRevenue = monthRevenueResult.status === "fulfilled"
        ? monthRevenueResult.value
        : { ...emptyRevenueReport, from: monthFrom, to: today };
      const weekRevenue = weekRevenueResult.status === "fulfilled"
        ? weekRevenueResult.value
        : makeEmptyWeeklyReport(weekFrom);
      const bookingsPage = bookingsResult.status === "fulfilled"
        ? bookingsResult.value
        : { items: [], totalCount: 0 };
      const usersPage = usersResult.status === "fulfilled"
        ? usersResult.value
        : { totalCount: 0 };
      const schedules = schedulesResult.status === "fulfilled" ? schedulesResult.value : [];

      setData({
        monthRevenue,
        weekRevenue,
        totalBookings: bookingsPage.totalCount,
        totalUsers: usersPage.totalCount,
        schedulesThisMonth: countSchedulesThisMonth(schedules),
        recentBookings: bookingsPage.items,
      });

      if ([monthRevenueResult, weekRevenueResult, bookingsResult, usersResult, schedulesResult].some((result) => result.status === "rejected")) {
        setError("Some dashboard data could not be loaded yet. Showing available data with zeroes for missing sections.");
      }
    } catch {
      setError("Dashboard data could not be loaded. Check that the API is running and your admin session is still valid.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadDashboard();
  }, []);

  return (
    <AdminLayout>
      <section className="admin-page-heading">
        <div>
          <h1>Dashboard Overview</h1>
          <p>Operational snapshot for bookings, schedules, revenue, and passengers.</p>
        </div>
        <button type="button" className="admin-text-action" onClick={loadDashboard}>
          <ReloadOutlined /> Refresh data
        </button>
      </section>

      {error && <div className="admin-save-banner admin-danger-panel">{error}</div>}

      {isLoading && (
        <section className="admin-empty-panel">
          <h2>Loading dashboard</h2>
          <p>Collecting revenue, bookings, users, and schedules from the backend.</p>
        </section>
      )}

      {!isLoading && data && (
        <>
          <section className="admin-stat-grid">
            <article className="admin-stat-card">
              <span className="stat-green"><DollarOutlined /></span>
              <div><small>Revenue this month</small><strong>{formatMoney(data.monthRevenue.netRevenue)}</strong></div>
            </article>
            <article className="admin-stat-card">
              <span className="stat-blue"><TransactionOutlined /></span>
              <div><small>Total bookings</small><strong>{data.totalBookings}</strong></div>
            </article>
            <article className="admin-stat-card">
              <span className="stat-purple"><TeamOutlined /></span>
              <div><small>Total users</small><strong>{data.totalUsers}</strong></div>
            </article>
            <article className="admin-stat-card">
              <span className="stat-orange"><CalendarOutlined /></span>
              <div><small>Schedules this month</small><strong>{data.schedulesThisMonth}</strong></div>
            </article>
          </section>

          <section className="admin-dashboard-grid">
            <article className="admin-panel">
              <div className="revenue-panel-title">
                <h2>Revenue - Last 7 Days</h2>
                <span>{formatMoney(data.weekRevenue.netRevenue)}</span>
              </div>
              <div className="dashboard-revenue-chart" role="img" aria-label="Revenue for the last seven days">
                {data.weekRevenue.dailyRevenue.map((point) => (
                  <div className="dashboard-revenue-day" key={point.date}>
                    <span
                      style={{ height: `${Math.max((point.revenue / maxWeeklyRevenue) * 100, point.revenue ? 8 : 0)}%` }}
                      title={formatMoney(point.revenue)}
                    />
                    <small>{formatChartDate(point.date)}</small>
                  </div>
                ))}
              </div>
            </article>

            <article className="admin-panel">
              <h2>Recent Bookings</h2>
              <div className="recent-booking-list">
                {data.recentBookings.map((booking) => (
                  <div className="recent-booking-row" key={booking.id}>
                    <div>
                      <strong>{booking.ticketNumber || booking.bookingReference}</strong>
                      <small>{booking.passengerName ?? booking.guestEmail ?? "Passenger"}</small>
                      <small>{booking.route}</small>
                    </div>
                    <div>
                      <b>{formatMoney(booking.amount)}</b>
                      <span className={statusClass(booking.bookingStatus)}>
                        {booking.bookingStatus}
                      </span>
                    </div>
                  </div>
                ))}
                {data.recentBookings.length === 0 && <p>No bookings have been created yet.</p>}
              </div>
            </article>
          </section>
        </>
      )}
    </AdminLayout>
  );
}

export default AdminDashboardPage;
