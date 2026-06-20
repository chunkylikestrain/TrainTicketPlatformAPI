import { ArrowLeftOutlined, CheckCircleOutlined, SaveOutlined, ShareAltOutlined } from "@ant-design/icons";
import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { createAdminRoute, getStations } from "../../api/adminApi";
import AdminLayout from "../../components/AdminLayout";
import type { Station } from "../../types/admin";

function AdminCreateRoutePage() {
  const navigate = useNavigate();
  const [stations, setStations] = useState<Station[]>([]);
  const [code, setCode] = useState("");
  const [fromId, setFromId] = useState("0");
  const [toId, setToId] = useState("0");
  const [distance, setDistance] = useState("168");
  const [duration, setDuration] = useState("01:21");
  const [status, setStatus] = useState("Active");
  const [saved, setSaved] = useState(false);

  useEffect(() => {
    getStations().then((loadedStations) => {
      setStations(loadedStations);
      setFromId(String(loadedStations[0]?.id ?? 0));
      setToId(String(loadedStations[1]?.id ?? loadedStations[0]?.id ?? 0));
    });
  }, []);

  const from = stations.find((station) => station.id === Number(fromId))?.name ?? "Origin station";
  const to = stations.find((station) => station.id === Number(toId))?.name ?? "Destination station";

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await createAdminRoute({
      code,
      departureStationId: Number(fromId),
      arrivalStationId: Number(toId),
      distanceKm: Number(distance),
      estimatedDurationMinutes: toMinutes(duration),
      operatingDays: "Daily",
      intermediateStops: "Debica\nTarnow",
      isActive: status === "Active",
    });
    setSaved(true);
    window.setTimeout(() => navigate("/admin/routes"), 900);
  }

  return (
    <AdminLayout>
      <section className="admin-page-heading">
        <div>
          <Link className="admin-back-link" to="/admin/routes"><ArrowLeftOutlined /> Back to routes</Link>
          <h1><ShareAltOutlined /> New Route</h1>
          <p>Create a route between stations so schedules can reuse it later.</p>
        </div>
      </section>

      {saved && (
        <div className="admin-save-banner">
          <CheckCircleOutlined /> Route saved locally. Backend connection comes next.
        </div>
      )}

      <section className="admin-form-layout">
        <form className="admin-detail-form" onSubmit={handleSubmit}>
          <fieldset>
            <legend>Route details</legend>
            <label>Route code<input value={code} onChange={(event) => setCode(event.target.value)} placeholder="Example: RZE-KRK" required /></label>
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
            <label>Intermediate stops<textarea defaultValue={"Debica\nTarnow"} rows={4} /></label>
            <label>Operating days<select defaultValue="Daily">
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
          <p>{from} to {to}</p>
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

export default AdminCreateRoutePage;
