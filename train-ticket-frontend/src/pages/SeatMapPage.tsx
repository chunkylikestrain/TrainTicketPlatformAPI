import { useEffect, useMemo, useState } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom";
import { createBookingHold } from "../api/bookingApi";
import { getTripById, getTripSeats } from "../api/tripApi";
import type { TripDetails, TripSeatAvailability } from "../types/trip";

function formatDate(value?: string) {
  if (!value) {
    return "Selected date";
  }

  return new Intl.DateTimeFormat("en", {
    weekday: "long",
    day: "numeric",
    month: "long",
  }).format(new Date(value));
}

function formatTime(value?: string) {
  if (!value) {
    return "--:--";
  }

  return new Intl.DateTimeFormat("en", {
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function chunkSeats(seats: TripSeatAvailability[]) {
  const rows: TripSeatAvailability[][] = [[], [], []];

  seats.forEach((seat, index) => {
    rows[index % rows.length].push(seat);
  });

  return rows;
}

function SeatMapPage() {
  const { tripId = "" } = useParams();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const selectedClass = searchParams.get("class") === "2" ? "2" : "1";
  const [trip, setTrip] = useState<TripDetails | null>(null);
  const [seats, setSeats] = useState<TripSeatAvailability[]>([]);
  const [selectedSeat, setSelectedSeat] = useState<TripSeatAvailability | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isCreatingHold, setIsCreatingHold] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!tripId) {
      return;
    }

    setIsLoading(true);
    setError("");

    Promise.all([getTripById(tripId), getTripSeats(tripId)])
      .then(([tripDetails, seatList]) => {
        setTrip(tripDetails);
        setSeats(seatList);
      })
      .catch(() => {
        setError("Could not load live seat availability. Check that the API is running and the trip exists.");
      })
      .finally(() => setIsLoading(false));
  }, [tripId]);

  const seatRows = useMemo(() => chunkSeats(seats), [seats]);

  async function handleConfirmSeat() {
    if (!trip || !selectedSeat) {
      return;
    }

    setIsCreatingHold(true);
    setError("");

    try {
      const booking = await createBookingHold({
        trainId: trip.trainId,
        tripId: trip.tripId,
        seatId: selectedSeat.seatId,
        travelDate: trip.departureTime,
      });

      const params = new URLSearchParams({
        class: selectedClass,
        car: selectedSeat.coach,
        seat: selectedSeat.number,
        bookingId: String(booking.id),
      });

      navigate(`/summary/${tripId}?${params.toString()}`);
    } catch {
      setError("Could not reserve this seat. It may have just been booked by someone else.");
    } finally {
      setIsCreatingHold(false);
    }
  }

  return (
    <main className="seat-map-page">
      <section className="seat-map-panel">
        <div className="seat-map-header">
          <h1>Choose your seat on the plan</h1>
          <Link to={`/summary/${tripId}?class=${selectedClass}`} aria-label="Close seat map">
            x
          </Link>
        </div>

        <div className="seat-list-toggle">
          <strong>I want to select a place from the list</strong>
          <span aria-hidden="true" />
        </div>

        <section className="seat-trip-meta">
          <strong>{trip?.trainName ?? "Train"}</strong>
          <span>{formatDate(trip?.departureTime)}</span>
          <span>{formatTime(trip?.departureTime)} &gt; {formatTime(trip?.arrivalTime)}</span>
          <span>
            {trip?.departureStationName ?? "Departure"} &gt; {trip?.arrivalStationName ?? "Arrival"}
          </span>
        </section>

        <section className="car-strip" aria-label="Train cars">
          <div className="train-direction">
            <span aria-hidden="true">-&gt;</span>
            <strong>Train direction</strong>
          </div>
          {[...new Set(seats.map((seat) => seat.coach))].map((coach, index) => (
            <button type="button" className={index === 0 ? "car-tab car-tab-active" : "car-tab"} key={coach}>
              <span>{index + 1}</span>
              <small>Car {coach}</small>
            </button>
          ))}
        </section>

        {isLoading && <div className="seat-map-notice">Loading live seats...</div>}
        {error && <div className="seat-map-notice">{error}</div>}

        {!isLoading && seats.length > 0 && (
          <section className="seat-map-stage">
            <button type="button" className="seat-nav" aria-label="Previous car">
              &lt;
            </button>
            <div className="coach-layout" aria-label="Car seat map">
              {seatRows.map((row, rowIndex) => (
                <div className={rowIndex === 1 ? "seat-row seat-row-middle" : "seat-row"} key={rowIndex}>
                  {row.map((seat) => {
                    const isSelected = selectedSeat?.seatId === seat.seatId;

                    return (
                      <button
                        type="button"
                        className={`seat-cell ${seat.isAvailable ? "seat-available" : "seat-unavailable"} ${
                          isSelected ? "seat-selected" : ""
                        }`}
                        disabled={!seat.isAvailable}
                        onClick={() => setSelectedSeat(seat)}
                        key={seat.seatId}
                      >
                        {seat.number}
                      </button>
                    );
                  })}
                  {rowIndex === 0 && <span className="coach-facility">WC</span>}
                  {rowIndex === 2 && <span className="coach-facility">Bag</span>}
                </div>
              ))}
              <div className="coach-class-marker">{selectedClass}</div>
            </div>
            <button type="button" className="seat-nav" aria-label="Next car">
              &gt;
            </button>
          </section>
        )}

        <section className="seat-legend">
          <span>Legend</span>
          <span><b className="legend-available" /> available</span>
          <span><b className="legend-unavailable" /> unavailable</span>
          <span><b className="legend-selected" /> selected</span>
        </section>

        <section className="seat-location-card">
          <span>Location</span>
          <strong>Passenger 1</strong>
          {selectedSeat ? (
            <>
              <strong>Class {selectedClass}</strong>
              <strong>Seat {selectedSeat.number}</strong>
              <strong>Car {selectedSeat.coach}</strong>
            </>
          ) : (
            <strong>Not selected</strong>
          )}
        </section>

        <div className="seat-map-notice">
          Choose 1 seat to activate the confirm your choice button
        </div>

        <button
          className={`seat-confirm ${selectedSeat ? "seat-confirm-active" : ""}`}
          type="button"
          disabled={!selectedSeat || isCreatingHold}
          onClick={handleConfirmSeat}
        >
          {isCreatingHold ? "Reserving..." : "I confirm my choice"}
        </button>
      </section>
    </main>
  );
}

export default SeatMapPage;
