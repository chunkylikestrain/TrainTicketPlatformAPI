import { DollarOutlined, EditOutlined, ExclamationCircleOutlined, FilterOutlined, SearchOutlined } from "@ant-design/icons";
import { useEffect, useMemo, useState } from "react";
import type { FormEvent } from "react";
import { getAdminSchedules, updateAdminSchedule } from "../../api/adminApi";
import AdminLayout from "../../components/AdminLayout";
import type { AdminSchedule } from "../../types/admin";

type PricingRow = {
  id: string;
  schedule: AdminSchedule;
  routeCode: string;
  route: string;
  seatType: "Class 1" | "Class 2";
  passenger: "Normal";
  price: number;
  status: string;
};

function formatMoney(value: number) {
  return `${value.toLocaleString("pl-PL", { minimumFractionDigits: 2, maximumFractionDigits: 2 })} PLN`;
}

function toRows(schedules: AdminSchedule[]): PricingRow[] {
  return schedules.flatMap((schedule) => [
    {
      id: `${schedule.id}-class-1`,
      schedule,
      routeCode: schedule.routeCode || `TRIP-${schedule.id}`,
      route: schedule.route,
      seatType: "Class 1" as const,
      passenger: "Normal" as const,
      price: schedule.class1Price,
      status: schedule.class1Price > 0 ? "Active" : "Missing",
    },
    {
      id: `${schedule.id}-class-2`,
      schedule,
      routeCode: schedule.routeCode || `TRIP-${schedule.id}`,
      route: schedule.route,
      seatType: "Class 2" as const,
      passenger: "Normal" as const,
      price: schedule.class2Price,
      status: schedule.class2Price > 0 ? "Active" : "Missing",
    },
  ]);
}

