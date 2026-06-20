import { CheckCircleOutlined, CloseCircleOutlined, DeleteOutlined, SearchOutlined } from "@ant-design/icons";
import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { adminCancelAndRefundBooking, getAdminBookings } from "../../api/adminApi";
import AdminLayout from "../../components/AdminLayout";
import type { AdminBooking } from "../../types/admin";

function AdminBookingsPage() {
  const [bookings, setBookings] = useState<AdminBooking[]>([]);
  const [query, setQuery] = useState("");
  const [bookingToCancel, setBookingToCancel] = useState<AdminBooking | null>(null);
  const [reason, setReason] = useState("Train service cancelled");

  useEffect(() => {
    getAdminBookings(query).then(setBookings);
  }, [query]);

  const refundedTotal = bookings
    .filter((booking) => booking.bookingStatus === "Refunded")
    .reduce((sum, booking) => sum + booking.amount, 0);

  async function handleCancelBooking(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!bookingToCancel) return;

    const updated = await adminCancelAndRefundBooking(bookingToCancel.id, reason);
    setBookings((current) => current.map((booking) => booking.id === updated.id ? updated : booking));
    setBookingToCancel(null);
    setReason("Train service cancelled");
  }

  return (
    <AdminLayout>
      <section className="admin-page-heading">
        <div>
          <h1><CheckCircleOutlined /> Manage Bookings</h1>
          <p>Review reservations and cancel bookings with a full refund when service cannot run.</p>
        </div>
      </section>

      <section className="admin-stat-grid pricing-stat-grid">
        <article className="admin-stat-card"><span className="stat-blue"><CheckCircleOutlined /></span><div><small>Confirmed</small><strong>{bookings.filter((booking) => booking.bookingStatus === "Confirmed").length}</strong></div></article>
        <article className="admin-stat-card"><span className="stat-orange"><CloseCircleOutlined /></span><div><small>Refunded</small><strong>{bookings.filter((booking) => booking.bookingStatus === "Refunded").length}</strong></div></article>
        <article className="admin-stat-card"><span className="stat-purple"><CheckCircleOutlined /></span><div><small>Refund total</small><strong>{refundedTotal.toFixed(2)} PLN</strong></div></article>
      </section>

      <section className="admin-filter-bar">
        <SearchOutlined />
        <input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Search by booking, passenger, route, or train..." />
      </section>

      {bookingToCancel && (
        <form className="admin-editor-panel admin-danger-panel" onSubmit={handleCancelBooking}>
          <h2>Cancel booking and issue full refund</h2>
          <p className="admin-editor-wide">
            Booking <strong>{bookingToCancel.bookingReference}</strong> will be cancelled and refunded <strong>{bookingToCancel.amount.toFixed(2)} PLN</strong>.
          </p>
          <label className="admin-editor-wide">Cancellation reason
            <select value={reason} onChange={(event) => setReason(event.target.value)}>
              <option>Train service cancelled</option>
              <option>Natural disaster or unsafe travel conditions</option>
              <option>Track closure</option>
              <option>Rolling stock failure</option>
              <option>Operational emergency</option>
            </select>
          </label>
          <div className="admin-form-actions">
            <button type="submit" className="admin-primary-button">Confirm full refund</button>
            <button type="button" className="admin-secondary-button" onClick={() => setBookingToCancel(null)}>Keep booking</button>
          </div>
        </form>
      )}

      <section className="admin-table-card">
        <table className="admin-table">
          <thead><tr><th>Booking</th><th>Passenger</th><th>Trip</th><th>Seat</th><th>Amount</th><th>Status</th><th>Actions</th></tr></thead>
          <tbody>
            {bookings.map((booking) => (
              <tr key={booking.id}>
                <td><strong>{booking.bookingReference}</strong><small>{booking.ticketNumber}</small></td>
                <td>{booking.passengerName || "Passenger"}<small>{booking.guestEmail}</small></td>
                <td>{booking.route}<small>{booking.trainName}</small></td>
                <td>{booking.seatLabel}</td>
                <td>{booking.amount.toFixed(2)} PLN</td>
                <td>
                  <span className={booking.bookingStatus === "Confirmed" ? "status-pill status-active" : "status-pill status-cancelled"}>{booking.bookingStatus}</span>
                  {booking.cancellationReason && <small>Reason: {booking.cancellationReason}</small>}
                </td>
                <td>
                  <button type="button" disabled={booking.bookingStatus !== "Confirmed"} onClick={() => setBookingToCancel(booking)} aria-label={`Cancel ${booking.bookingReference}`}><DeleteOutlined /></button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </AdminLayout>
  );
}

export default AdminBookingsPage;
