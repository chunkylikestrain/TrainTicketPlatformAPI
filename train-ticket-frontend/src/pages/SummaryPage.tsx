import { useEffect, useState } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom";
import { getUserEmail, hasAuthToken } from "../api/authSession";
import { getTripById } from "../api/tripApi";
import type { TripDetails } from "../types/trip";
import { formatTripDate, formatTripTime } from "../utils/tripDisplay";

function SummaryPage() {
  const { tripId } = useParams();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [isAccountPromptOpen, setIsAccountPromptOpen] = useState(false);
  const [trip, setTrip] = useState<TripDetails | null>(null);
  const [tripError, setTripError] = useState("");
  const selectedClass = searchParams.get("class") === "2" ? "2" : "1";
  const selectedSeat = searchParams.get("seat");
  const selectedCar = searchParams.get("car") ?? "1";
  const bookingId = searchParams.get("bookingId") ?? "";
  const summaryParams = new URLSearchParams({ class: selectedClass });

  if (bookingId) {
    summaryParams.set("bookingId", bookingId);
  }

  if (selectedSeat) {
    summaryParams.set("seat", selectedSeat);
    summaryParams.set("car", selectedCar);
  }

  const dataRequestUrl = `/data/${tripId}?${summaryParams.toString()}`;

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

  function handleGoToPayments() {
    const userEmail = getUserEmail();

    if (hasAuthToken() && userEmail) {
      const loggedInParams = new URLSearchParams(summaryParams);
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
            <strong>{trip?.departureStationName ?? "Loading..."}</strong>
          </div>
          <div>
            <span>To</span>
            <strong>{trip?.arrivalStationName ?? "Loading..."}</strong>
          </div>
          <div>
            <h1>
              {formatTripDate(trip?.departureTime)} {formatTripTime(trip?.departureTime)} &gt;{" "}
              {formatTripTime(trip?.arrivalTime)}
            </h1>
            <p>{trip?.departureStationName ?? "Departure"} &gt; {trip?.arrivalStationName ?? "Arrival"}</p>
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
            <strong>1 Passenger</strong>
          </div>
          <div>
            <span className="summary-ticket-icon" aria-hidden="true" />
            <strong>1x Normal Ticket</strong>
          </div>
          <Link to="/search">Change</Link>
        </section>

        <section className="summary-options-grid">
          <fieldset className="summary-option-card">
            <legend>Seat selection preferences</legend>
            <div className="seat-preference-icons" aria-label="Seat preference options">
              <button type="button" aria-label="Standard seat" />
              <button type="button" aria-label="Window seat" />
              <button type="button" aria-label="Table seat" />
              <button type="button" aria-label="Accessible space" />
              <button type="button" aria-label="Bicycle space" />
              <button type="button" aria-label="Quiet coach" />
            </div>
            <div className={`summary-outline-button ${selectedSeat ? "summary-seat-selected" : "summary-seat-empty"}`}>
              {selectedSeat ? (
                <>
                  <span>Car {selectedCar}, seat {selectedSeat}</span>
                  <Link className="summary-seat-change" to={`/seat-map/${tripId}?class=${selectedClass}`}>
                    Change
                  </Link>
                </>
              ) : (
                <Link className="summary-seat-action" to={`/seat-map/${tripId}?class=${selectedClass}`}>
                  Choose a place
                </Link>
              )}
            </div>
          </fieldset>

          <fieldset className="summary-option-card">
            <legend>Additional tickets</legend>
            <div className="addon-row">
              <div>
                <span className="paw-icon" aria-hidden="true" />
                <strong>Dog</strong>
              </div>
              <button type="button" disabled>
                -
              </button>
              <span>0</span>
              <button type="button">+</button>
            </div>
            <div className="addon-row">
              <div>
                <span className="bag-icon" aria-hidden="true" />
                <strong>Luggage</strong>
              </div>
              <button type="button" disabled>
                -
              </button>
              <span>0</span>
              <button type="button">+</button>
            </div>
          </fieldset>
        </section>

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
            The prices presented are indicative and published for informational purposes. The final price is
            available in the purchase summary before payment.
          </p>
          <strong>RailWay ticket platform</strong>
        </div>
      </section>

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
    </main>
  );
}

export default SummaryPage;
