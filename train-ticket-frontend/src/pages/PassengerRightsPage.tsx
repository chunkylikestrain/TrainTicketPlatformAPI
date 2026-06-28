import { Link } from "react-router-dom";

const obligationCards = [
  {
    title: "Carry the right documents",
    text: "During inspection, show a valid ticket, seat reservation where required, ID for named tickets, and proof for any discount you used.",
  },
  {
    title: "Check your ticket before travel",
    text: "Make sure the passenger name, route, date, train, class, discount, and seat match the journey you meant to buy.",
  },
  {
    title: "Look after your belongings",
    text: "Keep luggage, bicycles, and animals under your supervision and place bags only in luggage spaces, above your seat, or under your seat.",
  },
  {
    title: "Follow onboard rules",
    text: "Follow conductor instructions, safety notices, carriage rules, and quiet-zone expectations where they apply.",
  },
] as const;

const rightsCards = [
  {
    title: "Delay or cancellation",
    text: "If the service is cancelled or seriously disrupted, you may be offered an alternative journey, later travel, a reroute, or a refund depending on the case.",
  },
  {
    title: "Complaints and claims",
    text: "Keep your ticket number and booking reference. Complaints are usually tied to the journey date, ticket, or payment record.",
  },
  {
    title: "Accessibility support",
    text: "Passengers who need boarding, alighting, or mobility support should request help in advance so staff can prepare the right assistance.",
  },
  {
    title: "Lost property",
    text: "If you find something onboard, give it to the conductor. If you lose something, contact customer support with the train and journey details.",
  },
] as const;

const conductTips = [
  "Keep aisles, doors, toilets, and wheelchair spaces clear.",
  "Use headphones and keep calls short, especially in quiet areas.",
  "Let passengers leave the train before boarding.",
  "Store large bags safely so they cannot fall or block other passengers.",
  "Ask before moving another passenger's belongings.",
  "Keep food smells and mess under control, and take rubbish with you.",
  "Travel with pets only under the carrier's rules, and keep animals calm and supervised.",
  "Be ready before your stop so boarding and alighting stay smooth for everyone.",
] as const;

function PassengerRightsPage() {
  return (
    <main className="help-page passenger-rights-page">
      <section className="help-shell">
        <div className="help-heading">
          <Link to="/help">Back to help</Link>
          <h1>Passenger Rights And Obligations</h1>
          <p>
            A plain-language guide to ticket checks, onboard behaviour, luggage, animals, support,
            disruptions, and what to do when something goes wrong.
          </p>
        </div>

        <section className="passenger-rules-hero">
          <div>
            <span>Before you board</span>
            <h2>Have the ticket, reservation, ID, and discount document ready.</h2>
            <p>
              RailBook stores your QR ticket and PDF in My tickets, but the passenger is still responsible
              for travelling with the documents needed for inspection.
            </p>
          </div>
          <Link to="/profile" className="refund-policy-action">Open My tickets</Link>
        </section>

        <section className="passenger-rules-columns">
          <div>
            <h2>Your obligations</h2>
            <div className="passenger-rules-card-grid">
              {obligationCards.map((card) => (
                <article key={card.title}>
                  <h3>{card.title}</h3>
                  <p>{card.text}</p>
                </article>
              ))}
            </div>
          </div>

          <div>
            <h2>Your rights</h2>
            <div className="passenger-rules-card-grid">
              {rightsCards.map((card) => (
                <article key={card.title}>
                  <h3>{card.title}</h3>
                  <p>{card.text}</p>
                </article>
              ))}
            </div>
          </div>
        </section>

        <section className="passenger-conduct-panel">
          <div>
            <span>Good train etiquette</span>
            <h2>Small habits make the trip better for everyone.</h2>
          </div>
          <ul>
            {conductTips.map((tip) => (
              <li key={tip}>{tip}</li>
            ))}
          </ul>
        </section>

        <section className="refund-policy-notes">
          <article>
            <h2>Bicycles, luggage, and animals</h2>
            <p>
              Bicycles need an available bicycle space where the train supports them. Large luggage and animals
              may require extra conditions or extra tickets depending on the route and carrier rules.
            </p>
          </article>
          <article>
            <h2>Buying onboard</h2>
            <p>
              If you need to buy from the conductor, report before boarding or immediately after boarding where
              allowed. On trains with mandatory reservations, travel is possible only when a seat is available.
            </p>
          </article>
          <article>
            <h2>Need help?</h2>
            <p>
              For a booking, refund, invoice, lost property, or accessibility question, use your ticket number
              and booking reference when contacting support.
              <Link className="help-inline-link" to="/contact"> Open contact details.</Link>
            </p>
          </article>
        </section>
      </section>
    </main>
  );
}

export default PassengerRightsPage;
