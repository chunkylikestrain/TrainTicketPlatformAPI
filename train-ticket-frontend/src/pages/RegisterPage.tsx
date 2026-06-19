import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import axios from "axios";
import { register } from "../api/authApi";
import { hasAuthToken, saveProfileDisplayName } from "../api/authSession";

function RegisterPage() {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [repeatPassword, setRepeatPassword] = useState("");
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [phone, setPhone] = useState("");
  const [needsInvoice, setNeedsInvoice] = useState(false);
  const [acceptedRules, setAcceptedRules] = useState(false);
  const [acceptedMarketing, setAcceptedMarketing] = useState(false);
  const [acceptedEmailInfo, setAcceptedEmailInfo] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [showRepeatPassword, setShowRepeatPassword] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState("");
  const passwordChecks = [
    { label: "Between 8 and 32 characters", passes: password.length >= 8 && password.length <= 32 },
    { label: "At least one capital letter", passes: /[A-Z]/.test(password) },
    { label: "At least one digit", passes: /\d/.test(password) },
    { label: "At least one special character", passes: /[^A-Za-z0-9]/.test(password) },
  ];
  const isPasswordStrong = passwordChecks.every((check) => check.passes);
  const canSubmit =
    email.trim().length > 0 &&
    firstName.trim().length > 0 &&
    lastName.trim().length > 0 &&
    phone.trim().length > 0 &&
    isPasswordStrong &&
    password === repeatPassword &&
    acceptedRules &&
    !isSubmitting;

  useEffect(() => {
    if (hasAuthToken()) {
      navigate("/profile", { replace: true });
    }
  }, [navigate]);

  function setAllConsents(checked: boolean) {
    setAcceptedRules(checked);
    setAcceptedMarketing(checked);
    setAcceptedEmailInfo(checked);
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");

    if (!canSubmit) {
      setError("Fill in required fields, repeat the same password, and accept the mandatory consent.");
      return;
    }

    try {
      setIsSubmitting(true);
      await register({ email, phone, password });
      saveProfileDisplayName(email, `${firstName} ${lastName}`);
      setIsModalOpen(true);
    } catch (registerError) {
      if (axios.isAxiosError(registerError)) {
        if (!registerError.response) {
          setError("Registration failed because the API is unavailable. Start the backend and try again.");
          return;
        }

        if (typeof registerError.response.data === "string") {
          setError(registerError.response.data);
          return;
        }
      }

      setError("Registration failed. Check the fields and try again.");
    } finally {
      setIsSubmitting(false);
    }
  }

  function closeModal() {
    setIsModalOpen(false);
    navigate("/login?registered=true");
  }

  return (
    <main className="register-flow-page">
      <div className="registration-progress" aria-label="Registration steps">
        <strong>1. User data</strong>
        <span>2. Confirmation</span>
      </div>

      <form className="auth-flow-form register-flow-form" onSubmit={handleSubmit}>
        <Link to="/profile" className="register-close" aria-label="Close registration">x</Link>
        <h1>Registration</h1>
        <p className="required-note"><span>*</span> Required field</p>
        <h2>Your account details</h2>

        <label className="auth-flow-field">
          <span>E-mail *</span>
          <input value={email} onChange={(event) => setEmail(event.target.value)} type="email" required />
        </label>

        <label className="auth-flow-field password-field">
          <span>Password *</span>
          <input
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            type={showPassword ? "text" : "password"}
            required
          />
          <button type="button" onClick={() => setShowPassword(!showPassword)} aria-label="Show or hide password">
            eye
          </button>
        </label>

        <ul className="password-checklist">
          {passwordChecks.map((check) => (
            <li className={check.passes ? "password-check-passed" : ""} key={check.label}>
              {check.label}
            </li>
          ))}
        </ul>

        <label className="auth-flow-field password-field">
          <span>Repeat password *</span>
          <input
            value={repeatPassword}
            onChange={(event) => setRepeatPassword(event.target.value)}
            type={showRepeatPassword ? "text" : "password"}
            required
          />
          <button
            type="button"
            onClick={() => setShowRepeatPassword(!showRepeatPassword)}
            aria-label="Show or hide repeated password"
          >
            eye
          </button>
        </label>

        <label className="auth-flow-field">
          <span>First name *</span>
          <input value={firstName} onChange={(event) => setFirstName(event.target.value)} required />
        </label>

        <label className="auth-flow-field">
          <span>Last name *</span>
          <input value={lastName} onChange={(event) => setLastName(event.target.value)} required />
        </label>

        <label className="auth-flow-field">
          <span>Phone *</span>
          <input value={phone} onChange={(event) => setPhone(event.target.value)} type="tel" required />
        </label>

        <section className="registration-consents">
          <h2>VAT invoice</h2>
          <label className="plain-checkbox-row">
            <input checked={needsInvoice} onChange={(event) => setNeedsInvoice(event.target.checked)} type="checkbox" />
            <span>I want to provide invoice details</span>
          </label>

          <h2>Consents and statements</h2>
          <p><span>*</span> Mandatory consent</p>
          <label className="plain-checkbox-row">
            <input
              checked={acceptedRules && acceptedMarketing && acceptedEmailInfo}
              onChange={(event) => setAllConsents(event.target.checked)}
              type="checkbox"
            />
            <span>Select all consents</span>
          </label>
          <label className="plain-checkbox-row">
            <input
              checked={acceptedRules}
              onChange={(event) => setAcceptedRules(event.target.checked)}
              type="checkbox"
            />
            <span>
              * I declare that I am familiar with the <a href="#rules">ticket Regulations</a>, and I accept their
              conditions.
            </span>
          </label>
          <p className="registration-smallprint">
            The Controller of your personal data provided with reference to voluntary registration in this demo
            service has its registered office in Warsaw.
          </p>
          <label className="plain-checkbox-row">
            <input
              checked={acceptedMarketing}
              onChange={(event) => setAcceptedMarketing(event.target.checked)}
              type="checkbox"
            />
            <span>I consent to profiling the personal data provided by me in order to optimize the commercial offer.</span>
          </label>
          <label className="plain-checkbox-row">
            <input
              checked={acceptedEmailInfo}
              onChange={(event) => setAcceptedEmailInfo(event.target.checked)}
              type="checkbox"
            />
            <span>I consent to receiving information electronically to the e-mail address provided.</span>
          </label>
        </section>

        {error && <p className="form-error">{error}</p>}
        <button className="auth-flow-primary" type="submit" disabled={!canSubmit}>
          {isSubmitting ? "Creating account..." : "Save"}
        </button>
        <Link className="auth-flow-link" to="/login">Already registered? Log in</Link>
      </form>

      {isModalOpen && (
        <div className="registration-modal-backdrop" role="dialog" aria-modal="true" aria-labelledby="registration-modal-title">
          <section className="registration-modal">
            <button type="button" onClick={closeModal} aria-label="Close registration confirmation">x</button>
            <h2 id="registration-modal-title">Complete registration process</h2>
            <p>The account has been created for email address: <strong>{email}</strong></p>
            <p>To continue, log in with your new account.</p>
            <hr />
            <button className="registration-modal-action" type="button" onClick={closeModal}>
              Go to login
            </button>
          </section>
        </div>
      )}
    </main>
  );
}

export default RegisterPage;
