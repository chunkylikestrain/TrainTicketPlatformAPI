import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import axios from "axios";
import { getCurrentUser } from "../api/authApi";
import { getMyTickets, refundMyTicket } from "../api/bookingApi";
import { clearAuthSession, getProfileDisplayName, saveCurrentUser } from "../api/authSession";
import { downloadOrderTicketPdf, downloadTicketPdf } from "../api/ticketApi";
import type { CurrentUser } from "../types/auth";
import type { Booking } from "../types/booking";
import { getDisruptionMessage, getDisruptionSeverity, hasDisruption } from "../utils/disruptions";
import { groupTicketsByOrder, type TicketGroup } from "../utils/ticketGrouping";

const accountMenuItems = [
  "My tickets",
  "My invoices",
  "My data",
  "\"My IC\" Program",
  "My shopping profiles",
  "Settings",
  "Useful links",
];

const ticketSections = [
  { key: "tickets", label: "Tickets", empty: "You currently have no active tickets. Proceed to purchase tickets." },
  { key: "season", label: "Season tickets", empty: "You currently have no season tickets." },
  { key: "history", label: "Travel history", empty: "Your completed trips will appear here after arrival." },
  { key: "returned", label: "Returned", empty: "Your returned tickets will appear here." },
] as const;

type TicketSectionKey = typeof ticketSections[number]["key"];

function LegalFooter() {
  return (
    <section className="summary-legal profile-legal">
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
          The prices presented are <strong>indicative</strong>, published for informational purposes, and do not
          constitute an offer. The final prices are available in the purchase summary.
        </p>
        <p>
          <strong>
            The Controller of personal data provided in connection with voluntary registration on this service
            has its registered office in Warsaw.
          </strong>{" "}
          v
        </p>
      </div>
    </section>
  );
}

