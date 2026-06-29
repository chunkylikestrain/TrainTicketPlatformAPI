import { Link } from "react-router-dom";
import ed160Image from "../../../docs/NotMyTrainImages/ED160(Radial's).jpg";
import ed160FirstInterior from "../../../docs/NotMyTrainImages/ED1601stClassInterior(Wikicommons).jpg";
import ed160SecondInterior from "../../../docs/NotMyTrainImages/ED1602ndClassInterior(Wikicommons).jpg";
import ed161Image from "../../../docs/NotMyTrainImages/ED161(Radial's).jpg";
import ed161FirstInterior from "../../../docs/NotMyTrainImages/ED1611stClassInterior(Wikicommons).jpg";
import ed161SecondInterior from "../../../docs/NotMyTrainImages/ED1612ndClassInterior(Wikicommons).jpg";
import ed74Image from "../../../docs/NotMyTrainImages/ED74(Wkicommons).jpg";
import ed74FirstInterior from "../../../docs/NotMyTrainImages/ED741stClassinterior(Wikicommons).jpg";
import ep07Image from "../../../docs/MyTrainImages/EP07.jpg";
import ep08Image from "../../../docs/NotMyTrainImages/EP08(Radial's).jpg";
import ep09Image from "../../../docs/MyTrainImages/EP09.jpg";
import eu07Image from "../../../docs/NotMyTrainImages/EU07(Radial's).jpg";
import eu160Image from "../../../docs/MyTrainImages/EU160.jpg";
import icFirstClassImage from "../../../docs/MyTrainImages/IC1stClass.jpg";
import icFirstClassInterior from "../../../docs/MyTrainImages/IC1stClassInterior.jpg";
import icSecondClassImage from "../../../docs/NotMyTrainImages/IC2ndClass(Wkicommons).jpg";
import icSecondClassInterior from "../../../docs/MyTrainImages/IC2nClassOpenInterior.jpg";
import icTrainImage from "../../../docs/MyTrainImages/ICtrain.jpg";
import icTrainTwoImage from "../../../docs/MyTrainImages/ICtrain2.jpg";
import sd85Image from "../../../docs/MyTrainImages/SD85.png";
import sd85SecondInterior from "../../../docs/MyTrainImages/SD852ndClassInterior.jpg";
import sn84Image from "../../../docs/MyTrainImages/SN84.jpg";
import su160Image from "../../../docs/NotMyTrainImages/SU160(Radial's).jpg";
import su4210Image from "../../../docs/NotMyTrainImages/SU4210(Radial's).jpg";

const emuSets = [
  {
    name: "ED160 Flirt",
    image: ed160Image,
    interiors: [ed160FirstInterior, ed160SecondInterior],
    text:
      "A modern electric multiple unit for core IC routes, with bright interiors, accessible boarding areas, air conditioning, sockets, and open saloons.",
  },
  {
    name: "ED161 Dart",
    image: ed161Image,
    interiors: [ed161FirstInterior, ed161SecondInterior],
    text:
      "A long-distance EMU built for comfort on national routes, bringing a high-standard IC experience with first and second class seating.",
  },
  {
    name: "ED74",
    image: ed74Image,
    interiors: [ed74FirstInterior],
    text:
      "A refreshed EMU set used where IC needs modern electric service with a smaller, flexible formation and comfortable passenger areas.",
  },
];

const locomotiveCards = [
  {
    name: "EU160 Griffin",
    image: eu160Image,
    text:
      "The flagship modern IC locomotive, designed for strong 160 km/h electric intercity work and regular high-quality national services.",
  },
  {
    name: "EP09",
    image: ep09Image,
    text:
      "A classic express locomotive that remains closely associated with fast Polish intercity trains and locomotive-hauled passenger consists.",
  },
  {
    name: "EP08",
    image: ep08Image,
    text:
      "A classic express locomotive that bridges older IC traction with faster long-distance passenger services.",
  },
  {
    name: "EP07",
    image: ep07Image,
    text:
      "A familiar electric locomotive for conventional IC consists, especially where the service uses classic passenger coaches.",
  },
  {
    name: "EU07",
    image: eu07Image,
    text:
      "A long-serving universal electric locomotive that still appears on selected locomotive-hauled IC services.",
  },
];

const dieselLocomotiveCards = [
  {
    name: "SU160 Gama",
    image: su160Image,
    text:
      "A modern diesel locomotive for passenger routes where IC needs locomotive-hauled service beyond the electrified network.",
  },
  {
    name: "SU4210",
    image: su4210Image,
    text:
      "A diesel locomotive for shorter or regional IC sections, useful where classic coaches continue onto unelectrified routes.",
  },
];

const icFacts = [
  "IC services can be operated by modern EMUs, locomotive-hauled carriages, or selected diesel units, so the category adapts to both main-line and regional intercity routes.",
  "Passengers can expect reserved seats where a carriage plan is available, with first and second class options, discounts, add-on tickets, and ticket documents after purchase.",
  "The category links major cities, regional centres, and smaller communities, including places where a flexible consist or diesel unit is better suited than a premium train.",
  "Modern and modernised stock brings practical comfort: air conditioning, sockets, accessible spaces, luggage areas, and bicycle-friendly coaches where available.",
];

