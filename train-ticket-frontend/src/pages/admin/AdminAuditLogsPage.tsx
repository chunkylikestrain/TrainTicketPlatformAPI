import { FileSearchOutlined, ReloadOutlined, SearchOutlined } from "@ant-design/icons";
import { useEffect, useMemo, useState } from "react";
import { getAdminAuditLogs } from "../../api/adminApi";
import AdminLayout from "../../components/AdminLayout";
import type { AdminAuditLog, PagedResponse } from "../../types/admin";

const entityOptions = ["bookings", "discounts", "routes", "schedules", "trains", "users"];

function AdminAuditLogsPage() {
  const [logsPage, setLogsPage] = useState<PagedResponse<AdminAuditLog> | null>(null);
  const [query, setQuery] = useState("");
  const [entityType, setEntityType] = useState("");
  const [page, setPage] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");

  const params = useMemo(() => ({
    page,
    pageSize: 25,
    search: query.trim() || undefined,
    entityType: entityType || undefined,
  }), [entityType, page, query]);

  async function loadLogs() {
    setIsLoading(true);
    setError("");

    try {
      const data = await getAdminAuditLogs(params);
      setLogsPage(data);
    } catch {
      setLogsPage(null);
      setError("Audit logs could not be loaded. Check that the API is running and the database is up to date.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadLogs();
  }, [params]);

  function handleSearch(value: string) {
    setQuery(value);
    setPage(1);
  }

  function handleEntityChange(value: string) {
    setEntityType(value);
    setPage(1);
  }

  const logs = logsPage?.items ?? [];

  return (
    <AdminLayout>
      <section className="admin-page-heading">
        <div>
          <h1><FileSearchOutlined /> Admin Audit Logs</h1>
          <p>Review successful admin-side changes made through the control panel.</p>
        </div>
        <button type="button" className="admin-secondary-button" onClick={loadLogs}>
          <ReloadOutlined /> Refresh
        </button>
      </section>

      <section className="admin-filter-row">
        <label className="admin-filter-bar">
          <SearchOutlined />
          <input
            value={query}
            onChange={(event) => handleSearch(event.target.value)}
            placeholder="Search by admin, action, entity, path..."
          />
        </label>
        <label className="admin-select-filter">
          Entity
          <select value={entityType} onChange={(event) => handleEntityChange(event.target.value)}>
            <option value="">All entities</option>
            {entityOptions.map((option) => (
              <option key={option} value={option}>{option}</option>
            ))}
          </select>
        </label>
      </section>

      {error && <div className="admin-save-banner admin-danger-panel">{error}</div>}

      <section className="admin-table-card">
        <table className="admin-table audit-log-table">
          <thead>
            <tr>
              <th>Time</th>
              <th>Admin</th>
              <th>Action</th>
              <th>Target</th>
              <th>Request</th>
              <th>Client</th>
            </tr>
          </thead>
          <tbody>
            {isLoading && (
              <tr>
                <td colSpan={6}>Loading audit logs...</td>
              </tr>
            )}
            {!isLoading && logs.length === 0 && (
              <tr>
                <td colSpan={6}>No audit logs found. New admin changes will appear here.</td>
              </tr>
            )}
            {!isLoading && logs.map((log) => (
              <tr key={log.id}>
                <td>
                  <strong>{formatDateTime(log.createdAtUtc)}</strong>
                  <small>ID: {log.id}</small>
                </td>
                <td>
                  {log.adminEmail || "Admin"}
                  <small>{log.adminUserId ? `User #${log.adminUserId}` : "No user id"}</small>
                </td>
                <td>
                  <span className="status-pill status-active">{log.action}</span>
                  <small>{log.summary}</small>
                </td>
                <td>
                  {log.entityType || "admin"}
                  <small>{log.entityId ? `#${log.entityId}` : "Collection"}</small>
                </td>
                <td>
                  <span className="audit-method">{log.httpMethod}</span>
                  <small>{log.path}</small>
                  <small>Status {log.statusCode}</small>
                </td>
                <td>
                  {log.ipAddress || "Unknown IP"}
                  <small>{trimUserAgent(log.userAgent)}</small>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      {logsPage && logsPage.totalPages > 1 && (
        <section className="admin-pagination">
          <button
            type="button"
            className="admin-secondary-button"
            disabled={page <= 1}
            onClick={() => setPage((current) => Math.max(1, current - 1))}
          >
            Previous
          </button>
          <span>Page {logsPage.page} of {logsPage.totalPages}</span>
          <button
            type="button"
            className="admin-secondary-button"
            disabled={page >= logsPage.totalPages}
            onClick={() => setPage((current) => current + 1)}
          >
            Next
          </button>
        </section>
      )}
    </AdminLayout>
  );
}

function formatDateTime(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

function trimUserAgent(value: string) {
  if (!value) return "Unknown device";
  return value.length > 72 ? `${value.slice(0, 72)}...` : value;
}

export default AdminAuditLogsPage;
