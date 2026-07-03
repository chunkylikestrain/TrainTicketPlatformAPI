import {
  CheckCircleOutlined,
  CloudDownloadOutlined,
  EyeOutlined,
  ReloadOutlined,
  WarningOutlined,
} from "@ant-design/icons";
import axios from "axios";
import { useEffect, useMemo, useState } from "react";
import {
  getOpenRailwayRoutes,
  importOpenRailwayRoutesForDate,
  previewOpenRailwayRoute,
} from "../../api/adminApi";
import AdminLayout from "../../components/AdminLayout";
import type {
  OpenRailwayImportDateResult,
  OpenRailwayImportPreview,
  OpenRailwayRouteId,
} from "../../types/admin";

function todayIsoDate() {
  return new Date().toLocaleDateString("en-CA");
}

function routeKey(route: Pick<OpenRailwayRouteId, "scheduleId" | "orderId">) {
  return `${route.scheduleId}:${route.orderId}`;
}

function getOpenRailwayErrorMessage(errorValue: unknown, fallback: string) {
  if (!axios.isAxiosError(errorValue))
    return fallback;

  const data = errorValue.response?.data;
  if (data && typeof data === "object") {
    const problem = data as { detail?: unknown; title?: unknown; message?: unknown };
    if (typeof problem.detail === "string" && problem.detail.trim())
      return problem.detail;
    if (typeof problem.message === "string" && problem.message.trim())
      return problem.message;
    if (typeof problem.title === "string" && problem.title.trim())
      return problem.title;
  }

  if (typeof data === "string" && data.trim())
    return data;

  return fallback;
}

