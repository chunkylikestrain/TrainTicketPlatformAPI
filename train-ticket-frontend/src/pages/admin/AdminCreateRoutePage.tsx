import {
  ArrowDownOutlined,
  ArrowLeftOutlined,
  ArrowUpOutlined,
  CheckCircleOutlined,
  CloseOutlined,
  PlusOutlined,
  SaveOutlined,
  ShareAltOutlined,
} from "@ant-design/icons";
import axios from "axios";
import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { createAdminRoute, getAdminRoute, getStations, updateAdminRoute } from "../../api/adminApi";
import AdminLayout from "../../components/AdminLayout";
import type { Station } from "../../types/admin";

function AdminCreateRoutePage() {
  const navigate = useNavigate();
  const { routeId } = useParams();
  const editingRouteId = Number(routeId);
  const isEditing = Number.isInteger(editingRouteId) && editingRouteId > 0;
  const [stations, setStations] = useState<Station[]>([]);
  const [fromId, setFromId] = useState("0");
  const [toId, setToId] = useState("0");
  const [distance, setDistance] = useState("168");
  const [duration, setDuration] = useState("01:21");
  const [status, setStatus] = useState("Active");
  const [operatingDays, setOperatingDays] = useState("Daily");
  const [stopSearch, setStopSearch] = useState("");
  const [intermediateStops, setIntermediateStops] = useState<Station[]>([]);
  const [saved, setSaved] = useState(false);
  const [loadError, setLoadError] = useState("");
  const [saveError, setSaveError] = useState("");

  useEffect(() => {
    async function loadRouteForm() {
      try {
        const loadedStations = await getStations();
        setStations(loadedStations);

        if (!isEditing) {
          setFromId(String(loadedStations[0]?.id ?? 0));
          setToId(String(loadedStations[1]?.id ?? loadedStations[0]?.id ?? 0));
          return;
        }

        const route = await getAdminRoute(editingRouteId);
        setFromId(String(route.departureStationId));
        setToId(String(route.arrivalStationId));
        setDistance(String(route.distanceKm));
        setDuration(toTimeValue(route.estimatedDurationMinutes));
        setStatus(route.isActive ? "Active" : "Draft");
        setOperatingDays(route.operatingDays || "Daily");

        const stopIds = route.stops.length > 0
          ? route.stops.sort((left, right) => left.stopOrder - right.stopOrder).map((stop) => stop.stationId)
          : route.intermediateStopStationIds;
        setIntermediateStops(
          stopIds
            .map((stationId) => loadedStations.find((station) => station.id === stationId))
            .filter((station): station is Station => Boolean(station)));
      } catch {
        setLoadError("Could not load route details from the API.");
      }
    }

    loadRouteForm();
  }, [editingRouteId, isEditing]);

  const fromStation = stations.find((station) => station.id === Number(fromId));
  const toStation = stations.find((station) => station.id === Number(toId));
  const from = fromStation?.name ?? "Origin station";
  const to = toStation?.name ?? "Destination station";
  const code = fromStation && toStation ? `${fromStation.code}-${toStation.code}` : "";
  const routeName = fromStation && toStation ? `${fromStation.name} to ${toStation.name}` : "";
  const routeFingerprint = fromStation && toStation
    ? [fromStation, ...intermediateStops, toStation].map((station) => station.code.toUpperCase()).join(">")
    : "";
  const adminDisplayName = fromStation && toStation
    ? intermediateStops.length > 0
      ? `${fromStation.name} to ${toStation.name} via ${intermediateStops.slice(0, 3).map((station) => station.name).join(", ")}`
      : routeName
    : "";
  const stopOptions = stations.filter((station) =>
    station.id !== Number(fromId) &&
    station.id !== Number(toId) &&
    !intermediateStops.some((stop) => stop.id === station.id));

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSaveError("");
    setSaved(false);

    const routePayload = {
      code,
      name: routeName,
      adminDisplayName,
      routeFingerprint,
      departureStationId: Number(fromId),
      arrivalStationId: Number(toId),
      distanceKm: Number(distance),
      estimatedDurationMinutes: toMinutes(duration),
      operatingDays,
      intermediateStops: intermediateStops.map((station) => station.name).join("\n"),
      intermediateStopStationIds: intermediateStops.map((station) => station.id),
      stops: [],
      isActive: status === "Active",
    };

    try {
      if (isEditing) {
        await updateAdminRoute({
          id: editingRouteId,
          departureStationName: from,
          arrivalStationName: to,
          ...routePayload,
        });
      } else {
        await createAdminRoute(routePayload);
      }

      setSaved(true);
      window.setTimeout(() => navigate("/admin/routes"), 900);
    } catch (error) {
      setSaveError(getRouteSaveError(error));
    }
  }

  function addIntermediateStop() {
    const selected = findStationBySearchValue(stopSearch, stopOptions);
    if (!selected)
      return;

    setIntermediateStops((currentStops) => [...currentStops, selected]);
    setStopSearch("");
  }

  function moveIntermediateStop(index: number, direction: -1 | 1) {
    const nextIndex = index + direction;
    if (nextIndex < 0 || nextIndex >= intermediateStops.length)
      return;

    setIntermediateStops((currentStops) => {
      const nextStops = [...currentStops];
      const [selected] = nextStops.splice(index, 1);
      nextStops.splice(nextIndex, 0, selected);
      return nextStops;
    });
  }

  function removeIntermediateStop(id: number) {
    setIntermediateStops((currentStops) => currentStops.filter((station) => station.id !== id));
  }

  return (
    <AdminLayout>
      <section className="admin-page-heading">
        <div>
          <Link className="admin-back-link" to="/admin/routes"><ArrowLeftOutlined /> Back to routes</Link>
          <h1><ShareAltOutlined /> {isEditing ? "Edit Route" : "New Route"}</h1>
          <p>{isEditing ? "Adjust the route stations, travel timing, stops, and availability." : "Create a route between stations so schedules can reuse it later."}</p>
        </div>
      </section>

      {loadError && <div className="admin-save-banner">{loadError}</div>}
      {saveError && <div className="admin-save-banner admin-danger-panel">{saveError}</div>}
      {saved && (
        <div className="admin-save-banner">
          <CheckCircleOutlined /> Route saved.
        </div>
      )}

      <section className="admin-form-layout">
        <form className="admin-detail-form" onSubmit={handleSubmit}>
          <fieldset>
            <legend>Route details</legend>
            <label>Route code<input value={code} readOnly placeholder="Select stations first" required /></label>
            <label>Route name<input value={routeName} readOnly placeholder="Select stations first" /></label>
            <label>Origin station<select value={fromId} onChange={(event) => setFromId(event.target.value)} required>
              {stations.map((station) => <option key={station.id} value={station.id}>{station.name}</option>)}
            </select></label>
            <label>Destination station<select value={toId} onChange={(event) => setToId(event.target.value)} required>
              {stations.map((station) => <option key={station.id} value={station.id}>{station.name}</option>)}
            </select></label>
            <label>Status<select value={status} onChange={(event) => setStatus(event.target.value)}>
              <option>Active</option>
              <option>Draft</option>
              <option>Suspended</option>
            </select></label>
          </fieldset>

          <fieldset>
            <legend>Travel planning</legend>
            <label>Distance in km<input value={distance} onChange={(event) => setDistance(event.target.value)} min="1" type="number" required /></label>
            <label>Estimated duration<input value={duration} onChange={(event) => setDuration(event.target.value)} type="time" required /></label>
            <div className="admin-station-stack">
              <label htmlFor="intermediate-stop-search">Intermediate stops</label>
              <div className="admin-stop-search">
                <input
                  id="intermediate-stop-search"
                  list="intermediate-stop-options"
                  value={stopSearch}
                  onChange={(event) => setStopSearch(event.target.value)}
                  placeholder="Search station to add"
                />
                <datalist id="intermediate-stop-options">
                  {stopOptions.map((station) => (
                    <option key={station.id} value={stationOptionLabel(station)} />
                  ))}
                </datalist>
                <button type="button" onClick={addIntermediateStop}><PlusOutlined /> Add</button>
              </div>
              <ol className="admin-stop-list">
                {intermediateStops.length === 0 && <li className="admin-stop-empty">No intermediate stops added.</li>}
                {intermediateStops.map((station, index) => (
                  <li key={station.id}>
                    <span>
                      <b>{index + 1}</b>
                      {station.name}
                      <small>{station.code}</small>
                    </span>
                    <button type="button" onClick={() => moveIntermediateStop(index, -1)} disabled={index === 0} aria-label={`Move ${station.name} up`}>
                      <ArrowUpOutlined />
                    </button>
                    <button type="button" onClick={() => moveIntermediateStop(index, 1)} disabled={index === intermediateStops.length - 1} aria-label={`Move ${station.name} down`}>
                      <ArrowDownOutlined />
                    </button>
                    <button type="button" onClick={() => removeIntermediateStop(station.id)} aria-label={`Remove ${station.name}`}>
                      <CloseOutlined />
                    </button>
                  </li>
                ))}
              </ol>
            </div>
            <label>Operating days<select value={operatingDays} onChange={(event) => setOperatingDays(event.target.value)}>
              <option>Daily</option>
              <option>Weekdays</option>
              <option>Weekends</option>
              <option>Custom</option>
            </select></label>
          </fieldset>

          <div className="admin-form-actions">
            <button className="admin-primary-button" type="submit"><SaveOutlined /> Save route</button>
            <Link className="admin-secondary-button" to="/admin/routes">Cancel</Link>
          </div>
        </form>

        <aside className="admin-preview-card">
          <span className="admin-preview-icon"><ShareAltOutlined /></span>
          <small>Preview</small>
          <h2>{code || "Route code"}</h2>
          <p>{adminDisplayName || `${from} to ${to}`}</p>
          {routeFingerprint && <small>{routeFingerprint}</small>}
          {intermediateStops.length > 0 && (
            <ol className="admin-preview-stops">
              {intermediateStops.map((station) => <li key={station.id}>{station.name}</li>)}
            </ol>
          )}
          <dl>
            <div><dt>Distance</dt><dd>{distance || 0} km</dd></div>
            <div><dt>Duration</dt><dd>{duration}</dd></div>
            <div><dt>Status</dt><dd>{status}</dd></div>
          </dl>
        </aside>
      </section>
    </AdminLayout>
  );
}

