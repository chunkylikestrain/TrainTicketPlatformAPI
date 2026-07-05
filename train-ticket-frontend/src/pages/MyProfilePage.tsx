import { Fragment, useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import axios from "axios";
import PassengerLegalFooter from "../components/PassengerLegalFooter";
import { getCurrentUser } from "../api/authApi";
import { getMyTickets, refundMyTicket } from "../api/bookingApi";
import { clearAuthSession, getProfileDisplayName, saveCurrentUser } from "../api/authSession";
import { downloadInvoicePdf, getMyInvoices } from "../api/invoiceApi";
import { getMyLoyaltyAccount, getMyLoyaltyTransactions } from "../api/loyaltyApi";
import { downloadOrderTicketPdf, downloadTicketPdf } from "../api/ticketApi";
import type { CurrentUser } from "../types/auth";
import type { Booking } from "../types/booking";
import type { Invoice } from "../types/invoice";
import type { LoyaltyAccount, LoyaltyTransaction } from "../types/loyalty";
import { getDisruptionMessage, getDisruptionSeverity, hasDisruption } from "../utils/disruptions";
import { groupTicketsByJourney, groupTicketsByOrder, type TicketGroup } from "../utils/ticketGrouping";

const accountMenuItems = [
  { key: "tickets", label: "My tickets" },
  { key: "invoices", label: "My invoices" },
  { key: "data", label: "My data" },
  { key: "myic", label: "\"My IC\" Program" },
  { key: "help", label: "Help" },
] as const;

const ticketSections = [
  { key: "tickets", label: "Tickets", empty: "You currently have no active tickets. Proceed to purchase tickets." },
  { key: "history", label: "Travel history", empty: "Your completed trips will appear here after arrival." },
  { key: "returned", label: "Returned", empty: "Your returned tickets will appear here." },
] as const;

type TicketSectionKey = typeof ticketSections[number]["key"];
type AccountSectionKey = typeof accountMenuItems[number]["key"];

function MyProfilePage() {
  const [searchParams] = useSearchParams();
  const [currentUser, setCurrentUser] = useState<CurrentUser | null>(null);
  const [displayName, setDisplayName] = useState("");
  const [tickets, setTickets] = useState<Booking[]>([]);
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [loyaltyAccount, setLoyaltyAccount] = useState<LoyaltyAccount | null>(null);
  const [loyaltyTransactions, setLoyaltyTransactions] = useState<LoyaltyTransaction[]>([]);
  const [ticketError, setTicketError] = useState("");
  const [invoiceError, setInvoiceError] = useState("");
  const [loyaltyError, setLoyaltyError] = useState("");
  const [activeAccountSection, setActiveAccountSection] = useState<AccountSectionKey>("tickets");
  const [activeTicketSection, setActiveTicketSection] = useState<TicketSectionKey>("tickets");
  const [isTicketsLoading, setIsTicketsLoading] = useState(false);
  const [isInvoicesLoading, setIsInvoicesLoading] = useState(false);
  const [isLoyaltyLoading, setIsLoyaltyLoading] = useState(false);
  const [isDownloadingTicketId, setIsDownloadingTicketId] = useState<number | null>(null);
  const [isDownloadingInvoiceId, setIsDownloadingInvoiceId] = useState<number | null>(null);
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
    if (!currentUser || activeAccountSection !== "tickets") {
      setTickets([]);
      return;
    }

    setIsTicketsLoading(true);
    setTicketError("");

    getMyTickets(activeTicketSection)
      .then(setTickets)
      .catch(() => setTicketError("Could not load your tickets. Try refreshing after the API is running."))
      .finally(() => setIsTicketsLoading(false));
  }, [activeAccountSection, activeTicketSection, currentUser]);

  useEffect(() => {
    if (!currentUser || activeAccountSection !== "myic") {
      return;
    }

    setIsLoyaltyLoading(true);
    setLoyaltyError("");

    Promise.all([getMyLoyaltyAccount(), getMyLoyaltyTransactions()])
      .then(([account, transactions]) => {
        setLoyaltyAccount(account);
        setLoyaltyTransactions(transactions);
      })
      .catch(() => setLoyaltyError("Could not load My IC points. Try refreshing after the API is running."))
      .finally(() => setIsLoyaltyLoading(false));
  }, [activeAccountSection, currentUser]);

  useEffect(() => {
    if (!currentUser || activeAccountSection !== "invoices") {
      setInvoices([]);
      return;
    }

    setIsInvoicesLoading(true);
    setInvoiceError("");

    getMyInvoices()
      .then(setInvoices)
      .catch(() => setInvoiceError("Could not load your invoices. Try refreshing after the API is running."))
      .finally(() => setIsInvoicesLoading(false));
  }, [activeAccountSection, currentUser]);

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

  function formatMoney(value: number, currency = "PLN") {
    return `${value.toLocaleString("pl-PL", { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ${currency}`;
  }

  function getRefundButtonLabel(ticket: Booking) {
    if (ticket.bookingStatus === "Refunded" || ticket.paymentStatus === "Refunded") {
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

  async function handleDownloadInvoice(invoice: Invoice) {
    setInvoiceError("");
    setIsDownloadingInvoiceId(invoice.id);

    try {
      await downloadInvoicePdf(invoice);
    } catch {
      setInvoiceError("Could not download this invoice PDF.");
    } finally {
      setIsDownloadingInvoiceId(null);
    }
  }

  const ticketGroups = groupTicketsByOrder(tickets);
  const loyaltyRows = buildLoyaltyRows(loyaltyTransactions);
  const redeemablePoints = loyaltyAccount?.redeemablePoints ?? 0;
  const pendingPoints = loyaltyAccount?.pendingPoints ?? 0;
  const expiringPoints = loyaltyAccount?.expiringPoints ?? 0;
  const savedValue = loyaltyAccount?.redeemableValuePln ?? 0;
  const earnRate = loyaltyAccount?.earnRatePointsPerPln ?? 5;
  const redeemRate = loyaltyAccount?.redeemRatePointsPerPln ?? 100;
  const profileNameParts = getProfileNameParts(displayName, currentUser?.email);

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
            {accountMenuItems.map((item) => (
              item.key === "help" ? (
                <Link className="account-menu-button" key={item.key} to="/help">
                  {item.label}
                </Link>
              ) : (
                <button
                  className={
                    currentUser && activeAccountSection === item.key
                      ? "account-menu-button account-menu-button-active"
                      : currentUser
                        ? "account-menu-button"
                        : "account-menu-button account-menu-button-muted"
                  }
                  key={item.key}
                  onClick={() => currentUser && setActiveAccountSection(item.key)}
                  type="button"
                >
                  {item.label}
                </button>
              )
            ))}
          </aside>

          {currentUser && activeAccountSection === "tickets" ? (
            <section className="ticket-dashboard profile-dashboard">
              <h2>My tickets</h2>
              <div className="ticket-tabs" role="tablist" aria-label="Ticket sections">
                {ticketSections.map((section) => (
                  section.key === "history" ? (
                    <Fragment key={section.key}>
                      <span className="ticket-tab ticket-tab-unavailable">Season tickets</span>
                      <button
                        className={activeTicketSection === section.key ? "ticket-tab ticket-tab-active" : "ticket-tab"}
                        onClick={() => setActiveTicketSection(section.key)}
                        type="button"
                      >
                        {section.label}
                      </button>
                    </Fragment>
                  ) : (
                    <button
                      className={activeTicketSection === section.key ? "ticket-tab ticket-tab-active" : "ticket-tab"}
                      key={section.key}
                      onClick={() => setActiveTicketSection(section.key)}
                      type="button"
                    >
                      {section.label}
                    </button>
                  )
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
                          {!group.isOrder && <> - Booking: <b>{firstTicket.bookingReference}</b></>}
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

                        <div className="ticket-passenger-list">
                        {groupTicketsByJourney(group.tickets).map((journey) => (
                          <section className="ticket-journey-group" key={journey.direction}>
                            <div className="ticket-route-row">
                              <span>{journey.direction}</span>
                              <strong>{buildJourneyRoute(journey.tickets)}</strong>
                              <em>{buildJourneyTrains(journey.tickets)}</em>
                            </div>
                            {journey.tickets.map((ticket) => (
                            <div className="ticket-price-row" key={ticket.id}>
                              <span>
                                <b>{ticket.passengerName || currentUser.email}</b>
                                {ticket.seatLabel || `Seat ${ticket.seatId}`} - {ticket.discountName || "Normal Ticket"}{formatTicketExtras(ticket)}
                              </span>
                              <span>
                                <b>Price</b>
                                {formatMoney(ticket.amount)}
                              </span>
                            </div>
                            ))}
                          </section>
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
                        {activeTicketSection === "tickets" && (
                          <Link className="ticket-trip-link" to={`/trip/${firstTicket.id}`}>
                            Current trip
                          </Link>
                        )}
                        <Link className="ticket-feature-link" to={buildReturnTicketSearchUrl(firstTicket)}>
                          Purchase return ticket
                        </Link>
                        {group.tickets.map((ticket) => (
                          <div className="ticket-refund-option" key={ticket.id}>
                            {activeTicketSection === "tickets" && (
                              <p className={ticket.refundEligible ? "ticket-refund-policy" : "ticket-refund-policy ticket-refund-policy-closed"}>
                                {ticket.refundEligible
                                  ? `${formatMoney(ticket.refundableAmount, ticket.currency)} refundable. ${ticket.refundPolicyMessage}`
                                  : ticket.refundPolicyMessage}
                              </p>
                            )}
                            <button
                              className={ticket.refundEligible ? "refund-button" : "refund-button refund-button-refunded"}
                              type="button"
                              onClick={() => handleReturnTicket(ticket)}
                              disabled={
                                activeTicketSection !== "tickets" ||
                                isReturningTicketId === ticket.id ||
                                ticket.bookingStatus !== "Confirmed" ||
                                !ticket.refundEligible
                              }
                              title={ticket.refundPolicyMessage}
                            >
                              {isReturningTicketId === ticket.id ? "Returning..." : getRefundButtonLabel(ticket)}
                            </button>
                          </div>
                        ))}
                      </aside>
                    </div>
                  </article>
                  );
                })
              )}
            </section>
          ) : currentUser && activeAccountSection === "myic" ? (
            <section className="ticket-dashboard my-ic-dashboard">
              <h2>"My IC" Program</h2>

              <section className="my-ic-hero">
                <div className="my-ic-points-total">
                  <span aria-hidden="true">IC</span>
                  <strong>{redeemablePoints.toLocaleString("en-GB")} points</strong>
                  <small>Points to redeem</small>
                </div>

                <div className="my-ic-stat-grid">
                  <article>
                    <strong>+ {pendingPoints.toLocaleString("en-GB")} points</strong>
                    <span>Waiting</span>
                  </article>
                  <article>
                    <strong>{expiringPoints.toLocaleString("en-GB")} points</strong>
                    <span>Expiring</span>
                  </article>
                </div>
              </section>

              <section className="my-ic-savings">
                <strong>{formatMoney(savedValue)}</strong>
                <span>Available ticket value from your points</span>
              </section>

              <section className="my-ic-history">
                <h3>Transaction history</h3>
                {isLoyaltyLoading && <div className="status-message">Loading My IC transactions...</div>}
                {loyaltyError && <div className="status-message">{loyaltyError}</div>}
                {!isLoyaltyLoading && !loyaltyError && loyaltyRows.length === 0 ? (
                  <div className="status-message">Buy a confirmed ticket to start earning My IC points.</div>
                ) : (
                  <div className="my-ic-table" role="table" aria-label="My IC transaction history">
                    <div className="my-ic-table-head" role="row">
                      <span>Type</span>
                      <span>Ticket no.</span>
                      <span>Transaction date</span>
                      <span>Status</span>
                      <span>Valid</span>
                      <span>Number of points</span>
                    </div>
                    {loyaltyRows.map((row) => (
                      <div className="my-ic-table-row" role="row" key={row.id}>
                        <strong>{formatLoyaltyType(row.type)}</strong>
                        <span>{row.reference || "-"}</span>
                        <span>{formatTicketDate(row.transactionDate)}</span>
                        <span>{row.status}</span>
                        <span>
                          from {formatTicketDate(row.validFrom)}
                          {row.expiresAt ? ` to ${formatTicketDate(row.expiresAt)}` : ""}
                        </span>
                        <b>{formatSignedPoints(row.points)}</b>
                      </div>
                    ))}
                  </div>
                )}
              </section>

              <details className="my-ic-faq">
                <summary>Frequently asked questions</summary>
                <p>
                  RailBook awards {earnRate.toLocaleString("en-GB")} points for every 1 PLN spent on confirmed
                  tickets. Every {redeemRate.toLocaleString("en-GB")} points can redeem 1 PLN in a future checkout step.
                </p>
              </details>
            </section>
          ) : currentUser && activeAccountSection === "invoices" ? (
            <section className="ticket-dashboard profile-invoice-dashboard">
              <h2>My invoices</h2>

              <label className="ticket-search profile-invoice-search">
                <span>Search for an invoice</span>
                <input aria-label="Search for an invoice" />
                <b aria-hidden="true">Search</b>
              </label>

              {isInvoicesLoading && <div className="status-message">Loading your invoices...</div>}
              {invoiceError && <div className="status-message">{invoiceError}</div>}

              {!isInvoicesLoading && invoices.length === 0 ? (
                <section className="profile-empty-tickets profile-empty-invoices">
                  <div className="profile-invoice-empty-icon" aria-hidden="true" />
                  <p>You currently have no invoices. Request an invoice during checkout.</p>
                  <Link to="/">Buy a ticket</Link>
                </section>
              ) : (
                <div className="profile-invoice-list">
                  {invoices.map((invoice) => (
                    <article className="profile-invoice-card" key={invoice.id}>
                      <div>
                        <span>Invoice</span>
                        <strong>{invoice.invoiceNumber}</strong>
                        <small>{formatTicketDate(invoice.issuedAtUtc)} · {invoice.status}</small>
                      </div>
                      <div>
                        <span>Buyer</span>
                        <strong>{invoice.buyerName}</strong>
                        <small>{invoice.buyerEmail}</small>
                      </div>
                      <div>
                        <span>Total</span>
                        <strong>{formatMoney(invoice.totalAmount, invoice.currency)}</strong>
                        <small>VAT {formatMoney(invoice.vatAmount, invoice.currency)}</small>
                      </div>
                      <button
                        type="button"
                        onClick={() => handleDownloadInvoice(invoice)}
                        disabled={isDownloadingInvoiceId === invoice.id}
                      >
                        {isDownloadingInvoiceId === invoice.id ? "Downloading..." : "Download PDF"}
                      </button>
                    </article>
                  ))}
                </div>
              )}
            </section>
          ) : currentUser && activeAccountSection === "data" ? (
            <section className="ticket-dashboard profile-data-dashboard">
              <h2>My data</h2>

              <div className="profile-data-list">
                <ProfileDataField label="E-mail" value={currentUser.email} />
                <ProfileDataField label="First name" value={profileNameParts.firstName} />
                <ProfileDataField label="Last name" value={profileNameParts.lastName} />
                <ProfileDataField label="Password" value="************" />
              </div>

              <section className="profile-data-section">
                <div className="profile-data-section-heading">
                  <h3>Passenger data</h3>
                  <span className="profile-unavailable-action">Add passenger</span>
                </div>

                <div className="profile-passenger-grid">
                  <article className="profile-passenger-card">
                    <span>Primary passenger</span>
                    <strong>{displayName || currentUser.email}</strong>
                    <dl>
                      <div>
                        <dt>Ticket type</dt>
                        <dd>Normal Ticket</dd>
                      </div>
                      <div>
                        <dt>Passenger type</dt>
                        <dd>Adult</dd>
                      </div>
                      <div>
                        <dt>Document</dt>
                        <dd>Not saved</dd>
                      </div>
                    </dl>
                    <span className="profile-unavailable-action">Change</span>
                  </article>

                  <article className="profile-passenger-card">
                    <span>Checkout defaults</span>
                    <strong>{currentUser.email}</strong>
                    <dl>
                      <div>
                        <dt>Ticket email</dt>
                        <dd>Account email</dd>
                      </div>
                      <div>
                        <dt>Invoice data</dt>
                        <dd>Not saved</dd>
                      </div>
                      <div>
                        <dt>Contact phone</dt>
                        <dd>{currentUser.phone || "Not saved"}</dd>
                      </div>
                    </dl>
                    <span className="profile-unavailable-action">Change</span>
                  </article>
                </div>
              </section>

              <button
                className="profile-data-link-row"
                type="button"
                onClick={() => setActiveAccountSection("myic")}
              >
                <span>"My IC" Program data</span>
                <b aria-hidden="true">&gt;</b>
              </button>

              <section className="profile-close-account">
                <h3>Close account</h3>
                <p>We delete accounts in accordance with the RailBook terms and conditions.</p>
                <span className="profile-unavailable-action">Delete your account</span>
              </section>
            </section>
          ) : currentUser ? (
            <section className="profile-signin-panel profile-loading-panel">
              <h2>{accountMenuItems.find((item) => item.key === activeAccountSection)?.label}</h2>
              <p>This profile section is ready for a future feature.</p>
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

      <PassengerLegalFooter className="summary-legal profile-legal" />
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

function buildReturnTicketSearchUrl(ticket: Booking) {
  const [departureStation, arrivalStation] = splitRoute(ticket.route);
  const searchDate = addDays(formatSearchDate(ticket.travelDate), 1);
  const params = new URLSearchParams({
    departureStation: arrivalStation,
    arrivalStation: departureStation,
    date: searchDate,
    time: "08:00",
    tripType: "oneWay",
    adults: "1",
    children: "0",
    discounts: ticket.discountCode || "normal",
  });

  return `/search?${params.toString()}`;
}

function splitRoute(route: string) {
  const [departure, arrival] = route.split(/\s*(?:->|→)\s*/);
  return [
    departure?.trim() || "Departure station",
    arrival?.trim() || "Arrival station",
  ] as const;
}

function formatSearchDate(value: string) {
  if (/^\d{4}-\d{2}-\d{2}/.test(value)) {
    return value.slice(0, 10);
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return new Date().toISOString().slice(0, 10);
  }

  return date.toISOString().slice(0, 10);
}

function addDays(dateValue: string, days: number) {
  const date = new Date(`${dateValue}T12:00:00`);
  date.setDate(date.getDate() + days);
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${date.getFullYear()}-${month}-${day}`;
}

function buildLoyaltyRows(transactions: LoyaltyTransaction[]) {
  return transactions
    .map((transaction) => ({
      id: transaction.id,
      type: transaction.type,
      reference: transaction.reference,
      transactionDate: transaction.transactionDateUtc,
      validFrom: transaction.validFromUtc,
      expiresAt: transaction.expiresAtUtc,
      status: transaction.status,
      points: transaction.points,
    }))
    .sort((first, second) => new Date(second.transactionDate).getTime() - new Date(first.transactionDate).getTime());
}

function formatLoyaltyType(type: string) {
  if (type === "TicketPurchase") {
    return (
      <>
        Ticket
        <br />
        purchase
      </>
    );
  }

  return type.replace(/([a-z])([A-Z])/g, "$1 $2");
}

function formatSignedPoints(points: number) {
  const sign = points >= 0 ? "+" : "";
  return `${sign}${points.toLocaleString("en-GB")} points`;
}

function ProfileDataField({ label, value }: { label: string; value: string }) {
  return (
    <article className="profile-data-field">
      <span>{label}</span>
      <strong>{value}</strong>
      <span className="profile-unavailable-action">Change</span>
    </article>
  );
}

function getProfileNameParts(displayName: string, email?: string) {
  const fallback = email?.split("@")[0] || "Passenger";
  const parts = (displayName || fallback).trim().split(/\s+/).filter(Boolean);

  return {
    firstName: parts[0] || fallback,
    lastName: parts.length > 1 ? parts.slice(1).join(" ") : "-",
  };
}

export default MyProfilePage;
