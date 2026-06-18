import { Link, useParams, useSearchParams } from "react-router-dom";

function OrderSummaryPage() {
  const { tripId } = useParams();
  const [searchParams] = useSearchParams();
  const selectedClass = searchParams.get("class") === "2" ? "2" : "1";
  const email = searchParams.get("email") ?? "";
  const price = selectedClass === "1" ? "134,00 PLN" : "90,00 PLN";
  const vat = selectedClass === "1" ? "9,93 PLN" : "6,67 PLN";

  return (
    <main className="order-summary-page">
      <section className="connection-hero order-summary-hero" aria-hidden="true">
        <div className="connection-train" />
      </section>

      <section className="order-summary-content">
        <p className="previous-system-note">
          Previous system: <a href="#previous">old-ticket.example</a> - invoices, refunds, and data changes
        </p>

        <nav className="checkout-steps order-summary-steps" aria-label="Purchase steps">
          <Link to="/">Home</Link>
          <Link to="/">Search engine</Link>
          <Link to="/search">List of connections</Link>
          <Link to={`/summary/${tripId}?class=${selectedClass}`}>Your travel</Link>
          <strong>Summary</strong>
          <span>Payment</span>
          <span>Ticket</span>
        </nav>

        <section className="final-summary-card">
          <div className="final-summary-top">
            <div className="final-timeline">
              <h1>Friday, 19 June</h1>
              <div>
                <span className="final-line" aria-hidden="true" />
                <p><strong>06:06</strong> Rzeszow Glowny</p>
                <p><strong>07:27</strong> Krakow Gl.</p>
              </div>
            </div>

            <div className="final-train-details">
              <p><b>EIP</b> <strong>3508</strong></p>
              <p>Car 1, seat 46, by the window</p>
              <span>A place at the table</span>
            </div>

            <div className="final-passenger-details">
              <span>Time to buy: <strong>9:59</strong></span>
              <p>1 passenger</p>
              <p>1x Normal Ticket</p>
              <p>{selectedClass} class</p>
              <a href="#offer">Check the offer details</a>
            </div>
          </div>

          <div className="final-price-panel">
            <div>
              <strong>Outbound journey</strong>
              <span>A-Base price</span>
            </div>
            <div>
              <span>Price</span>
              <strong>{price}</strong>
            </div>
          </div>
        </section>

        <section className="amount-due-panel">
          <div>
            <h2>Amount due</h2>
            <span>8% VAT</span>
            {email && <p>Ticket will be sent to {email}</p>}
          </div>
          <div>
            <strong>{price}</strong>
            <span>{vat}</span>
          </div>
        </section>

        <section className="order-summary-actions">
          <Link to={`/checkout/${tripId}?class=${selectedClass}&email=${encodeURIComponent(email)}`}>Payment</Link>
          <Link to={`/data/${tripId}?class=${selectedClass}`}>Cancel</Link>
        </section>
      </section>

      <section className="summary-footer-train" aria-hidden="true">
        <div className="connection-train" />
      </section>

      <section className="summary-legal">
        <div>
          <h2>Technological break.</h2>
          <p>
            Please remember about scheduled technical breaks in the online sales system. You cannot buy tickets
            during this break.
          </p>
          <a href="#accessibility">Declaration of Accessibility</a>
        </div>
        <div>
          <p>
            The prices presented are indicative and published for informational purposes. The final prices are
            available in this purchase summary before payment.
          </p>
          <strong>RailWay demo frontend for TrainTicketPlatformAPI</strong>
        </div>
      </section>
    </main>
  );
}

export default OrderSummaryPage;
