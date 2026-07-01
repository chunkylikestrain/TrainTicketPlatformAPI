import {
  CoffeeOutlined,
  FileProtectOutlined,
  FileTextOutlined,
  HomeOutlined,
  MessageOutlined,
  PhoneOutlined,
  QuestionCircleOutlined,
  SafetyCertificateOutlined,
  TagsOutlined,
} from "@ant-design/icons";
import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import TrainSearchForm from "../components/TrainSearchForm";
import ed250Image from "../../../docs/MyTrainImages/ED250.jpg";
import explorePolandImage from "../../../docs/MyCityImages/Krakow.jpg";
import studentOfferImage from "../../../docs/NotMyTrainImages/ED160(Radial's).jpg";

const quickActions = [
  {
    title: "Sleeper cars and couchettes",
    to: "/offers/sleeper",
    icon: <HomeOutlined />,
  },
  {
    title: "Domestic offers",
    to: "/offers/domestic",
    icon: <TagsOutlined />,
  },
  {
    title: "Meal while travelling",
    to: "/offers/meal",
    icon: <CoffeeOutlined />,
  },
];

const informationCards: Array<{ title: string; text: string; to: string; icon: ReactNode }> = [
  {
    title: "Refund policy",
    text: "Check refund windows, returned ticket status, and cancellation handling.",
    to: "/help/refund-policy",
    icon: <FileProtectOutlined />,
  },
  {
    title: "Passenger rights",
    text: "Read journey rules, disruption rights, and onboard obligations.",
    to: "/help/passenger-rights",
    icon: <SafetyCertificateOutlined />,
  },
  {
    title: "Invoices",
    text: "Generate an invoice at checkout and download it later from your account.",
    to: "/profile",
    icon: <FileTextOutlined />,
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
  {
    label: "Complaints",
    to: "/contact",
    icon: <MessageOutlined />,
  },
  {
    label: "Contact",
    to: "/contact",
    icon: <PhoneOutlined />,
  },
  {
    label: "Frequently Asked Questions",
    to: "/help/faq",
    icon: <QuestionCircleOutlined />,
  },
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
            <Link to={action.to} key={action.title} className="quick-action-card">
              <span className="ticket-icon" aria-hidden="true">{action.icon}</span>
              {action.title}
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
              <span className="info-link-icon" aria-hidden="true">{card.icon}</span>
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
            <Link to={link.to} className="passenger-card" key={link.label}>
              <span className="line-icon" aria-hidden="true">{link.icon}</span>
              {link.label}
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

export default HomePage;
