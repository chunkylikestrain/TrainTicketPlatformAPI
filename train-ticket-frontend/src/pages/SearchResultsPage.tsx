import { useEffect, useRef, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import ItineraryCard from "../components/ItineraryCard";
import { searchItineraries } from "../api/tripApi";
import type { TripItinerarySearchResult } from "../types/trip";
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

function formatTime(value: string) {
  return new Intl.DateTimeFormat("en", {
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function getStoredItinerary(key: string) {
  try {
    const raw = window.sessionStorage.getItem(key);
    return raw ? JSON.parse(raw) as TripItinerarySearchResult : null;
  } catch {
    return null;
  }
}

function SearchResultsPage() {
  const [searchParams] = useSearchParams();
  const [itineraries, setItineraries] = useState<TripItinerarySearchResult[]>([]);
  const [selectedOutbound, setSelectedOutbound] = useState<TripItinerarySearchResult | null>(() =>
    getStoredItinerary("railbook-round-trip-outbound"),
  );
  const [selectedReturn, setSelectedReturn] = useState<TripItinerarySearchResult | null>(() =>
    getStoredItinerary("railbook-round-trip-return"),
  );
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const [expandedItineraryId, setExpandedItineraryId] = useState<string | null>(null);

  const departureStation = searchParams.get("departureStation") ?? "";
  const arrivalStation = searchParams.get("arrivalStation") ?? "";
  const date = searchParams.get("date") ?? "";
  const time = searchParams.get("time") ?? "";
  const tripType = searchParams.get("tripType") === "roundTrip" ? "roundTrip" : "oneWay";
  const returnDate = searchParams.get("returnDate") ?? "";
  const returnTime = searchParams.get("returnTime") ?? "";
  const isRoundTrip = tripType === "roundTrip";
  const isRoundTripComplete = isRoundTrip && selectedOutbound != null && selectedReturn != null;
  const isChoosingReturn = isRoundTrip && selectedOutbound != null;
  const activeDepartureStation = isChoosingReturn ? arrivalStation : departureStation;
  const activeArrivalStation = isChoosingReturn ? departureStation : arrivalStation;
  const activeDate = isChoosingReturn ? returnDate : date;
  const activeTime = isChoosingReturn ? returnTime : time;
  const activeDirectionLabel = isChoosingReturn ? "return" : "outbound";
  const passengerCounts = getPassengerCounts(searchParams);
  const discountCodes = getDiscountCodes(searchParams, passengerCounts);
  const filterCodes = getFilterCodes(searchParams);
  const purchaseQuery = copyPurchasePreferenceParams(searchParams).toString();
  const currentSearchUrl = `/search?${searchParams.toString()}`;
  const filterSelectionUrl = buildFilterSelectionUrl(currentSearchUrl, searchParams);
  const searchSignature = [departureStation, arrivalStation, date, time, returnDate, returnTime, tripType].join("|");
  const previousSearchSignature = useRef(searchSignature);

  useEffect(() => {
    if (previousSearchSignature.current === searchSignature) {
      return;
    }

    previousSearchSignature.current = searchSignature;
    setSelectedOutbound(null);
    setSelectedReturn(null);
    window.sessionStorage.removeItem("railbook-round-trip-outbound");
    window.sessionStorage.removeItem("railbook-round-trip-return");
  }, [searchSignature]);

  useEffect(() => {
    if (!activeDepartureStation || !activeArrivalStation || !activeDate) {
      return;
    }

    setIsLoading(true);
    setError("");
    setExpandedItineraryId(null);

    searchItineraries({
      departureStation: activeDepartureStation,
      arrivalStation: activeArrivalStation,
      date: activeDate,
      time: activeTime,
    })
      .then(setItineraries)
      .catch(() => {
        setItineraries([]);
        setError("Connections could not be loaded from the API. Check that the backend is running and your search date has schedules.");
      })
      .finally(() => setIsLoading(false));
  }, [activeArrivalStation, activeDate, activeDepartureStation, activeTime]);

  function chooseItinerary(itinerary: TripItinerarySearchResult) {
    if (!isRoundTrip) {
      setExpandedItineraryId(expandedItineraryId === itinerary.itineraryId ? null : itinerary.itineraryId);
      return;
    }

    if (!selectedOutbound) {
      setSelectedOutbound(itinerary);
      setSelectedReturn(null);
      window.sessionStorage.setItem("railbook-round-trip-outbound", JSON.stringify(itinerary));
      window.sessionStorage.removeItem("railbook-round-trip-return");
      return;
    }

    setSelectedReturn(itinerary);
    window.sessionStorage.setItem("railbook-round-trip-return", JSON.stringify(itinerary));
  }

  function resetRoundTripSelection() {
    setSelectedOutbound(null);
    setSelectedReturn(null);
    window.sessionStorage.removeItem("railbook-round-trip-outbound");
    window.sessionStorage.removeItem("railbook-round-trip-return");
  }

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
            <strong>{activeDepartureStation || "Rzeszow Glowny"}</strong>
            <span aria-hidden="true">-&gt;</span>
            <strong>{activeArrivalStation || "Krakow Gl."}</strong>
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
          <h1>{formatLongDate(activeDate)}</h1>
          <Link to={filterSelectionUrl}>Filters</Link>
          <button type="button">Sat, 20 June &gt;</button>
        </div>

        {isRoundTrip && (
          <section className="round-trip-progress" aria-label="Round trip selection progress">
            <div className={selectedOutbound ? "round-trip-step-complete" : "round-trip-step-active"}>
              <strong>1. Outbound</strong>
              <span>
                {selectedOutbound
                  ? `${formatTime(selectedOutbound.departureTime)} ${departureStation} -> ${arrivalStation}`
                  : `${departureStation} -> ${arrivalStation}`}
              </span>
            </div>
            <div className={selectedReturn ? "round-trip-step-complete" : isChoosingReturn ? "round-trip-step-active" : ""}>
              <strong>2. Return</strong>
              <span>
                {selectedReturn
                  ? `${formatTime(selectedReturn.departureTime)} ${arrivalStation} -> ${departureStation}`
                  : `${arrivalStation} -> ${departureStation}`}
              </span>
            </div>
            <button type="button" onClick={resetRoundTripSelection}>
              Start over
            </button>
          </section>
        )}

        {isRoundTrip && selectedOutbound && selectedReturn && (
          <section className="round-trip-ready-card">
            <strong>Round trip selected</strong>
            <p>
              Outbound and return connections are ready. Seat selection for both journeys comes next.
            </p>
            <div className="itinerary-seat-actions">
              <Link to={`/seat-map/${selectedOutbound.segments[0].tripId}?${buildRoundTripSeatParams(searchParams, "1")}`}>
                Choose class 1
              </Link>
              <Link to={`/seat-map/${selectedOutbound.segments[0].tripId}?${buildRoundTripSeatParams(searchParams, "2")}`}>
                Choose class 2
              </Link>
            </div>
          </section>
        )}

        {(isLoading || error || itineraries.length === 0) && (
          <div className={`connection-notice ${error ? "connection-notice-warning" : ""}`} aria-live="polite">
            <strong>
              {isLoading
                ? "Loading live connections..."
                : `No live connections on ${formatNoticeDate(activeDate)} before ${activeTime || "the selected time"}.`}
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
          {itineraries.map((itinerary, index) => (
            <ItineraryCard
              itinerary={itinerary}
              key={itinerary.itineraryId}
              rank={index}
              isExpanded={expandedItineraryId === itinerary.itineraryId}
              purchaseQuery={purchaseQuery}
              selectionActionLabel={isRoundTrip ? `${isRoundTripComplete ? "Change" : "Select"} ${activeDirectionLabel}` : undefined}
              onChooseItinerary={() => chooseItinerary(itinerary)}
              onSelect={() => {
                if (isRoundTrip) {
                  chooseItinerary(itinerary);
                  return;
                }

                setExpandedItineraryId(
                  expandedItineraryId === itinerary.itineraryId ? null : itinerary.itineraryId,
                );
              }}
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

function buildRoundTripSeatParams(searchParams: URLSearchParams, selectedClass: "1" | "2") {
  const params = new URLSearchParams(searchParams);
  params.set("class", selectedClass);
  params.set("tripType", "roundTrip");
  return params.toString();
}

export default SearchResultsPage;
