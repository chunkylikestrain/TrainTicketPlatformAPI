import { Link } from "react-router-dom";

const helpOptions = [
  {
    title: "How to buy a ticket",
    text: "Follow the purchase flow from search to seat selection, payment, ticket PDF, and account storage.",
    href: "#faq",
  },
  {
    title: "Refunds and invoices",
    text: "How ticket returns work, where invoice PDFs are saved, and what happens after payment.",
    href: "/help/refund-policy",
  },
  {
    title: "Ticket rules",
    text: "Passenger details, discounts, seat reservations, ticket validity, and travel document rules.",
    href: "/help/passenger-rights",
  },
  {
    title: "Contact support",
    text: "Get help with a booking, payment, ticket download, invoice, or account issue.",
    href: "/contact",
  },
] as const;

const buyingSteps = [
  {
    title: "1. Search for a journey",
    text: "Choose origin, destination, date, and departure time on the home page. For a return journey, turn on round trip and fill in the return date and time too.",
    visual: "search",
  },
  {
    title: "2. Set travelers, filters, and discounts",
    text: "Use the traveler box to add adults or children. Open discounts to choose a discount per passenger. Children must have a selected discount, while adults can keep Normal Ticket.",
    visual: "preferences",
  },
  {
    title: "3. Choose a connection",
    text: "The results page shows direct and transfer journeys, travel time, route, train names, and class prices. Pick the class you want before moving to seat selection.",
    visual: "results",
  },
  {
    title: "4. Pick seats on the plan",
    text: "Choose a seat for every passenger. Green seats are available, orange seats are selected, and grey seats cannot be booked for that journey or class.",
    visual: "seats",
  },
  {
    title: "5. Review and pay",
    text: "Check passenger names, discounts, route, seats, final price, My IC points, and invoice choice. After payment, RailBook creates QR tickets and sends/saves the ticket PDF.",
    visual: "payment",
  },
  {
    title: "6. Use your account later",
    text: "Open My tickets for active tickets, travel history, returns, QR codes, and PDFs. Open My invoices for invoice PDFs. My data keeps passenger and checkout defaults together.",
    visual: "account",
  },
] as const;

function HelpVisual({ type }: { type: string }) {
  if (type === "search") {
    return (
      <div className="help-mock help-search-mock" aria-hidden="true">
        <div><span>From</span><strong>Rzeszow Glowny</strong></div>
        <div><span>To</span><strong>Gdynia Glowna</strong></div>
        <div><span>When</span><strong>28-Jun-2026</strong></div>
        <div><span>Time</span><strong>13:38</strong></div>
        <button type="button">Search</button>
      </div>
    );
  }

  if (type === "preferences") {
    return (
      <div className="help-mock help-preferences-mock" aria-hidden="true">
        <article><span>Filters</span><strong>Any train</strong><small>Direct · Wi-Fi · Bicycle</small></article>
        <article><span>Discounts</span><strong>1x Normal Ticket</strong><small>Change discounts</small></article>
        <article><span>Travelers</span><strong>2x Adults</strong><small>- 2 +</small></article>
      </div>
    );
  }

  if (type === "results") {
    return (
      <div className="help-mock help-result-mock" aria-hidden="true">
        <span>1 transfer</span>
        <strong>05:48 AM &gt; 01:27 PM</strong>
        <p>Rzeszow Glowny -&gt; Gdynia Glowna</p>
        <small>IC 7310 Malczewski + IC 3806 Zefir</small>
        <button type="button">Choose class 2</button>
      </div>
    );
  }

  if (type === "seats") {
    return (
      <div className="help-mock help-seat-mock" aria-hidden="true">
        {Array.from({ length: 18 }, (_, index) => (
          <span className={index === 8 ? "selected" : index === 13 ? "unavailable" : ""} key={index}>
            {index + 1}
          </span>
        ))}
      </div>
    );
  }

  if (type === "payment") {
    return (
      <div className="help-mock help-payment-mock" aria-hidden="true">
        <div><span>Amount due</span><strong>90,00 PLN</strong></div>
        <label><span /> I want an invoice</label>
        <button type="button">Pay</button>
      </div>
    );
  }

  return (
    <div className="help-mock help-account-mock" aria-hidden="true">
      <span>My tickets</span>
      <span>My invoices</span>
      <span>My data</span>
      <span>"My IC" Program</span>
    </div>
  );
}

function HelpPage() {
  return (
    <main className="help-page">
      <section className="help-shell">
        <div className="help-heading">
          <Link to="/profile">Back to account</Link>
          <h1>Help</h1>
          <p>Use this page as a quick guide to buying tickets, choosing discounts, paying, and finding your documents later.</p>
        </div>

        <section className="help-option-grid" aria-label="Help options">
          {helpOptions.map((option) => (
            <Link className="help-option-card" to={option.href} key={option.title}>
              <span>{option.title}</span>
              <p>{option.text}</p>
              <b aria-hidden="true">&gt;</b>
            </Link>
          ))}
        </section>

        <section className="help-guide" id="faq">
          <div className="help-guide-heading">
            <h2>How to use RailBook</h2>
            <p>The purchase flow is designed to work from left to right: search, configure travelers, choose a train, select seats, pay, then manage the ticket in your account.</p>
          </div>

          {buyingSteps.map((step) => (
            <article className="help-guide-step" key={step.title}>
              <div>
                <h3>{step.title}</h3>
                <p>{step.text}</p>
              </div>
              <HelpVisual type={step.visual} />
            </article>
          ))}
        </section>

        <section className="help-detail-grid">
          <article id="refunds">
            <h2>Refunds</h2>
            <p>Open My tickets, find an active confirmed ticket, and use Refund. Returned tickets move to the Returned tab so they stay visible as account history.</p>
          </article>
          <article id="refunds-invoices">
            <h2>Invoices</h2>
            <p>Tick the invoice option during checkout. After successful payment, the invoice is saved in My invoices and can be downloaded as a PDF.</p>
          </article>
          <article id="ticket-rules">
            <h2>Ticket rules</h2>
            <p>Passenger names, discounts, route, class, and selected seats must match the journey shown on the ticket. Discount eligibility is shown in the app, while document checks happen outside it.</p>
          </article>
          <article id="contact">
            <h2>Contact support</h2>
            <p>
              Use the booking reference, ticket number, invoice number, and payment status when asking for help.
              <Link className="help-inline-link" to="/contact"> Open contact details.</Link>
            </p>
          </article>
        </section>
      </section>
    </main>
  );
}

export default HelpPage;
