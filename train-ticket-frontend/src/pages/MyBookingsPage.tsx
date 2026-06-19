import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { getGuestTickets, refundGuestTicket } from "../api/bookingApi";
import type { Booking } from "../types/booking";

function MyBookingsPage() {
  const [searchParams] = useSearchParams();
  const email = searchParams.get("email") || "nguyentrongminhkhoa@gmail.com";
  const [isRefunded, setIsRefunded] = useState(false);
  const [tickets, setTickets] = useState<Booking[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");
  const [refundMessage, setRefundMessage] = useState("");
  const ticket = tickets[0];
  const activeTab = isRefunded ? "Returned" : "Tickets";

  useEffect(() => {
    setIsLoading(true);
    setError("");

    getGuestTickets(email)
      .then((guestTickets) => {
        setTickets(guestTickets);
        setIsRefunded(guestTickets.some((guestTicket) => guestTicket.bookingStatus === "Refunded"));
      })
      .catch(() => {
        setError("Could not load guest tickets. Check that the API is running and this email has a paid ticket.");
      })
      .finally(() => setIsLoading(false));
  }, [email]);

  async function handleRefund() {
    if (!ticket?.ticketNumber) {
      return;
    }

    setRefundMessage("");
    setError("");

    try {
      const refundedTicket = await refundGuestTicket(ticket.ticketNumber, email);
      setTickets((currentTickets) =>
        currentTickets.map((currentTicket) =>
          currentTicket.id === refundedTicket.id ? refundedTicket : currentTicket
        )
      );
      setIsRefunded(true);
      setRefundMessage("Your refund request is saved for this guest ticket. The ticket now appears under Returned.");
    } catch {
      setError("Could not refund this ticket. It may be too close to departure or already refunded.");
    }
  }

  function formatTicketDate(value?: string) {
    if (!value) {
      return "19.06.2026";
    }

    return new Intl.DateTimeFormat("en-GB").format(new Date(value));
  }

  return (
    <main className="tickets-page">
      <section className="connection-hero" aria-hidden="true">
        <div className="connection-train" />
      </section>

      <p className="previous-system-note">
        Previous system: <a href="#previous-system">ticket.railway.example</a> - only invoices, refunds, data
        changes <a href="#details">Details</a>
      </p>

      <section className="tickets-content">
        <h1>My account</h1>

        <div className="guest-account-banner">
          <span aria-hidden="true">ID</span>
          <strong>Guest account: {email}</strong>
        </div>

        <div className="account-layout">
          <aside className="account-sidebar" aria-label="Account menu">
            <button className="account-menu-button account-menu-button-active">My tickets</button>
            <button className="account-menu-button">My invoices</button>
            <button className="account-menu-button account-menu-button-muted">My data</button>
            <button className="account-menu-button account-menu-button-muted">"Moje IC" Program</button>
            <button className="account-menu-button account-menu-button-muted">My shopping profiles</button>
            <button className="account-menu-button account-menu-button-muted">Settings</button>
            <button className="account-menu-button">Manage your ticket - Guest account</button>
            <button className="account-menu-button">Useful links</button>
          </aside>

          <section className="ticket-dashboard">
            <h2>My tickets</h2>

            <div className="ticket-tabs" role="tablist" aria-label="Ticket sections">
              {["Tickets", "Travel history", "Returned"].map((tab) => (
                <button
                  className={activeTab === tab ? "ticket-tab ticket-tab-active" : "ticket-tab"}
                  key={tab}
                  type="button"
                >
                  {tab}
                </button>
              ))}
            </div>

            <label className="ticket-search">
              <span>Search for a ticket</span>
              <input aria-label="Search for a ticket" />
              <b aria-hidden="true">Search</b>
            </label>

            {isLoading && <div className="status-message">Loading guest tickets...</div>}
            {error && <div className="status-message">{error}</div>}
            {!isLoading && !error && !ticket && (
              <div className="status-message">No tickets found for this guest email yet.</div>
            )}

            {ticket && (
            <article className={isRefunded ? "guest-ticket-card ticket-card-returned" : "guest-ticket-card"}>
              <div className="ticket-card-header">
                <p className="ticket-number-line">
                  <span>Ticket number</span> <strong>{ticket.ticketNumber || ticket.bookingReference}</strong>
                  <small>Info Purchase: <b>EIC2</b></small>
                </p>
                <div className="ticket-tools" aria-label="Calendar and wallet options">
                  <button type="button">Cal</button>
                  <button type="button">G</button>
                </div>
              </div>

              <div className="ticket-card-body">
                <div className="ticket-trip-details">
                  <div className="ticket-meta-grid">
                    <div>
                      <span>Date</span>
                      <strong>{formatTicketDate(ticket.travelDate)}</strong>
                    </div>
                    <div>
                      <span>Travel time: 1h 33min</span>
                      <strong>06:54 <b>&gt;</b> 08:27</strong>
                    </div>
                    <div>
                      <span>Class 2</span>
                      <strong>23,03 PLN</strong>
                    </div>
                  </div>

                  <div className="ticket-route-row">
                    <span>Route</span>
                    <strong>Rzeszow Glowny</strong>
                    <b>&gt;</b>
                    <strong>Krakow Gl.</strong>
                    <em>IC 3806</em>
                  </div>

                  <div className="ticket-extra-section">
                    <h3>Additional tickets</h3>
                    <div className="extra-ticket-pill">
                      <span aria-hidden="true">Bike</span>
                      <strong>1x Bicycle</strong>
                    </div>
                    <p className="ticket-number-line secondary-ticket">
                      <span>ticket number</span> <strong>WH57810600</strong>
                    </p>
                    <div className="ticket-price-row">
                      <span>
                        <b>Ticket</b>
                        Bicycle
                      </span>
                      <span>
                        <b>Price</b>
                        9,10 PLN
                      </span>
                    </div>
                  </div>
                </div>

                <aside className="ticket-feature-column">
                  <button type="button">Download PDF</button>
                  <strong>Other features for this ticket</strong>
                  <button type="button">Purchase return ticket</button>
                    <button
                    className={isRefunded ? "refund-button refund-button-refunded" : "refund-button"}
                    type="button"
                    onClick={handleRefund}
                    disabled={isRefunded}
                  >
                    {isRefunded ? "Refund requested" : "Refund"}
                  </button>
                  <button type="button">Exchange</button>
                  <button type="button">Change data</button>
                  <button type="button">VAT invoice</button>
                </aside>
              </div>

              {(isRefunded || refundMessage) && (
                <div className="refund-status" role="status">
                  {refundMessage || "This ticket is marked as returned."}
                </div>
              )}
            </article>
            )}

            <div className="ticket-pagination" aria-label="Ticket list pagination">
              <button type="button" aria-label="Previous page">&lt;</button>
              <strong>1</strong>
              <button type="button" aria-label="Next page">&gt;</button>
            </div>
          </section>
        </div>
      </section>

      <section className="summary-legal">
        <div>
          <h2>Technological break.</h2>
          <p>
            Please remember about the technological break in the online sales system from 11:45pm - 0:30 am.
            You cannot buy any tickets during this break.
          </p>
          <p><strong>Deactivation of the e-IC 1.0 service</strong> v</p>
          <a href="#accessibility">Declaration of Accessibility</a>
        </div>
        <div>
          <p>
            The prices presented are <strong>indicative</strong>, published for informational purposes, and do
            not constitute an offer. The final prices are available in the purchase summary.
          </p>
          <p>
            <strong>
              The Controller of personal data provided in connection with voluntary registration on the service
              has its registered office in Warsaw.
            </strong>{" "}
            v
          </p>
        </div>
      </section>
    </main>
  );
}

export default MyBookingsPage;
