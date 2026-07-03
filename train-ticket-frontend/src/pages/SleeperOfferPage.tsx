import { HomeOutlined, TeamOutlined, UserOutlined } from "@ant-design/icons";
import { Link } from "react-router-dom";
import sleeperCoach from "../../../docs/MyTrainImages/TLKSleeper.jpg";
import sleeperCompartment from "../../../docs/NotMyTrainImages/TLKSleeperCarInterior2(Wikicommons).jpg";
import sleeperCorridor from "../../../docs/NotMyTrainImages/TLKSleeperCarInterior1(Wikicommons).jpg";

const sleeperOptions = [
  {
    title: "Single sleeper",
    label: "1-person compartment",
    icon: <UserOutlined />,
    text: "A private compartment for passengers who want the quietest overnight journey.",
  },
  {
    title: "Double sleeper",
    label: "2-person compartment",
    icon: <TeamOutlined />,
    text: "A comfortable option for two passengers travelling together on a night train.",
  },
  {
    title: "Triple sleeper",
    label: "3-person compartment",
    icon: <TeamOutlined />,
    text: "A classic sleeper arrangement for a small group or shared overnight travel.",
  },
];

const couchetteOptions = [
  {
    title: "Four-berth couchette",
    label: "4-person compartment",
    icon: <HomeOutlined />,
  },
  {
    title: "Six-berth couchette",
    label: "6-person compartment",
    icon: <HomeOutlined />,
  },
];

const overnightRoutes = [
  ["Warsaw - Munich", "Przemysl - Munich", "Warsaw - Budapest", "Przemysl - Budapest", "Gdynia - Prague", "Warsaw - Rijeka", "Warsaw - Szklarska Poreba"],
  ["Przemysl - Swinoujscie", "Chelm - Swinoujscie", "Cracow - Kolobrzeg", "Gdynia - Zakopane", "Zakopane - Swinoujscie", "Cracow - Hel"],
  ["Hel - Bohumin", "Cracow - Lebork", "Lebork - Bohumin", "Cracow - Swinoujscie", "Bielsko-Biala - Swinoujscie", "Bielsko-Biala - Kolobrzeg"],
];

function SleeperOfferPage() {
  return (
    <main className="sleeper-offer-page">
      <section className="sleeper-hero" style={{ backgroundImage: `url(${sleeperCoach})` }}>
        <div className="sleeper-hero-overlay">
          <p>Night travel</p>
          <h1>Do you value peace and comfort during night travel?</h1>
          <span>Choose a sleeper car or a couchette when the timetable offers one.</span>
        </div>
      </section>

      <section className="sleeper-shell">
        <nav className="breadcrumb-nav" aria-label="Breadcrumb">
          <Link to="/">Home</Link>
          <span>-&gt;</span>
          <Link to="/#passenger-info">For Passengers</Link>
          <span>-&gt;</span>
          <Link to="/offers">Offers</Link>
          <span>-&gt;</span>
          <span>Sleeper cars and couchettes</span>
        </nav>

        <section className="sleeper-intro-panel">
          <h2>Discover comfortable night travel in a sleeper car or couchette.</h2>
          <p>
            Overnight rail lets you cover a long distance while resting. Sleeper compartments are the quieter, more
            private option, while couchettes are a practical way to travel through the night at a lower supplement.
          </p>
        </section>

        <section className="sleeper-options-section" aria-labelledby="sleeper-compartments-title">
          <h2 id="sleeper-compartments-title">We offer sleeper compartments</h2>
          <div className="sleeper-options-grid">
            {sleeperOptions.map((option) => (
              <article key={option.title} className="sleeper-option-card">
                <span className="sleeper-option-icon" aria-hidden="true">
                  {option.icon}
                </span>
                <strong>{option.label}</strong>
                <h3>{option.title}</h3>
                <p>{option.text}</p>
              </article>
            ))}
          </div>
        </section>

        <section className="sleeper-image-row" aria-label="Sleeper car interior">
          <img src={sleeperCompartment} alt="Sleeper car compartment" />
          <img src={sleeperCorridor} alt="Sleeper car corridor" />
        </section>

        <section className="sleeper-options-section" aria-labelledby="couchette-compartments-title">
          <h2 id="couchette-compartments-title">Or a couchette compartment</h2>
          <div className="sleeper-options-grid couchette-options-grid">
            {couchetteOptions.map((option) => (
              <article key={option.title} className="sleeper-option-card">
                <span className="sleeper-option-icon" aria-hidden="true">
                  {option.icon}
                </span>
                <strong>{option.label}</strong>
                <h3>{option.title}</h3>
                <p>
                  A shared berth compartment for passengers who want to sleep on board without booking a private
                  sleeper room.
                </p>
              </article>
            ))}
          </div>
        </section>

        <section className="sleeper-cta-panel">
          <h2>Book your place now and experience the unique charm of night travel.</h2>
          <Link to="/">Search night trains</Link>
        </section>

        <section className="sleeper-routes-section" aria-labelledby="night-routes-title">
          <h2 id="night-routes-title">Available sleeper train connections</h2>
          <div className="sleeper-routes-panel">
            {overnightRoutes.map((column, index) => (
              <ul key={`overnight-route-column-${index}`}>
                {column.map((route) => (
                  <li key={route}>{route}</li>
                ))}
              </ul>
            ))}
          </div>
        </section>
      </section>
    </main>
  );
}

export default SleeperOfferPage;
