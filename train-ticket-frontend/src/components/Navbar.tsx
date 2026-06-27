import { UserOutlined } from "@ant-design/icons";
import { Link, NavLink } from "react-router-dom";
import { getProfileDisplayName, getUserEmail, getUserRole, hasAuthToken } from "../api/authSession";

function getNavbarDisplayName(email: string) {
  const savedName = getProfileDisplayName(email);

  if (savedName !== email) {
    return savedName;
  }

  return email.split("@")[0] || "My account";
}

function Navbar() {
  const userEmail = getUserEmail();
  const isLoggedIn = hasAuthToken() && Boolean(userEmail);
  const isAdmin = getUserRole() === "Admin";
  const displayName = isLoggedIn && userEmail ? getNavbarDisplayName(userEmail) : "";

  return (
    <header className="site-header">
      <Link to="/" className="brand" aria-label="RailBook home">
        <span className="brand-mark">RB</span>
        <span className="brand-text">RailBook</span>
      </Link>

      <nav className="main-nav" aria-label="Primary navigation">
        <NavLink to="/">For Passengers</NavLink>
        <NavLink to="/profile">My tickets</NavLink>
        {isAdmin && <NavLink to="/admin">Admin</NavLink>}
        <a href="#passenger-info">Help</a>
        <a href="#offers">Offers</a>
      </nav>

      <div className="nav-tools" aria-label="Account tools">
        <Link to="/profile" className="login-link">
          <UserOutlined className="navbar-account-icon" aria-hidden="true" />
          {isLoggedIn ? <span className="navbar-account-name">{displayName}</span> : "Log in / Register"}
        </Link>
      </div>
    </header>
  );
}

export default Navbar;
