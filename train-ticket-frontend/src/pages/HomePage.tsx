import { Link } from "react-router-dom";
import TrainSearchForm from "../components/TrainSearchForm";

const quickActions = [
  "Season ticket",
  "Seat reservations",
  "Rail passes",
];

const informationCards = [
  {
    title: "Customer Service Centres",
    text: "Find support before, during, or after your journey.",
  },
  {
    title: "Mobile Application",
    text: "A future home for app links and mobile tickets.",
  },
  {
    title: "Railway Cards",
    text: "Placeholder for loyalty cards and passenger discounts.",
  },
];

const featureCards = [
  {
    title: "Weekend offers",
    imageClass: "feature-image-station",
  },
  {
    title: "Explore Poland by rail",
    imageClass: "feature-image-city",
  },
  {
    title: "Our trains",
    imageClass: "feature-image-train",
  },
  {
    title: "Student offer",
    imageClass: "feature-image-passengers",
  },
];

const passengerLinks = [
  "Complaints",
  "Contact",
  "Special assistance",
  "Frequently Asked Questions",
];

function HomePage() {
  return (
    <main>
      <section className="hero-section">
        <div className="journey-line" aria-hidden="true">
          <span />
          <strong>Journey is yours</strong>
          <span />
        </div>

        <div className="hero-visual" aria-label="Modern intercity train at a station">
          <div className="train-scene">
            <div className="train-body">
              <span className="train-window" />
              <span className="train-window" />
              <span className="train-window" />
              <span className="train-window" />
            </div>
            <div className="train-nose" />
            <div className="rails" />
          </div>
        </div>

        <div className="search-panel">
          <TrainSearchForm />
        </div>

        <div className="quick-actions" id="offers">
          {quickActions.map((action) => (
            <Link to="/search" key={action} className="quick-action-card">
              <span className="ticket-icon" aria-hidden="true" />
              {action}
              <span className="arrow" aria-hidden="true">-&gt;</span>
            </Link>
          ))}
        </div>
      </section>

      <section className="content-section">
        <h1>Important information for passengers</h1>
        <div className="info-link-grid">
          {informationCards.map((card) => (
            <a href="#passenger-info" key={card.title} className="info-link-card">
              <span>
                <strong>{card.title}</strong>
                <small>{card.text}</small>
              </span>
              <span className="arrow" aria-hidden="true">-&gt;</span>
            </a>
          ))}
        </div>
      </section>

      <section className="content-section">
        <p className="eyebrow">Thematic information</p>
        <h2>What's important in intercity</h2>
        <div className="feature-grid">
          {featureCards.map((card) => (
            <article className="feature-card" key={card.title}>
              <div className={`feature-image ${card.imageClass}`} />
              <h3>{card.title}</h3>
            </article>
          ))}
        </div>
      </section>

      <section className="content-section" id="passenger-info">
        <p className="eyebrow">Frequently searched</p>
        <h2>For Passengers</h2>
        <div className="passenger-grid">
          {passengerLinks.map((link) => (
            <a href="#passenger-info" className="passenger-card" key={link}>
              <span className="line-icon" aria-hidden="true" />
              {link}
            </a>
          ))}
        </div>
      </section>

      <footer className="site-footer">
        <span>RailWay ticket platform</span>
        <span>Demo frontend for TrainTicketPlatformAPI</span>
      </footer>
    </main>
  );
}

export default HomePage;
