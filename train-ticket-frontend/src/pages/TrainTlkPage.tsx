import { Link } from "react-router-dom";
import ep07Image from "../../../docs/MyTrainImages/EP07.jpg";
import eu07Image from "../../../docs/NotMyTrainImages/EU07(Radial's).jpg";
import sn84Image from "../../../docs/MyTrainImages/SN84.jpg";
import su4210Image from "../../../docs/NotMyTrainImages/SU4210(Radial's).jpg";
import tlkSecondClass from "../../../docs/MyTrainImages/TLK2ndClass.jpg";
import tlkSecondClassInterior from "../../../docs/MyTrainImages/TLK2ndClassInterior.jpg";
import tlkSleeper from "../../../docs/MyTrainImages/TLKSleeper.jpg";
import tlkSleeperInteriorOne from "../../../docs/NotMyTrainImages/TLKSleeperCarInterior1(Wikicommons).jpg";
import tlkSleeperInteriorTwo from "../../../docs/NotMyTrainImages/TLKSleeperCarInterior2(Wikicommons).jpg";
import tlkTrainImage from "../../../docs/MyTrainImages/TLKtrain.jpg";

const tlkFacts = [
  "TLK is the lowest-price long-distance category, built for passengers who want to travel far without paying premium fares.",
  "The network reaches deep into Poland, including smaller cities, holiday routes, overnight services, and places beyond the main premium corridors.",
  "Most services use classic locomotive-hauled coaches, often with EP07 or EU07 family locomotives on electrified routes.",
  "On unelectrified sections, diesel locomotives such as SU4210 or diesel units such as SN84 can keep the service connected.",
];

const tlkElectricLocos = [
  {
    name: "EP07",
    image: ep07Image,
    text:
      "A familiar electric locomotive for classic TLK consists, well suited to long-distance trains built from conventional coaches.",
  },
  {
    name: "EU07",
    image: eu07Image,
    text:
      "A universal electric locomotive from the same 07 family, often seen on everyday passenger services across Poland.",
  },
];

const tlkDieselVehicles = [
  {
    name: "SU4210",
    image: su4210Image,
    text:
      "A diesel locomotive for non-electrified routes, allowing TLK coach services to continue beyond the wires.",
  },
  {
    name: "SKPL SN84",
    image: sn84Image,
    text:
      "A diesel unit for thinner regional intercity links, useful where a full locomotive-hauled consist would be too heavy for demand.",
  },
];

const tlkCarriages = [
  { label: "Second class coach", image: tlkSecondClass },
  { label: "Second class compartment", image: tlkSecondClassInterior },
  { label: "Sleeper coach", image: tlkSleeper },
  { label: "Sleeper corridor", image: tlkSleeperInteriorOne },
  { label: "Sleeper compartment", image: tlkSleeperInteriorTwo },
];

function TrainTlkPage() {
  return (
    <main className="train-detail-page">
      <article className="train-detail-content eip-longform tlk-longform">
        <nav className="breadcrumb-nav trains-breadcrumb" aria-label="Breadcrumb">
          <Link to="/">Home</Link>
          <span>-&gt;</span>
          <Link to="/#passenger-info">For Passengers</Link>
          <span>-&gt;</span>
          <Link to="/trains">Our trains</Link>
          <span>-&gt;</span>
          <span>Twoje Linie Kolejowe (TLK)</span>
        </nav>

        <p className="eyebrow">Twoje Linie Kolejowe (TLK)</p>
        <h1>Twoje Linie Kolejowe (TLK)</h1>

        <p className="train-detail-lead eic-lead">
          TLK is the practical, cheapest long-distance service for passengers who care most about reaching the
          destination at a good price. It covers the big routes, but also the farthest nooks and crannies of Poland.
        </p>

        <img className="eip-hero-image tlk-hero-image" src={tlkTrainImage} alt="TLK train at a railway yard" />

        <section className="eip-fact-grid" aria-label="TLK category highlights">
          {tlkFacts.map((fact) => (
            <article key={fact}>
              <span aria-hidden="true">TLK</span>
              <p>{fact}</p>
            </article>
          ))}
        </section>

        <section className="eip-info-section">
          <h2>Affordable long-distance travel</h2>
          <p>
            Twoje Linie Kolejowe is the budget-minded intercity category. Passengers get a reserved long-distance
            connection, classic coaches, and broad route coverage, while the service keeps prices lower than the
            faster premium categories.
          </p>
          <p>
            TLK trains are especially useful for journeys where reach matters as much as speed: late evening services,
            seasonal routes, smaller cities, and routes where a conventional train can serve more stops along the way.
          </p>
        </section>

        <section className="ic-fleet-section">
          <div className="ic-section-heading">
            <p className="eyebrow">Electric locomotive-hauled TLK</p>
            <h2>Mostly the 07 family</h2>
            <p>
              On electrified routes, TLK is usually a classic coach train. EP07 and EU07 family locomotives are a
              natural fit: dependable, familiar, and able to work the wide mix of TLK carriages used around the country.
            </p>
          </div>

          <div className="eic-locomotive-grid tlk-locomotive-grid">
            {tlkElectricLocos.map((locomotive) => (
              <article key={locomotive.name} className="eic-locomotive-card">
                <img src={locomotive.image} alt={`${locomotive.name} locomotive`} />
                <div>
                  <h2>{locomotive.name}</h2>
                  <p>{locomotive.text}</p>
                </div>
              </article>
            ))}
          </div>
        </section>

        <section className="ic-fleet-section">
          <div className="ic-section-heading">
            <p className="eyebrow">Unelectrified routes</p>
            <h2>Diesel traction where the wires end</h2>
            <p>
              TLK can also serve routes where electric locomotives cannot go. Diesel locomotive-hauled trains and
              diesel units help keep long-distance travel available in places that would otherwise sit outside the
              main electric network.
            </p>
          </div>

          <div className="eic-locomotive-grid tlk-locomotive-grid">
            {tlkDieselVehicles.map((vehicle) => (
              <article key={vehicle.name} className="eic-locomotive-card">
                <img src={vehicle.image} alt={`${vehicle.name} diesel vehicle`} />
                <div>
                  <h2>{vehicle.name}</h2>
                  <p>{vehicle.text}</p>
                </div>
              </article>
            ))}
          </div>
        </section>

        <section className="ic-fleet-section">
          <div className="ic-section-heading">
            <p className="eyebrow">Carriages</p>
            <h2>Classic coaches, compartments and sleepers</h2>
            <p>
              TLK carriage sets can vary from route to route. Passengers may meet first class, second class, mixed
              coaches, compartment interiors, and sleeper cars on night or long-distance services.
            </p>
          </div>

          <section className="tlk-carriage-gallery" aria-label="TLK carriage and interior gallery">
            {tlkCarriages.map((carriage) => (
              <figure key={carriage.label}>
                <img src={carriage.image} alt={carriage.label} />
                <figcaption>{carriage.label}</figcaption>
              </figure>
            ))}
          </section>
        </section>

        <section className="eip-info-section">
          <h2>Services and route coverage</h2>
          <p>
            TLK is not the fastest train family, but it is one of the most useful. It keeps long-distance travel within
            reach for passengers who want low fares, overnight options, broad coverage, and a straightforward reserved
            seat rather than premium extras.
          </p>
        </section>
      </article>
    </main>
  );
}

export default TrainTlkPage;
