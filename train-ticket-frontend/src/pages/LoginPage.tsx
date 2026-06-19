import { useState } from "react";
import { useEffect } from "react";
import type { FormEvent } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import axios from "axios";
import { login } from "../api/authApi";
import { hasAuthToken, saveLoginSession } from "../api/authSession";

function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const canSubmit = email.trim().length > 0 && password.trim().length > 0 && !isSubmitting;
  const notice = new URLSearchParams(location.search).get("registered") === "true"
    ? "Registration complete. You can log in now."
    : "";

  useEffect(() => {
    if (hasAuthToken()) {
      navigate("/profile", { replace: true });
    }
  }, [navigate]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    setIsSubmitting(true);

    try {
      const response = await login({ email, password });
      saveLoginSession(response, email);
      navigate("/profile?loggedIn=true");
    } catch (loginError) {
      if (axios.isAxiosError(loginError)) {
        if (!loginError.response) {
          setError("Login failed because the API is unavailable. Start the backend and try again.");
          return;
        }

        if (loginError.response.status === 401) {
          setError("The email or password is incorrect.");
          return;
        }
      }

      setError("Login failed. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="auth-flow-page">
      <form className="auth-flow-form login-flow-form" onSubmit={handleSubmit}>
        <h1>Log in</h1>
        {notice && <p className="form-success">{notice}</p>}
        <p className="required-note"><span>*</span> Required field</p>

        <label className="auth-flow-field">
          <span>E-mail</span>
          <input value={email} onChange={(event) => setEmail(event.target.value)} type="email" required />
        </label>

        <label className="auth-flow-field password-field">
          <span>Password</span>
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

        {error && <p className="form-error">{error}</p>}
        <button className="auth-flow-primary" type="submit" disabled={!canSubmit}>
          {isSubmitting ? "Logging in..." : "Log in"}
        </button>
        <Link className="auth-flow-link" to="#forgot-password">I forgot my password</Link>
        <p className="signup-prompt">Don't have an account?</p>
        <Link className="auth-flow-secondary" to="/register">Sign up</Link>
      </form>
    </main>
  );
}

export default LoginPage;
