import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { getGuestTickets, refundGuestTicket } from "../api/bookingApi";
import { downloadOrderTicketPdf, downloadTicketPdf } from "../api/ticketApi";
import type { Booking } from "../types/booking";
import { groupTicketsByJourney, groupTicketsByOrder, isPastTicket, isReturnedTicket, type TicketGroup } from "../utils/ticketGrouping";

type GuestTicketTab = "Tickets" | "Travel history" | "Returned";

function MyBookingsPage() {
  const [searchParams] = useSearchParams();
  const email = searchParams.get("email") || "nguyentrongminhkhoa@gmail.com";
  const [tickets, setTickets] = useState<Booking[]>([]);
  const [activeTab, setActiveTab] = useState<GuestTicketTab>("Tickets");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");
  const [refundMessage, setRefundMessage] = useState("");
  const [downloadMessage, setDownloadMessage] = useState("");
  const [isDownloadingKey, setIsDownloadingKey] = useState("");
  const visibleTickets = tickets.filter((ticket) => {
    if (activeTab === "Returned") {
      return isReturnedTicket(ticket);
    }

    if (activeTab === "Travel history") {
      return !isReturnedTicket(ticket) && isPastTicket(ticket);
    }

    return !isReturnedTicket(ticket) && !isPastTicket(ticket);
  });
  const ticketGroups = groupTicketsByOrder(visibleTickets);

  useEffect(() => {
    setIsLoading(true);
    setError("");

    getGuestTickets(email)
      .then((guestTickets) => {
        setTickets(guestTickets);
      })
      .catch(() => {
        setError("Could not load guest tickets. Check that the API is running and this email has a paid ticket.");
      })
      .finally(() => setIsLoading(false));
  }, [email]);

  async function handleRefund(ticket: Booking) {
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
      setActiveTab("Returned");
      setRefundMessage("Your refund request is saved for this guest ticket. The ticket now appears under Returned.");
    } catch {
      setError("Could not refund this ticket. It may be too close to departure or already refunded.");
    }
  }

  async function handleDownloadPdf(group: TicketGroup) {
    const ticket = group.tickets[0];
    if (!ticket) {
      return;
    }

    setDownloadMessage("");
    setError("");
    setIsDownloadingKey(group.key);

    try {
      if (group.isOrder && group.orderId) {
        await downloadOrderTicketPdf(group.orderId, email);
      } else {
        await downloadTicketPdf(ticket.id, email, ticket.ticketNumber || ticket.bookingReference);
      }
    } catch {
      setDownloadMessage("Could not download the ticket PDF. The ticket may not be confirmed yet.");
    } finally {
      setIsDownloadingKey("");
    }
  }

  function formatTicketDate(value?: string) {
    if (!value) {
      return "19.06.2026";
    }

    return new Intl.DateTimeFormat("en-GB").format(new Date(value));
  }

  function formatTicketTime(value?: string | null) {
    if (!value) {
      return "--:--";
    }

    return new Intl.DateTimeFormat("en", {
      hour: "2-digit",
      minute: "2-digit",
    }).format(new Date(value));
  }

  function formatMoney(value: number, currency = "PLN") {
    return `${value.toLocaleString("pl-PL", { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ${currency}`;
  }

  function getRefundButtonLabel(ticket: Booking) {
    if (isReturnedTicket(ticket)) {
      return "Refund requested";
    }

    const passenger = ticket.passengerName || ticket.ticketNumber || "ticket";
    if (ticket.refundEligible) {
      return `Refund ${passenger} - ${formatMoney(ticket.refundableAmount, ticket.currency)}`;
    }

    return `Refund ${passenger}`;
  }

  function formatTicketExtras(ticket: Booking) {
    const extras = [];
    if (ticket.dogTicketCount > 0) {
      extras.push(`${ticket.dogTicketCount}x dog`);
    }
    if (ticket.largeBaggageTicketCount > 0) {
      extras.push(`${ticket.largeBaggageTicketCount}x large baggage`);
    }

    return extras.length > 0 ? ` - ${extras.join(", ")}` : "";
  }

  return (
    <main className="tickets-page">
      <section className="connection-hero" aria-hidden="true">
        <div className="connection-train" />
      </section>

      <p className="previous-system-note">
        Account services: invoices, refunds, and passenger data changes are available from this page.
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
            <button className="account-menu-button">Manage your ticket - Guest account</button>
            <Link className="account-menu-button" to="/help">Help</Link>
          </aside>

          <section className="ticket-dashboard">
            <h2>My tickets</h2>

            <div className="ticket-tabs" role="tablist" aria-label="Ticket sections">
              {["Tickets", "Travel history", "Returned"].map((tab) => (
                <button
                  className={activeTab === tab ? "ticket-tab ticket-tab-active" : "ticket-tab"}
                  key={tab}
                  onClick={() => setActiveTab(tab as GuestTicketTab)}
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
            {!isLoading && !error && ticketGroups.length === 0 && (
              <div className="status-message">No tickets found for this guest email yet.</div>
            )}

            {ticketGroups.map((group) => {
              const firstTicket = group.tickets[0];
              const isRefunded = group.tickets.every(isReturnedTicket);
              return (
            <article className={isRefunded ? "guest-ticket-card ticket-card-returned" : "guest-ticket-card"} key={group.key}>
              <div className="ticket-card-header">
                <p className="ticket-number-line">
                  <span>{group.isOrder ? "Order" : "Ticket number"}</span>{" "}
                  <strong>{group.isOrder ? `Order #${group.orderId}` : firstTicket.ticketNumber || firstTicket.bookingReference}</strong>
                  <small>{group.tickets.length} {group.tickets.length === 1 ? "ticket" : "tickets"}</small>
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
                      <strong>{formatTicketDate(firstTicket.travelDate)}</strong>
                    </div>
                    <div>
                      <span>Travel time</span>
                      <strong>{formatTicketTime(firstTicket.departureTime)} <b>&gt;</b> {formatTicketTime(firstTicket.arrivalTime)}</strong>
                    </div>
                    <div>
                      <span>Total</span>
                      <strong>{formatMoney(group.totalAmount, group.currency)}</strong>
                    </div>
                  </div>

                  <div className="ticket-passenger-list">
                    {groupTicketsByJourney(group.tickets).map((journey) => (
                      <section className="ticket-journey-group" key={journey.direction}>
                        <div className="ticket-route-row">
                          <span>{journey.direction}</span>
                          <strong>{buildJourneyRoute(journey.tickets)}</strong>
                          <em>{buildJourneyTrains(journey.tickets)}</em>
                        </div>
                        {journey.tickets.map((passengerTicket) => (
                          <div className="ticket-price-row" key={passengerTicket.id}>
                            <span>
                              <b>{passengerTicket.passengerName || "Passenger"}</b>
                              {passengerTicket.seatLabel || `Seat ${passengerTicket.seatId}`} - {passengerTicket.discountName || "Normal Ticket"}{formatTicketExtras(passengerTicket)}
                            </span>
                            <span>
                              <b>Price</b>
                              {formatMoney(passengerTicket.amount, passengerTicket.currency)}
                            </span>
                          </div>
                        ))}
                      </section>
                    ))}
                  </div>
                </div>

                <aside className="ticket-feature-column">
                  <button type="button" onClick={() => handleDownloadPdf(group)} disabled={isDownloadingKey === group.key || isRefunded}>
                    {isDownloadingKey === group.key ? "Downloading..." : group.isOrder ? "Download order PDF" : "Download PDF"}
                  </button>
                  <strong>{group.isOrder ? "Other features for this order" : "Other features for this ticket"}</strong>
                  <button type="button">Purchase return ticket</button>
                  {group.tickets.map((passengerTicket) => (
                    <div className="ticket-refund-option" key={passengerTicket.id}>
                      {activeTab === "Tickets" && (
                        <p className={passengerTicket.refundEligible ? "ticket-refund-policy" : "ticket-refund-policy ticket-refund-policy-closed"}>
                          {passengerTicket.refundEligible
                            ? `${formatMoney(passengerTicket.refundableAmount, passengerTicket.currency)} refundable. ${passengerTicket.refundPolicyMessage}`
                            : passengerTicket.refundPolicyMessage}
                        </p>
                      )}
                      <button
                        className={passengerTicket.refundEligible ? "refund-button" : "refund-button refund-button-refunded"}
                        type="button"
                        onClick={() => handleRefund(passengerTicket)}
                        disabled={isReturnedTicket(passengerTicket) || !passengerTicket.refundEligible}
                        title={passengerTicket.refundPolicyMessage}
                      >
                        {getRefundButtonLabel(passengerTicket)}
                      </button>
                    </div>
                  ))}
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
              {downloadMessage && (
                <div className="refund-status" role="status">
                  {downloadMessage}
                </div>
              )}
            </article>
              );
            })}

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

function buildJourneyRoute(tickets: Booking[]) {
  const first = tickets[0];
  const last = tickets[tickets.length - 1];
  if (!first || !last) {
    return "Selected route";
  }

  const start = first.route.split(" -> ")[0];
  const end = last.route.split(" -> ")[1];
  return start && end ? `${start} -> ${end}` : first.route || "Selected route";
}

function buildJourneyTrains(tickets: Booking[]) {
  const trains = [...new Set(tickets.map((ticket) => ticket.trainName).filter(Boolean))];
  return trains.length > 0 ? trains.join(" + ") : "Train";
}

export default MyBookingsPage;

