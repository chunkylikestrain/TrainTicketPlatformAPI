import { CalendarOutlined, DeleteOutlined, PlusOutlined } from "@ant-design/icons";
import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { deleteAdminSchedule, getAdminSchedules } from "../../api/adminApi";
import AdminLayout from "../../components/AdminLayout";
import type { AdminSchedule } from "../../types/admin";
import { getDisruptionMessage, getDisruptionSeverity, hasDisruption } from "../../utils/disruptions";

function AdminSchedulesPage() {
  const [schedules, setSchedules] = useState<AdminSchedule[]>([]);
  const [error, setError] = useState("");

  useEffect(() => {
    getAdminSchedules().then(setSchedules).catch(() => setError("Could not load schedules from the API."));
  }, []);

  async function handleDelete(id: number) {
    await deleteAdminSchedule(id);
    setSchedules((current) => current.filter((schedule) => schedule.id !== id));
  }

  return (
    <AdminLayout>
      <section className="admin-page-heading">
        <div>
          <h1><CalendarOutlined /> Manage Schedules</h1>
          <p>Assign trains to routes for specific dates and times.</p>
        </div>
        <Link className="admin-primary-button" to="/admin/schedules/new"><PlusOutlined /> Add schedule</Link>
      </section>

      {error && <div className="admin-save-banner">{error}</div>}

      <section className="admin-table-card">
        <table className="admin-table">
          <thead><tr><th>Train</th><th>Route</th><th>Schedule timings</th><th>Platform</th><th>Status</th><th>Actions</th></tr></thead>
          <tbody>
            {schedules.map((schedule) => (
              <tr key={schedule.id}>
                <td><strong>{schedule.trainCode}</strong></td>
                <td>{schedule.route}</td>
                <td><strong>Dep:</strong> {formatDate(schedule.departureTime)}<br /><small>Arr: {formatDate(schedule.arrivalTime)}</small></td>
                <td>
                  Plat. {schedule.platform || "-"} track {schedule.track || "-"}
                  {schedule.hasPlatformChange && (
                    <small>Changed from {schedule.originalPlatform || "-"} / {schedule.originalTrack || "-"}</small>
                  )}
                </td>
                <td>
                  <span className={`status-pill status-${schedule.status.toLowerCase().replace(" ", "-")}`}>{schedule.status}</span>
                  {hasDisruption(schedule) && (
                    <div className={`admin-disruption-note disruption-${getDisruptionSeverity(schedule) || "notice"}`}>
                      {getDisruptionMessage(schedule)}
                    </div>
                  )}
                </td>
                <td><button type="button" onClick={() => handleDelete(schedule.id)} aria-label="Delete schedule"><DeleteOutlined /></button></td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </AdminLayout>
  );
}

function formatDate(value: string) {
  return value.replace("T", " ").slice(0, 16);
}

export default AdminSchedulesPage;
