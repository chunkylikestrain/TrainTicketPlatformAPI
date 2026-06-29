import { Link } from "react-router-dom";
import ed250Image from "../../../docs/MyTrainImages/ED250.jpg";
import ed250FirstClassOne from "../../../docs/NotMyTrainImages/ED2501stClassInterior1(Wikicommons).jpg";
import ed250FirstClassTwo from "../../../docs/NotMyTrainImages/ED2501stClassInterior2(Wkicommons).jpg";
import ed250SecondClassOne from "../../../docs/NotMyTrainImages/ED2502ndClassInterior1(Wkicommons).jpg";
import ed250SecondClassTwo from "../../../docs/NotMyTrainImages/ED2502ndClassInterior2(Wkicommons).jpg";
import ed250SecondClassThree from "../../../docs/NotMyTrainImages/ED2502ndClassInterior3(Wkicommons).jpg";
import ed250DriverCab from "../../../docs/NotMyTrainImages/ED250DriverCab(Wikicommons).jpg";
import eipLogo from "../../../docs/NotMyTrainImages/EICLogo(Wikicommons).svg";

const technicalSpecs = [
  ["Length", "187.4 m"],
  ["Maximum width of body", "2.8 m"],
  ["Maximum height above rail", "4.1 m"],
  ["Maximum operating speed", "250 km/h"],
  ["Power", "5,500 kW"],
  ["Estimated service weight", "395.5 t"],
  ["Number of sections", "7"],
  ["Passenger seats", "402"],
  ["RailBook EIP trainsets", "20 ED250 units"],
];

const serviceHighlights = [
  "Reserved seating is required, so every EIP ticket is tied to a specific train, carriage, and seat.",
  "First and second class are available, with fold-out tables, reading lights, power sockets, luggage shelves, and coat hooks near seats.",
  "Carriage 3 includes the dining/bar area and accessible facilities, matching the ED250 layout used in RailBook.",
  "Bicycle spaces, family compartments, wheelchair spaces, and large-luggage areas are shown during seat selection when available.",
  "EIP services are designed for long-distance intercity routes such as Warsaw, Krakow, Katowice, Wroclaw, Szczecin, and the Tri-City.",
];

const quietZoneRules = [
  "Keep phone calls and loud conversations outside the Quiet Zone.",
  "Use headphones and keep device volume low.",
  "Silence laptop, tablet, and phone notifications before boarding.",
  "Food and drink orders are better handled in the dining area rather than at a quiet-zone seat.",
];

const seatLayoutFacts = [
  "Unit 1 is first class.",
  "Unit 2 is second class with family/compartment and open-space seating.",
  "Unit 3 is the dining unit and includes wheelchair accessibility.",
  "Units 4 to 6 are second class open-space seating.",
  "Unit 7 is the quiet second-class cab unit.",
];

const eipGallery = [
  { src: ed250DriverCab, alt: "ED250 driver's cab" },
  { src: ed250FirstClassOne, alt: "ED250 first class seating with tables" },
  { src: ed250FirstClassTwo, alt: "ED250 first class open saloon" },
  { src: ed250SecondClassOne, alt: "ED250 second class seating" },
  { src: ed250SecondClassTwo, alt: "ED250 second class saloon" },
  { src: ed250SecondClassThree, alt: "ED250 second class headrest detail" },
];

function TrainEipPage() {
  return (
    <main className="train-detail-page">
      <article className="train-detail-content eip-longform">
        <nav className="breadcrumb-nav trains-breadcrumb" aria-label="Breadcrumb">
          <Link to="/">Home</Link>
          <span>-&gt;</span>
          <Link to="/#passenger-info">For Passengers</Link>
          <span>-&gt;</span>
          <Link to="/trains">Our trains</Link>
          <span>-&gt;</span>
          <span>Express InterCity Premium (EIP)</span>
        </nav>

        <p className="eyebrow">Express InterCity Premium (EIP)</p>
        <h1>Express InterCity Premium (EIP)</h1>

        <img
          className="eip-hero-image"
          src={ed250Image}
          alt="ED250 Express InterCity Premium train at a station platform"
        />

        <section className="eip-info-section eip-intro-section">
          <h2>Specifications and services</h2>
          <p>
            Express InterCity Premium is RailBook's fastest premium train category. It is represented by ED250
            Pendolino trainsets, built for comfortable long-distance travel with guaranteed seat reservation,
            modern interiors, accessible facilities, and a dedicated dining area.
          </p>
          <p>
            EIP trains connect the largest Polish cities and support the full RailBook booking flow:
            search, class selection, seat-map selection, add-ons, discounts, loyalty points, payment, PDF tickets, and
            current-trip details.
          </p>
        </section>

        <section className="eip-fact-grid" aria-label="EIP service highlights">
          {serviceHighlights.map((fact) => (
            <article key={fact}>
              <span aria-hidden="true">EIP</span>
              <p>{fact}</p>
            </article>
          ))}
        </section>

        <section className="eip-info-section">
          <h2>Our offer</h2>
          <p>
            EIP fares are shown directly in the connection list. Passengers can combine eligible statutory discounts
            with RailBook loyalty points at checkout, and the final amount is confirmed again before payment.
          </p>
          <p>
            Because EIP is a reserved-seat service, passengers should buy before boarding. In RailBook, the booking
            hold keeps selected seats temporarily unavailable while the checkout is in progress.
          </p>
        </section>

        <section className="eip-gallery-section" aria-label="ED250 interior gallery">
          {eipGallery.map((image) => (
            <img key={image.alt} src={image.src} alt={image.alt} />
          ))}
        </section>

        <section className="eip-info-section">
          <h2>Do you know that...</h2>
          <p>
            The ED250 nose shape is designed for aerodynamic running, helping reduce noise and vibration at higher
            speeds. The train uses a distributed electric drive and fixed seven-section formation, which gives the
            passenger areas a smooth, open feel.
          </p>
        </section>

        <section className="eip-info-section eip-quiet-zone">
          <div>
            <h2>Quiet Zone</h2>
            <p>
              The Quiet Zone is intended for passengers who want a calm journey. In RailBook's ED250 model, it is the
              seventh second-class unit.
            </p>
          </div>
          <ul>
            {quietZoneRules.map((rule) => (
              <li key={rule}>{rule}</li>
            ))}
          </ul>
        </section>

        <section className="eip-info-section">
          <h2>Seat distribution</h2>
          <p>
            RailBook follows a fixed ED250 composition so the passenger seat map can show the right carriage class,
            dining unit, family areas, accessible places, and quiet-zone carriage.
          </p>
          <div className="eip-layout-strip" aria-label="ED250 carriage layout summary">
            {seatLayoutFacts.map((fact, index) => (
              <div key={fact}>
                <strong>{index + 1}</strong>
                <span>{fact.replace(/^Unit \d+ is /, "").replace(/^Units \d+ to \d+ are /, "")}</span>
              </div>
            ))}
          </div>
        </section>

        <section className="eip-info-section">
          <h2>Technical data</h2>
          <dl className="eip-spec-table">
            {technicalSpecs.map(([label, value]) => (
              <div key={label}>
                <dt>{label}</dt>
                <dd>{value}</dd>
              </div>
            ))}
          </dl>
        </section>

        <section className="eip-logo-panel" aria-label="Express InterCity Premium logo">
          <img src={eipLogo} alt="EIP Express InterCity Premium logo" />
        </section>
      </article>
    </main>
  );
}

export default TrainEipPage;
