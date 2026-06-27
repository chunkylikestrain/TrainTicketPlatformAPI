import { useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { getBookingById } from "../api/bookingApi";
import { getTicketQrSvgBlob } from "../api/ticketApi";
import { getTripById } from "../api/tripApi";
import type { Booking } from "../types/booking";
import type { TripCallingPatternStop, TripDetails } from "../types/trip";
import { getDisruptionMessage, getDisruptionSeverity, hasDisruption } from "../utils/disruptions";
import { formatTripDate, formatTripTime } from "../utils/tripDisplay";

type StopRow = TripCallingPatternStop & {
  adjustedArrival: Date | null;
  adjustedDeparture: Date | null;
  arrivalShiftMinutes: number;
  departureShiftMinutes: number;
  status: "current" | "next" | "future" | "destination";
};

function CurrentTripPage() {
  const { bookingId = "" } = useParams();
  const [booking, setBooking] = useState<Booking | null>(null);
  const [trip, setTrip] = useState<TripDetails | null>(null);
  const [qrUrl, setQrUrl] = useState("");
  const [error, setError] = useState("");
  const [now, setNow] = useState(() => new Date());

  useEffect(() => {
    const timerId = window.setInterval(() => setNow(new Date()), 30000);
    return () => window.clearInterval(timerId);
  }, []);

  useEffect(() => {
    if (!bookingId) {
      return;
    }

    setError("");
    getBookingById(bookingId)
      .then((loadedBooking) => {
        setBooking(loadedBooking);
        if (loadedBooking.tripId) {
          return getTripById(loadedBooking.tripId).then(setTrip);
        }
      })
      .catch(() => setError("Could not load this trip. Try opening it again from My tickets."));
  }, [bookingId]);

  useEffect(() => {
    if (!bookingId) {
      return;
    }

    getTicketQrSvgBlob(bookingId)
      .then((blob) => {
        const nextUrl = window.URL.createObjectURL(blob);
        setQrUrl((current) => {
          if (current) {
            window.URL.revokeObjectURL(current);
          }
          return nextUrl;
        });
      })
      .catch(() => setQrUrl(""));
  }, [bookingId]);

  useEffect(() => {
    return () => {
      if (qrUrl) {
        window.URL.revokeObjectURL(qrUrl);
      }
    };
  }, [qrUrl]);

  const stationRows = useMemo(() => {
    if (!trip || !booking) {
      return [];
    }

    return buildVisibleStops(trip, booking, now);
  }, [booking, now, trip]);

  const currentStop = stationRows.find((stop) => stop.status === "current");
  const currentDestinationStop = stationRows.find((stop) =>
    stop.status === "destination" &&
    stop.adjustedArrival &&
    stop.adjustedArrival <= now &&
    (!stop.adjustedDeparture || now <= stop.adjustedDeparture),
  );
  const nextStop = stationRows.find((stop) => stop.status === "next") ?? stationRows[0];
  const passengerStart = booking?.segmentDepartureTime ?? booking?.departureTime;
  const passengerEnd = booking?.segmentArrivalTime ?? booking?.arrivalTime;

  return (
    <main className="current-trip-page">
      <section className="current-trip-shell">
        <Link className="current-trip-back" to="/profile">Back to My tickets</Link>

        {error && <div className="status-message">{error}</div>}

        <section className="current-trip-ticket">
          {qrUrl ? (
            <img src={qrUrl} alt="Ticket QR code" />
          ) : (
            <div className="qr-placeholder" aria-hidden="true">
              <span />
              <span />
              <span />
              <span />
            </div>
          )}
          <div>
            <span>{booking?.ticketNumber || booking?.bookingReference || "Ticket"}</span>
            <h1>{booking?.route || "Current trip"}</h1>
            <p>{booking?.trainName || trip?.trainName || "Train"} · {booking?.seatLabel || "Selected seat"}</p>
          </div>
        </section>

        {booking && (
          <section className="current-trip-details">
            <div>
              <span>Date</span>
              <strong>{formatTripDate(passengerStart ?? booking.travelDate)}</strong>
            </div>
            <div>
              <span>Your journey</span>
              <strong>{formatTripTime(passengerStart ?? undefined)} → {formatTripTime(passengerEnd ?? undefined)}</strong>
            </div>
            <div>
              <span>Status</span>
              <strong>
                {currentStop
                  ? `At ${currentStop.stationName}`
                  : currentDestinationStop
                    ? `At ${currentDestinationStop.stationName}`
                    : nextStop ? `Next: ${nextStop.stationName}` : booking.bookingStatus}
              </strong>
            </div>
            <div>
              <span>Platform / track</span>
              <strong>{booking.platform || trip?.platform || "-"} / {booking.track || trip?.track || "-"}</strong>
            </div>
          </section>
        )}

        {booking && hasDisruption(booking) && (
          <section className={`current-trip-alert disruption-${getDisruptionSeverity(booking) || "notice"}`}>
            <strong>Service update</strong>
            <span>{getDisruptionMessage(booking)}</span>
          </section>
        )}

        <section className="current-trip-progress">
          <div>
            <h2>Upcoming stops</h2>
            <p>
              Passed stations disappear as the train progresses. The list ends at your arrival station.
            </p>
          </div>

          {stationRows.length === 0 ? (
            <div className="status-message">Calling pattern is not available for this ticket.</div>
          ) : (
            <ol className="current-stop-list">
              {stationRows.map((stop) => (
                <li className={`current-stop-row current-stop-${stop.status}`} key={`${stop.stopOrder}-${stop.stationId}`}>
                  <span className="current-stop-dot" aria-hidden="true" />
                  <div>
                    <strong>{stop.stationName}</strong>
                    <small>{formatStopStatus(stop.status)}</small>
                  </div>
                  <time>
                    <span>
                      Arr {formatStopTime(stop.adjustedArrival)}
                      <b>{formatShift(stop.arrivalShiftMinutes)}</b>
                    </span>
                    <span>
                      Dep {formatStopTime(stop.adjustedDeparture)}
                      <b>{formatShift(stop.departureShiftMinutes)}</b>
                    </span>
                  </time>
                </li>
              ))}
            </ol>
          )}
        </section>
      </section>
    </main>
  );
}

function buildVisibleStops(trip: TripDetails, booking: Booking, now: Date): StopRow[] {
  const delayMinutes = booking.delayMinutes ?? trip.delayMinutes ?? 0;
  const departureDelay = Math.max(0, delayMinutes);
  const departureOrder = booking.segmentDepartureOrder ?? trip.departureStopOrder;
  const arrivalOrder = booking.segmentArrivalOrder ?? trip.arrivalStopOrder;

  const rows = trip.callingPattern
    .filter((stop) => stop.stopOrder >= departureOrder && stop.stopOrder <= arrivalOrder)
    .map((stop) => {
      const scheduledArrival = stop.arrivalTime ? new Date(stop.arrivalTime) : null;
      const scheduledDeparture = stop.departureTime ? new Date(stop.departureTime) : null;
      const arrivalShiftMinutes = delayMinutes;
      const departureShiftMinutes = departureDelay;
      return {
        ...stop,
        adjustedArrival: addMinutes(scheduledArrival, arrivalShiftMinutes),
        adjustedDeparture: addMinutes(scheduledDeparture, departureShiftMinutes),
        arrivalShiftMinutes,
        departureShiftMinutes,
        status: "future" as StopRow["status"],
      };
    });

  let passedStopIndex = -1;
  rows.forEach((stop, index) => {
    const departure = stop.adjustedDeparture ?? stop.adjustedArrival;
    if (departure && departure < now) {
      passedStopIndex = index;
    }
  });

  const visibleRows = rows.slice(passedStopIndex + 1);
  const currentIndex = visibleRows.findIndex((stop) => {
    const arrival = stop.adjustedArrival;
    const departure = stop.adjustedDeparture ?? arrival;
    return Boolean(arrival && departure && arrival <= now && now <= departure);
  });

  return visibleRows.map((stop, index) => {
    const isDestination = stop.stopOrder === arrivalOrder;
    if (isDestination) {
      return { ...stop, status: "destination" };
    }

    if (index === currentIndex) {
      return { ...stop, status: "current" };
    }

    if (currentIndex < 0 && index === 0) {
      return { ...stop, status: "next" };
    }

    if (currentIndex >= 0 && index === currentIndex + 1) {
      return { ...stop, status: "next" };
    }

    return stop;
  });
}

function addMinutes(value: Date | null, minutes: number) {
  return value ? new Date(value.getTime() + minutes * 60000) : null;
}

function formatStopTime(value: Date | null) {
  return value
    ? new Intl.DateTimeFormat("en", { hour: "2-digit", minute: "2-digit" }).format(value)
    : "--:--";
}

function formatShift(minutes: number) {
  if (minutes === 0) {
    return "";
  }

  return minutes > 0 ? ` +${minutes}` : ` ${minutes}`;
}

function formatStopStatus(status: StopRow["status"]) {
  switch (status) {
    case "current":
      return "Train is here now";
    case "next":
      return "Next station";
    case "destination":
      return "Your arrival station";
    default:
      return "Upcoming";
  }
}

export default CurrentTripPage;
