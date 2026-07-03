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
  const now = new Date();
  const month = String(now.getMonth() + 1).padStart(2, "0");
  const day = String(now.getDate()).padStart(2, "0");
  return `${now.getFullYear()}-${month}-${day}`;
}

function getCurrentLocalTime() {
  const now = new Date();
  const hours = String(now.getHours()).padStart(2, "0");
  const minutes = String(now.getMinutes()).padStart(2, "0");
  return `${hours}:${minutes}`;
}

function addDays(dateValue: string, days: number) {
  const date = new Date(`${dateValue}T12:00:00`);
  date.setDate(date.getDate() + days);
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${date.getFullYear()}-${month}-${day}`;
}

function TrainSearchForm({ compact = false }: TrainSearchFormProps) {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const initialCounts = getPassengerCounts(searchParams);
  const initialTripType = searchParams.get("tripType") === "roundTrip" ? "roundTrip" : "oneWay";
  const [departureStation, setDepartureStation] = useState(searchParams.get("departureStation") ?? "Rzeszow Glowny");
  const [arrivalStation, setArrivalStation] = useState(searchParams.get("arrivalStation") ?? "Gdynia Glowna");
  const [date, setDate] = useState(searchParams.get("date") ?? getToday());
  const [time, setTime] = useState(searchParams.get("time") ?? getCurrentLocalTime());
  const [tripType, setTripType] = useState<"oneWay" | "roundTrip">(initialTripType);
  const [returnDate, setReturnDate] = useState(searchParams.get("returnDate") ?? addDays(searchParams.get("date") ?? getToday(), 1));
  const [returnTime, setReturnTime] = useState(searchParams.get("returnTime") ?? getCurrentLocalTime());
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

  useEffect(() => {
    const departureFromUrl = searchParams.get("departureStation");
    const arrivalFromUrl = searchParams.get("arrivalStation");

    if (departureFromUrl) {
      setDepartureStation(departureFromUrl);
    }

    if (arrivalFromUrl) {
      setArrivalStation(arrivalFromUrl);
    }
  }, [searchParams, stations]);

  useEffect(() => {
    if (tripType === "roundTrip" && returnDate < date) {
      setReturnDate(addDays(date, 1));
    }
  }, [date, returnDate, tripType]);

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

    if (tripType === "roundTrip" && `${returnDate}T${returnTime || "00:00"}` < `${date}T${time || "00:00"}`) {
      setSearchError("Return date and time must be after the outbound departure.");
      return;
    }

    const query = new URLSearchParams({
      departureStation: departure.name,
      arrivalStation: arrival.name,
      date,
      time,
    });
    query.set("tripType", tripType);
    if (tripType === "roundTrip") {
      query.set("returnDate", returnDate);
      query.set("returnTime", returnTime);
    }
    writePurchasePreferenceParams(query, counts, visibleDiscounts, filters);

    window.sessionStorage.removeItem("railbook-round-trip-outbound");
    window.sessionStorage.removeItem("railbook-round-trip-return");
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
  currentPreferenceParams.set("departureStation", departureStation);
  currentPreferenceParams.set("arrivalStation", arrivalStation);
  currentPreferenceParams.set("date", date);
  currentPreferenceParams.set("time", time);
  currentPreferenceParams.set("tripType", tripType);
  if (tripType === "roundTrip") {
    currentPreferenceParams.set("returnDate", returnDate);
    currentPreferenceParams.set("returnTime", returnTime);
  } else {
    currentPreferenceParams.delete("returnDate");
    currentPreferenceParams.delete("returnTime");
  }
  const homeReturnUrl = `/?${currentPreferenceParams.toString()}`;

  return (
    <form className={`journey-search ${compact ? "journey-search-compact" : ""}`} onSubmit={handleSubmit}>
      <fieldset className="trip-type-toggle">
        <legend>Journey type</legend>
        <label>
          <input
            checked={tripType === "oneWay"}
            name="tripType"
            onChange={() => setTripType("oneWay")}
            type="radio"
          />
          <span>One way</span>
        </label>
        <label>
          <input
            checked={tripType === "roundTrip"}
            name="tripType"
            onChange={() => setTripType("roundTrip")}
            type="radio"
          />
          <span>Round trip</span>
        </label>
      </fieldset>

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

      {tripType === "roundTrip" && (
        <section className="return-journey-fields" aria-label="Return journey">
          <strong>Return journey</strong>
          <div>
            <span>{arrivalStation || "Arrival"} -&gt; {departureStation || "Departure"}</span>
          </div>
          <label className="journey-field">
            <span>Return date</span>
            <input
              min={date}
              name="returnDate"
              onChange={(event) => setReturnDate(event.target.value)}
              required
              type="date"
              value={returnDate}
            />
          </label>
          <label className="journey-field">
            <span>Return time</span>
            <input
              name="returnTime"
              onChange={(event) => setReturnTime(event.target.value)}
              type="time"
              value={returnTime}
            />
          </label>
        </section>
      )}

      <section className="journey-preferences" aria-label="Search preferences">
        <div className="preference-box">
          <div>
            <span>Filters</span>
            <strong>{filters.length > 0 ? `${filters.length} selected` : "Any train"}</strong>
          </div>
          <Link to={buildFilterSelectionUrl(homeReturnUrl, currentPreferenceParams)}>Change filters</Link>
        </div>

        <div className="preference-box">
          <div>
            <span>Discounts</span>
            <strong>{formatDiscountSummary(visibleDiscounts)}</strong>
          </div>
          <Link to={buildDiscountSelectionUrl(homeReturnUrl, currentPreferenceParams)}>Change discounts</Link>
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
    .replace(/[Łł]/g, "l")
    .replace(/[Đđ]/g, "d")
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLowerCase();
}

export default TrainSearchForm;
