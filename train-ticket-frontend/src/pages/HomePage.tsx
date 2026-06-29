import { Link } from "react-router-dom";
import TrainSearchForm from "../components/TrainSearchForm";
import ed250Image from "../../../docs/MyTrainImages/ED250.jpg";
import explorePolandImage from "../../../docs/MyCityImages/Krakow.jpg";
import studentOfferImage from "../../../docs/NotMyTrainImages/ED160(Radial's).jpg";

const quickActions = [
  "Season ticket",
  "Seat reservations",
  "Rail passes",
];

const informationCards = [
  {
    title: "Customer Service Centres",
    text: "Find support before, during, or after your journey.",
    to: "/contact",
  },
  {
    title: "Mobile Application",
    text: "Account access and ticket information for mobile travelers.",
    to: "/help",
  },
  {
    title: "Railway Cards",
    text: "Manage passenger discounts and reusable travel preferences.",
    to: "/help",
  },
];

const featureCards = [
  {
    title: "Explore Poland by rail",
    image: explorePolandImage,
    to: "/offers/explore",
  },
  {
    title: "Our trains",
    image: ed250Image,
    to: "/trains",
  },
  {
    title: "Student offer",
    image: studentOfferImage,
    to: "/offers/student",
  },
];

const passengerLinks = [
  "Complaints",
  "Contact",
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
            <Link to={card.to} key={card.title} className="info-link-card">
              <span>
                <strong>{card.title}</strong>
                <small>{card.text}</small>
              </span>
              <span className="arrow" aria-hidden="true">-&gt;</span>
            </Link>
          ))}
        </div>
      </section>

      <section className="content-section">
        <p className="eyebrow">Thematic information</p>
        <h2>What's important in intercity</h2>
        <div className="feature-grid">
          {featureCards.map((card) => (
            <Link className="feature-card" key={card.title} to={card.to}>
              <img className="feature-image" src={card.image} alt="" />
              <h3>{card.title}</h3>
            </Link>
          ))}
        </div>
      </section>

      <section className="content-section" id="passenger-info">
        <p className="eyebrow">Frequently searched</p>
        <h2>For Passengers</h2>
        <div className="passenger-grid">
          {passengerLinks.map((link) => (
            <Link to={getPassengerLinkTarget(link)} className="passenger-card" key={link}>
              <span className="line-icon" aria-hidden="true" />
              {link}
            </Link>
          ))}
        </div>
      </section>

      <footer className="site-footer">
        <span>RailWay ticket platform</span>
        <span>ASP.NET Core and React ticket booking system</span>
      </footer>
    </main>
  );
}

function getPassengerLinkTarget(label: string) {
  if (label === "Contact") {
    return "/contact";
  }

  if (label === "Frequently Asked Questions") {
    return "/help/faq";
  }

  return "/help";
}

export default HomePage;
