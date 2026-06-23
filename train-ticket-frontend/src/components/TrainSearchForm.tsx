import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { getStations } from "../api/stationApi";
import type { Station } from "../types/station";
import {
  buildDiscountSelectionUrl,
  buildFilterSelectionUrl,
  formatDiscountSummary,
  formatPassengerSummary,
  getDiscountCodes,
  getFilterCodes,
  getPassengerCounts,
  getPassengerTotal,
  writePurchasePreferenceParams,
} from "../utils/purchasePreferences";

type TrainSearchFormProps = {
  compact?: boolean;
};

function getToday() {
  return new Date().toISOString().slice(0, 10);
}

function TrainSearchForm({ compact = false }: TrainSearchFormProps) {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const initialCounts = getPassengerCounts(searchParams);
  const [departureStation, setDepartureStation] = useState("Rzeszow Glowny");
  const [arrivalStation, setArrivalStation] = useState("Gdynia Glowna");
  const [date, setDate] = useState(getToday());
  const [time, setTime] = useState("13:38");
  const [adults, setAdults] = useState(initialCounts.adults);
  const [children, setChildren] = useState(initialCounts.children);
  const filters = getFilterCodes(searchParams);
  const [discounts, setDiscounts] = useState(() => getDiscountCodes(searchParams, initialCounts));
  const [stations, setStations] = useState<Station[]>([]);
  const [stationError, setStationError] = useState("");
  const [searchError, setSearchError] = useState("");
  const counts = { adults, children };
  const totalTravelers = getPassengerTotal(counts);
  const visibleDiscounts = discounts.slice(0, totalTravelers);

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
    writePurchasePreferenceParams(query, counts, visibleDiscounts, filters);

    navigate(`/search?${query.toString()}`);
  }

  function swapStations() {
    setDepartureStation(arrivalStation);
    setArrivalStation(departureStation);
  }

  function updateAdults(nextAdults: number) {
    const normalizedAdults = Math.max(1, Math.min(6 - children, nextAdults));
    setAdults(normalizedAdults);
    resizeDiscounts(normalizedAdults, children);
  }

  function updateChildren(nextChildren: number) {
    const normalizedChildren = Math.max(0, Math.min(6 - adults, nextChildren));
    setChildren(normalizedChildren);
    resizeDiscounts(adults, normalizedChildren);
  }

  function resizeDiscounts(nextAdults: number, nextChildren: number) {
    const nextTotal = Math.max(1, nextAdults + nextChildren);
    setDiscounts((current) =>
      Array.from({ length: nextTotal }, (_, index) => current[index] ?? (index < nextAdults ? "normal" : "child37")),
    );
  }

  const currentPreferenceParams = writePurchasePreferenceParams(
    new URLSearchParams(searchParams),
    counts,
    visibleDiscounts,
    filters,
  );

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

      <section className="journey-preferences" aria-label="Search preferences">
        <div className="preference-box">
          <div>
            <span>Filters</span>
            <strong>{filters.length > 0 ? `${filters.length} selected` : "Any train"}</strong>
          </div>
          <Link to={buildFilterSelectionUrl("/", currentPreferenceParams)}>Change filters</Link>
        </div>

        <div className="preference-box">
          <div>
            <span>Discounts</span>
            <strong>{formatDiscountSummary(visibleDiscounts)}</strong>
          </div>
          <Link to={buildDiscountSelectionUrl("/", currentPreferenceParams)}>Change discounts</Link>
        </div>

        <div className="preference-box">
          <div>
            <span>Number of travelers</span>
            <strong>{formatPassengerSummary(counts)}</strong>
          </div>
          <div className="traveler-counters">
            <PreferenceCounter label="Adults" value={adults} min={1} max={6 - children} onChange={updateAdults} />
            <PreferenceCounter label="Children" value={children} min={0} max={6 - adults} onChange={updateChildren} />
          </div>
        </div>
      </section>

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

type PreferenceCounterProps = {
  label: string;
  value: number;
  min: number;
  max: number;
  onChange: (value: number) => void;
};

function PreferenceCounter({ label, value, min, max, onChange }: PreferenceCounterProps) {
  return (
    <div className="preference-counter">
      <span>{label}</span>
      <button type="button" disabled={value <= min} onClick={() => onChange(value - 1)} aria-label={`Remove ${label}`}>
        -
      </button>
      <strong>{value}</strong>
      <button type="button" disabled={value >= max} onClick={() => onChange(value + 1)} aria-label={`Add ${label}`}>
        +
      </button>
    </div>
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
