import { Link } from "react-router-dom";
import type { TripSearchResult } from "../types/trip";

type TripCardProps = {
  trip: TripSearchResult;
  rank?: number;
  isExpanded?: boolean;
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

function formatPrice(value: number | null, classOffset: number, currency: string) {
  if (value == null) {
    return "TBA";
  }

  return `${(value + classOffset).toFixed(2).replace(".", ",")} ${currency || "PLN"}`;
}

function TripCard({ trip, rank = 0, isExpanded = false, onSelect }: TripCardProps) {
  const isFast = rank < 2;
  const classOnePrice = formatPrice(trip.lowestFare, 44, trip.currency);
  const classTwoPrice = formatPrice(trip.lowestFare, 0, trip.currency);

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

      {isExpanded && (
        <div className="connection-expanded-panel">
          <div className="connection-details">
            <button type="button" className="details-toggle">
              Show connection details
            </button>

            <div className="route-timeline">
              <div className="route-stop">
                <strong>{formatTime(trip.departureTime)}</strong>
                <span className="timeline-dot" />
                <div>
                  <b>{trip.departureStationName}</b>
                  <small>{trip.trainName}</small>
                </div>
                <span>Plat. 2 track 2</span>
              </div>

              <div className="route-stop">
                <strong>{formatTime(trip.arrivalTime)}</strong>
                <span className="timeline-dot" />
                <div>
                  <b>{trip.arrivalStationName}</b>
                </div>
                <span>Plat. 3 track 1</span>
              </div>
            </div>
          </div>

          <div className="class-choice-grid">
            <div className="class-choice-card">
              <span>Class 1</span>
              <div className="seat-glyph" aria-hidden="true" />
              <strong>{classOnePrice}</strong>
              <Link to={`/seat-map/${trip.tripId}?class=1`}>Choose class 1</Link>
            </div>

            <div className="class-choice-card">
              <span>Class 2</span>
              <div className="seat-glyph" aria-hidden="true" />
              <strong>{classTwoPrice}</strong>
              <Link to={`/seat-map/${trip.tripId}?class=2`}>Choose class 2</Link>
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
