import { BarChartOutlined, DollarOutlined, ReloadOutlined, RollbackOutlined, TransactionOutlined } from "@ant-design/icons";
import { useEffect, useMemo, useState } from "react";
import { getAdminRevenueReport } from "../../api/adminApi";
import AdminLayout from "../../components/AdminLayout";
import type { AdminRevenueReport } from "../../types/admin";

function toInputDate(date: Date) {
  return date.toISOString().slice(0, 10);
}

function formatMoney(value: number) {
  return `${value.toLocaleString("pl-PL", { minimumFractionDigits: 2, maximumFractionDigits: 2 })} PLN`;
}

function formatShortDate(value: string) {
  return new Intl.DateTimeFormat("en", { month: "short", day: "2-digit" }).format(new Date(value));
}

function getDefaultFromDate() {
  const date = new Date();
  date.setDate(date.getDate() - 13);
  return toInputDate(date);
}

function AdminRevenuePage() {
  const [from, setFrom] = useState(getDefaultFromDate);
  const [to, setTo] = useState(() => toInputDate(new Date()));
  const [report, setReport] = useState<AdminRevenueReport | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");

  const maxDailyValue = useMemo(() => {
    if (!report?.dailyRevenue.length) return 1;
    return Math.max(...report.dailyRevenue.map((point) => Math.max(point.revenue, point.refunds)), 1);
  }, [report]);

  async function loadReport() {
    setIsLoading(true);
    setError("");

    try {
      const data = await getAdminRevenueReport(from, to);
      setReport(data);
    } catch {
      setError("Revenue report could not be loaded. Check that the API is running and your admin session is still valid.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadReport();
  }, []);

  return (
    <AdminLayout>
      <section className="admin-page-heading">
        <div>
          <h1><BarChartOutlined /> Revenue Reports</h1>
          <p>Track earnings, refunds, paid bookings, route performance, and recent payment activity.</p>
        </div>
        <button type="button" className="admin-text-action" onClick={loadReport}>
          <ReloadOutlined /> Refresh data
        </button>
      </section>

      <section className="admin-filter-bar revenue-filter-bar">
        <label>
          From
          <input type="date" value={from} onChange={(event) => setFrom(event.target.value)} />
        </label>
        <label>
          To
          <input type="date" value={to} onChange={(event) => setTo(event.target.value)} />
        </label>
        <button type="button" className="admin-primary-button" onClick={loadReport}>
          Apply range
        </button>
      </section>

      {error && <div className="admin-save-banner admin-danger-panel">{error}</div>}

      {isLoading && (
        <section className="admin-empty-panel">
          <h2>Loading revenue data</h2>
          <p>Pulling the latest booking report from the backend.</p>
        </section>
      )}

      {!isLoading && report && (
        <>
          <section className="admin-stat-grid revenue-stat-grid">
            <article className="admin-stat-card">
              <span className="stat-green"><DollarOutlined /></span>
              <div><small>Gross sales</small><strong>{formatMoney(report.grossRevenue)}</strong></div>
            </article>
            <article className="admin-stat-card">
              <span className="stat-orange"><RollbackOutlined /></span>
              <div><small>Refunds</small><strong>{formatMoney(report.refunds)}</strong></div>
            </article>
            <article className="admin-stat-card">
              <span className="stat-blue"><TransactionOutlined /></span>
              <div><small>Net revenue</small><strong>{formatMoney(report.netRevenue)}</strong></div>
            </article>
            <article className="admin-stat-card">
              <span className="stat-purple"><BarChartOutlined /></span>
              <div><small>Avg order</small><strong>{formatMoney(report.averageOrderValue)}</strong></div>
            </article>
          </section>

          <section className="admin-dashboard-grid revenue-dashboard-grid">
            <article className="admin-panel revenue-chart-panel">
              <div className="revenue-panel-title">
                <h2>Earnings by day</h2>
                <span>{report.paidBookings} paid / {report.refundedBookings} refunded</span>
              </div>
              <div className="revenue-chart" role="img" aria-label="Daily revenue and refunds chart">
                {report.dailyRevenue.map((point) => (
                  <div className="revenue-day" key={point.date}>
                    <div className="revenue-bars">
                      <span
                        className="revenue-bar-positive"
                        style={{ height: `${Math.max((point.revenue / maxDailyValue) * 100, point.revenue ? 8 : 0)}%` }}
                        title={`Revenue ${formatMoney(point.revenue)}`}
                      />
                      <span
                        className="revenue-bar-refund"
                        style={{ height: `${Math.max((point.refunds / maxDailyValue) * 100, point.refunds ? 8 : 0)}%` }}
                        title={`Refunds ${formatMoney(point.refunds)}`}
                      />
                    </div>
                    <small>{formatShortDate(point.date)}</small>
                  </div>
                ))}
              </div>
              <div className="revenue-chart-legend">
                <span><i className="legend-revenue" /> Sales</span>
                <span><i className="legend-refund" /> Refunds</span>
              </div>
            </article>

            <article className="admin-panel">
              <h2>Report summary</h2>
              <dl className="revenue-summary-list">
                <div><dt>Total bookings</dt><dd>{report.totalBookings}</dd></div>
                <div><dt>Paid bookings</dt><dd>{report.paidBookings}</dd></div>
                <div><dt>Refunded bookings</dt><dd>{report.refundedBookings}</dd></div>
                <div><dt>Net revenue</dt><dd>{formatMoney(report.netRevenue)}</dd></div>
              </dl>
            </article>
          </section>

          <section className="admin-dashboard-grid revenue-lower-grid">
            <article className="admin-table-card">
              <div className="admin-table-heading">
                <h2>Top routes by revenue</h2>
                <span>Paid bookings only</span>
              </div>
              <table className="admin-table">
                <thead>
                  <tr>
                    <th>Route</th>
                    <th>Paid bookings</th>
                    <th>Revenue</th>
                  </tr>
                </thead>
                <tbody>
                  {report.routeBreakdown.map((route) => (
                    <tr key={route.route}>
                      <td>{route.route}</td>
                      <td>{route.paidBookings}</td>
                      <td>{formatMoney(route.revenue)}</td>
                    </tr>
                  ))}
                  {report.routeBreakdown.length === 0 && (
                    <tr>
                      <td colSpan={3}>No paid route activity in this date range.</td>
                    </tr>
                  )}
                </tbody>
              </table>
            </article>

            <article className="admin-panel">
              <h2>Recent financial activity</h2>
              <div className="recent-booking-list">
                {report.recentActivity.map((activity) => (
                  <div className="recent-booking-row" key={`${activity.bookingReference}-${activity.date}-${activity.status}`}>
                    <div>
                      <strong>{activity.ticketNumber || activity.bookingReference}</strong>
                      <small>{activity.passengerName}</small>
                      <small>{activity.route}</small>
                    </div>
                    <div>
                      <b>{formatMoney(activity.amount)}</b>
                      <span className={activity.status === "Refunded" ? "status-pill status-danger" : "status-pill status-active"}>
                        {activity.status}
                      </span>
                    </div>
                  </div>
                ))}
                {report.recentActivity.length === 0 && <p>No payments or refunds in this date range.</p>}
              </div>
            </article>
          </section>
        </>
      )}
    </AdminLayout>
  );
}

export default AdminRevenuePage;
