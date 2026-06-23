import { useMemo, useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom";
import { updateGuestBookingData } from "../api/bookingApi";

function DataRequestPage() {
  const { tripId } = useParams();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const selectedClass = searchParams.get("class") === "2" ? "2" : "1";
  const [email, setEmail] = useState("");
  const [emailRepeat, setEmailRepeat] = useState("");
  const [acceptedTerms, setAcceptedTerms] = useState(false);
  const [marketingConsent, setMarketingConsent] = useState(false);
  const [electronicInfoConsent, setElectronicInfoConsent] = useState(false);
  const [showError, setShowError] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [apiError, setApiError] = useState("");
  const bookingId = searchParams.get("bookingId") ?? "";
  const selectedSeat = searchParams.get("seat") ?? "";
  const selectedCar = searchParams.get("car") ?? "";
  const backParams = new URLSearchParams(searchParams);
  backParams.set("class", selectedClass);

  if (bookingId) {
    backParams.set("bookingId", bookingId);
  }

  if (selectedSeat) {
    backParams.set("seat", selectedSeat);
    backParams.set("car", selectedCar || "1");
  }

  const canSave = useMemo(() => {
    return email.length > 0 && email === emailRepeat && acceptedTerms;
  }, [acceptedTerms, email, emailRepeat]);

  function handleAllConsents() {
    const nextValue = !(acceptedTerms && marketingConsent && electronicInfoConsent);
    setAcceptedTerms(nextValue);
    setMarketingConsent(nextValue);
    setElectronicInfoConsent(nextValue);
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!canSave || !bookingId) {
      setShowError(true);
      return;
    }

    setIsSaving(true);
    setApiError("");

    try {
      await updateGuestBookingData(bookingId, {
        guestEmail: email,
        passengerName: "Guest passenger",
        acceptedTerms,
        acceptedMarketing: marketingConsent,
      });

      const params = new URLSearchParams(searchParams);
      params.set("class", selectedClass);
      params.set("email", email);
      params.set("bookingId", bookingId);

      if (selectedSeat) {
        params.set("seat", selectedSeat);
        params.set("car", selectedCar || "1");
      }

      navigate(`/order-summary/${tripId}?${params.toString()}`);
    } catch {
      setApiError("Could not save guest data. Check that the API is running and the booking hold has not expired.");
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <main className="data-page">
      <form className="data-form" onSubmit={handleSubmit}>
        <Link className="data-back-link" to={`/summary/${tripId}?${backParams.toString()}`}>
          &lt; Enter data
        </Link>

        <label className="data-field">
          <span>E-mail *</span>
          <input value={email} onChange={(event) => setEmail(event.target.value)} type="email" required />
        </label>

        <label className="data-field">
          <span>E-mail repeat *</span>
          <input
            value={emailRepeat}
            onChange={(event) => setEmailRepeat(event.target.value)}
            type="email"
            required
          />
        </label>

        <p className="required-note">* Required field</p>

        <div className="consent-row">
          <button
            type="button"
            className={`consent-switch ${
              acceptedTerms && marketingConsent && electronicInfoConsent ? "consent-switch-on" : ""
            }`}
            onClick={handleAllConsents}
            aria-pressed={acceptedTerms && marketingConsent && electronicInfoConsent}
          />
          <strong>Select all consents</strong>
        </div>

        <label className="consent-row">
          <input
            checked={acceptedTerms}
            onChange={(event) => setAcceptedTerms(event.target.checked)}
            type="checkbox"
          />
          <span className={acceptedTerms ? "consent-switch consent-switch-on" : "consent-switch"} aria-hidden="true" />
          <strong>
            * I declare that I am familiar with the <a href="#terms">ticket regulations</a>, and I accept their
            conditions.
          </strong>
        </label>

        <section className="data-controller-note">
          <p>
            The controller of your personal data provided for this guest purchase is RailWay Ticket Platform.
            We use this email address to send ticket confirmation and travel documents.
          </p>
          <button type="button">Details</button>
        </section>

        <label className="consent-row">
          <input
            checked={marketingConsent}
            onChange={(event) => setMarketingConsent(event.target.checked)}
            type="checkbox"
          />
          <span className={marketingConsent ? "consent-switch consent-switch-on" : "consent-switch"} aria-hidden="true" />
          <strong>
            I consent to profiling purchase data in order to optimize the commercial offer.
          </strong>
        </label>

        <label className="consent-row">
          <input
            checked={electronicInfoConsent}
            onChange={(event) => setElectronicInfoConsent(event.target.checked)}
            type="checkbox"
          />
          <span
            className={electronicInfoConsent ? "consent-switch consent-switch-on" : "consent-switch"}
            aria-hidden="true"
          />
          <strong>
            I consent to sending information electronically to the e-mail address provided.
          </strong>
        </label>

        <p className="required-note">* Mandatory consent</p>

        {showError && (
          <p className="data-error">
            Enter matching e-mail addresses, accept the mandatory consent, and choose a real seat first.
          </p>
        )}
        {apiError && <p className="data-error">{apiError}</p>}

        <button className="data-save-button" type="submit">
          {isSaving ? "Saving..." : "Save"}
        </button>
      </form>
    </main>
  );
}

export default DataRequestPage;
