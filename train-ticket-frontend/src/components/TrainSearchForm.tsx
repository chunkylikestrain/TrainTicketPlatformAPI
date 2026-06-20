import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { getStations } from "../api/stationApi";
import type { Station } from "../types/station";

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
  const [stations, setStations] = useState<Station[]>([]);
  const [stationError, setStationError] = useState("");
  const [searchError, setSearchError] = useState("");

  useEffect(() => {
    getStations()
      .then((loadedStations) => {
        setStations(loadedStations);
        setDepartureStation(getDefaultStationName(loadedStations, "Rzeszów Główny", "RZE"));
        setArrivalStation(getDefaultStationName(loadedStations, "Gdynia Główna", "GDY"));
      })
      .catch(() => setStationError("Station suggestions are unavailable. You can still type a station name."));
  }, []);

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSearchError("");

    const departure = resolveStationInput(departureStation, stations);
    const arrival = resolveStationInput(arrivalStation, stations);

    if (!departure || !arrival) {
      setSearchError("Choose stations from the suggestions before searching.");
      return;
    }

    if (departure.id === arrival.id) {
      setSearchError("Departure and arrival stations must be different.");
      return;
    }

    const query = new URLSearchParams({
      departureStation: departure.name,
      arrivalStation: arrival.name,
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
          list="departure-station-suggestions"
          value={departureStation}
          onChange={(event) => setDepartureStation(event.target.value)}
          onBlur={() => setDepartureStation(normalizeStationInput(departureStation, stations))}
          name="departureStation"
          type="text"
          autoComplete="off"
          required
        />
      </label>

      <button className="swap-button" type="button" onClick={swapStations} aria-label="Swap stations">
        <span aria-hidden="true">Swap</span>
      </button>

      <label className="journey-field">
        <span>To</span>
        <input
          list="arrival-station-suggestions"
          value={arrivalStation}
          onChange={(event) => setArrivalStation(event.target.value)}
          onBlur={() => setArrivalStation(normalizeStationInput(arrivalStation, stations))}
          name="arrivalStation"
          type="text"
          autoComplete="off"
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

      <datalist id="departure-station-suggestions">
        {stations.map((station) => <option key={station.id} value={stationOptionLabel(station)} />)}
      </datalist>
      <datalist id="arrival-station-suggestions">
        {stations.map((station) => <option key={station.id} value={stationOptionLabel(station)} />)}
      </datalist>

      {(stationError || searchError) && (
        <p className="journey-search-message">{searchError || stationError}</p>
      )}
    </form>
  );
}

function stationOptionLabel(station: Station) {
  const locality = station.localityName || station.city;
  return locality && locality !== station.name
    ? `${station.name} (${station.code}, ${locality})`
    : `${station.name} (${station.code})`;
}

function getDefaultStationName(stations: Station[], fallbackName: string, fallbackCode: string) {
  return stations.find((station) => station.code.toLowerCase() === fallbackCode.toLowerCase())?.name ?? fallbackName;
}

function normalizeStationInput(value: string, stations: Station[]) {
  return resolveStationInput(value, stations)?.name ?? value;
}

function resolveStationInput(value: string, stations: Station[]) {
  const normalizedValue = normalizeSearchText(value);
  if (!normalizedValue)
    return undefined;

  return stations.find((station) => {
    const candidates = [
      station.name,
      station.code,
      station.city,
      station.localityName ?? "",
      stationOptionLabel(station),
    ];

    return candidates.some((candidate) => normalizeSearchText(candidate) === normalizedValue);
  });
}

function normalizeSearchText(value: string) {
  return value
    .trim()
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLowerCase();
}

export default TrainSearchForm;
