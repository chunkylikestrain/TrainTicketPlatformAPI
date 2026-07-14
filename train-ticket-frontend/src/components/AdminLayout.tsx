import type { ReactNode } from "react";
import { useEffect } from "react";
import { NavLink, useNavigate } from "react-router-dom";
import {
  BarChartOutlined,
  CalendarOutlined,
  CheckCircleOutlined,
  CloudDownloadOutlined,
  DashboardOutlined,
  DollarOutlined,
  FileSearchOutlined,
  GiftOutlined,
  LogoutOutlined,
  ProfileOutlined,
  ShareAltOutlined,
  TeamOutlined,
  UserOutlined,
} from "@ant-design/icons";
import { clearAuthSession, getUserEmail, getUserRole, hasAuthToken } from "../api/authSession";

type AdminLayoutProps = {
  children: ReactNode;
};

const adminLinks = [
  { to: "/admin", label: "Dashboard", icon: <DashboardOutlined /> },
  { to: "/admin/trains", label: "Trains", icon: <ProfileOutlined /> },
  { to: "/admin/routes", label: "Routes", icon: <ShareAltOutlined /> },
  { to: "/admin/open-railway", label: "Schedule sync", icon: <CloudDownloadOutlined /> },
  { to: "/admin/schedules", label: "Schedules", icon: <CalendarOutlined /> },
  { to: "/admin/pricing", label: "Pricing", icon: <DollarOutlined /> },
  { to: "/admin/bookings", label: "Bookings", icon: <CheckCircleOutlined /> },
  { to: "/admin/users", label: "Users", icon: <TeamOutlined /> },
  { to: "/admin/discounts", label: "Discounts", icon: <GiftOutlined /> },
  { to: "/admin/revenue", label: "Revenue", icon: <BarChartOutlined /> },
  { to: "/admin/audit-logs", label: "Audit logs", icon: <FileSearchOutlined /> },
];

function AdminLayout({ children }: AdminLayoutProps) {
  const navigate = useNavigate();
  const isAdmin = hasAuthToken() && getUserRole() === "Admin";
  const userEmail = getUserEmail() ?? "Admin";

  useEffect(() => {
    if (!isAdmin) {
      clearAuthSession();
      navigate("/login", { replace: true });
    }
  }, [isAdmin, navigate]);

  function handleLogout() {
    clearAuthSession();
    navigate("/profile");
  }

  if (!isAdmin) {
    return null;
  }

  return (
    <main className="admin-shell">
      <aside className="admin-sidebar">
        <NavLink to="/admin" className="admin-brand" aria-label="RailBook admin dashboard">
          <span>RailBook</span>
          <strong>Admin</strong>
        </NavLink>

        <nav className="admin-nav" aria-label="Admin navigation">
          {adminLinks.map((link) => (
            <NavLink
              className={({ isActive }) => (isActive ? "admin-nav-link admin-nav-active" : "admin-nav-link")}
              end={link.to === "/admin"}
              key={link.to}
              to={link.to}
            >
              {link.icon}
              <span>{link.label}</span>
            </NavLink>
          ))}
        </nav>
      </aside>

      <section className="admin-main">
        <header className="admin-topbar">
          <strong>Control Panel</strong>
          <div className="admin-topbar-actions">
            <a href="/" target="_blank" rel="noreferrer">Main site</a>
            <span className="admin-user-icon"><UserOutlined /></span>
            <span>
              <b>{userEmail}</b>
              <small>Administrator</small>
            </span>
            <button type="button" aria-label="Log out" onClick={handleLogout}><LogoutOutlined /></button>
          </div>
        </header>

        <div className="admin-content">{children}</div>
      </section>
    </main>
  );
}

export default AdminLayout;