function AdminPricingPage() {
  const [schedules, setSchedules] = useState<AdminSchedule[]>([]);
  const [query, setQuery] = useState("");
  const [seatFilter, setSeatFilter] = useState("All seats");
  const [passengerFilter, setPassengerFilter] = useState("All passengers");
  const [editingRow, setEditingRow] = useState<PricingRow | null>(null);
  const [draftPrice, setDraftPrice] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const rows = useMemo(() => toRows(schedules), [schedules]);
  const visibleRows = rows.filter((row) => {
    const matchesSearch = `${row.routeCode} ${row.route} ${row.seatType}`.toLowerCase().includes(query.toLowerCase());
    const matchesSeat = seatFilter === "All seats" || row.seatType === seatFilter;
    const matchesPassenger = passengerFilter === "All passengers" || row.passenger === passengerFilter;
    return matchesSearch && matchesSeat && matchesPassenger;
  });
  const configuredRouteCount = new Set(rows.filter((row) => row.price > 0).map((row) => row.routeCode)).size;
  const missingRouteCount = new Set(rows.filter((row) => row.price <= 0).map((row) => row.routeCode)).size;

  async function loadPricing() {
    setIsLoading(true);
    setError("");

    try {
      const data = await getAdminSchedules();
      setSchedules(data);
    } catch {
      setSchedules([]);
      setError("Pricing could not be loaded. Add schedules first, then their fare rules will appear here.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadPricing();
  }, []);

  function openEdit(row: PricingRow) {
    setEditingRow(row);
    setDraftPrice(String(row.price));
    setSuccess("");
    setError("");
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!editingRow) return;

    const nextPrice = Number(draftPrice);
    if (Number.isNaN(nextPrice) || nextPrice < 0) {
      setError("Price must be zero or greater.");
      return;
    }

    const updatedSchedule = {
      ...editingRow.schedule,
      class1Price: editingRow.seatType === "Class 1" ? nextPrice : editingRow.schedule.class1Price,
      class2Price: editingRow.seatType === "Class 2" ? nextPrice : editingRow.schedule.class2Price,
    };

    try {
      const saved = await updateAdminSchedule(updatedSchedule);
      setSchedules((current) => current.map((schedule) => schedule.id === saved.id ? saved : schedule));
      setEditingRow(null);
      setSuccess("Pricing rule saved.");
    } catch {
      setError("Pricing rule could not be saved. Check that the schedule still exists.");
    }
  }

  return (
    <AdminLayout>
      <section className="admin-page-heading">
        <div>
          <h1><DollarOutlined /> Manage Pricing</h1>
          <p>Adjust real Class 1 and Class 2 fares for scheduled trips.</p>
        </div>
        <button type="button" className="admin-secondary-button" onClick={loadPricing}>
          Refresh pricing
        </button>
      </section>

      <section className="admin-stat-grid pricing-stat-grid">
        <article className="admin-stat-card"><span className="stat-blue"><DollarOutlined /></span><div><small>Total fare rules</small><strong>{rows.length}</strong></div></article>
        <article className="admin-stat-card"><span className="stat-green"><FilterOutlined /></span><div><small>Configured routes</small><strong>{configuredRouteCount}</strong></div></article>
        <article className="admin-stat-card"><span className="stat-orange"><ExclamationCircleOutlined /></span><div><small>Missing pricing</small><strong>{missingRouteCount} Routes</strong></div></article>
      </section>

      <section className="admin-filter-bar">
        <SearchOutlined />
        <input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Search by station name or route code..." />
        <select value={seatFilter} onChange={(event) => setSeatFilter(event.target.value)}>
          <option>All seats</option><option>Class 1</option><option>Class 2</option>
        </select>
        <select value={passengerFilter} onChange={(event) => setPassengerFilter(event.target.value)}>
          <option>All passengers</option><option>Normal</option>
        </select>
      </section>

      {error && <div className="admin-save-banner admin-danger-panel">{error}</div>}
      {success && <div className="admin-save-banner">{success}</div>}

      {editingRow && (
        <form className="admin-editor-panel" onSubmit={handleSubmit}>
          <h2>Edit fare rule</h2>
          <label>Route<p>{editingRow.routeCode} - {editingRow.route}</p></label>
          <label>Seat type<p>{editingRow.seatType}</p></label>
          <label>Passenger<p>{editingRow.passenger}</p></label>
          <label>Base price<input value={draftPrice} onChange={(event) => setDraftPrice(event.target.value)} min="0" step="0.01" type="number" /></label>
          <div className="admin-form-actions">
            <button type="submit" className="admin-primary-button">Save price</button>
            <button type="button" className="admin-secondary-button" onClick={() => setEditingRow(null)}>Cancel</button>
          </div>
        </form>
      )}

      <section className="admin-table-card">
        <table className="admin-table">
          <thead><tr><th>Route details</th><th>Seat type</th><th>Passenger</th><th>Price</th><th>Status</th><th>Actions</th></tr></thead>
          <tbody>
            {isLoading && (
              <tr>
                <td colSpan={6}>Loading pricing rules...</td>
              </tr>
            )}
            {!isLoading && visibleRows.length === 0 && (
              <tr>
                <td colSpan={6}>No pricing rules found. Create schedules first so their Class 1 and Class 2 fares can be managed here.</td>
              </tr>
            )}
            {!isLoading && visibleRows.map((row) => (
              <tr key={row.id}>
                <td><strong className="route-code-pill">{row.routeCode}</strong> {row.route}</td>
                <td>{row.seatType}</td>
                <td>{row.passenger}</td>
                <td>{formatMoney(row.price)}</td>
                <td>
                  <span className={row.status === "Active" ? "status-pill status-active" : "status-pill status-warning"}>
                    {row.status}
                  </span>
                </td>
                <td><button type="button" onClick={() => openEdit(row)} aria-label={`Edit ${row.routeCode} ${row.seatType}`}><EditOutlined /></button></td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </AdminLayout>
  );
}

export default AdminPricingPage;
