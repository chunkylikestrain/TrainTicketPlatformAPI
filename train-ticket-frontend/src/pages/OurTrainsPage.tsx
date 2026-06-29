import { Link } from "react-router-dom";

const trainCategories = [
  {
    id: "eip",
    title: "Express InterCity Premium (EIP)",
    intro: "Express InterCity Premium brings the fastest RailBook journeys on the core intercity routes.",
    details: "Modern fixed trainsets, first and second class, dining space, quiet areas, accessible facilities, air conditioning, and comfortable open-space seating.",
    imageClass: "train-card-image-eip",
    to: "/trains/eip",
  },
  {
    id: "eic",
    title: "Express InterCity (EIC)",
    intro: "Comfortable express trains formed from higher-standard intercity rolling stock.",
    details: "A strong choice for long-distance travel with reserved seating, first and second class coaches, and selected services with restaurant or catering cars.",
    imageClass: "train-card-image-eic",
    to: "/trains/eic",
  },
  {
    id: "ic",
    title: "InterCity (IC)",
    intro: "The everyday long-distance train category for national routes across Poland.",
    details: "IC services connect major cities and regional centres with reserved seating, flexible rolling stock, and a mix of compartment and open-space coaches.",
    imageClass: "train-card-image-ic",
    to: "/trains/ic",
  },
  {
    id: "tlk",
    title: "Twoje Linie Kolejowe (TLK)",
    intro: "Twoje Linie Kolejowe is the practical intercity category for wider route coverage.",
    details: "TLK trains focus on accessible long-distance travel, classic locomotive-hauled coaches, seat reservations, and dependable national connections.",
    imageClass: "train-card-image-tlk",
    to: "/trains/tlk",
  },
];

function OurTrainsPage() {
  return (
    <main className="trains-page">
      <section className="trains-content">
        <nav className="breadcrumb-nav trains-breadcrumb" aria-label="Breadcrumb">
          <Link to="/">Home</Link>
          <span>-&gt;</span>
          <Link to="/#passenger-info">For Passengers</Link>
          <span>-&gt;</span>
          <span>Our trains</span>
        </nav>

        <p className="eyebrow">For passengers</p>
        <h1>Our trains</h1>

        <section className="train-category-grid" aria-label="RailBook train categories">
          {trainCategories.map((category) => (
            <article className="train-category-card" id={category.id} key={category.id}>
              <div className={`train-category-image ${category.imageClass}`} aria-hidden="true" />
              <div className="train-category-body">
                <h2>{category.title}</h2>
                <p>{category.intro}</p>
                <p>{category.details}</p>
                <Link className="train-more-link" to={category.to}>More</Link>
              </div>
            </article>
          ))}
        </section>
      </section>
    </main>
  );
}

export default OurTrainsPage;
