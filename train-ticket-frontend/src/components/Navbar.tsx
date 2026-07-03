import { UserOutlined } from "@ant-design/icons";
import { useRef, useState } from "react";
import { Link, NavLink } from "react-router-dom";
import { getProfileDisplayName, getUserEmail, getUserRole, hasAuthToken } from "../api/authSession";

const passengerMenuSections = [
  {
    title: "Offers",
    to: "/offers",
    links: [
      { label: "Sleeper cars and couchettes", to: "/offers/sleeper" },
      { label: "Domestic offers", to: "/offers/domestic" },
      { label: "Explore Poland by rail", to: "/offers/explore" },
      { label: "Meal while travelling", to: "/offers/meal" },
    ],
  },
  {
    title: "Customer Service",
    to: "/help",
    links: [
      { label: "FAQ", to: "/help/faq" },
      { label: "Contact", to: "/contact" },
      { label: "Refund policy", to: "/help/refund-policy" },
      { label: "Passenger rights", to: "/help/passenger-rights" },
    ],
  },
  {
    title: "Our trains",
    to: "/trains",
    links: [
      { label: "Express InterCity Premium (EIP)", to: "/trains/eip" },
      { label: "Express InterCity (EIC)", to: "/trains/eic" },
      { label: "InterCity (IC)", to: "/trains/ic" },
      { label: "Twoje Linie Kolejowe (TLK)", to: "/trains/tlk" },
    ],
  },
];

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
  const [isPassengerMenuOpen, setIsPassengerMenuOpen] = useState(false);
  const closePassengerMenuTimer = useRef<number | null>(null);

  const openPassengerMenu = () => {
    if (closePassengerMenuTimer.current !== null) {
      window.clearTimeout(closePassengerMenuTimer.current);
    }

    setIsPassengerMenuOpen(true);
  };

  const closePassengerMenuSoon = () => {
    if (closePassengerMenuTimer.current !== null) {
      window.clearTimeout(closePassengerMenuTimer.current);
    }

    closePassengerMenuTimer.current = window.setTimeout(() => {
      setIsPassengerMenuOpen(false);
    }, 180);
  };

  return (
    <header className="site-header">
      <Link to="/" className="brand" aria-label="RailBook home">
        <span className="brand-mark">RB</span>
        <span className="brand-text">RailBook</span>
      </Link>

      <nav className="main-nav" aria-label="Primary navigation">
        <div
          className={`nav-dropdown${isPassengerMenuOpen ? " nav-dropdown-open" : ""}`}
          onBlur={closePassengerMenuSoon}
          onFocus={openPassengerMenu}
          onMouseEnter={openPassengerMenu}
          onMouseLeave={closePassengerMenuSoon}
        >
          <button
            type="button"
            className="nav-dropdown-trigger"
            aria-haspopup="true"
            aria-expanded={isPassengerMenuOpen}
            onClick={() => setIsPassengerMenuOpen((current) => !current)}
          >
            For Passengers <span aria-hidden="true">v</span>
          </button>
          <div className="nav-dropdown-panel" role="menu" onMouseEnter={openPassengerMenu}>
            {passengerMenuSections.map((section) => (
              <section key={section.title} className="nav-dropdown-section">
                <h2>
                  <Link to={section.to} role="menuitem">
                    {section.title}
                  </Link>
                </h2>
                {section.links.map((link) => (
                  <Link key={`${section.title}-${link.label}`} to={link.to} role="menuitem">
                    {link.label}
                  </Link>
                ))}
              </section>
            ))}
          </div>
        </div>
        <NavLink to="/profile">My tickets</NavLink>
        {isAdmin && <NavLink to="/admin">Admin</NavLink>}
        <NavLink to="/help">Help</NavLink>
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
