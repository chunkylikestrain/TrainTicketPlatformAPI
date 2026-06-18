import { useState } from "react";
import type { FormEvent } from "react";
import { useNavigate } from "react-router-dom";

type TrainSearchFormProps = {
  compact?: boolean;
};

function getToday() {
  return new Date().toISOString().slice(0, 10);
}

function TrainSearchForm({ compact = false }: TrainSearchFormProps) {
  const navigate = useNavigate();
  const [departureStation, setDepartureStation] = useState("Rzeszow Glowny");
  const [arrivalStation, setArrivalStation] = useState("Gdynia Glowna");
  const [date, setDate] = useState(getToday());
  const [time, setTime] = useState("13:38");

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const query = new URLSearchParams({
      departureStation,
      arrivalStation,
      date,
    });

    navigate(`/search?${query.toString()}`);
  }

  function swapStations() {
    setDepartureStation(arrivalStation);
    setArrivalStation(departureStation);
  }

  return (
    <form className={`journey-search ${compact ? "journey-search-compact" : ""}`} onSubmit={handleSubmit}>
      <label className="journey-field">
        <span>From</span>
        <input
          value={departureStation}
          onChange={(event) => setDepartureStation(event.target.value)}
          name="departureStation"
          type="text"
          required
        />
      </label>

      <button className="swap-button" type="button" onClick={swapStations} aria-label="Swap stations">
        <span aria-hidden="true">Swap</span>
      </button>

      <label className="journey-field">
        <span>To</span>
        <input
          value={arrivalStation}
          onChange={(event) => setArrivalStation(event.target.value)}
          name="arrivalStation"
          type="text"
          required
        />
      </label>

      <label className="journey-field">
        <span>When</span>
        <input value={date} onChange={(event) => setDate(event.target.value)} name="date" type="date" required />
      </label>

      <label className="journey-field">
        <span>Time</span>
        <input value={time} onChange={(event) => setTime(event.target.value)} name="time" type="time" />
      </label>

      <button className="search-button" type="submit">
        Search
      </button>
    </form>
  );
}

export default TrainSearchForm;
