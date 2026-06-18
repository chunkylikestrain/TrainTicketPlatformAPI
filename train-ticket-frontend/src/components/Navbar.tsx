import { Link, NavLink } from "react-router-dom";

function Navbar() {
  return (
    <header className="site-header">
      <Link to="/" className="brand" aria-label="RailWay home">
        <span className="brand-mark">RW</span>
        <span className="brand-text">RailWay</span>
      </Link>

      <nav className="main-nav" aria-label="Primary navigation">
        <NavLink to="/">For Passengers</NavLink>
        <NavLink to="/bookings">My Bookings</NavLink>
        <a href="#passenger-info">Help</a>
        <a href="#offers">Offers</a>
      </nav>

      <div className="nav-tools" aria-label="Account and display tools">
        <button type="button" className="icon-button" aria-label="Accessibility options">
          A
        </button>
        <button type="button" className="icon-button" aria-label="Language English">
          EN
        </button>
        <Link to="/login" className="login-link">
          Log in / Register
        </Link>
      </div>
    </header>
  );
}

export default Navbar;
