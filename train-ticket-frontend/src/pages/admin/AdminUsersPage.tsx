import { DeleteOutlined, EditOutlined, SearchOutlined, TeamOutlined } from "@ant-design/icons";
import axios from "axios";
import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { deleteAdminUser, getAdminUsers, updateAdminUser } from "../../api/adminApi";
import AdminLayout from "../../components/AdminLayout";
import type { AdminUser } from "../../types/admin";

function AdminUsersPage() {
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [query, setQuery] = useState("");
  const [editingUser, setEditingUser] = useState<AdminUser | null>(null);
  const [draft, setDraft] = useState<AdminUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");

  function getLoadErrorMessage(errorValue: unknown) {
    if (axios.isAxiosError(errorValue)) {
      if (errorValue.response?.status === 401 || errorValue.response?.status === 403) {
        return "Users could not be loaded because your current session is not authorized as Admin.";
      }

      if (errorValue.response) {
        return `Users could not be loaded. API returned ${errorValue.response.status}.`;
      }
    }

    return "Users could not be loaded. Check that the API is running and points to the database you are viewing.";
  }

  useEffect(() => {
    let isCurrent = true;

    async function loadUsers() {
      setIsLoading(true);
      setError("");

      try {
        const data = await getAdminUsers(query);
        if (isCurrent) setUsers(data);
      } catch (loadError) {
        if (isCurrent) {
          setUsers([]);
          setError(getLoadErrorMessage(loadError));
        }
      } finally {
        if (isCurrent) setIsLoading(false);
      }
    }

    void loadUsers();
    return () => {
      isCurrent = false;
    };
  }, [query]);

  function openEdit(user: AdminUser) {
    setEditingUser(user);
    setDraft(user);
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!draft) return;
    const saved = await updateAdminUser(draft);
    setUsers((current) => current.map((user) => user.id === saved.id ? saved : user));
    setEditingUser(null);
    setDraft(null);
  }

  async function handleDelete(id: number) {
    await deleteAdminUser(id);
    setUsers((current) => current.filter((user) => user.id !== id));
  }

  return (
    <AdminLayout>
      <section className="admin-page-heading">
        <div>
          <h1><TeamOutlined /> Manage Users</h1>
          <p>Review passenger accounts, roles, and account status.</p>
        </div>
      </section>

      <section className="admin-filter-bar">
        <SearchOutlined />
        <input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Search by email or phone..." />
      </section>

      {error && <div className="admin-save-banner admin-danger-panel">{error}</div>}

      {editingUser && draft && (
        <form className="admin-editor-panel" onSubmit={handleSubmit}>
          <h2>Edit user</h2>
          <label>Name<input value={draft.displayName} onChange={(event) => setDraft({ ...draft, displayName: event.target.value })} /></label>
          <label>Email<input value={draft.email} onChange={(event) => setDraft({ ...draft, email: event.target.value })} type="email" required /></label>
          <label>Phone<input value={draft.phone} onChange={(event) => setDraft({ ...draft, phone: event.target.value })} required /></label>
          <label>Role<select value={draft.role} onChange={(event) => setDraft({ ...draft, role: event.target.value })}>
            <option>Passenger</option><option>Admin</option>
          </select></label>
          <label>Status<select value={draft.status} onChange={(event) => setDraft({ ...draft, status: event.target.value })}>
            <option>Active</option><option>Suspended</option>
          </select></label>
          <div className="admin-form-actions">
            <button type="submit" className="admin-primary-button">Save user</button>
            <button type="button" className="admin-secondary-button" onClick={() => setEditingUser(null)}>Cancel</button>
          </div>
        </form>
      )}

      <section className="admin-table-card">
        <table className="admin-table">
          <thead><tr><th>User</th><th>Contact</th><th>Role</th><th>Status</th><th>Actions</th></tr></thead>
          <tbody>
            {isLoading && (
              <tr>
                <td colSpan={5}>Loading users...</td>
              </tr>
            )}
            {!isLoading && users.length === 0 && (
              <tr>
                <td colSpan={5}>No users found. New registered passengers and admins will appear here.</td>
              </tr>
            )}
            {!isLoading && users.map((user) => (
              <tr key={user.id}>
                <td><strong>{user.displayName || user.email}</strong><small>ID: {user.id}</small></td>
                <td>{user.email}<small>{user.phone}</small></td>
                <td>{user.role}</td>
                <td><span className={user.status === "Active" ? "status-pill status-active" : "status-pill status-warning"}>{user.status}</span></td>
                <td>
                  <button type="button" onClick={() => openEdit(user)} aria-label={`Edit ${user.email}`}><EditOutlined /></button>
                  <button type="button" onClick={() => handleDelete(user.id)} aria-label={`Delete ${user.email}`}><DeleteOutlined /></button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </AdminLayout>
  );
}

export default AdminUsersPage;