function AdminOpenRailwayImportPage() {
  const [date, setDate] = useState(todayIsoDate());
  const [limit, setLimit] = useState(25);
  const [routes, setRoutes] = useState<OpenRailwayRouteId[]>([]);
  const [selectedKeys, setSelectedKeys] = useState<Set<string>>(new Set());
  const [preview, setPreview] = useState<OpenRailwayImportPreview | null>(null);
  const [batchResult, setBatchResult] = useState<OpenRailwayImportDateResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [working, setWorking] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const selectedRoutes = useMemo(
    () => routes.filter((route) => selectedKeys.has(routeKey(route))),
    [routes, selectedKeys]);

  useEffect(() => {
    loadRoutes();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function loadRoutes() {
    setLoading(true);
    setError("");
    setSuccess("");
    setPreview(null);
    setBatchResult(null);

    try {
      const response = await getOpenRailwayRoutes(date, limit);
      setRoutes(response.routes ?? []);
      setSelectedKeys(new Set());
      setSuccess(`Loaded ${response.returnedCount} of ${response.count} route identifiers for ${response.date}.`);
    } catch (errorValue) {
      setError(getOpenRailwayErrorMessage(
        errorValue,
        "Open Railway routes could not be loaded. Check the API key, activation status, and backend connection."));
    } finally {
      setLoading(false);
    }
  }

  function toggleRoute(route: OpenRailwayRouteId) {
    const key = routeKey(route);
    setSelectedKeys((current) => {
      const next = new Set(current);
      if (next.has(key)) {
        next.delete(key);
      } else {
        next.add(key);
      }
      return next;
    });
  }

  function toggleAllVisible() {
    setSelectedKeys((current) => {
      if (routes.length > 0 && current.size === routes.length) {
        return new Set();
      }
      return new Set(routes.map(routeKey));
    });
  }

  async function handlePreview(route: OpenRailwayRouteId) {
    setWorking(true);
    setError("");
    setSuccess("");
    setBatchResult(null);

    try {
      const result = await previewOpenRailwayRoute(route.scheduleId, route.orderId);
      setPreview(result);
      setSuccess(`Preview loaded for ${result.trainName || result.trainCode}.`);
    } catch (errorValue) {
      setError(getOpenRailwayErrorMessage(errorValue, "This route could not be previewed from Open Railway."));
    } finally {
      setWorking(false);
    }
  }

  async function runBatch(dryRun: boolean) {
    setWorking(true);
    setError("");
    setSuccess("");
    setPreview(null);

    try {
      const result = await importOpenRailwayRoutesForDate(date, {
        limit,
        dryRun,
        routes: selectedRoutes.map((route) => ({
          scheduleId: route.scheduleId,
          orderId: route.orderId,
        })),
      });
      setBatchResult(result);
      setSuccess(dryRun
        ? `Previewed ${result.succeededCount} route imports.`
        : `Imported ${result.succeededCount} routes into RailBook.`);
    } catch (errorValue) {
      setError(getOpenRailwayErrorMessage(
        errorValue,
        "The batch import request failed. Try a smaller selection or check the backend logs."));
    } finally {
      setWorking(false);
    }
  }

  return (
    <AdminLayout>
      <section className="admin-page-heading">
        <div>
          <h1><CloudDownloadOutlined /> Open Railway Import</h1>
          <p>Preview and import PLK route, station, train, timetable, and default consist data into RailBook.</p>
        </div>
        <button type="button" className="admin-secondary-button" onClick={loadRoutes} disabled={loading || working}>
          <ReloadOutlined /> Refresh
        </button>
      </section>

      {error && <div className="admin-save-banner admin-danger-panel">{error}</div>}
      {success && <div className="admin-save-banner">{success}</div>}

      <section className="admin-table-card open-railway-controls">
        <label>
          Operating date
          <input type="date" value={date} onChange={(event) => setDate(event.target.value)} />
        </label>
        <label>
          Route limit
          <input
            type="number"
            min={1}
            max={100}
            value={limit}
            onChange={(event) => setLimit(Math.max(1, Math.min(100, Number(event.target.value) || 1)))}
          />
        </label>
        <div className="open-railway-actions">
          <button type="button" className="admin-secondary-button" onClick={loadRoutes} disabled={loading || working}>
            Load route IDs
          </button>
          <button type="button" className="admin-secondary-button" onClick={() => runBatch(true)} disabled={working}>
            Dry-run selected
          </button>
          <button type="button" className="admin-primary-button" onClick={() => runBatch(false)} disabled={working}>
            Import selected
          </button>
        </div>
      </section>

      <section className="admin-stats-grid">
        <article className="admin-stat-card">
          <span className="stat-blue"><CloudDownloadOutlined /></span>
          <div><small>Loaded routes</small><strong>{routes.length}</strong></div>
        </article>
        <article className="admin-stat-card">
          <span className="stat-orange"><CheckCircleOutlined /></span>
          <div><small>Selected routes</small><strong>{selectedRoutes.length || "Batch limit"}</strong></div>
        </article>
        <article className="admin-stat-card">
          <span className="stat-purple"><WarningOutlined /></span>
          <div><small>Failures</small><strong>{batchResult?.failedCount ?? 0}</strong></div>
        </article>
      </section>

      <section className="admin-table-card">
        <div className="open-railway-table-heading">
          <h2>Route identifiers</h2>
          <button type="button" className="admin-secondary-button" onClick={toggleAllVisible} disabled={routes.length === 0}>
            {selectedKeys.size === routes.length && routes.length > 0 ? "Clear selection" : "Select all visible"}
          </button>
        </div>

        <table className="admin-table">
          <thead>
            <tr>
              <th>Select</th>
              <th>Schedule</th>
              <th>Order</th>
              <th>Train order</th>
              <th>Name</th>
              <th>Carrier</th>
              <th>Preview</th>
            </tr>
          </thead>
          <tbody>
            {routes.map((route) => (
              <tr key={routeKey(route)}>
                <td>
                  <input
                    aria-label={`Select route ${route.scheduleId}/${route.orderId}`}
                    checked={selectedKeys.has(routeKey(route))}
                    type="checkbox"
                    onChange={() => toggleRoute(route)}
                  />
                </td>
                <td>{route.scheduleId}</td>
                <td>{route.orderId}</td>
                <td>{route.trainOrderId}</td>
                <td>{route.name || "Unnamed route"}</td>
                <td>{route.carrierCode || "-"}</td>
                <td>
                  <button type="button" onClick={() => handlePreview(route)} disabled={working} aria-label="Preview route">
                    <EyeOutlined />
                  </button>
                </td>
              </tr>
            ))}
            {routes.length === 0 && (
              <tr><td colSpan={7}>{loading ? "Loading route identifiers..." : "No route identifiers loaded yet."}</td></tr>
            )}
          </tbody>
        </table>
      </section>

      {preview && (
        <section className="admin-table-card open-railway-preview">
          <h2>{preview.trainName}</h2>
          <p>
            {preview.category || "Train"} {preview.trainCode} · {preview.stops.length} stops · carrier {preview.carrierCode || "-"}
          </p>
          <ol className="open-railway-stop-list">
            {preview.stops.map((stop) => (
              <li key={`${stop.externalStationId}-${stop.orderNumber}`}>
                <strong>Station {stop.externalStationId}</strong>
                <span>arr {stop.arrival || "-"} / dep {stop.departure || "-"}</span>
                <small>platform {stop.platform || "-"}, track {stop.track || "-"}</small>
              </li>
            ))}
          </ol>
        </section>
      )}

      {batchResult && (
        <section className="admin-table-card">
          <h2>{batchResult.dryRun ? "Dry-run results" : "Import results"}</h2>
          <div className="open-railway-result-summary">
            <span>Requested: {batchResult.requestedCount}</span>
            <span>Succeeded: {batchResult.succeededCount}</span>
            <span>Failed: {batchResult.failedCount}</span>
          </div>
          <table className="admin-table">
            <thead>
              <tr><th>Schedule</th><th>Order</th><th>Status</th><th>Route shape</th><th>Result</th></tr>
            </thead>
            <tbody>
              {batchResult.items.map((item) => (
                <tr key={`${item.scheduleId}:${item.orderId}`}>
                  <td>{item.scheduleId}</td>
                  <td>{item.orderId}</td>
                  <td><span className={item.status === "Failed" ? "status-pill status-warning" : "status-pill status-active"}>{item.status}</span></td>
                  <td>{item.import ? (
                    <span className={item.import.routeCreated ? "status-pill status-active" : "status-pill status-on-time"}>
                      {item.import.routeCreated ? "New" : "Reused"}
                    </span>
                  ) : "-"}</td>
                  <td>{renderImportResult(item)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>
      )}
    </AdminLayout>
  );
}

function renderImportResult(item: OpenRailwayImportDateResult["items"][number]) {
  if (item.error)
    return item.error;

  if (item.import) {
    return (
      <span className="open-railway-result-detail">
        <strong>{item.import.adminDisplayName || item.import.routeName}</strong>
        <small>{item.import.routeCode} - {item.import.routeFingerprint}</small>
      </span>
    );
  }

  return item.preview?.trainName || "-";
}

export default AdminOpenRailwayImportPage;