function toMinutes(value: string) {
  const [hours, minutes] = value.split(":").map(Number);
  return (hours * 60) + minutes;
}

function toTimeValue(totalMinutes: number) {
  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;
  return `${String(hours).padStart(2, "0")}:${String(minutes).padStart(2, "0")}`;
}

function stationOptionLabel(station: Station) {
  return `${station.name} (${station.code})`;
}

function findStationBySearchValue(value: string, stations: Station[]) {
  const normalizedValue = value.trim().toLowerCase();
  return stations.find((station) =>
    station.name.toLowerCase() === normalizedValue ||
    station.code.toLowerCase() === normalizedValue ||
    stationOptionLabel(station).toLowerCase() === normalizedValue);
}

function getRouteSaveError(error: unknown) {
  if (!axios.isAxiosError(error))
    return "Route could not be saved. Please try again.";

  if (error.response?.status === 409) {
    return typeof error.response.data === "string"
      ? error.response.data
      : "A route with this code already exists.";
  }

  if (error.response?.status === 400) {
    return typeof error.response.data === "string"
      ? error.response.data
      : "Route details are invalid. Please check the selected stations and stops.";
  }

  if (error.response?.status === 401 || error.response?.status === 403)
    return "Your admin session expired. Please log in again.";

  if (error.response)
    return `Route could not be saved. API returned ${error.response.status}.`;

  return "Route could not be saved. Check that the API is running.";
}

export default AdminCreateRoutePage;
