import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import TripCard from "../components/TripCard";
import { searchTrips } from "../api/tripApi";
import type { TripSearchResult } from "../types/trip";
import {
  buildFilterSelectionUrl,
  copyPurchasePreferenceParams,
  formatDiscountSummary,
  formatPassengerSummary,
  getDiscountCodes,
  getFilterCodes,
  getPassengerCounts,
} from "../utils/purchasePreferences";

function formatLongDate(value: string) {
  if (!value) {
    return "Select a date";
  }

  return new Intl.DateTimeFormat("en", {
    weekday: "short",
    day: "numeric",
    month: "long",
  }).format(new Date(`${value}T12:00:00`));
}

function formatNoticeDate(value: string) {
  if (!value) {
    return "the selected date";
  }

  return new Intl.DateTimeFormat("en-GB").format(new Date(`${value}T12:00:00`));
}

function SearchResultsPage() {
  const [searchParams] = useSearchParams();
  const [trips, setTrips] = useState<TripSearchResult[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const [expandedTripId, setExpandedTripId] = useState<number | null>(null);

  const departureStation = searchParams.get("departureStation") ?? "";
  const arrivalStation = searchParams.get("arrivalStation") ?? "";
  const date = searchParams.get("date") ?? "";
  const passengerCounts = getPassengerCounts(searchParams);
  const discountCodes = getDiscountCodes(searchParams, passengerCounts);
  const filterCodes = getFilterCodes(searchParams);
  const purchaseQuery = copyPurchasePreferenceParams(searchParams).toString();
  const currentSearchUrl = `/search?${searchParams.toString()}`;
  const filterSelectionUrl = buildFilterSelectionUrl(currentSearchUrl, searchParams);

  useEffect(() => {
    if (!departureStation || !arrivalStation || !date) {
      return;
    }

    setIsLoading(true);
    setError("");

    searchTrips({ departureStation, arrivalStation, date })
      .then(setTrips)
      .catch(() => {
        setTrips([]);
        setError("Connections could not be loaded from the API. Check that the backend is running and your search date has schedules.");
      })
      .finally(() => setIsLoading(false));
  }, [arrivalStation, date, departureStation]);

  return (
    <main className="connection-page">
      <section className="connection-hero" aria-hidden="true">
        <div className="connection-train" />
      </section>

      <section className="connection-content">
        <nav className="checkout-steps" aria-label="Purchase steps">
          <Link to="/">Home</Link>
          <Link to="/">Search engine</Link>
          <strong>List of connections</strong>
          <span>Your travel</span>
          <span>Summary</span>
          <span>Payment</span>
          <span>Ticket</span>
          <a href="#print">Print version</a>
        </nav>

        <div className="connection-summary">
          <div>
            <strong>{departureStation || "Rzeszow Glowny"}</strong>
            <span aria-hidden="true">-&gt;</span>
            <strong>{arrivalStation || "Krakow Gl."}</strong>
          </div>
          <div>
            <span>{formatPassengerSummary(passengerCounts)}</span>
            <span>{formatDiscountSummary(discountCodes)}</span>
            {filterCodes.length > 0 && <span>{filterCodes.length} filters</span>}
            <Link to="/">Change</Link>
          </div>
        </div>

        <div className="date-toolbar">
          <button type="button">&lt; Thu, 18 June</button>
          <h1>{formatLongDate(date)}</h1>
          <Link to={filterSelectionUrl}>Filters</Link>
          <button type="button">Sat, 20 June &gt;</button>
        </div>

        {(isLoading || error || trips.length === 0) && (
          <div className={`connection-notice ${error ? "connection-notice-warning" : ""}`} aria-live="polite">
            <strong>
              {isLoading
                ? "Loading live connections..."
                : `No live connections on ${formatNoticeDate(date)} before 06:06.`}
            </strong>
            <p>
              {error ||
                "Try another date, route, or make sure this route has a schedule in the admin panel."}
            </p>
          </div>
        )}

        <button type="button" className="timeline-button timeline-muted">
          Earlier
        </button>

        <section className="connection-list" aria-label="Available connections">
          {trips.map((trip, index) => (
            <TripCard
              trip={trip}
              key={trip.tripId}
              rank={index}
              isExpanded={expandedTripId === trip.tripId}
              purchaseQuery={purchaseQuery}
              onSelect={() => setExpandedTripId(expandedTripId === trip.tripId ? null : trip.tripId)}
            />
          ))}
        </section>

        <button type="button" className="timeline-button">
          Later
        </button>

        <Link to="/" className="back-to-search">
          Go back to the search engine
        </Link>

        <section className="results-footer-note">
          <div>
            <h2>Technological break</h2>
            <p>
              Online ticket sales may pause briefly for maintenance. During that time, searching remains
              available but buying a ticket can be temporarily disabled.
            </p>
            <a href="#accessibility">Declaration of Accessibility</a>
          </div>
          <div>
            <p>
              The prices presented are indicative and shown for information purposes. Final prices and seat
              availability are confirmed in the purchase summary.
            </p>
            <strong>RailWay ticket platform</strong>
          </div>
        </section>
      </section>
    </main>
  );
}

export default SearchResultsPage;
