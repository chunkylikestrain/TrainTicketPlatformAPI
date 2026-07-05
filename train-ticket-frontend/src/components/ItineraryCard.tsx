import { Link } from "react-router-dom";
import type { TripItinerarySearchResult, TripItinerarySegment } from "../types/trip";

type ItineraryCardProps = {
  itinerary: TripItinerarySearchResult;
  rank?: number;
  isExpanded?: boolean;
  purchaseQuery?: string;
  selectionActionLabel?: string;
  onSelect?: () => void;
  onChooseItinerary?: () => void;
};

function formatTime(value: string) {
  return new Intl.DateTimeFormat("en", {
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function formatMinutes(totalMinutes: number) {
  const minutes = Math.max(0, Math.round(totalMinutes));
  const hours = Math.floor(minutes / 60);
  const rest = minutes % 60;

  return `${hours}h ${rest}min`;
}

function formatPrice(value: number | null, classOffset: number, currency: string) {
  if (value == null) {
    return "TBA";
  }

  return `${(value + classOffset).toFixed(2).replace(".", ",")} ${currency || "PLN"}`;
}

function formatPlatform(segment: TripItinerarySegment) {
  const platform = segment.platform ? `Plat. ${segment.platform}` : "Plat. -";
  const track = segment.track ? `track ${segment.track}` : "track -";
  return `${platform} ${track}`;
}

function getIntermediateStopCount(segment: TripItinerarySegment) {
  if (segment.callingPattern.length === 0) {
    return 0;
  }

  return Math.max(0, segment.callingPattern.length - 2);
}

function formatJourneySummary(itinerary: TripItinerarySearchResult) {
  const intermediateStops = itinerary.segments.reduce(
    (total, segment) => total + getIntermediateStopCount(segment),
    0);
  const stopLabel = intermediateStops === 1 ? "intermediate stop" : "intermediate stops";
  const transferLabel = itinerary.transferCount === 1 ? "transfer" : "transfers";

  if (itinerary.transferCount === 0) {
    return `${intermediateStops} ${stopLabel}`;
  }

  return `${itinerary.transferCount} ${transferLabel}, ${intermediateStops} ${stopLabel}`;
}

function hasSleeperSegment(itinerary: TripItinerarySearchResult) {
  return itinerary.segments.some((segment) => segment.serviceType.toLowerCase() === "sleeper train");
}

function encodeItinerarySegments(itinerary: TripItinerarySearchResult) {
  return window.btoa(
    encodeURIComponent(JSON.stringify(itinerary.segments)),
  );
}

function ItineraryCard({
  itinerary,
  rank = 0,
  isExpanded = false,
  purchaseQuery = "",
  selectionActionLabel,
  onSelect,
  onChooseItinerary,
}: ItineraryCardProps) {
  const isFast = rank < 2;
  const isDirect = itinerary.transferCount === 0;
  const isSleeperConnection = hasSleeperSegment(itinerary);
  const firstSegment = itinerary.segments[0];
  const lastSegment = itinerary.segments[itinerary.segments.length - 1];
  const classOnePrice = formatPrice(itinerary.lowestFare, 44, itinerary.currency);
  const classTwoPrice = formatPrice(itinerary.lowestFare, 0, itinerary.currency);

  function classUrl(selectedClass: "1" | "2") {
    const segment = itinerary.segments[0];
    const params = new URLSearchParams(purchaseQuery);
    params.set("class", selectedClass);
    params.set("fromStationId", String(segment.departureStationId));
    params.set("toStationId", String(segment.arrivalStationId));
    params.set("fromStation", segment.departureStationName);
    params.set("toStation", segment.arrivalStationName);
    return `/seat-map/${segment.tripId}?${params.toString()}`;
  }

  function itinerarySeatUrl(selectedClass: "1" | "2") {
    const segment = itinerary.segments[0];
    const finalSegment = itinerary.segments[itinerary.segments.length - 1];
    const params = new URLSearchParams(purchaseQuery);
    params.set("class", selectedClass);
    params.set("itineraryId", itinerary.itineraryId);
    params.set("itinerarySegments", encodeItinerarySegments(itinerary));
    params.set("fromStationId", String(segment.departureStationId));
    params.set("toStationId", String(finalSegment.arrivalStationId));
    params.set("fromStation", segment.departureStationName);
    params.set("toStation", finalSegment.arrivalStationName);
    return `/seat-map/${segment.tripId}?${params.toString()}`;
  }

  return (
    <article className={`trip-card ${isExpanded ? "trip-card-expanded" : ""}`}>
      <div className="connection-badges">
        <span>{isDirect ? "Direct" : `${itinerary.transferCount} transfer${itinerary.transferCount === 1 ? "" : "s"}`}</span>
        {isSleeperConnection && <span>Sleeper train</span>}
        {isFast && <span>Fastest</span>}
      </div>

      <div className="connection-main">
        <div className="connection-time-block">
          <span>Travel time: {formatMinutes(itinerary.totalDurationMinutes)}</span>
          <strong>
            {formatTime(itinerary.departureTime)} <em>-&gt;</em> {formatTime(itinerary.arrivalTime)}
          </strong>
        </div>

        <div className="connection-route-block">
          <span>Journey</span>
          <p>
            {firstSegment?.departureStationName ?? "Departure"} <em>-&gt;</em>{" "}
            {lastSegment?.arrivalStationName ?? "Arrival"}
          </p>
          <strong>{itinerary.segments.map((segment) => segment.trainName).join(" + ")}</strong>
          <small>{formatJourneySummary(itinerary)}</small>
        </div>

        <div className="connection-price-block">
          <span>Class 1</span>
          <strong>{classOnePrice}</strong>
        </div>

        <div className="connection-price-block">
          <span>Class 2</span>
          <strong>{classTwoPrice}</strong>
        </div>
      </div>

      {!isDirect && (
        <div className="itinerary-transfer-summary">
          {itinerary.segments.slice(0, -1).map((segment) => (
            <span key={`${segment.tripId}-${segment.segmentIndex}`}>
              Change at {segment.arrivalStationName}: {segment.transferAfterMinutes} min
            </span>
          ))}
        </div>
      )}

      {isExpanded && (
        <div className="connection-expanded-panel">
          <div className="connection-details">
            <span className="details-toggle">
              Show itinerary details
            </span>

            <div className="route-timeline itinerary-timeline">
              {itinerary.segments.map((segment, index) => (
                <div className="itinerary-segment-block" key={`${segment.tripId}-${segment.departureStationId}-${segment.arrivalStationId}`}>
                  {index > 0 && (
                    <div className="transfer-row">
                      <strong>{itinerary.segments[index - 1].transferAfterMinutes} min transfer</strong>
                      <span>{segment.departureStationName}</span>
                    </div>
                  )}
                  <div className="route-stop">
                    <strong>{formatTime(segment.departureTime)}</strong>
                    <span className="timeline-dot" />
                    <div>
                      <b>{segment.departureStationName}</b>
                      <small>{segment.serviceType ? `${segment.trainName} - ${segment.serviceType}` : segment.trainName}</small>
                    </div>
                    <span>{formatPlatform(segment)}</span>
                  </div>
                  <div className="route-stop">
                    <strong>{formatTime(segment.arrivalTime)}</strong>
                    <span className="timeline-dot" />
                    <div>
                      <b>{segment.arrivalStationName}</b>
                      <small>{formatMinutes(segment.durationMinutes)}</small>
                    </div>
                    <span>{segment.hasDisruption ? "Service update" : segment.status}</span>
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="class-choice-grid">
            {selectionActionLabel ? (
              <div className="itinerary-next-step-card">
                <strong>{selectionActionLabel}</strong>
                <p>Use this connection for the current round-trip journey step.</p>
                <div className="itinerary-seat-actions">
                  <button type="button" onClick={onChooseItinerary}>
                    {selectionActionLabel}
                  </button>
                </div>
              </div>
            ) : isDirect ? (
              <>
                <div className="class-choice-card">
                  <span>Class 1</span>
                  <div className="seat-glyph" aria-hidden="true" />
                  <strong>{classOnePrice}</strong>
                  <Link to={classUrl("1")}>Choose class 1</Link>
                </div>

                <div className="class-choice-card">
                  <span>Class 2</span>
                  <div className="seat-glyph" aria-hidden="true" />
                  <strong>{classTwoPrice}</strong>
                  <Link to={classUrl("2")}>Choose class 2</Link>
                </div>
              </>
            ) : (
              <div className="itinerary-next-step-card">
                <strong>Choose seats by segment</strong>
                <p>Select a class, then pick seats for each train in this itinerary.</p>
                <div className="itinerary-seat-actions">
                  <Link to={itinerarySeatUrl("1")}>Choose class 1</Link>
                  <Link to={itinerarySeatUrl("2")}>Choose class 2</Link>
                </div>
              </div>
            )}
          </div>
        </div>
      )}

      <div className="connection-card-footer">
        {!isExpanded && (
          <button type="button" className="trip-action" onClick={onSelect}>
            {selectionActionLabel ?? (isDirect ? "Buy a ticket" : "View itinerary")}
          </button>
        )}
      </div>
    </article>
  );
}

export default ItineraryCard;
