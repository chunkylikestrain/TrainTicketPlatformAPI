import { Link } from "react-router-dom";
import type { TripCallingPatternStop, TripSearchResult } from "../types/trip";
import { getDisruptionMessage, getDisruptionSeverity, hasDisruption } from "../utils/disruptions";

type TripCardProps = {
  trip: TripSearchResult;
  rank?: number;
  isExpanded?: boolean;
  purchaseQuery?: string;
  onSelect?: () => void;
};

function formatTime(value: string) {
  return new Intl.DateTimeFormat("en", {
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function formatDuration(start: string, end: string) {
  const minutes = Math.max(0, Math.round((new Date(end).getTime() - new Date(start).getTime()) / 60000));
  const hours = Math.floor(minutes / 60);
  const rest = minutes % 60;

  return `${hours}h ${rest}min`;
}

function formatStopTime(stop: TripCallingPatternStop) {
  if (stop.arrivalTime && stop.departureTime && stop.arrivalTime !== stop.departureTime) {
    return `${formatTime(stop.arrivalTime)}-${formatTime(stop.departureTime)}`;
  }

  const time = stop.departureTime ?? stop.arrivalTime;
  return time ? formatTime(time) : "-";
}

function formatPlatform(stop: TripCallingPatternStop) {
  const platform = stop.platform ? `Plat. ${stop.platform}` : "Plat. -";
  const track = stop.track ? `track ${stop.track}` : "track -";
  return `${platform} ${track}`;
}

function formatPrice(value: number | null, classOffset: number, currency: string) {
  if (value == null) {
    return "TBA";
  }

  return `${(value + classOffset).toFixed(2).replace(".", ",")} ${currency || "PLN"}`;
}

function TripCard({ trip, rank = 0, isExpanded = false, purchaseQuery = "", onSelect }: TripCardProps) {
  const isFast = rank < 2;
  const classOnePrice = formatPrice(trip.lowestFare, 44, trip.currency);
  const classTwoPrice = formatPrice(trip.lowestFare, 0, trip.currency);
  const disruptionMessage = getDisruptionMessage(trip);
  const disruptionSeverity = getDisruptionSeverity(trip);

  function classUrl(selectedClass: "1" | "2") {
    const params = new URLSearchParams(purchaseQuery);
    params.set("class", selectedClass);
    params.set("fromStationId", String(trip.departureStationId));
    params.set("toStationId", String(trip.arrivalStationId));
    params.set("fromStation", trip.departureStationName);
    params.set("toStation", trip.arrivalStationName);
    return `/seat-map/${trip.tripId}?${params.toString()}`;
  }

  return (
    <article className={`trip-card ${isExpanded ? "trip-card-expanded" : ""}`}>
      <div className="connection-badges">
        <span>Direct</span>
        {isFast && <span>Fastest</span>}
      </div>

      <div className="connection-main">
        <div className="connection-time-block">
          <span>Travel time: {formatDuration(trip.departureTime, trip.arrivalTime)}</span>
          <strong>
            {formatTime(trip.departureTime)} <em>-&gt;</em> {formatTime(trip.arrivalTime)}
          </strong>
        </div>

        <div className="connection-route-block">
          <span>Route</span>
          <p>
            {trip.departureStationName} <em>-&gt;</em> {trip.arrivalStationName}
          </p>
          <strong>{trip.trainName}</strong>
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

      {hasDisruption(trip) && disruptionMessage && (
        <div className={`disruption-banner disruption-${disruptionSeverity || "notice"}`}>
          <strong>Service update</strong>
          <span>{disruptionMessage}</span>
        </div>
      )}

      {isExpanded && (
        <div className="connection-expanded-panel">
          <div className="connection-details">
            <button type="button" className="details-toggle">
              Show connection details
            </button>

            <div className="route-timeline">
              {(trip.callingPattern.length > 0
                ? trip.callingPattern
                : [
                    {
                      stationId: trip.departureStationId,
                      stationCode: trip.departureStationCode,
                      stationName: trip.departureStationName,
                      stopOrder: trip.departureStopOrder,
                      arrivalTime: null,
                      departureTime: trip.departureTime,
                      arrivalOffsetMinutes: null,
                      departureOffsetMinutes: null,
                      dwellMinutes: 0,
                      platform: "",
                      track: "",
                      stopType: "Terminus",
                    },
                    {
                      stationId: trip.arrivalStationId,
                      stationCode: trip.arrivalStationCode,
                      stationName: trip.arrivalStationName,
                      stopOrder: trip.arrivalStopOrder,
                      arrivalTime: trip.arrivalTime,
                      departureTime: null,
                      arrivalOffsetMinutes: null,
                      departureOffsetMinutes: null,
                      dwellMinutes: 0,
                      platform: "",
                      track: "",
                      stopType: "Terminus",
                    },
                  ]).map((stop, index) => (
                <div className="route-stop" key={`${stop.stationId}-${stop.stopOrder}`}>
                  <strong>{formatStopTime(stop)}</strong>
                  <span className="timeline-dot" />
                  <div>
                    <b>{stop.stationName}</b>
                    <small>
                      {index === 0
                        ? trip.trainName
                        : stop.dwellMinutes > 0
                          ? `${stop.dwellMinutes} min stop`
                          : stop.stopType}
                    </small>
                  </div>
                  <span>{formatPlatform(stop)}</span>
                </div>
              ))}
            </div>
          </div>

          <div className="class-choice-grid">
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

            <a className="station-trains-link" href="#station-trains">
              Check the list of trains for station {trip.departureStationName}
            </a>
          </div>
        </div>
      )}

      <div className="connection-card-footer">
        <button type="button">Check seat availability</button>
        {!isExpanded && (
          <button type="button" className="trip-action" onClick={onSelect}>
            Buy a ticket
          </button>
        )}
      </div>
    </article>
  );
}

export default TripCard;
