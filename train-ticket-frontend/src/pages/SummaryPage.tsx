import { useEffect, useState } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom";
import PassengerLegalFooter from "../components/PassengerLegalFooter";
import { updateBookingExtras, updateBookingOrderExtras } from "../api/bookingApi";
import { getUserEmail, hasAuthToken } from "../api/authSession";
import { getTripById } from "../api/tripApi";
import type { TripDetails } from "../types/trip";
import { formatTripDate, formatTripTime } from "../utils/tripDisplay";
import {
  buildDiscountSelectionUrl,
  formatDiscountSummary,
  formatPassengerSummary,
  getDiscountCodes,
  getPassengerCounts,
} from "../utils/purchasePreferences";

const DOG_TICKET_PRICE = 15;
const LARGE_BAGGAGE_TICKET_PRICE = 5;
type ExtraConfirmationType = "dog" | "baggage" | null;

function SummaryPage() {
  const { tripId } = useParams();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [isAccountPromptOpen, setIsAccountPromptOpen] = useState(false);
  const [extraConfirmation, setExtraConfirmation] = useState<ExtraConfirmationType>(null);
  const [dogTicketCount, setDogTicketCount] = useState(() => clampExtraCount(searchParams.get("dogs"), 0, 1));
  const [largeBaggageTicketCount, setLargeBaggageTicketCount] = useState(() => clampExtraCount(searchParams.get("bags"), 0, 10));
  const [syncedAmount, setSyncedAmount] = useState<number | null>(null);
  const [extraError, setExtraError] = useState("");
  const [trip, setTrip] = useState<TripDetails | null>(null);
  const [tripError, setTripError] = useState("");
  const selectedClass = searchParams.get("class") === "2" ? "2" : "1";
  const selectedSeat = searchParams.get("seat");
  const selectedCar = searchParams.get("car") ?? "1";
  const bookingId = searchParams.get("bookingId") ?? "";
  const orderId = searchParams.get("orderId") ?? "";
  const bookingIds = searchParams.get("bookingIds") ?? "";
  const selectedSeatList = (searchParams.get("seats") ?? "")
    .split(",")
    .map((seat) => seat.trim())
    .filter(Boolean);
  const segmentDepartureName = searchParams.get("fromStation");
  const segmentArrivalName = searchParams.get("toStation");
  const passengerCounts = getPassengerCounts(searchParams);
  const discountCodes = getDiscountCodes(searchParams, passengerCounts);
  const summaryParams = new URLSearchParams(searchParams);
  summaryParams.set("class", selectedClass);
  summaryParams.set("dogs", String(dogTicketCount));
  summaryParams.set("bags", String(largeBaggageTicketCount));
  if (syncedAmount != null) {
    summaryParams.set("amount", String(syncedAmount));
  }

  if (bookingId) {
    summaryParams.set("bookingId", bookingId);
  }

  if (orderId) {
    summaryParams.set("orderId", orderId);
  }

  if (bookingIds) {
    summaryParams.set("bookingIds", bookingIds);
  }

  if (selectedSeat) {
    summaryParams.set("seat", selectedSeat);
    summaryParams.set("car", selectedCar);
  }

  const dataRequestUrl = `/data/${tripId}?${summaryParams.toString()}`;
  const currentSummaryUrl = `/summary/${tripId}?${summaryParams.toString()}`;
  const discountSelectionUrl = buildDiscountSelectionUrl(currentSummaryUrl, summaryParams);
  const extrasAmount = dogTicketCount * DOG_TICKET_PRICE + largeBaggageTicketCount * LARGE_BAGGAGE_TICKET_PRICE;

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

  async function syncExtras() {
    if (!bookingId && !orderId) {
      return null;
    }

    const request = {
      dogTicketCount,
      largeBaggageTicketCount,
    };

    if (orderId) {
      const updatedOrder = await updateBookingOrderExtras(orderId, request);
      setSyncedAmount(updatedOrder.amount);
      return updatedOrder.amount;
    }

    const updatedBooking = await updateBookingExtras(bookingId, request);
    setSyncedAmount(updatedBooking.amount);
    return updatedBooking.amount;
  }

  async function handleGoToPayments() {
    setExtraError("");

    let latestAmount = syncedAmount;
    try {
      latestAmount = await syncExtras();
    } catch {
      setExtraError("Could not save dog or large baggage tickets. Try again before payment.");
      return;
    }

    const userEmail = getUserEmail();
    const nextParams = new URLSearchParams(summaryParams);
    if (latestAmount != null) {
      nextParams.set("amount", String(latestAmount));
    }

    if (hasAuthToken() && userEmail) {
      const loggedInParams = new URLSearchParams(nextParams);
      loggedInParams.set("email", userEmail);
      navigate(`/order-summary/${tripId}?${loggedInParams.toString()}`);
      return;
    }

    setIsAccountPromptOpen(true);
  }

  return (
    <main className="summary-page">
      <section className="summary-content">
        <p className="previous-system-note">
          Account services: invoices, refunds, and passenger data changes are available after purchase.
        </p>

        <nav className="checkout-steps summary-steps" aria-label="Purchase steps">
          <Link to="/">Home</Link>
          <Link to="/">Search engine</Link>
          <Link to="/search">List of connections</Link>
          <strong>Your travel</strong>
          <span>Summary</span>
          <span>Payment</span>
          <span>Ticket</span>
        </nav>

        <section className="summary-trip-panel">
          <div>
            <span>From</span>
            <strong>{segmentDepartureName ?? trip?.departureStationName ?? "Loading..."}</strong>
          </div>
          <div>
            <span>To</span>
            <strong>{segmentArrivalName ?? trip?.arrivalStationName ?? "Loading..."}</strong>
          </div>
          <div>
            <h1>
              {formatTripDate(trip?.departureTime)} {formatTripTime(trip?.departureTime)} &gt;{" "}
              {formatTripTime(trip?.arrivalTime)}
            </h1>
            <p>
              {segmentDepartureName ?? trip?.departureStationName ?? "Departure"} &gt;{" "}
              {segmentArrivalName ?? trip?.arrivalStationName ?? "Arrival"}
            </p>
            <b>{trip?.trainName ?? "Selected train"}</b>
          </div>
        </section>

        {tripError && <p className="data-error">{tripError}</p>}

        <section className="summary-ticket-strip">
          <div>
            <span className="summary-ticket-icon" aria-hidden="true" />
            <strong>{selectedClass} Class</strong>
          </div>
          <div>
            <span className="summary-passenger-icon" aria-hidden="true" />
            <strong>{formatPassengerSummary(passengerCounts)}</strong>
          </div>
          <div>
            <span className="summary-ticket-icon" aria-hidden="true" />
            <strong>{formatDiscountSummary(discountCodes)}</strong>
          </div>
          <Link to={discountSelectionUrl}>Change</Link>
        </section>

        <section className="summary-options-grid">
          <fieldset className="summary-option-card">
            <legend>Seat selection preferences</legend>
            <div className="seat-preference-icons" aria-label="Seat preference options">
              <span aria-label="Standard seat" />
              <span aria-label="Window seat" />
              <span aria-label="Table seat" />
              <span aria-label="Accessible space" />
              <span aria-label="Bicycle space" />
              <span aria-label="Quiet coach" />
            </div>
            <div className={`summary-outline-button ${selectedSeat || selectedSeatList.length > 0 ? "summary-seat-selected" : "summary-seat-empty"}`}>
              {selectedSeat || selectedSeatList.length > 0 ? (
                <>
                  <span>
                    {selectedSeatList.length > 1
                      ? selectedSeatList.map(formatSeatToken).join(", ")
                      : `Car ${selectedCar}, seat ${selectedSeat}`}
                  </span>
                  <Link className="summary-seat-change" to={`/seat-map/${tripId}?${summaryParams.toString()}`}>
                    Change
                  </Link>
                </>
              ) : (
                <Link className="summary-seat-action" to={`/seat-map/${tripId}?${summaryParams.toString()}`}>
                  Choose a place
                </Link>
              )}
            </div>
          </fieldset>

          <fieldset className="summary-option-card">
            <legend>Additional tickets</legend>
            <p className="addon-warning">
              Dog tickets cost {DOG_TICKET_PRICE} PLN. Large baggage tickets cost {LARGE_BAGGAGE_TICKET_PRICE} PLN.
              Keep these extras visible on your ticket for inspection.
            </p>
            <div className="addon-row">
              <div>
                <span className="paw-icon" aria-hidden="true" />
                <strong>Dog</strong>
              </div>
              <button
                type="button"
                disabled={dogTicketCount === 0}
                onClick={() => {
                  setDogTicketCount(0);
                  setSyncedAmount(null);
                }}
              >
                -
              </button>
              <span>{dogTicketCount}</span>
              <button type="button" disabled={dogTicketCount >= 1} onClick={() => setExtraConfirmation("dog")}>+</button>
            </div>
            <div className="addon-row">
              <div>
                <span className="bag-icon" aria-hidden="true" />
                <strong>Luggage</strong>
              </div>
              <button
                type="button"
                disabled={largeBaggageTicketCount === 0}
                onClick={() => {
                  setLargeBaggageTicketCount((current) => Math.max(0, current - 1));
                  setSyncedAmount(null);
                }}
              >
                -
              </button>
              <span>{largeBaggageTicketCount}</span>
              <button type="button" onClick={() => setExtraConfirmation("baggage")}>+</button>
            </div>
            {extrasAmount > 0 && (
              <strong className="addon-total">
                Additional tickets: {formatMoney(extrasAmount)}
              </strong>
            )}
          </fieldset>
        </section>

        {extraError && <p className="data-error">{extraError}</p>}

        <section className="summary-actions">
          <button type="button" onClick={handleGoToPayments}>
            Go to payments
          </button>
          <Link to="/search">Go back</Link>
        </section>
      </section>

      <section className="summary-footer-train" aria-hidden="true">
        <div className="connection-train" />
      </section>

      <PassengerLegalFooter />

      {isAccountPromptOpen && (
        <div className="account-modal-backdrop" role="presentation">
          <section className="account-modal" role="dialog" aria-modal="true" aria-labelledby="account-modal-title">
            <button
              type="button"
              className="account-modal-close"
              onClick={() => setIsAccountPromptOpen(false)}
              aria-label="Close account prompt"
            >
              x
            </button>

            <div className="account-modal-actions">
              <h2 id="account-modal-title">Sign in</h2>
              <Link to="/login" className="account-primary-action">
                Log in
              </Link>
              <Link to="/register" className="account-secondary-action">
                Register
              </Link>
              <Link to={dataRequestUrl} className="guest-link">
                Continue as guest
              </Link>
            </div>

            <div className="account-benefits">
              <h3>Join and enjoy the benefits</h3>
              <ul>
                <li>Personalized buying profiles</li>
                <li>You do not have to enter all your data with every purchase</li>
                <li>Shopping cart and season tickets</li>
                <li>Tickets are available any time in your account</li>
              </ul>
              <div className="traveler-illustration" aria-hidden="true">
                <span className="traveler-head" />
                <span className="traveler-body" />
                <span className="traveler-case" />
              </div>
            </div>
          </section>
        </div>
      )}

      {extraConfirmation && (
        <div className="account-modal-backdrop" role="presentation">
          <section className="account-modal addon-confirmation-modal" role="dialog" aria-modal="true" aria-labelledby="addon-confirmation-title">
            <button
              type="button"
              className="account-modal-close"
              onClick={() => setExtraConfirmation(null)}
              aria-label="Close confirmation"
            >
              x
            </button>
            <h2 id="addon-confirmation-title">Confirmation</h2>
            {extraConfirmation === "dog" ? (
              <div className="addon-confirmation-copy">
                <p className="addon-confirmation-lead">You selected a seat for a person traveling with a dog.</p>
                <p className="addon-confirmation-note">
                  Additional fee will be charged for traveling with a dog.
                  <Link to="/help/passenger-rights"> You can check the fees and rules of transportation.</Link>
                </p>
                <p className="addon-confirmation-question">Do you confirm that you will travel with a dog?</p>
              </div>
            ) : (
              <div className="addon-confirmation-copy">
                <p className="addon-confirmation-lead">A seat has been selected for a traveler with large luggage.</p>
                <p className="addon-confirmation-note">
                  Additional fee will be charged for transporting the luggage.
                  <Link to="/help/passenger-rights"> You can check the fees and rules of transportation.</Link>
                </p>
                <p className="addon-confirmation-question">Please confirm that you will travel with the luggage.</p>
              </div>
            )}
            <div className="addon-confirmation-actions">
              <button type="button" onClick={() => setExtraConfirmation(null)}>Resign</button>
              <button
                type="button"
                onClick={() => {
                  if (extraConfirmation === "dog") {
                    setDogTicketCount(1);
                  } else {
                    setLargeBaggageTicketCount((current) => Math.min(10, current + 1));
                  }
                  setSyncedAmount(null);
                  setExtraConfirmation(null);
                }}
              >
                Confirm
              </button>
            </div>
          </section>
        </div>
      )}
    </main>
  );
}

function formatSeatToken(token: string) {
  const [car, seat] = token.split("-");
  return car && seat ? `Car ${car}, seat ${seat}` : token;
}

function clampExtraCount(value: string | null, min: number, max: number) {
  const parsed = Number(value);
  if (!Number.isFinite(parsed)) {
    return min;
  }

  return Math.min(max, Math.max(min, Math.trunc(parsed)));
}

function formatMoney(value: number, currency = "PLN") {
  return `${value.toLocaleString("pl-PL", { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ${currency}`;
}

export default SummaryPage;
