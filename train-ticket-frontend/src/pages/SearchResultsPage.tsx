import { useEffect, useRef, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import ItineraryCard from "../components/ItineraryCard";
import PassengerLegalFooter from "../components/PassengerLegalFooter";
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

const INITIAL_CONNECTION_COUNT = 4;
const CONNECTION_INCREMENT = 3;

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

function shiftDate(value: string, days: number) {
  if (!value) {
    return "";
  }

  const nextDate = new Date(`${value}T12:00:00`);
  if (Number.isNaN(nextDate.getTime())) {
    return "";
  }

  nextDate.setDate(nextDate.getDate() + days);
  return nextDate.toISOString().slice(0, 10);
}

function formatTime(value: string) {
  return new Intl.DateTimeFormat("en", {
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function getDateTimeValue(value: string) {
  const dateTime = new Date(value).getTime();
  return Number.isNaN(dateTime) ? 0 : dateTime;
}

function getSameDayConnections(results: TripItinerarySearchResult[], travelDate: string) {
  if (!travelDate) {
    return [];
  }

  return [...results]
    .filter((itinerary) => itinerary.departureTime.slice(0, 10) === travelDate)
    .sort((left, right) => getDateTimeValue(left.departureTime) - getDateTimeValue(right.departureTime));
}

function getInitialConnectionStart(
  results: TripItinerarySearchResult[],
  travelDate: string,
  travelTime: string,
) {
  if (!travelDate || !travelTime) {
    return 0;
  }

  const cutoff = new Date(`${travelDate}T${travelTime}`).getTime();
  if (Number.isNaN(cutoff)) {
    return 0;
  }

  const nextConnectionIndex = results.findIndex((itinerary) => {
    const departureTime = getDateTimeValue(itinerary.departureTime);
    return departureTime >= cutoff;
  });

  return nextConnectionIndex === -1 ? results.length : nextConnectionIndex;
}

type StoredRoundTripItinerary = {
  searchKey: string;
  itinerary: TripItinerarySearchResult;
};

function getStoredItinerary(storageKey: string, searchKey: string, searchParams: URLSearchParams, direction: "Outbound" | "Return") {
  try {
    const raw = window.sessionStorage.getItem(storageKey);

    if (!raw) {
      return null;
    }

    const stored = JSON.parse(raw) as Partial<StoredRoundTripItinerary>;

    if (
      stored.searchKey !== searchKey ||
      !stored.itinerary ||
      !itineraryMatchesSearch(stored.itinerary, searchParams, direction)
    ) {
      window.sessionStorage.removeItem(storageKey);
      return null;
    }

    return stored.itinerary;
  } catch {
    window.sessionStorage.removeItem(storageKey);
    return null;
  }
}

function storeItinerary(storageKey: string, searchKey: string, itinerary: TripItinerarySearchResult) {
  const stored: StoredRoundTripItinerary = {
    searchKey,
    itinerary,
  };

  window.sessionStorage.setItem(storageKey, JSON.stringify(stored));
}

function SearchResultsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const roundTripSearchKey = buildRoundTripSearchKey(searchParams);
  const [itineraries, setItineraries] = useState<TripItinerarySearchResult[]>([]);
  const [selectedOutbound, setSelectedOutbound] = useState<TripItinerarySearchResult | null>(() =>
    getStoredItinerary("railbook-round-trip-outbound", roundTripSearchKey, searchParams, "Outbound"),
  );
  const [selectedReturn, setSelectedReturn] = useState<TripItinerarySearchResult | null>(() =>
    getStoredItinerary("railbook-round-trip-return", roundTripSearchKey, searchParams, "Return"),
  );
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const [expandedItineraryId, setExpandedItineraryId] = useState<string | null>(null);
  const [visibleConnectionStart, setVisibleConnectionStart] = useState(0);
  const [visibleConnectionEnd, setVisibleConnectionEnd] = useState(INITIAL_CONNECTION_COUNT);

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
  const activeDateParam = isChoosingReturn ? "returnDate" : "date";
  const previousDate = shiftDate(activeDate, -1);
  const nextDate = shiftDate(activeDate, 1);
  const returnSearchSignature = [roundTripSearchKey, returnDate, returnTime].join("|");
  const previousOutboundSearchSignature = useRef(roundTripSearchKey);
  const previousReturnSearchSignature = useRef(returnSearchSignature);

  useEffect(() => {
    if (previousOutboundSearchSignature.current === roundTripSearchKey) {
      return;
    }

    previousOutboundSearchSignature.current = roundTripSearchKey;
    setSelectedOutbound(null);
    setSelectedReturn(null);
    window.sessionStorage.removeItem("railbook-round-trip-outbound");
    window.sessionStorage.removeItem("railbook-round-trip-return");
  }, [roundTripSearchKey]);

  useEffect(() => {
    if (previousReturnSearchSignature.current === returnSearchSignature) {
      return;
    }

    previousReturnSearchSignature.current = returnSearchSignature;
    setSelectedReturn(null);
    window.sessionStorage.removeItem("railbook-round-trip-return");
  }, [returnSearchSignature]);

  useEffect(() => {
    if (!activeDepartureStation || !activeArrivalStation || !activeDate) {
      return;
    }

    setIsLoading(true);
    setError("");
    setExpandedItineraryId(null);
    setVisibleConnectionStart(0);
    setVisibleConnectionEnd(INITIAL_CONNECTION_COUNT);

    searchItineraries({
      departureStation: activeDepartureStation,
      arrivalStation: activeArrivalStation,
      date: activeDate,
      time: "00:00",
    })
      .then((results) => {
        const sameDayConnections = getSameDayConnections(results, activeDate);
        const firstVisibleIndex = getInitialConnectionStart(sameDayConnections, activeDate, activeTime);

        setItineraries(sameDayConnections);
        setVisibleConnectionStart(firstVisibleIndex);
        setVisibleConnectionEnd(Math.min(firstVisibleIndex + INITIAL_CONNECTION_COUNT, sameDayConnections.length));
      })
      .catch(() => {
        setItineraries([]);
        setVisibleConnectionStart(0);
        setVisibleConnectionEnd(0);
        setError("Connections could not be loaded from the API. Check that the backend is running and your search date has schedules.");
      })
      .finally(() => setIsLoading(false));
  }, [activeArrivalStation, activeDate, activeDepartureStation, activeTime]);

  const visibleItineraries = itineraries.slice(visibleConnectionStart, visibleConnectionEnd);
  const hasVisibleConnections = visibleItineraries.length > 0;
  const hasEarlierConnections = visibleConnectionStart > 0;
  const hasLaterConnections = visibleConnectionEnd < itineraries.length;
  const firstVisibleItinerary = visibleItineraries[0];
  const firstVisibleDepartureTime = firstVisibleItinerary ? formatTime(firstVisibleItinerary.departureTime) : activeTime;
  const lastVisibleItinerary = visibleItineraries[visibleItineraries.length - 1];
  const lastVisibleDepartureTime = lastVisibleItinerary ? formatTime(lastVisibleItinerary.departureTime) : activeTime;
  const noConnectionsForWholeDay = !isLoading && !error && itineraries.length === 0;
  const searchAfterLastSameDayConnection =
    !isLoading && !error && itineraries.length > 0 && !hasVisibleConnections && visibleConnectionStart >= itineraries.length;
  const isEarlyMorningSearch = Boolean(activeTime) && activeTime < "06:00";
  const showPreviousDayBoundary =
    !isLoading && !error && isEarlyMorningSearch && !hasEarlierConnections && Boolean(previousDate) && !noConnectionsForWholeDay;
  const showNextDayBoundary = !isLoading && !error && hasVisibleConnections && !hasLaterConnections && Boolean(nextDate);
  const showConnectionNotice = isLoading || error || noConnectionsForWholeDay || searchAfterLastSameDayConnection;

  function showEarlierConnections() {
    setVisibleConnectionStart((current) => Math.max(current - CONNECTION_INCREMENT, 0));
  }

  function showLaterConnections() {
    setVisibleConnectionEnd((current) =>
      Math.min(current + CONNECTION_INCREMENT, itineraries.length),
    );
  }

  function navigateToActiveDate(nextDateValue: string) {
    if (!nextDateValue) {
      return;
    }

    const nextParams = new URLSearchParams(searchParams);
    nextParams.set(activeDateParam, nextDateValue);
    setSearchParams(nextParams);
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  function chooseItinerary(itinerary: TripItinerarySearchResult) {
    if (!isRoundTrip) {
      setExpandedItineraryId(expandedItineraryId === itinerary.itineraryId ? null : itinerary.itineraryId);
      return;
    }

    if (!selectedOutbound) {
      setSelectedOutbound(itinerary);
      setSelectedReturn(null);
      storeItinerary("railbook-round-trip-outbound", roundTripSearchKey, itinerary);
      window.sessionStorage.removeItem("railbook-round-trip-return");
      return;
    }

    setSelectedReturn(itinerary);
    storeItinerary("railbook-round-trip-return", roundTripSearchKey, itinerary);
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
          <button type="button" onClick={() => navigateToActiveDate(previousDate)} disabled={!previousDate}>
            &lt; {formatLongDate(previousDate)}
          </button>
          <h1>{formatLongDate(activeDate)}</h1>
          <Link to={filterSelectionUrl}>Filters</Link>
          <button type="button" onClick={() => navigateToActiveDate(nextDate)} disabled={!nextDate}>
            {formatLongDate(nextDate)} &gt;
          </button>
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

        {showConnectionNotice && (
          <div className={`connection-notice ${error ? "connection-notice-warning" : ""}`} aria-live="polite">
            <strong>
              {isLoading
                ? "Loading live connections..."
                : noConnectionsForWholeDay
                  ? `No connections on ${formatNoticeDate(activeDate)}.`
                  : `No connections on ${formatNoticeDate(activeDate)} after ${activeTime || "the selected time"}.`}
            </strong>
            <p>
              {error ||
                (noConnectionsForWholeDay
                  ? "Try the previous or next operating day, or make sure this route has a schedule in the admin panel."
                  : "Use Earlier to show previous same-day connections, or check the next operating day.")}
            </p>
            {!isLoading && !error && (
              <div className="connection-notice-actions">
                {hasEarlierConnections && (
                  <button type="button" className="connection-notice-link" onClick={showEarlierConnections}>
                    Show earlier connections
                  </button>
                )}
                {previousDate && noConnectionsForWholeDay && (
                  <button type="button" className="connection-notice-link" onClick={() => navigateToActiveDate(previousDate)}>
                    See {formatNoticeDate(previousDate)}
                  </button>
                )}
                {nextDate && (
                  <button type="button" className="connection-notice-link" onClick={() => navigateToActiveDate(nextDate)}>
                    See {formatNoticeDate(nextDate)}
                  </button>
                )}
              </div>
            )}
          </div>
        )}

        {showPreviousDayBoundary && (
          <div className="connection-notice connection-boundary-notice" aria-live="polite">
            <strong>No earlier same-day connections before {activeTime}.</strong>
            <p>
              Overnight trains may have started the previous evening.{" "}
              <button type="button" className="connection-notice-link" onClick={() => navigateToActiveDate(previousDate)}>
                See connections on {formatNoticeDate(previousDate)}.
              </button>
            </p>
          </div>
        )}

        {hasVisibleConnections && (
          <div className="connection-window-summary" aria-live="polite">
            <span>
              Showing connections {visibleConnectionStart + 1}-{visibleConnectionEnd} of {itineraries.length} on{" "}
              {formatNoticeDate(activeDate)}.
            </span>
            <span>
              {firstVisibleDepartureTime} - {lastVisibleDepartureTime}
            </span>
          </div>
        )}

        <button
          type="button"
          className={`timeline-button ${hasEarlierConnections ? "" : "timeline-muted"}`}
          onClick={showEarlierConnections}
          disabled={!hasEarlierConnections}
        >
          Earlier
        </button>

        <section className="connection-list" aria-label="Available connections">
          {visibleItineraries.map((itinerary, index) => (
            <ItineraryCard
              itinerary={itinerary}
              key={itinerary.itineraryId}
              rank={visibleConnectionStart + index}
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

        <button
          type="button"
          className={`timeline-button ${hasLaterConnections ? "" : "timeline-muted"}`}
          onClick={showLaterConnections}
          disabled={!hasLaterConnections}
        >
          Later
        </button>

        {showNextDayBoundary && (
          <div className="connection-notice connection-boundary-notice" aria-live="polite">
            <strong>No later same-day connections after {lastVisibleDepartureTime || "the last shown train"}.</strong>
            <p>
              You have reached the end of {formatNoticeDate(activeDate)}.{" "}
              <button type="button" className="connection-notice-link" onClick={() => navigateToActiveDate(nextDate)}>
                See connections on {formatNoticeDate(nextDate)}.
              </button>
            </p>
          </div>
        )}

        <Link to="/" className="back-to-search">
          Go back to the search engine
        </Link>

        <PassengerLegalFooter className="results-footer-note" />
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

function buildRoundTripSearchKey(searchParams: URLSearchParams) {
  const keyParams = new URLSearchParams();
  const keys = [
    "departureStation",
    "arrivalStation",
    "date",
    "time",
    "tripType",
    "returnDate",
    "returnTime",
    "adults",
    "children",
    "discounts",
    "filters",
  ];

  keys.forEach((key) => {
    const values = searchParams.getAll(key);

    values.forEach((value) => {
      keyParams.append(key, value.trim());
    });
  });

  keyParams.sort();
  return keyParams.toString();
}

function itineraryMatchesSearch(
  itinerary: TripItinerarySearchResult,
  searchParams: URLSearchParams,
  direction: "Outbound" | "Return",
) {
  const firstSegment = itinerary.segments[0];
  const lastSegment = itinerary.segments[itinerary.segments.length - 1];

  if (!firstSegment || !lastSegment) {
    return false;
  }

  const expectedDeparture = direction === "Outbound"
    ? searchParams.get("departureStation")
    : searchParams.get("arrivalStation");
  const expectedArrival = direction === "Outbound"
    ? searchParams.get("arrivalStation")
    : searchParams.get("departureStation");
  const expectedDate = direction === "Outbound"
    ? searchParams.get("date")
    : searchParams.get("returnDate");

  return normalizeSearchValue(firstSegment.departureStationName) === normalizeSearchValue(expectedDeparture) &&
    normalizeSearchValue(lastSegment.arrivalStationName) === normalizeSearchValue(expectedArrival) &&
    firstSegment.departureTime.slice(0, 10) === expectedDate;
}

function normalizeSearchValue(value: string | null | undefined) {
  return (value ?? "")
    .trim()
    .replace(/[Łł]/g, "l")
    .replace(/[Đđ]/g, "d")
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLowerCase();
}

export default SearchResultsPage;
