import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { Link, useParams, useSearchParams } from "react-router-dom";
import BookingExpiredModal from "../components/BookingExpiredModal";
import { getUserEmail, hasAuthToken } from "../api/authSession";
import { getBookingOrder, getGuestTickets, updateGuestBookingData } from "../api/bookingApi";
import { confirmPayment, createOrderPaymentIntent, createPaymentIntent } from "../api/paymentApi";
import {
  downloadTicketPdf,
  downloadOrderTicketPdf,
  getOrderTickets,
  getTicketArtifact,
  getTicketQrSvgBlob,
  sendOrderTicketsEmail,
  sendTicketEmail,
} from "../api/ticketApi";
import { getTripById } from "../api/tripApi";
import type { TicketArtifact } from "../types/ticket";
import type { TripDetails } from "../types/trip";
import {
  formatTripDate,
  formatTripPrice,
  formatTripTime,
  getTripPriceLabel,
  getTripVatLabel,
} from "../utils/tripDisplay";
import {
  buildDiscountSelectionUrl,
  formatDiscountSummary,
  formatPassengerSummary,
  getDiscountCodes,
  getPassengerCounts,
} from "../utils/purchasePreferences";

const paymentMethods = ["BLIK", "Payment by PayU transfer", "Payment card", "Google Pay"];
type PaymentStatus = "form" | "processing" | "paid";

