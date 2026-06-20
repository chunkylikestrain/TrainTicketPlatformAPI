import { ArrowLeftOutlined, CheckCircleOutlined, ProfileOutlined, SaveOutlined } from "@ant-design/icons";
import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { createAdminTrain } from "../../api/adminApi";
import AdminLayout from "../../components/AdminLayout";

function AdminCreateTrainPage() {
  const navigate = useNavigate();
  const [code, setCode] = useState("IC-");
  const [name, setName] = useState("");
  const [carriages, setCarriages] = useState("2");
  const [seatsPerCarriage, setSeatsPerCarriage] = useState("40");
  const [trainType, setTrainType] = useState("InterCity");
  const [status, setStatus] = useState("Active");
  const [saved, setSaved] = useState(false);

  const totalSeats = Number(carriages || 0) * Number(seatsPerCarriage || 0);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await createAdminTrain({
      code,
      name,
      type: trainType,
      carriageCount: Number(carriages),
      seatsPerCarriage: Number(seatsPerCarriage),
      status,
      departureStation: "",
      arrivalStation: "",
      departureTime: new Date().toISOString(),
      arrivalTime: new Date().toISOString(),
    });
    setSaved(true);
    window.setTimeout(() => navigate("/admin/trains"), 900);
  }

  return (
    <AdminLayout>
      <section className="admin-page-heading">
        <div>
          <Link className="admin-back-link" to="/admin/trains"><ArrowLeftOutlined /> Back to trains</Link>
          <h1><ProfileOutlined /> New Train</h1>
          <p>Create a physical train record before assigning it to routes and schedules.</p>
        </div>
      </section>

      {saved && (
        <div className="admin-save-banner">
          <CheckCircleOutlined /> Train saved locally. Backend connection comes next.
        </div>
      )}

      <section className="admin-form-layout">
        <form className="admin-detail-form" onSubmit={handleSubmit}>
          <fieldset>
            <legend>Train identity</legend>
            <label>Train code<input value={code} onChange={(event) => setCode(event.target.value)} required /></label>
            <label>Display name<input value={name} onChange={(event) => setName(event.target.value)} placeholder="Example: Baltic Express" required /></label>
            <label>Train type<select value={trainType} onChange={(event) => setTrainType(event.target.value)}>
              <option>InterCity</option>
              <option>Express InterCity</option>
              <option>Regional</option>
              <option>Night train</option>
            </select></label>
            <label>Status<select value={status} onChange={(event) => setStatus(event.target.value)}>
              <option>Active</option>
              <option>Maintenance</option>
              <option>Retired</option>
            </select></label>
          </fieldset>

          <fieldset>
            <legend>Capacity plan</legend>
            <label>Carriages<input value={carriages} onChange={(event) => setCarriages(event.target.value)} min="1" type="number" required /></label>
            <label>Seats per carriage<input value={seatsPerCarriage} onChange={(event) => setSeatsPerCarriage(event.target.value)} min="1" type="number" required /></label>
            <label>Default class layout<select defaultValue="Mixed">
              <option>Mixed</option>
              <option>Class 1 only</option>
              <option>Class 2 only</option>
            </select></label>
            <label>Accessibility spaces<input defaultValue="2" min="0" type="number" /></label>
          </fieldset>

          <div className="admin-form-actions">
            <button className="admin-primary-button" type="submit"><SaveOutlined /> Save train</button>
            <Link className="admin-secondary-button" to="/admin/trains">Cancel</Link>
          </div>
        </form>

        <aside className="admin-preview-card">
          <span className="admin-preview-icon"><ProfileOutlined /></span>
          <small>Preview</small>
          <h2>{code || "Train code"}</h2>
          <p>{name || "Train name"}</p>
          <dl>
            <div><dt>Type</dt><dd>{trainType}</dd></div>
            <div><dt>Status</dt><dd>{status}</dd></div>
            <div><dt>Carriages</dt><dd>{carriages}</dd></div>
            <div><dt>Total seats</dt><dd>{Number.isFinite(totalSeats) ? totalSeats : 0}</dd></div>
          </dl>
        </aside>
      </section>
    </AdminLayout>
  );
}

export default AdminCreateTrainPage;