function TrainIcPage() {
  return (
    <main className="train-detail-page">
      <article className="train-detail-content eip-longform ic-longform">
        <nav className="breadcrumb-nav trains-breadcrumb" aria-label="Breadcrumb">
          <Link to="/">Home</Link>
          <span>-&gt;</span>
          <Link to="/#passenger-info">For Passengers</Link>
          <span>-&gt;</span>
          <Link to="/trains">Our trains</Link>
          <span>-&gt;</span>
          <span>InterCity (IC)</span>
        </nav>

        <p className="eyebrow">InterCity (IC)</p>
        <h1>InterCity (IC)</h1>

        <p className="train-detail-lead eic-lead">
          InterCity is the broadest passenger train category. It covers modern EMU sets, locomotive-hauled trains with
          varied coach formations, and selected diesel services for rural or unelectrified routes.
        </p>

        <section className="eip-fact-grid" aria-label="IC category highlights">
          {icFacts.map((fact) => (
            <article key={fact}>
              <span aria-hidden="true">IC</span>
              <p>{fact}</p>
            </article>
          ))}
        </section>

        <section className="ic-fleet-section">
          <div className="ic-section-heading">
            <p className="eyebrow">Modern EMU sets</p>
            <h2>ED160, ED161 and ED74</h2>
            <p>
              On many IC routes, passengers meet fixed electric trainsets first: bright saloons, accessible boarding
              areas, first and second class seating, air conditioning, and a predictable layout from coach to coach.
            </p>
          </div>

          <div className="ic-emu-grid">
            {emuSets.map((set) => (
              <article className="ic-emu-card" key={set.name}>
                <img src={set.image} alt={`${set.name} train`} />
                <div className="ic-emu-card-body">
                  <h3>{set.name}</h3>
                  <p>{set.text}</p>
                </div>
                <div className="ic-interior-strip">
                  {set.interiors.map((interior, index) => (
                    <img key={interior} src={interior} alt={`${set.name} ${index === 0 ? "first" : "second"} class interior`} />
                  ))}
                </div>
              </article>
            ))}
          </div>
        </section>

        <section className="ic-fleet-section">
          <div className="ic-section-heading">
            <p className="eyebrow">Locomotive-hauled IC</p>
            <h2>Classic consists with modern traction</h2>
            <p>
              IC is also a large locomotive-hauled category. These trains can be built from compartment and
              non-compartment coaches, upgraded cars, family or bicycle-friendly carriages, and different locomotive
              types depending on route and availability.
            </p>
          </div>

          <div className="eic-locomotive-grid ic-locomotive-grid">
            {locomotiveCards.map((locomotive) => (
              <article key={locomotive.name} className="eic-locomotive-card">
                <img src={locomotive.image} alt={`${locomotive.name} locomotive`} />
                <div>
                  <h2>{locomotive.name}</h2>
                  <p>{locomotive.text}</p>
                </div>
              </article>
            ))}
          </div>

          <section className="eip-gallery-section" aria-label="Locomotive-hauled IC train gallery">
            <img src={icTrainImage} alt="IC locomotive-hauled train at a platform" />
            <img src={icTrainTwoImage} alt="Modernised IC passenger coaches" />
          </section>

          <section className="ic-carriage-gallery" aria-label="IC carriage and interior gallery">
            <figure>
              <img src={icFirstClassImage} alt="IC first class carriage" />
              <figcaption>First class coach</figcaption>
            </figure>
            <figure>
              <img src={icFirstClassInterior} alt="IC first class interior" />
              <figcaption>First class interior</figcaption>
            </figure>
            <figure>
              <img src={icSecondClassImage} alt="IC second class carriage" />
              <figcaption>Second class coach</figcaption>
            </figure>
            <figure>
              <img src={icSecondClassInterior} alt="IC second class interior" />
              <figcaption>Second class interior</figcaption>
            </figure>
          </section>
        </section>

        <section className="ic-fleet-section">
          <div className="ic-section-heading">
            <p className="eyebrow">Rural diesel service</p>
            <h2>Diesel locomotives and SD85 DMU</h2>
            <p>
              Some IC routes need to reach places where electric locomotive service is not practical. For those rural
              or unelectrified sections, diesel locomotives can haul passenger coaches, while the SKPL SD85 DMU covers
              routes that are better suited to a self-contained diesel unit.
            </p>
          </div>

          <div className="eic-locomotive-grid ic-locomotive-grid">
            {dieselLocomotiveCards.map((locomotive) => (
              <article key={locomotive.name} className="eic-locomotive-card">
                <img src={locomotive.image} alt={`${locomotive.name} diesel locomotive`} />
                <div>
                  <h2>{locomotive.name}</h2>
                  <p>{locomotive.text}</p>
                </div>
              </article>
            ))}
          </div>

          <article className="ic-dmu-card">
            <img src={sd85Image} alt="SD85 diesel multiple unit train" />
            <div>
              <h3>SKPL SD85</h3>
              <p>
                A diesel multiple unit for non-electrified intercity links. It keeps the IC promise of a bookable
                long-distance service while serving routes where electric EMUs and locomotive-hauled trains cannot
                operate directly.
              </p>
            </div>
          </article>

          <section className="eip-gallery-section" aria-label="SD85 interior gallery">
            <img src={sd85SecondInterior} alt="SD85 second class interior" />
            <img src={sn84Image} alt="Diesel multiple unit on a regional intercity route" />
          </section>
        </section>

        <section className="eip-info-section">
          <h2>Services and range of connections</h2>
          <p>
            IC services cover domestic trunk routes, regional intercity links, and selected international or border
            connections. The passenger experience depends on the assigned stock, but the essentials stay familiar:
            choose a connection, select class and seats where available, add dog or baggage tickets if needed, apply
            discounts, pay, and receive ticket documents.
          </p>
          <p>
            The category is intentionally flexible. A passenger may see a polished ED160 on one route, an EU160-hauled
            carriage set on another, and an SD85 diesel unit on a rural route, all under the InterCity brand.
          </p>
        </section>
      </article>
    </main>
  );
}

export default TrainIcPage;
