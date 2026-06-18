import { useMemo, useState } from "react";
import { Link, useParams, useSearchParams } from "react-router-dom";

const upperSeats = [86, 85, 76, 75, 66, 65, 56, 55, 46, 45, 36, 35, 26, 25, 16, 15];
const middleSeats = [83, 74, 73, 64, 63, 54, 53, 44, 43, 34, 33, 24, 23, 14];
const lowerSeats = [82, 81, 72, 71, 62, 61, 52, 51, 42, 41, 32, 31, 22, 21, 12];
const availableSeats = new Set([76, 66, 65, 83, 74, 64, 46, 45, 43, 34, 25, 23, 14, 42, 41, 32, 22]);

type SeatButtonProps = {
  seat: number;
  selectedSeat: number | null;
  onSelect: (seat: number) => void;
};

function SeatButton({ seat, selectedSeat, onSelect }: SeatButtonProps) {
  const isAvailable = availableSeats.has(seat);
  const isSelected = selectedSeat === seat;

  return (
    <button
      type="button"
      className={`seat-cell ${isAvailable ? "seat-available" : "seat-unavailable"} ${
        isSelected ? "seat-selected" : ""
      }`}
      disabled={!isAvailable}
      onClick={() => onSelect(seat)}
    >
      {seat}
    </button>
  );
}

function SeatMapPage() {
  const { tripId } = useParams();
  const [searchParams] = useSearchParams();
  const selectedClass = searchParams.get("class") === "2" ? "2" : "1";
  const [selectedSeat, setSelectedSeat] = useState<number | null>(null);

  const confirmUrl = useMemo(() => {
    const params = new URLSearchParams({ class: selectedClass });

    if (selectedSeat) {
      params.set("car", "1");
      params.set("seat", String(selectedSeat));
    }

    return `/summary/${tripId}?${params.toString()}`;
  }, [selectedClass, selectedSeat, tripId]);

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
          <strong>EIP 3508</strong>
          <span>Friday, 19 June</span>
          <span>06:06 &gt; 07:27</span>
          <span>Rzeszow Glowny &gt; Krakow Gl.</span>
        </section>

        <section className="car-strip" aria-label="Train cars">
          <div className="train-direction">
            <span aria-hidden="true">-&gt;</span>
            <strong>Train direction</strong>
          </div>
          {[1, 2, 3, 4, 5, 6].map((car) => (
            <button type="button" className={car === 1 ? "car-tab car-tab-active" : "car-tab"} key={car}>
              <span>{car}</span>
              <small>Car {car}</small>
            </button>
          ))}
        </section>

        <section className="seat-map-stage">
          <button type="button" className="seat-nav" aria-label="Previous car">
            &lt;
          </button>
          <div className="coach-layout" aria-label="Car 1 seat map">
            <div className="seat-row">
              {upperSeats.map((seat) => (
                <SeatButton seat={seat} selectedSeat={selectedSeat} onSelect={setSelectedSeat} key={seat} />
              ))}
              <span className="coach-facility">WC</span>
            </div>
            <div className="seat-row seat-row-middle">
              {middleSeats.map((seat) => (
                <SeatButton seat={seat} selectedSeat={selectedSeat} onSelect={setSelectedSeat} key={seat} />
              ))}
            </div>
            <div className="coach-class-marker">1</div>
            <div className="seat-row">
              {lowerSeats.map((seat) => (
                <SeatButton seat={seat} selectedSeat={selectedSeat} onSelect={setSelectedSeat} key={seat} />
              ))}
              <span className="coach-facility">Bag</span>
            </div>
          </div>
          <button type="button" className="seat-nav" aria-label="Next car">
            &gt;
          </button>
        </section>

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
              <strong>Seat {selectedSeat}</strong>
              <strong>Car 1</strong>
            </>
          ) : (
            <strong>Not selected</strong>
          )}
        </section>

        <div className="seat-map-notice">
          Choose 1 seat to activate the confirm your choice button
        </div>

        <Link className={`seat-confirm ${selectedSeat ? "seat-confirm-active" : ""}`} to={selectedSeat ? confirmUrl : "#"}>
          I confirm my choice
        </Link>
      </section>
    </main>
  );
}

export default SeatMapPage;
