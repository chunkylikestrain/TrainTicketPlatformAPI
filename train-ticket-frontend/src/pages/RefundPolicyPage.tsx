import { Link } from "react-router-dom";

const refundWindows = [
  {
    title: "More than 24 hours before departure",
    value: "100%",
    text: "The ticket can be returned with no refund fee.",
  },
  {
    title: "24 hours to 2 hours before departure",
    value: "90%",
    text: "A 10% refund fee is deducted from the ticket price.",
  },
  {
    title: "2 hours to 30 minutes before departure",
    value: "50%",
    text: "A 50% refund fee is deducted from the ticket price.",
  },
  {
    title: "Less than 30 minutes before departure",
    value: "Closed",
    text: "Self-service refund is no longer available from the account page.",
  },
] as const;

function RefundPolicyPage() {
  return (
    <main className="help-page refund-policy-page">
      <section className="help-shell">
        <div className="help-heading">
          <Link to="/help">Back to help</Link>
          <h1>Refund Policy</h1>
          <p>
            RailBook calculates refund eligibility from the ticket departure time. The same rule is shown on
            active tickets before the Refund button can be used.
          </p>
        </div>

        <section className="refund-policy-hero">
          <div>
            <span>Self-service returns</span>
            <h2>Refund amount depends on how close the train is to departure.</h2>
          </div>
          <Link to="/profile" className="refund-policy-action">Open My tickets</Link>
        </section>

        <section className="refund-policy-grid" aria-label="Refund windows">
          {refundWindows.map((window) => (
            <article key={window.title}>
              <strong>{window.value}</strong>
              <h2>{window.title}</h2>
              <p>{window.text}</p>
            </article>
          ))}
        </section>

        <section className="refund-policy-notes">
          <article>
            <h2>Cancelled train services</h2>
            <p>
              If the train service is cancelled by the operator, the ticket is eligible for a full refund even
              when the normal self-service window would already be closed.
            </p>
          </article>
          <article>
            <h2>Returned tickets</h2>
            <p>
              After a refund is requested, the ticket moves to the Returned tab. The ticket remains visible as
              account history, but its PDF download and refund action are no longer active.
            </p>
          </article>
          <article>
            <h2>Invoices</h2>
            <p>
              Invoice PDFs stay in My invoices after checkout. Returning a ticket does not remove the original
              invoice record from the account.
            </p>
          </article>
        </section>
      </section>
    </main>
  );
}

export default RefundPolicyPage;
