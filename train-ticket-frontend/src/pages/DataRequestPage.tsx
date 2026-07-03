import { useMemo, useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom";
import { getBookingOrder, updateGuestBookingData } from "../api/bookingApi";

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
  const [showControllerDetails, setShowControllerDetails] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [apiError, setApiError] = useState("");
  const bookingId = searchParams.get("bookingId") ?? "";
  const orderId = searchParams.get("orderId") ?? "";
  const bookingIds = searchParams.get("bookingIds") ?? "";
  const selectedSeat = searchParams.get("seat") ?? "";
  const selectedCar = searchParams.get("car") ?? "";
  const backParams = new URLSearchParams(searchParams);
  backParams.set("class", selectedClass);

  if (bookingId) {
    backParams.set("bookingId", bookingId);
  }

  if (orderId) {
    backParams.set("orderId", orderId);
  }

  if (bookingIds) {
    backParams.set("bookingIds", bookingIds);
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

    if (!canSave || (!bookingId && !orderId)) {
      setShowError(true);
      return;
    }

    setIsSaving(true);
    setApiError("");

    try {
      if (orderId) {
        const order = await getBookingOrder(orderId);
        await Promise.all(
          order.bookings.map((booking, index) =>
            updateGuestBookingData(booking.id, {
              guestEmail: email,
              passengerName: booking.passengerName || `Guest passenger ${index + 1}`,
              acceptedTerms,
              acceptedMarketing: marketingConsent,
            }),
          ),
        );
      } else {
        await updateGuestBookingData(bookingId, {
          guestEmail: email,
          passengerName: "Guest passenger",
          acceptedTerms,
          acceptedMarketing: marketingConsent,
        });
      }

      const params = new URLSearchParams(searchParams);
      params.set("class", selectedClass);
      params.set("email", email);
      if (bookingId) {
        params.set("bookingId", bookingId);
      }
      if (orderId) {
        params.set("orderId", orderId);
      }
      if (bookingIds) {
        params.set("bookingIds", bookingIds);
      }

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

        <label className="consent-row">
          <input
            checked={acceptedTerms && marketingConsent && electronicInfoConsent}
            onChange={handleAllConsents}
            type="checkbox"
          />
          <span className="consent-check" aria-hidden="true" />
          <strong>Select all consents</strong>
        </label>

        <label className="consent-row">
          <input
            checked={acceptedTerms}
            onChange={(event) => setAcceptedTerms(event.target.checked)}
            type="checkbox"
          />
          <span className="consent-check" aria-hidden="true" />
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
          {showControllerDetails && (
            <div className="data-controller-details">
              <p>
                Your data is used only to create the booking, send ticket documents, handle payment confirmation,
                and support any later refund or passenger-service request linked to this purchase.
              </p>
              <p>
                Mandatory consent is required to complete the ticket contract. Optional marketing and profiling
                consents can be left unchecked.
              </p>
            </div>
          )}
          <button
            type="button"
            className={showControllerDetails ? "data-controller-details-open" : ""}
            onClick={() => setShowControllerDetails((current) => !current)}
            aria-expanded={showControllerDetails}
          >
            Details
          </button>
        </section>

        <label className="consent-row">
          <input
            checked={marketingConsent}
            onChange={(event) => setMarketingConsent(event.target.checked)}
            type="checkbox"
          />
          <span className="consent-check" aria-hidden="true" />
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
          <span className="consent-check" aria-hidden="true" />
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
