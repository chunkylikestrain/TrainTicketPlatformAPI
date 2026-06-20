import { DeleteOutlined, EditOutlined, PlusOutlined, ShareAltOutlined } from "@ant-design/icons";
import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { deleteAdminRoute, getAdminRoutes } from "../../api/adminApi";
import AdminLayout from "../../components/AdminLayout";
import type { AdminRoute } from "../../types/admin";

function AdminRoutesPage() {
  const [routes, setRoutes] = useState<AdminRoute[]>([]);
  const [error, setError] = useState("");

  useEffect(() => {
    getAdminRoutes().then(setRoutes).catch(() => setError("Could not load routes from the API."));
  }, []);

  async function handleDelete(id: number) {
    await deleteAdminRoute(id);
    setRoutes((current) => current.filter((route) => route.id !== id));
  }

  return (
    <AdminLayout>
      <section className="admin-page-heading">
        <div>
          <h1><ShareAltOutlined /> Manage Routes</h1>
          <p>Define station pairs, route codes, travel distance, and route availability.</p>
        </div>
        <Link className="admin-primary-button" to="/admin/routes/new"><PlusOutlined /> Add route</Link>
      </section>

      {error && <div className="admin-save-banner">{error}</div>}

      <section className="admin-table-card">
        <table className="admin-table">
          <thead>
            <tr><th>Route code</th><th>Origin</th><th>Destination</th><th>Distance</th><th>Duration</th><th>Status</th><th>Actions</th></tr>
          </thead>
          <tbody>
            {routes.map((route) => (
              <tr key={route.id}>
                <td><span className="route-code-pill">{route.code}</span></td>
                <td>{route.departureStationName}</td>
                <td>{route.arrivalStationName}</td>
                <td>{route.distanceKm} km</td>
                <td>{route.estimatedDurationMinutes} min</td>
                <td><span className={route.isActive ? "status-pill status-active" : "status-pill status-warning"}>{route.isActive ? "Active" : "Draft"}</span></td>
                <td>
                  <Link to={`/admin/routes/${route.id}/edit`} aria-label={`Edit ${route.code}`}><EditOutlined /></Link>
                  <button type="button" onClick={() => handleDelete(route.id)} aria-label={`Delete ${route.code}`}><DeleteOutlined /></button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </AdminLayout>
  );
}

export default AdminRoutesPage;