function BookingCheckoutPage() {
  const { tripId } = useParams();
  const [searchParams] = useSearchParams();
  const selectedClass = searchParams.get("class") === "2" ? "2" : "1";
  const email = searchParams.get("email") ?? "";
  const bookingId = searchParams.get("bookingId") ?? "";
  const orderId = searchParams.get("orderId") ?? "";
  const bookingIds = searchParams.get("bookingIds") ?? "";
  const selectedSeat = searchParams.get("seat") ?? "46";
  const selectedCar = searchParams.get("car") ?? "1";
  const selectedSeatList = (searchParams.get("seats") ?? "")
    .split(",")
    .map((seat) => seat.trim())
    .filter(Boolean);
  const segmentDepartureName = searchParams.get("fromStation");
  const segmentArrivalName = searchParams.get("toStation");
  const passengerCounts = getPassengerCounts(searchParams);
  const discountCodes = getDiscountCodes(searchParams, passengerCounts);
  const [trip, setTrip] = useState<TripDetails | null>(null);
  const [tripError, setTripError] = useState("");
  const committedAmount = Number(searchParams.get("amount"));
  const committedCurrency = searchParams.get("currency") ?? "PLN";
  const hasCommittedAmount = Number.isFinite(committedAmount) && committedAmount > 0;
  const price = hasCommittedAmount
    ? formatTripPrice(committedAmount, committedCurrency)
    : getTripPriceLabel(trip, selectedClass);
  const vat = hasCommittedAmount
    ? formatTripPrice(committedAmount * 0.08, committedCurrency)
    : getTripVatLabel(trip, selectedClass);
  const passengerTotal = passengerCounts.adults + passengerCounts.children;
  const [travelerNames, setTravelerNames] = useState<string[]>(() =>
    Array.from({ length: passengerTotal }, (_, index) => (index === 0 ? "Trong Nguyen" : `Passenger ${index + 1}`)),
  );
  const [needsInvoice, setNeedsInvoice] = useState(false);
  const [acceptedRules, setAcceptedRules] = useState(true);
  const [paymentMethod, setPaymentMethod] = useState("Google Pay");
  const [co2Compensation, setCo2Compensation] = useState(false);
  const [secondsLeft, setSecondsLeft] = useState(33);
  const [isExpiredModalOpen, setIsExpiredModalOpen] = useState(searchParams.get("expired") === "true");
  const [paymentStatus, setPaymentStatus] = useState<PaymentStatus>("form");
  const [error, setError] = useState("");
  const [ticketNumbers, setTicketNumbers] = useState<string[]>([]);
  const [ticketArtifacts, setTicketArtifacts] = useState<TicketArtifact[]>([]);
  const [ticketOrderReference, setTicketOrderReference] = useState("");
  const [ticketQrUrl, setTicketQrUrl] = useState("");
  const [ticketDownloadError, setTicketDownloadError] = useState("");
  const sessionEmail = getUserEmail();
  const effectiveEmail = email || sessionEmail || "";
  const isLoggedInPurchase = hasAuthToken() && Boolean(sessionEmail);
  const myTicketsUrl = isLoggedInPurchase
    ? "/profile"
    : `/bookings?email=${encodeURIComponent(effectiveEmail || "nguyentrongminhkhoa@gmail.com")}`;
  const flowParams = new URLSearchParams(searchParams);
  flowParams.set("class", selectedClass);
  flowParams.set("email", email);

  if (bookingId) {
    flowParams.set("bookingId", bookingId);
  }

  if (orderId) {
    flowParams.set("orderId", orderId);
  }

  if (bookingIds) {
    flowParams.set("bookingIds", bookingIds);
  }

  const currentCheckoutUrl = `/checkout/${tripId}?${flowParams.toString()}`;
  const discountSelectionUrl = buildDiscountSelectionUrl(currentCheckoutUrl, flowParams);

  if (selectedSeat) {
    flowParams.set("seat", selectedSeat);
    flowParams.set("car", selectedCar);
  }

  useEffect(() => {
    if (!tripId) {
      return;
    }

    getTripById(tripId)
      .then((tripDetails) => {
        setTrip(tripDetails);
        setTripError("");
      })
      .catch(() => {
        setTrip(null);
        setTripError("Selected trip details could not be loaded. Go back to the connection list and choose the train again.");
      });
  }, [tripId]);

  useEffect(() => {
    if (secondsLeft <= 0) {
      setIsExpiredModalOpen(true);
      return;
    }

    const timerId = window.setTimeout(() => setSecondsLeft((current) => current - 1), 1000);
    return () => window.clearTimeout(timerId);
  }, [secondsLeft]);

  useEffect(() => {
    setTravelerNames((current) => {
      const next = current.slice(0, passengerTotal);
      while (next.length < passengerTotal) {
        next.push(`Passenger ${next.length + 1}`);
      }
      return next;
    });
  }, [passengerTotal]);

  useEffect(() => {
    return () => {
      if (ticketQrUrl) {
        window.URL.revokeObjectURL(ticketQrUrl);
      }
    };
  }, [ticketQrUrl]);

  function formatTimer(totalSeconds: number) {
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    return `${minutes}:${String(seconds).padStart(2, "0")}`;
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (secondsLeft <= 0) {
      setIsExpiredModalOpen(true);
      return;
    }

    const hasMissingTravelerName = travelerNames.some((name) => !name.trim());
    if (hasMissingTravelerName || !acceptedRules || !paymentMethod || (!bookingId && !orderId) || !effectiveEmail) {
      setError("Enter traveler names, accept the rules, choose a payment method, and make sure guest data is saved.");
      return;
    }

    setError("");
    setPaymentStatus("processing");

    try {
      let artifacts: TicketArtifact[] = [];
      let firstBookingId = bookingId;

      if (orderId) {
        const order = await getBookingOrder(orderId);
        await Promise.all(
          order.bookings.map((booking, index) =>
            updateGuestBookingData(booking.id, {
              guestEmail: effectiveEmail,
              passengerName: travelerNames[index]?.trim() || `Passenger ${index + 1}`,
              acceptedTerms: acceptedRules,
              acceptedMarketing: false,
            }),
          ),
        );

        const intent = await createOrderPaymentIntent(orderId);
        await confirmPayment(intent.paymentIntentId, "tok_success");
        const orderTickets = await getOrderTickets(orderId, effectiveEmail);
        artifacts = orderTickets.tickets;
        setTicketOrderReference(orderTickets.orderReference);
        firstBookingId = artifacts[0]?.bookingId ? String(artifacts[0].bookingId) : order.bookings[0]?.id.toString() ?? "";
      } else {
        await updateGuestBookingData(bookingId, {
          guestEmail: effectiveEmail,
          passengerName: travelerNames[0],
          acceptedTerms: acceptedRules,
          acceptedMarketing: false,
        });

        const intent = await createPaymentIntent(bookingId);
        await confirmPayment(intent.paymentIntentId, "tok_success");
        const artifact = await getTicketArtifact(bookingId, effectiveEmail);
        artifacts = [artifact];
      }

      const qrBlob = await getTicketQrSvgBlob(firstBookingId, effectiveEmail);
      const nextQrUrl = window.URL.createObjectURL(qrBlob);
      const guestTickets = await getGuestTickets(effectiveEmail);
      const paidTicketNumbers = artifacts
        .map((artifact) => artifact.ticketNumber)
        .filter(Boolean);

      if (ticketQrUrl) {
        window.URL.revokeObjectURL(ticketQrUrl);
      }

      setTicketArtifacts(artifacts);
      setTicketQrUrl(nextQrUrl);
      const fallbackTicketNumbers = guestTickets
        .filter((ticket) =>
          orderId
            ? artifacts.some((artifact) => artifact.bookingId === ticket.id)
            : ticket.id === Number(bookingId),
        )
        .map((ticket) => ticket.ticketNumber)
        .filter(Boolean);
      setTicketNumbers(
        paidTicketNumbers.length > 0
          ? paidTicketNumbers
          : fallbackTicketNumbers.length > 0
            ? fallbackTicketNumbers
            : ["Ticket pending"],
      );
      setPaymentStatus("paid");

      const emailDelivery = orderId
        ? sendOrderTicketsEmail(orderId, effectiveEmail)
        : sendTicketEmail(bookingId, effectiveEmail);

      emailDelivery
        .then((delivery) => {
          setTicketArtifacts((current) =>
            current.map((artifact) => ({
              ...artifact,
              emailDeliveryStatus: "deliveries" in delivery ? `${delivery.sentCount}/${delivery.requestedCount} sent` : delivery.status,
              emailSentAtUtc: "deliveries" in delivery ? delivery.deliveries[0]?.sentAtUtc ?? null : delivery.sentAtUtc,
            })),
          );
        })
        .catch(() => {
          setTicketArtifacts((current) => current.map((artifact) => ({ ...artifact, emailDeliveryStatus: "Not sent" })));
        });
    } catch {
      setPaymentStatus("form");
      setError("Payment could not be completed. The booking hold may have expired or the API is unavailable.");
    }
  }

  async function handleDownloadTicketPdf() {
    if (!bookingId && ticketArtifacts.length === 0) {
      return;
    }

    setTicketDownloadError("");

    try {
      if (orderId) {
        await downloadOrderTicketPdf(orderId, effectiveEmail, ticketOrderReference);
        return;
      }

      await downloadTicketPdf(bookingId, effectiveEmail, ticketArtifacts[0]?.ticketNumber || ticketNumbers[0]);
    } catch {
      setTicketDownloadError("Ticket PDF could not be downloaded. Try opening My tickets and downloading it again.");
    }
  }

  const firstTicketArtifact = ticketArtifacts[0] ?? null;

  if (paymentStatus === "processing") {
    return (
      <main className="order-summary-page payment-page">
        <section className="payment-processing-card">
          <div className="processing-train-art" aria-hidden="true">
            <div className="processing-city" />
            <div className="processing-train" />
          </div>
          <h1>Payment processing.</h1>
          <p>
            Payment confirmation must be received within 15 minutes of booking, no later than the closing time
            for a given connection. If there is no payment confirmation within this time, the ticket will be
            canceled.
          </p>
        </section>
      </main>
    );
  }

  if (paymentStatus === "paid") {
    return (
      <main className="order-summary-page payment-page">
        <section className="payment-success-content">
          <Link to="/" className="success-close" aria-label="Close purchase confirmation">
            x
          </Link>

          <h1>Thank you for purchasing all tickets</h1>

          <section className="ticket-success-card">
            {ticketQrUrl ? (
              <img className="ticket-qr-image" src={ticketQrUrl} alt="Ticket QR code" />
            ) : (
              <div className="qr-placeholder" aria-hidden="true">
                <span />
                <span />
                <span />
                <span />
              </div>
            )}
            <div>
              <p>payment status <strong>PAID</strong></p>
              <h2>
                {firstTicketArtifact?.route ??
                  `${segmentDepartureName ?? trip?.departureStationName ?? "Departure"} > ${segmentArrivalName ?? trip?.arrivalStationName ?? "Arrival"}`}
              </h2>
              <p>
                {ticketNumbers.length === 1 ? "ticket number: " : "ticket numbers: "}
                {ticketNumbers.map((ticketNumber) => (
                  <span key={ticketNumber}>
                    <strong>{ticketNumber}</strong><br />
                  </span>
                ))}
              </p>
              <button className="ticket-pdf-button" type="button" onClick={handleDownloadTicketPdf}>
                Download PDF
              </button>
              {firstTicketArtifact?.emailDeliveryStatus && (
                <p>ticket email <strong>{firstTicketArtifact.emailDeliveryStatus}</strong></p>
              )}
              {ticketDownloadError && <p className="data-error">{ticketDownloadError}</p>}
            </div>
          </section>

          <Link
            to={myTicketsUrl}
            className="show-tickets-button"
          >
            Show tickets
          </Link>

          <section className="loyalty-panel">
            <div className="loyalty-logo">Moje IC</div>
            <h2>Collect points with "Moje IC"</h2>
            {isLoggedInPurchase ? (
              <>
                <p>Your account can keep tickets, shopping profiles, and loyalty activity together.</p>
                <Link to="/profile">Open account</Link>
              </>
            ) : (
              <>
                <p>Join the Program and redeem points for trips. Earn up to 500 welcome points.</p>
                <p>By clicking "Join now" you will be asked to log in or register an account.</p>
                <Link to="/register">Join now</Link>
              </>
            )}
          </section>

          {!isLoggedInPurchase && (
            <section className="post-purchase-panel">
              <h2>Create an account and enjoy the benefits</h2>
              <ul>
                <li>Create shopping profiles</li>
                <li>Use the shopping cart</li>
                <li>Tickets are available at any time in your account</li>
                <li>You do not have to enter all your data with every purchase</li>
              </ul>
              <Link to="/register">Create an account</Link>
            </section>
          )}

          <section className="feedback-weather">
            <h2>Share your opinion</h2>
            <a href="#feedback">Open passenger feedback form</a>
            <div className="weather-grid">
              <article>
                <span>{formatTripDate(trip?.departureTime)}</span>
                <strong>{segmentDepartureName ?? trip?.departureStationName ?? "Departure"}</strong>
                <p>light rain</p>
                <b>19 C</b>
              </article>
              <article>
                <span>{formatTripDate(trip?.arrivalTime)}</span>
                <strong>{segmentArrivalName ?? trip?.arrivalStationName ?? "Arrival"}</strong>
                <p>light rain</p>
                <b>22 C</b>
              </article>
            </div>
          </section>
        </section>
      </main>
    );
  }

  return (
    <main className="order-summary-page payment-page">
      <section className="order-summary-content">
        <nav className="checkout-steps order-summary-steps" aria-label="Purchase steps">
          <Link to="/">Home</Link>
          <Link to="/">Search engine</Link>
          <Link to="/search">List of connections</Link>
          <Link to={`/summary/${tripId}?${flowParams.toString()}`}>Your travel</Link>
          <Link to={`/order-summary/${tripId}?${flowParams.toString()}`}>Summary</Link>
          <strong>Payment</strong>
          <span>Ticket</span>
        </nav>

        <section className="final-summary-card">
          <div className="final-summary-top">
            <div className="final-timeline">
              <h1>{formatTripDate(trip?.departureTime)}</h1>
              <div>
                <span className="final-line" aria-hidden="true" />
                <p><strong>{formatTripTime(trip?.departureTime)}</strong> {segmentDepartureName ?? trip?.departureStationName ?? "Departure"}</p>
                <p><strong>{formatTripTime(trip?.arrivalTime)}</strong> {segmentArrivalName ?? trip?.arrivalStationName ?? "Arrival"}</p>
              </div>
            </div>

            <div className="final-train-details">
              <p><strong>{trip?.trainName ?? "Selected train"}</strong></p>
              <p>
                {selectedSeatList.length > 1
                  ? selectedSeatList.map(formatSeatToken).join(", ")
                  : `Car ${selectedCar}, seat ${selectedSeat}, by the window`}
              </p>
              <span>A place at the table</span>
            </div>

            <div className="final-passenger-details">
              <span>Time to buy: <strong>{formatTimer(secondsLeft)}</strong></span>
              <p>{formatPassengerSummary(passengerCounts)}</p>
              <p>{formatDiscountSummary(discountCodes)}</p>
              <p>{selectedClass} class</p>
              <Link to={discountSelectionUrl}>Change discounts</Link>
            </div>
          </div>

          <div className="final-price-panel">
            <div>
              <strong>Outbound journey</strong>
              <span>A-Base price</span>
            </div>
            <div>
              <span>Price</span>
              <strong>{price}</strong>
            </div>
          </div>
        </section>

        {tripError && <p className="data-error">{tripError}</p>}

        <form className="payment-form-card" onSubmit={handleSubmit}>
          <section>
            <h2>{passengerTotal > 1 ? "Travelers' first and last names" : "Traveler's first and last name"}</h2>
            <div className="payment-traveler-grid">
              {travelerNames.map((travelerName, index) => (
                <label className="payment-text-field" key={`traveler-${index}`}>
                  <span>Passenger {index + 1}</span>
                  <input
                    value={travelerName}
                    onChange={(event) =>
                      setTravelerNames((current) =>
                        current.map((name, itemIndex) => (itemIndex === index ? event.target.value : name)),
                      )
                    }
                  />
                </label>
              ))}
            </div>
            <p className="payment-help">
              Enter your actual data. On the train, the conductor may ask you for proof of identity.
            </p>
            <p className="payment-help">Each traveler receives their own ticket in this order.</p>
          </section>

          <section>
            <h2>Invoice</h2>
            <label className="payment-checkbox-row">
              <input
                checked={needsInvoice}
                onChange={(event) => setNeedsInvoice(event.target.checked)}
                type="checkbox"
              />
              <span>Provide billing information</span>
            </label>
          </section>

          <section>
            <h2>Rules and Regulations</h2>
            <label className="payment-checkbox-row">
              <input
                checked={acceptedRules}
                onChange={(event) => setAcceptedRules(event.target.checked)}
                type="checkbox"
              />
              <span>I accept <a href="#rules">the ticket Rules and Regulations</a></span>
            </label>
          </section>

          <section>
            <h2>Payment method</h2>
            <div className="payment-method-list">
              {paymentMethods.map((method) => (
                <label className="payment-method-row" key={method}>
                  <input
                    checked={paymentMethod === method}
                    onChange={() => setPaymentMethod(method)}
                    name="paymentMethod"
                    type="radio"
                  />
                  <span>{method}</span>
                  <b>{method === "Google Pay" ? "G Pay" : method === "Payment card" ? "Card" : method}</b>
                </label>
              ))}
            </div>

            <label className="co2-row">
              <span>CO2 compensation</span>
              <button
                type="button"
                className={co2Compensation ? "consent-switch consent-switch-on" : "consent-switch"}
                onClick={() => setCo2Compensation(!co2Compensation)}
                aria-pressed={co2Compensation}
              />
            </label>
          </section>

          <section className="amount-due-panel payment-amount-due">
            <div>
              <h2>Amount due</h2>
              <span>8% VAT</span>
              {email && <p>Ticket will be sent to {email}</p>}
            </div>
            <div>
              <strong>{price}</strong>
              <span>{vat}</span>
            </div>
          </section>

          {error && <p className="data-error">{error}</p>}

          <section className="order-summary-actions payment-actions">
            <button type="submit">Pay</button>
            <Link to={`/order-summary/${tripId}?${flowParams.toString()}`}>Cancel</Link>
          </section>
        </form>
      </section>

      <section className="summary-footer-train" aria-hidden="true">
        <div className="connection-train" />
      </section>

      <section className="summary-legal">
        <div>
          <h2>Technological break.</h2>
          <p>
            Please remember about scheduled technical breaks in the online sales system. You cannot buy tickets
            during this break.
          </p>
          <a href="#accessibility">Declaration of Accessibility</a>
        </div>
        <div>
          <p>
            The prices presented are indicative and published for informational purposes. Final confirmation
            appears after payment is completed.
          </p>
          <strong>RailWay ticket platform</strong>
        </div>
      </section>

      <BookingExpiredModal isOpen={isExpiredModalOpen} />
    </main>
  );
}

function formatSeatToken(token: string) {
  const [car, seat] = token.split("-");
  return car && seat ? `Car ${car}, seat ${seat}` : token;
}

export default BookingCheckoutPage;
