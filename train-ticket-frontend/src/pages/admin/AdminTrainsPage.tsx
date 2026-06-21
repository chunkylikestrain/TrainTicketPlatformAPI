import { DeleteOutlined, EditOutlined, PlusOutlined, ProfileOutlined } from "@ant-design/icons";
import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { deleteAdminTrain, getAdminTrains } from "../../api/adminApi";
import AdminLayout from "../../components/AdminLayout";
import type { AdminTrain } from "../../types/admin";

function AdminTrainsPage() {
  const [trains, setTrains] = useState<AdminTrain[]>([]);
  const [error, setError] = useState("");

  useEffect(() => {
    getAdminTrains().then(setTrains).catch(() => setError("Could not load trains from the API."));
  }, []);

  async function handleDelete(id: number) {
    await deleteAdminTrain(id);
    setTrains((current) => current.filter((train) => train.id !== id));
  }

  return (
    <AdminLayout>
      <section className="admin-page-heading">
        <div>
          <h1><ProfileOutlined /> Manage Trains</h1>
          <p>Configure physical trains and their carriages.</p>
        </div>
        <Link className="admin-primary-button" to="/admin/trains/new"><PlusOutlined /> Add new train</Link>
      </section>

      {error && <div className="admin-save-banner">{error}</div>}

      <section className="admin-table-card">
        <table className="admin-table">
          <thead>
            <tr><th>Train code</th><th>Name</th><th>Type</th><th>Consist</th><th>Status</th><th>Actions</th></tr>
          </thead>
          <tbody>
            {trains.map((train) => (
              <tr key={train.id}>
                <td><strong>{train.code || train.id}</strong></td>
                <td>{train.name}</td>
                <td>{train.type}</td>
                <td>
                  {train.locomotive && <small>{train.locomotive}</small>}
                  {train.carriageCount} cars ({totalSeats(train)} seats)
                </td>
                <td><span className={train.status === "Active" ? "status-pill status-active" : "status-pill status-warning"}>{train.status}</span></td>
                <td>
                  <Link to={`/admin/trains/${train.id}/edit`} aria-label={`Edit ${train.code}`}><EditOutlined /></Link>
                  <button type="button" onClick={() => handleDelete(train.id)} aria-label={`Delete ${train.code}`}><DeleteOutlined /></button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </AdminLayout>
  );
}

function totalSeats(train: AdminTrain) {
  if (train.carriages?.length)
    return train.carriages.reduce((sum, carriage) => sum + carriage.seatCount, 0);

  return train.carriageCount * train.seatsPerCarriage;
}

export default AdminTrainsPage;
