import { Link } from "react-router-dom";

const offerCategories = [
  {
    id: "sleeper",
    title: "Sleeper cars and couchettes",
    intro: "Night journeys with places to rest on longer routes.",
    details: "Plan overnight travel with couchette and sleeper-style options where they are available in the timetable.",
    imageClass: "offer-card-image-sleeper",
    to: "/offers/sleeper",
  },
  {
    id: "domestic",
    title: "Domestic",
    intro: "National offers for everyday intercity travel.",
    details: "Find standard fares, statutory discounts, student travel, loyalty points, and add-ons for routes inside Poland.",
    imageClass: "offer-card-image-domestic",
    to: "/offers/domestic",
  },
  {
    id: "explore",
    title: "Explore Poland with RailBook",
    intro: "Travel ideas for city breaks and longer trips.",
    details: "Search connections between major cities, regional centres, and popular long-distance railway corridors.",
    imageClass: "offer-card-image-explore",
    to: "/offers/explore",
  },
  {
    id: "meal",
    title: "Meal while travelling",
    intro: "A warm drink or meal can make a long journey feel much better.",
    details: "Learn about WARS dining cars, minibar service, snacks, drinks, and restaurant-car etiquette.",
    imageClass: "offer-card-image-meal",
    to: "/offers/meal",
  },
];

function OffersPage() {
  return (
    <main className="trains-page offers-list-page">
      <section className="trains-content">
        <nav className="breadcrumb-nav trains-breadcrumb" aria-label="Breadcrumb">
          <Link to="/">Home</Link>
          <span>-&gt;</span>
          <Link to="/#passenger-info">For Passengers</Link>
          <span>-&gt;</span>
          <span>Offers</span>
        </nav>

        <p className="eyebrow">For passengers</p>
        <h1>Offers</h1>

        <section className="train-category-grid offer-category-grid" aria-label="RailBook offer categories">
          {offerCategories.map((offer) => (
            <article className="train-category-card offer-category-card" id={offer.id} key={offer.id}>
              <div className={`train-category-image offer-category-image ${offer.imageClass}`} aria-hidden="true" />
              <div className="train-category-body">
                <h2>{offer.title}</h2>
                <p>{offer.intro}</p>
                <p>{offer.details}</p>
                <Link className="train-more-link" to={offer.to}>
                  More
                </Link>
              </div>
            </article>
          ))}
        </section>
      </section>
    </main>
  );
}

export default OffersPage;
