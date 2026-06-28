import { Link } from "react-router-dom";

function ContactPage() {
  return (
    <main className="contact-page">
      <section className="contact-shell">
        <nav className="contact-breadcrumb" aria-label="Breadcrumb">
          <Link to="/">Home</Link>
          <span aria-hidden="true">&gt;</span>
          <Link to="/help">Help</Link>
          <span aria-hidden="true">&gt;</span>
          <strong>Contact</strong>
        </nav>

        <p className="contact-eyebrow">Customer service</p>
        <h1>Contact</h1>

        <section className="contact-panel">
          <h2>Contact details</h2>
          <h3>RailBook Service Call Center</h3>
          <p>
            Our consultants can help with search, booking holds, payment, ticket PDFs, invoices, refunds,
            and account access.
          </p>
          <p>The support line works twenty-four/seven:</p>
          <p>
            <a href="tel:+48223222222">+48 22 322 22 22</a>
            <span> Cost of connection pursuant to operator price list.</span>
          </p>
        </section>

        <section className="contact-panel">
          <h2>Contact via e-mail</h2>
          <p>RailBook support will answer questions sent to these addresses:</p>
          <div className="contact-email-grid">
            <a href="mailto:support@railbook.local">
              support@railbook.local
              <span>ticket and account support</span>
            </a>
            <a href="mailto:invoices@railbook.local">
              invoices@railbook.local
              <span>invoice and refund support</span>
            </a>
          </div>
        </section>
      </section>
    </main>
  );
}

export default ContactPage;
