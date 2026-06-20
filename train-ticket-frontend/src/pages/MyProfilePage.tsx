import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import axios from "axios";
import { getCurrentUser } from "../api/authApi";
import { clearAuthSession, getProfileDisplayName, saveCurrentUser } from "../api/authSession";
import type { CurrentUser } from "../types/auth";

const accountMenuItems = [
  "My tickets",
  "My invoices",
  "My data",
  "\"My IC\" Program",
  "My shopping profiles",
  "Settings",
  "Useful links",
];

function LegalFooter() {
  return (
    <section className="summary-legal profile-legal">
      <div>
        <h2>Technological break.</h2>
        <p>
          Please remember about the technological break in the online sales system from 11:45pm - 0:30 am.
          You cannot buy any tickets during this break.
        </p>
        <p><strong>Deactivation of the e-IC 1.0 service</strong> v</p>
        <a href="#accessibility">Declaration of Accessibility</a>
      </div>
      <div>
        <p>
          The prices presented are <strong>indicative</strong>, published for informational purposes, and do not
          constitute an offer. The final prices are available in the purchase summary.
        </p>
        <p>
          <strong>
            The Controller of personal data provided in connection with voluntary registration on this service
            has its registered office in Warsaw.
          </strong>{" "}
          v
        </p>
      </div>
    </section>
  );
}

function MyProfilePage() {
  const [searchParams] = useSearchParams();
  const [currentUser, setCurrentUser] = useState<CurrentUser | null>(null);
  const [displayName, setDisplayName] = useState("");
  const [isLoading, setIsLoading] = useState(Boolean(localStorage.getItem("authToken")));
  const [notice, setNotice] = useState(
    searchParams.get("loggedIn") === "true" ? "You are logged in." : ""
  );

  useEffect(() => {
    const token = localStorage.getItem("authToken");

    if (!token) {
      setIsLoading(false);
      return;
    }

    getCurrentUser()
      .then((user) => {
        setCurrentUser(user);
        setDisplayName(getProfileDisplayName(user.email));
        saveCurrentUser(user);
      })
      .catch((profileError) => {
        setCurrentUser(null);
        if (axios.isAxiosError(profileError) && !profileError.response) {
          setNotice("The API is unavailable right now. Your browser still has your login token.");
          return;
        }

        clearAuthSession();
        setNotice("Your session expired. Please log in again.");
      })
      .finally(() => setIsLoading(false));
  }, []);

  function handleLogout() {
    clearAuthSession();
    setCurrentUser(null);
    setDisplayName("");
    setNotice("You have been logged out.");
  }

  return (
    <main className="tickets-page profile-page">
      <section className="connection-hero" aria-hidden="true">
        <div className="connection-train" />
      </section>

      <p className="previous-system-note">
        Account services: invoices, refunds, and passenger data changes are available from your profile.
      </p>

      <section className="tickets-content">
        <h1>My account</h1>

        <div className="guest-account-banner profile-banner">
          <span aria-hidden="true">ID</span>
          {currentUser ? <strong>{displayName}</strong> : <strong>{isLoading ? "Checking session..." : "Not logged in"}</strong>}
          {currentUser && (
            <button className="profile-logout" type="button" onClick={handleLogout}>
              log out
            </button>
          )}
        </div>

        {notice && <div className="profile-notice">{notice}</div>}

        <div className="account-layout">
          <aside className="account-sidebar" aria-label="Account menu">
            {accountMenuItems.map((item, index) => (
              <button
                className={
                  currentUser && index === 0
                    ? "account-menu-button account-menu-button-active"
                    : currentUser
                      ? "account-menu-button"
                      : "account-menu-button account-menu-button-muted"
                }
                key={item}
                type="button"
              >
                {item}
              </button>
            ))}
          </aside>

          {currentUser ? (
            <section className="ticket-dashboard profile-dashboard">
              <h2>My tickets</h2>
              <div className="ticket-tabs" role="tablist" aria-label="Ticket sections">
                {["Tickets", "Season tickets", "Travel history", "Returned"].map((tab, index) => (
                  <button
                    className={index === 0 ? "ticket-tab ticket-tab-active" : "ticket-tab"}
                    key={tab}
                    type="button"
                  >
                    {tab}
                  </button>
                ))}
              </div>

              <label className="ticket-search">
                <span>Search for a ticket</span>
                <input aria-label="Search for a ticket" />
                <b aria-hidden="true">Search</b>
              </label>

              <section className="profile-empty-tickets">
                <div className="profile-empty-icon" aria-hidden="true" />
                <p>You currently have no tickets. Proceed to purchase tickets.</p>
                <Link to="/">Buy a ticket</Link>
              </section>
            </section>
          ) : isLoading ? (
            <section className="profile-signin-panel profile-loading-panel" aria-live="polite">
              <h2>Checking your session</h2>
              <p>One moment while we confirm your account.</p>
            </section>
          ) : (
            <section className="profile-signin-panel">
              <h2>Sign in</h2>
              <Link className="account-primary-action" to="/login">
                Log in
              </Link>
              <Link className="account-secondary-action" to="/register">
                Register
              </Link>
              <Link className="guest-link profile-guest-link" to="/">
                Continue as Guest
              </Link>
            </section>
          )}
        </div>
      </section>

      <LegalFooter />
    </main>
  );
}

export default MyProfilePage;
