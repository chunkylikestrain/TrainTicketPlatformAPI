import { ArrowLeftOutlined, CalendarOutlined, CheckCircleOutlined, SaveOutlined } from "@ant-design/icons";
import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { createAdminSchedule, getAdminRoutes, getAdminTrains } from "../../api/adminApi";
import AdminLayout from "../../components/AdminLayout";
import type { AdminRoute, AdminTrain } from "../../types/admin";

function AdminCreateSchedulePage() {
  const navigate = useNavigate();
  const [trains, setTrains] = useState<AdminTrain[]>([]);
  const [routes, setRoutes] = useState<AdminRoute[]>([]);
  const [trainId, setTrainId] = useState("0");
  const [routeId, setRouteId] = useState("0");
  const [departure, setDeparture] = useState("2026-06-20T06:06");
  const [arrival, setArrival] = useState("2026-06-20T07:27");
  const [platform, setPlatform] = useState("2");
  const [track, setTrack] = useState("1");
  const [basePrice, setBasePrice] = useState("134.00");
  const [status, setStatus] = useState("On time");
  const [saved, setSaved] = useState(false);

  useEffect(() => {
    Promise.all([getAdminTrains(), getAdminRoutes()]).then(([loadedTrains, loadedRoutes]) => {
      setTrains(loadedTrains);
      setRoutes(loadedRoutes);
      setTrainId(String(loadedTrains[0]?.id ?? 0));
      setRouteId(String(loadedRoutes[0]?.id ?? 0));
    });
  }, []);

  const train = trains.find((item) => item.id === Number(trainId));
  const route = routes.find((item) => item.id === Number(routeId));
  const routeLabel = route ? `${route.departureStationName} -> ${route.arrivalStationName}` : "Route";

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await createAdminSchedule({
      trainId: Number(trainId),
      trainRouteId: Number(routeId),
      departureTime: departure,
      arrivalTime: arrival,
      platform,
      track,
      status,
      class1Price: Number(basePrice),
      class2Price: Math.round(Number(basePrice) * 0.67 * 100) / 100,
    });
    setSaved(true);
    window.setTimeout(() => navigate("/admin/schedules"), 900);
  }

  return (
    <AdminLayout>
      <section className="admin-page-heading">
        <div>
          <Link className="admin-back-link" to="/admin/schedules"><ArrowLeftOutlined /> Back to schedules</Link>
          <h1><CalendarOutlined /> New Schedule</h1>
          <p>Assign a train to a route with exact departure and arrival details.</p>
        </div>
      </section>

      {saved && (
        <div className="admin-save-banner">
          <CheckCircleOutlined /> Schedule saved locally. Backend connection comes next.
        </div>
      )}

      <section className="admin-form-layout">
        <form className="admin-detail-form" onSubmit={handleSubmit}>
          <fieldset>
            <legend>Connection</legend>
            <label>Train<select value={trainId} onChange={(event) => setTrainId(event.target.value)}>
              {trains.map((option) => <option key={option.id} value={option.id}>{option.code || option.name}</option>)}
            </select></label>
            <label>Route<select value={routeId} onChange={(event) => setRouteId(event.target.value)}>
              {routes.map((option) => <option key={option.id} value={option.id}>{option.code} - {option.departureStationName} to {option.arrivalStationName}</option>)}
            </select></label>
            <label>Status<select value={status} onChange={(event) => setStatus(event.target.value)}>
              <option>On time</option>
              <option>Delayed</option>
              <option>Cancelled</option>
            </select></label>
          </fieldset>

          <fieldset>
            <legend>Timing</legend>
            <label>Departure<input value={departure} onChange={(event) => setDeparture(event.target.value)} type="datetime-local" required /></label>
            <label>Arrival<input value={arrival} onChange={(event) => setArrival(event.target.value)} type="datetime-local" required /></label>
            <label>Platform<input value={platform} onChange={(event) => setPlatform(event.target.value)} required /></label>
            <label>Track<input value={track} onChange={(event) => setTrack(event.target.value)} required /></label>
          </fieldset>

          <fieldset>
            <legend>Ticketing</legend>
            <label>Base class 1 price<input value={basePrice} onChange={(event) => setBasePrice(event.target.value)} min="0" step="0.01" type="number" required /></label>
            <label>Class 2 multiplier<select defaultValue="0.67">
              <option value="0.67">67% of class 1</option>
              <option value="0.75">75% of class 1</option>
              <option value="0.85">85% of class 1</option>
            </select></label>
            <label>Dynamic pricing<select defaultValue="Preview only">
              <option>Preview only</option>
              <option>Enabled</option>
              <option>Disabled</option>
            </select></label>
          </fieldset>

          <div className="admin-form-actions">
            <button className="admin-primary-button" type="submit"><SaveOutlined /> Save schedule</button>
            <Link className="admin-secondary-button" to="/admin/schedules">Cancel</Link>
          </div>
        </form>

        <aside className="admin-preview-card">
          <span className="admin-preview-icon"><CalendarOutlined /></span>
          <small>Preview</small>
          <h2>{train?.code || train?.name || "Train"}</h2>
          <p>{routeLabel}</p>
          <dl>
            <div><dt>Departure</dt><dd>{departure.replace("T", " ")}</dd></div>
            <div><dt>Arrival</dt><dd>{arrival.replace("T", " ")}</dd></div>
            <div><dt>Platform</dt><dd>{platform}, track {track}</dd></div>
            <div><dt>Class 1</dt><dd>{basePrice} PLN</dd></div>
          </dl>
        </aside>
      </section>
    </AdminLayout>
  );
}

export default AdminCreateSchedulePage;