function MyProfilePage() {
  const [searchParams] = useSearchParams();
  const [currentUser, setCurrentUser] = useState<CurrentUser | null>(null);
  const [displayName, setDisplayName] = useState("");
  const [tickets, setTickets] = useState<Booking[]>([]);
  const [ticketError, setTicketError] = useState("");
  const [activeTicketSection, setActiveTicketSection] = useState<TicketSectionKey>("tickets");
  const [isTicketsLoading, setIsTicketsLoading] = useState(false);
  const [isDownloadingTicketId, setIsDownloadingTicketId] = useState<number | null>(null);
  const [isReturningTicketId, setIsReturningTicketId] = useState<number | null>(null);
  const [isLoading, setIsLoading] = useState(Boolean(localStorage.getItem("authToken")));
  const [notice, setNotice] = useState(
    searchParams.get("loggedIn") === "true" ? "You are logged in." : ""
  );

  useEffect(() => {
    const token = localStorage.getItem("authToken");

    if (!token) {
      setIsLoading(false);
      return;
    }

    getCurrentUser()
      .then((user) => {
        setCurrentUser(user);
        setDisplayName(getProfileDisplayName(user.email));
        saveCurrentUser(user);
      })
      .catch((profileError) => {
        if (axios.isAxiosError(profileError) && !profileError.response) {
          setNotice("The API is unavailable right now. Your browser still has your login token.");
          return;
        }
        setCurrentUser(null);
        clearAuthSession();
        setNotice("Your session expired. Please log in again.");
      })
      .finally(() => {
        setIsLoading(false);
      });
  }, []);

  useEffect(() => {
    if (!currentUser) {
      setTickets([]);
      return;
    }

    if (activeTicketSection === "season") {
      setTickets([]);
      setTicketError("");
      setIsTicketsLoading(false);
      return;
    }

    setIsTicketsLoading(true);
    setTicketError("");

    getMyTickets(activeTicketSection)
      .then(setTickets)
      .catch(() => setTicketError("Could not load your tickets. Try refreshing after the API is running."))
      .finally(() => setIsTicketsLoading(false));
  }, [activeTicketSection, currentUser]);

  function handleLogout() {
    clearAuthSession();
    setCurrentUser(null);
    setDisplayName("");
    setNotice("You have been logged out.");
  }

  async function handleDownloadPdf(ticket: Booking) {
    setTicketError("");
    setIsDownloadingTicketId(ticket.id);

    try {
      await downloadTicketPdf(ticket.id, undefined, ticket.ticketNumber || ticket.bookingReference);
    } catch {
      setTicketError("Could not download this ticket PDF.");
    } finally {
      setIsDownloadingTicketId(null);
    }
  }

  async function handleDownloadGroupPdf(group: TicketGroup) {
    const firstTicket = group.tickets[0];
    if (!firstTicket) {
      return;
    }

    setTicketError("");
    setIsDownloadingTicketId(firstTicket.id);

    try {
      if (group.isOrder && group.orderId) {
        await downloadOrderTicketPdf(group.orderId);
      } else {
        await downloadTicketPdf(firstTicket.id, undefined, firstTicket.ticketNumber || firstTicket.bookingReference);
      }
    } catch {
      setTicketError("Could not download this ticket PDF.");
    } finally {
      setIsDownloadingTicketId(null);
    }
  }

  async function handleReturnTicket(ticket: Booking) {
    setTicketError("");
    setIsReturningTicketId(ticket.id);

    try {
      await refundMyTicket(ticket.id);
      setActiveTicketSection("returned");
    } catch (returnError) {
      if (axios.isAxiosError(returnError) && typeof returnError.response?.data === "string") {
        setTicketError(returnError.response.data);
      } else {
        setTicketError("Could not return this ticket.");
      }
    } finally {
      setIsReturningTicketId(null);
    }
  }

  function formatTicketDate(value?: string) {
    if (!value) {
      return "Selected date";
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

  function formatMoney(value: number) {
    return `${value.toLocaleString("pl-PL", { minimumFractionDigits: 2, maximumFractionDigits: 2 })} PLN`;
  }

  const ticketGroups = groupTicketsByOrder(tickets);

  return (
    <main className="tickets-page profile-page">
      <section className="connection-hero" aria-hidden="true">
        <div className="connection-train" />
      </section>

      <p className="previous-system-note">
        Account services: invoices, refunds, and passenger data changes are available from your profile.
      </p>

      <section className="tickets-content">
        <h1>My account</h1>

        <div className="guest-account-banner profile-banner">
          <span aria-hidden="true">ID</span>
          {currentUser ? <strong>{displayName}</strong> : <strong>{isLoading ? "Checking session..." : "Not logged in"}</strong>}
          {currentUser && (
            <button className="profile-logout" type="button" onClick={handleLogout}>
              log out
            </button>
          )}
        </div>

        {notice && <div className="profile-notice">{notice}</div>}

        <div className="account-layout">
          <aside className="account-sidebar" aria-label="Account menu">
            {accountMenuItems.map((item, index) => (
              <button
                className={
                  currentUser && index === 0
                    ? "account-menu-button account-menu-button-active"
                    : currentUser
                      ? "account-menu-button"
                      : "account-menu-button account-menu-button-muted"
                }
                key={item}
                type="button"
              >
                {item}
              </button>
            ))}
          </aside>

          {currentUser ? (
            <section className="ticket-dashboard profile-dashboard">
              <h2>My tickets</h2>
              <div className="ticket-tabs" role="tablist" aria-label="Ticket sections">
                {ticketSections.map((section) => (
                  <button
                    className={activeTicketSection === section.key ? "ticket-tab ticket-tab-active" : "ticket-tab"}
                    key={section.key}
                    onClick={() => setActiveTicketSection(section.key)}
                    type="button"
                  >
                    {section.label}
                  </button>
                ))}
              </div>

              <label className="ticket-search">
                <span>Search for a ticket</span>
                <input aria-label="Search for a ticket" />
                <b aria-hidden="true">Search</b>
              </label>

              {isTicketsLoading && <div className="status-message">Loading your tickets...</div>}
              {ticketError && <div className="status-message">{ticketError}</div>}

              {!isTicketsLoading && ticketGroups.length === 0 ? (
                <section className="profile-empty-tickets">
                  <div className="profile-empty-icon" aria-hidden="true" />
                  <p>{ticketSections.find((section) => section.key === activeTicketSection)?.empty}</p>
                  {activeTicketSection === "tickets" && <Link to="/">Buy a ticket</Link>}
                </section>
              ) : (
                ticketGroups.map((group) => {
                  const firstTicket = group.tickets[0];
                  return (
                  <article className="guest-ticket-card" key={group.key}>
                    <div className="ticket-card-header">
                      <p className="ticket-number-line">
                        <span>{group.isOrder ? "Order" : "Ticket number"}</span>{" "}
                        <strong>{group.isOrder ? `Order #${group.orderId}` : firstTicket.ticketNumber || firstTicket.bookingReference}</strong>
                        <small>
                          {group.tickets.length} {group.tickets.length === 1 ? "ticket" : "tickets"}
                          {!group.isOrder && <> · Booking: <b>{firstTicket.bookingReference}</b></>}
                        </small>
                      </p>
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
                            <span>{group.isOrder ? "Passengers" : "Seat"}</span>
                            <strong>{group.isOrder ? group.tickets.length : firstTicket.seatLabel || `Seat ${firstTicket.seatId}`}</strong>
                          </div>
                          <div>
                            <span>Status</span>
                            <strong>{firstTicket.bookingStatus}</strong>
                          </div>
                        </div>

                        {activeTicketSection === "tickets" && hasDisruption(firstTicket) && (
                          <div className={`ticket-disruption-banner disruption-${getDisruptionSeverity(firstTicket) || "notice"}`}>
                            <strong>Service update</strong>
                            <span>{getDisruptionMessage(firstTicket)}</span>
                          </div>
                        )}

                        <div className="ticket-route-row">
                          <span>Route</span>
                          <strong>{firstTicket.route || "Selected route"}</strong>
                          <em>{firstTicket.trainName || "Train"}</em>
                        </div>

                        <div className="ticket-passenger-list">
                        {group.tickets.map((ticket) => (
                        <div className="ticket-price-row" key={ticket.id}>
                          <span>
                            <b>{ticket.passengerName || currentUser.email}</b>
                            {ticket.seatLabel || `Seat ${ticket.seatId}`} · {ticket.discountName || "Normal Ticket"}
                          </span>
                          <span>
                            <b>Price</b>
                            {formatMoney(ticket.amount)}
                          </span>
                        </div>
                        ))}
                        {group.isOrder && (
                          <div className="ticket-order-total">
                            <span>Order total</span>
                            <strong>{formatMoney(group.totalAmount)}</strong>
                          </div>
                        )}
                        </div>
                      </div>

                      <aside className="ticket-feature-column">
                        <button
                          type="button"
                          onClick={() => group.isOrder ? handleDownloadGroupPdf(group) : handleDownloadPdf(firstTicket)}
                          disabled={
                            isDownloadingTicketId === firstTicket.id ||
                            group.tickets.some((ticket) => ticket.bookingStatus !== "Confirmed")
                          }
                        >
                          {isDownloadingTicketId === firstTicket.id ? "Downloading..." : group.isOrder ? "Download order PDF" : "Download PDF"}
                        </button>
                        <strong>{group.isOrder ? "Other features for this order" : "Other features for this ticket"}</strong>
                        <button type="button" disabled>Purchase return ticket</button>
                        {group.tickets.map((ticket) => (
                        <button
                          type="button"
                          onClick={() => handleReturnTicket(ticket)}
                          disabled={
                            activeTicketSection !== "tickets" ||
                            isReturningTicketId === ticket.id ||
                            ticket.bookingStatus !== "Confirmed"
                          }
                          key={ticket.id}
                        >
                          {isReturningTicketId === ticket.id ? "Returning..." : `Refund ${ticket.passengerName || ticket.ticketNumber || "ticket"}`}
                        </button>
                        ))}
                        <button type="button" disabled>Exchange</button>
                        <button type="button" disabled>Change data</button>
                      </aside>
                    </div>
                  </article>
                  );
                })
              )}
            </section>
          ) : isLoading ? (
            <section className="profile-signin-panel profile-loading-panel" aria-live="polite">
              <h2>Checking your session</h2>
              <p>One moment while we confirm your account.</p>
            </section>
          ) : (
            <section className="profile-signin-panel">
              <h2>Sign in</h2>
              <Link className="account-primary-action" to="/login">
                Log in
              </Link>
              <Link className="account-secondary-action" to="/register">
                Register
              </Link>
              <Link className="guest-link profile-guest-link" to="/">
                Continue as Guest
              </Link>
            </section>
          )}
        </div>
      </section>

      <LegalFooter />
    </main>
  );
}

export default MyProfilePage;
