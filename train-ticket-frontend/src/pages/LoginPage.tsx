import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { login } from "../api/authApi";

function LoginPage() {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");

    try {
      const response = await login({ email, password });
      localStorage.setItem("authToken", response.token);
      localStorage.setItem("userRole", response.role);
      localStorage.setItem("userId", String(response.userId));
      navigate("/bookings");
    } catch {
      setError("Login failed. Check your email and password.");
    }
  }

  return (
    <main className="auth-page">
      <form className="auth-card" onSubmit={handleSubmit}>
        <p className="eyebrow">Welcome back</p>
        <h1>Log in</h1>
        <label>
          Email
          <input value={email} onChange={(event) => setEmail(event.target.value)} type="email" required />
        </label>
        <label>
          Password
          <input value={password} onChange={(event) => setPassword(event.target.value)} type="password" required />
        </label>
        {error && <p className="form-error">{error}</p>}
        <button type="submit">Log in</button>
        <p>
          New passenger? <Link to="/register">Create an account</Link>
        </p>
      </form>
    </main>
  );
}

export default LoginPage;
