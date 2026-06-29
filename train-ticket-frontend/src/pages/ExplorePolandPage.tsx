import { Link } from "react-router-dom";
import gdanskImage from "../../../docs/MyCityImages/Gdansk,Sopot,Gdynia.jpg";
import katowiceImage from "../../../docs/NotMyCityImages/Katowice(Wikicommons).jpg";
import krakowImage from "../../../docs/MyCityImages/Krakow.jpg";
import opoleImage from "../../../docs/NotMyCityImages/Opole(Wikicommons).jpg";
import poznanImage from "../../../docs/NotMyCityImages/Poznan(Wikicommons).jpg";
import przemyslImage from "../../../docs/NotMyCityImages/Przemyśl(Wikicommons).jpg";
import rzeszowImage from "../../../docs/MyCityImages/Rzeszow.jpg";
import szczecinImage from "../../../docs/NotMyCityImages/Szczecin(Wikicommons).jpg";
import tarnowImage from "../../../docs/NotMyCityImages/Tarnow(Wikicommons).jpg";
import warsawImage from "../../../docs/NotMyCityImages/Warsaw(Wikicommons).jpg";
import wroclawImage from "../../../docs/MyCityImages/Wroclaw.jpg";

const destinations = [
  {
    id: "przemysl",
    name: "Przemyśl",
    image: przemyslImage,
    lead: "For years, Przemyśl enjoyed the title of royal city. Here, you can see history on every corner.",
    body:
      "Przemyśl is one of the oldest towns in Poland and a brilliant gateway to Podkarpackie. The old town, Kazimierzowski Castle, and the Przemyśl Archcathedral reward a slow walk, while the surrounding hills make the city a strong starting point for a longer rail trip through south-eastern Poland.",
  },
  {
    id: "krakow",
    name: "Kraków",
    image: krakowImage,
    lead: "Kraków is one of Europe’s best-known city-break destinations.",
    body:
      "The former capital of Poland gathers the Wawel Hill, St. Mary’s Basilica, the Cloth Hall, and the Main Market Square into one walkable historic centre. Kazimierz adds a distinct evening atmosphere, while the city’s railway links make it an easy anchor for journeys across southern Poland.",
  },
  {
    id: "tarnow",
    name: "Tarnów",
    image: tarnowImage,
    lead: "A stop in Tarnów can turn into a surprisingly rich city break.",
    body:
      "Tarnów is often called a gem of the Polish Renaissance. Its old town has a calm rhythm, colourful tenements, a stylish town hall, and the Gothic Cathedral Basilica. It is a compact destination where a short rail stop can still feel full of atmosphere.",
  },
  {
    id: "katowice",
    name: "Katowice",
    image: katowiceImage,
    lead: "Katowice is not only mines. The heart of Upper Silesia has a lot to offer.",
    body:
      "Katowice mixes industrial heritage with modern culture. The Silesian Museum, Nikiszowiec, Spodek, and the Polish National Radio Symphony Orchestra show very different sides of the city. It is a strong choice for passengers who want architecture, music, and regional history in one trip.",
  },
  {
    id: "warsaw",
    name: "Warsaw",
    image: warsawImage,
    lead: "A capital full of skyscrapers, reconstructed old streets, museums, and riverside walks.",
    body:
      "Warsaw brings together the Old Town, Royal Castle, Łazienki Park, the Vistula boulevards, and a skyline that feels different from any other Polish city. The museums alone can fill a weekend, and the rail network makes Warsaw one of the easiest hubs for onward travel.",
  },
  {
    id: "opole",
    name: "Opole",
    image: opoleImage,
    lead: "Opole is called the capital of Polish song, but music is only the beginning.",
    body:
      "The city is known for its festival tradition, riverside views, cathedral, and atmospheric old town around the Opole Venice. It is a smaller destination with a relaxed centre, good evening light, and enough character to justify more than a quick transfer.",
  },
  {
    id: "gdansk-sopot-gdynia",
    name: "Gdańsk / Sopot / Gdynia",
    image: gdanskImage,
    lead: "The Tri-City combines history, seaside promenades, port life, and long Baltic beaches.",
    body:
      "Gdańsk carries major European history in its streets, from the Main Town to the shipyard area. Sopot offers the longest wooden pier on the Baltic and a classic seaside atmosphere, while Gdynia adds modernist architecture, museums, and an active harbour. Together they make one of Poland’s strongest rail destinations.",
  },
  {
    id: "szczecin",
    name: "Szczecin",
    image: szczecinImage,
    lead: "Szczecin is a city for sailing, green spaces, port views, and relaxed waterfront walks.",
    body:
      "The capital of Zachodniopomorskie has a wide, open feeling shaped by water and greenery. The Philharmonic, Chrobry Embankment, old port views, and nearby sailing areas make Szczecin a natural destination for passengers heading towards north-western Poland.",
  },
  {
    id: "poznan",
    name: "Poznań",
    image: poznanImage,
    lead: "In Poznań, all roads lead to the market square and its colourful tenements.",
    body:
      "Poznań is a lively mix of historic streets, the Renaissance town hall, Ostrów Tumski, parks, shopping, and food. The city’s famous goats and St. Martin’s croissants give it a memorable local flavour, while its central location makes it easy to reach by intercity trains.",
  },
  {
    id: "rzeszow",
    name: "Rzeszów",
    image: rzeszowImage,
    lead: "Rzeszów is one of Poland’s fastest-growing cities.",
    body:
      "Rzeszów’s market square, town hall, underground tourist route, 3 Maja street, Lubomirski Castle, and riverside areas make it an inviting base in south-eastern Poland. It is also a practical starting point for trips deeper into Podkarpackie.",
  },
  {
    id: "wroclaw",
    name: "Wrocław",
    image: wroclawImage,
    lead: "Which Polish city has the most bridges? Wrocław, of course.",
    body:
      "Wrocław is built around islands, bridges, riverside walks, and a beautiful market square. Ostrów Tumski, the cathedral, Panorama Racławicka, and the city’s famous small sculptures make it a destination that rewards wandering as much as planning.",
  },
];

function ExplorePolandPage() {
  return (
    <main className="explore-page">
      <section className="explore-shell">
        <nav className="breadcrumb-nav trains-breadcrumb" aria-label="Breadcrumb">
          <Link to="/">Home</Link>
          <span>-&gt;</span>
          <Link to="/#passenger-info">For Passengers</Link>
          <span>-&gt;</span>
          <Link to="/offers">Offers</Link>
          <span>-&gt;</span>
          <span>Explore Poland by rail</span>
        </nav>

        <p className="eyebrow">Offers</p>
        <h1>Explore Poland with RailBook</h1>
        <p className="explore-lead">
          Poland is full of city breaks, historic squares, seaside routes, music cities, and regional
          gateways. RailBook helps you turn those places into practical journeys by rail.
        </p>

        <section className="destination-list" aria-label="Destination details">
          {destinations.map((destination, index) => (
            <article
              className={`destination-section${index % 2 === 1 ? " destination-section-alt" : ""}`}
              id={destination.id}
              key={`${destination.id}-details`}
            >
              <div className="destination-copy">
                <p className="eyebrow">Destination</p>
                <h2>{destination.name}</h2>
                <p className="destination-lead">{destination.lead}</p>
                <h3>Why {destination.name}</h3>
                <p>{destination.body}</p>
                <Link to="/search">Search connections</Link>
              </div>
              <img src={destination.image} alt="" />
            </article>
          ))}
        </section>
      </section>
    </main>
  );
}

export default ExplorePolandPage;
