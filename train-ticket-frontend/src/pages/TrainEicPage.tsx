import { Link } from "react-router-dom";
import eicTrain from "../../../docs/MyTrainImages/EIC2ndClass.jpg";
import eicFirstClass from "../../../docs/MyTrainImages/EIC1stClass.jpg";
import eicFirstInterior from "../../../docs/MyTrainImages/IC1stClassInterior.jpg";
import eicSecondInterior from "../../../docs/MyTrainImages/IC2nClassOpenInterior.jpg";
import eu200Image from "../../../docs/MyTrainImages/EU200.jpg";
import eu44Image from "../../../docs/MyTrainImages/EU44.jpg";

const eicHighlights = [
  "Express InterCity services use higher-standard locomotive-hauled coaches with full seat reservation.",
  "EIC trains are intended for fast national routes, normally planned around 160 km/h running and up to 200 km/h where infrastructure and rolling stock allow.",
  "First and second class coaches are available, with air conditioning, comfortable seats, luggage space, and power where the coach type supports it.",
  "Selected services include a restaurant car or minibar trolley, so longer daytime journeys feel closer to the premium EIP experience.",
  "Reserved seats, discounts, add-on tickets, loyalty points, PDFs, and live trip details all work through the RailBook booking flow.",
];

const locomotiveHighlights = [
  {
    name: "EU200 Griffin",
    image: eu200Image,
    alt: "EU200 Griffin locomotive in PKP Intercity colours",
    details:
      "A modern multi-system locomotive used for high-quality long-distance services. It represents the newest generation of PKP Intercity electric traction and is suited to 160-200 km/h express work.",
  },
  {
    name: "EU44 Husarz",
    image: eu44Image,
    alt: "EU44 Husarz locomotive in PKP Intercity colours",
    details:
      "A proven Siemens Eurosprinter locomotive used on flagship intercity routes. Its power and reliability make it one of the strongest locomotives in the express roster.",
  },
];

const eicGallery = [
  { src: eicTrain, alt: "EIC second class coach at a platform" },
  { src: eicFirstClass, alt: "EIC first class coach" },
  { src: eicFirstInterior, alt: "EIC first class interior with seats and tables" },
  { src: eicSecondInterior, alt: "EIC second class open coach interior" },
];

const serviceFacts = [
  ["Reservation", "Full seat reservation on RailBook EIC services"],
  ["Speed", "160-200 km/h with suitable locomotive, coaches, and route"],
  ["Rolling stock", "Top locomotive-hauled sets, including EU200 and EU44 services"],
  ["Classes", "First and second class coaches"],
  ["Comfort", "Air conditioning, luggage areas, catering on selected services"],
];

function TrainEicPage() {
  return (
    <main className="train-detail-page">
      <article className="train-detail-content eip-longform eic-longform">
        <nav className="breadcrumb-nav trains-breadcrumb" aria-label="Breadcrumb">
          <Link to="/">Home</Link>
          <span>-&gt;</span>
          <Link to="/#passenger-info">For Passengers</Link>
          <span>-&gt;</span>
          <Link to="/trains">Our trains</Link>
          <span>-&gt;</span>
          <span>Express InterCity (EIC)</span>
        </nav>

        <p className="eyebrow">Express InterCity (EIC)</p>
        <h1>Express InterCity (EIC)</h1>

        <p className="train-detail-lead eic-lead">
          Express InterCity is RailBook's fast locomotive-hauled express category: comfortable coaches, full seat
          reservation, premium rolling stock, and long-distance services designed around 160-200 km/h running.
        </p>

        <img className="eip-hero-image eic-hero-image" src={eicTrain} alt="Express InterCity coach at a station platform" />

        <section className="eip-info-section eip-intro-section">
          <h2>Fast express service with top rolling stock</h2>
          <p>
            EIC sits between the fixed ED250 Premium service and the broader InterCity category. The train is formed
            from high-standard coaches, usually with a strong electric locomotive at the front, giving passengers a
            reserved-seat express service without losing the flexibility of classic carriage sets.
          </p>
          <p>
            On the strongest routes, EIC services can be planned for 160-200 km/h operation. In RailBook, that makes
            them a natural flagship option for passengers who want speed, comfort, and a familiar seat-reservation
            flow.
          </p>
        </section>

        <section className="eip-fact-grid" aria-label="EIC service highlights">
          {eicHighlights.map((fact) => (
            <article key={fact}>
              <span aria-hidden="true">EIC</span>
              <p>{fact}</p>
            </article>
          ))}
        </section>

        <section className="eip-info-section">
          <h2>Top locomotives in the roster</h2>
          <p>
            EIC services are especially convincing when they are hauled by the strongest modern locomotives in the PKP
            Intercity fleet. RailBook highlights EU200 and EU44 because they represent the upper end of locomotive-led
            intercity performance.
          </p>
        </section>

        <section className="eic-locomotive-grid" aria-label="EIC locomotive highlights">
          {locomotiveHighlights.map((locomotive) => (
            <article key={locomotive.name} className="eic-locomotive-card">
              <img src={locomotive.image} alt={locomotive.alt} />
              <div>
                <h2>{locomotive.name}</h2>
                <p>{locomotive.details}</p>
              </div>
            </article>
          ))}
        </section>

        <section className="eip-info-section">
          <h2>Services</h2>
          <p>
            EIC trains enforce full seat reservation. First class passengers can expect a quieter, roomier journey,
            while second class remains a comfortable express product with proper luggage areas and reserved seating.
          </p>
          <p>
            Selected services include catering through a restaurant car or minibar trolley. Bicycle, accessible, and
            family-friendly facilities depend on the exact carriage set assigned to the trip and are shown where the
            train data supports them.
          </p>
        </section>

        <section className="eip-gallery-section" aria-label="EIC coach and interior gallery">
          {eicGallery.map((image) => (
            <img key={image.alt} src={image.src} alt={image.alt} />
          ))}
        </section>

        <section className="eip-info-section">
          <h2>Do you know that...</h2>
          <p>
            EIC was created as an express InterCity brand for high-quality national routes. In practice, it combines
            the comfort of upgraded coaches with the operational flexibility of locomotive-hauled trainsets.
          </p>
          <p>
            Unlike EIP, which uses fixed ED250 units, EIC can be formed from different coach sets. That makes the
            carriage layout more varied, while still keeping the important passenger-facing promise: fast running,
            seat reservation, and comfortable long-distance travel.
          </p>
        </section>

        <section className="eip-info-section">
          <h2>Technical and service summary</h2>
          <dl className="eip-spec-table">
            {serviceFacts.map(([label, value]) => (
              <div key={label}>
                <dt>{label}</dt>
                <dd>{value}</dd>
              </div>
            ))}
          </dl>
        </section>
      </article>
    </main>
  );
}

export default TrainEicPage;
