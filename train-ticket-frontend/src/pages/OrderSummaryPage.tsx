import { useEffect, useState } from "react";
import { Link, useParams, useSearchParams } from "react-router-dom";
import { getTripById } from "../api/tripApi";
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

function OrderSummaryPage() {
  const { tripId } = useParams();
  const [searchParams] = useSearchParams();
  const [trip, setTrip] = useState<TripDetails | null>(null);
  const [tripError, setTripError] = useState("");
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
  const committedAmount = Number(searchParams.get("amount"));
  const committedCurrency = searchParams.get("currency") ?? "PLN";
  const hasCommittedAmount = Number.isFinite(committedAmount) && committedAmount > 0;
  const price = hasCommittedAmount
    ? formatTripPrice(committedAmount, committedCurrency)
    : getTripPriceLabel(trip, selectedClass);
  const vat = hasCommittedAmount
    ? formatTripPrice(committedAmount * 0.08, committedCurrency)
    : getTripVatLabel(trip, selectedClass);
  const checkoutParams = new URLSearchParams(searchParams);
  checkoutParams.set("class", selectedClass);
  checkoutParams.set("email", email);
  const dataParams = new URLSearchParams(searchParams);
  dataParams.set("class", selectedClass);
  const currentSummaryUrl = `/order-summary/${tripId}?${checkoutParams.toString()}`;
  const discountSelectionUrl = buildDiscountSelectionUrl(currentSummaryUrl, checkoutParams);

  if (bookingId) {
    checkoutParams.set("bookingId", bookingId);
    dataParams.set("bookingId", bookingId);
  }

  if (orderId) {
    checkoutParams.set("orderId", orderId);
    dataParams.set("orderId", orderId);
  }

  if (bookingIds) {
    checkoutParams.set("bookingIds", bookingIds);
    dataParams.set("bookingIds", bookingIds);
  }

  if (selectedSeat) {
    checkoutParams.set("seat", selectedSeat);
    checkoutParams.set("car", selectedCar);
    dataParams.set("seat", selectedSeat);
    dataParams.set("car", selectedCar);
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

  return (
    <main className="order-summary-page">
      <section className="connection-hero order-summary-hero" aria-hidden="true">
        <div className="connection-train" />
      </section>

      <section className="order-summary-content">
        <p className="previous-system-note">
          Account services: invoices, refunds, and passenger data changes are available after purchase.
        </p>

        <nav className="checkout-steps order-summary-steps" aria-label="Purchase steps">
          <Link to="/">Home</Link>
          <Link to="/">Search engine</Link>
          <Link to="/search">List of connections</Link>
          <Link to={`/summary/${tripId}?${dataParams.toString()}`}>Your travel</Link>
          <strong>Summary</strong>
          <span>Payment</span>
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
                  : `Car ${selectedCar}, seat ${selectedSeat}`}
              </p>
              <span>A place at the table</span>
            </div>

            <div className="final-passenger-details">
              <span>Time to buy: <strong>9:59</strong></span>
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

        <section className="amount-due-panel">
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

        <section className="order-summary-actions">
          <Link to={`/checkout/${tripId}?${checkoutParams.toString()}`}>Payment</Link>
          <Link to={`/data/${tripId}?${dataParams.toString()}`}>Cancel</Link>
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
            The prices presented are indicative and published for informational purposes. The final prices are
            available in this purchase summary before payment.
          </p>
          <strong>RailWay ticket platform</strong>
        </div>
      </section>
    </main>
  );
}

function formatSeatToken(token: string) {
  const [car, seat] = token.split("-");
  return car && seat ? `Car ${car}, seat ${seat}` : token;
}

export default OrderSummaryPage;
