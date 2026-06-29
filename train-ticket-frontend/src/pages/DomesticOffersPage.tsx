import { Link } from "react-router-dom";

const domesticOffers = [
  {
    id: "student",
    title: "Student offer",
    intro: "Students get to know the Intercity trains.",
    details: "Use the student discount flow to apply the right reduced ticket before searching for a connection.",
    imageClass: "offer-card-image-student",
    to: "/offers/student",
  },
  {
    id: "big-family",
    title: "Big Family Offer",
    intro: "Travelling with the entire family does not have to cost too much.",
    details: "Information for families using Big Family Card style discounts on domestic journeys.",
    imageClass: "offer-card-image-big-family",
    to: "/offers/domestic/big-family",
  },
  {
    id: "senior",
    title: "Senior's Ticket",
    intro: "Show off your age and travel for less.",
    details: "Rules for senior passengers, age checks, and selecting the correct discount before checkout.",
    imageClass: "offer-card-image-senior",
    to: "/offers/domestic/senior",
  },
  {
    id: "family-ticket",
    title: "Family Ticket",
    intro: "A special offer for small groups travelling with a child.",
    details: "A practical guide for small family groups travelling with at least one child.",
    imageClass: "offer-card-image-family",
    to: "/offers/domestic/family-ticket",
  },
  {
    id: "bicycles",
    title: "Transport of bicycles",
    intro: "Take your bicycle with you on selected train services.",
    details: "Where bicycles can be placed, when spaces are limited, and what passengers should check.",
    imageClass: "offer-card-image-bicycle",
    to: "/offers/domestic/bicycles",
  },
  {
    id: "pets",
    title: "Transporting dogs, cats and other pets",
    intro: "Travel with your pet and add the correct transport ticket.",
    details: "RailBook supports dog add-on tickets during checkout so the ticket artifact shows the pet transport fee.",
    imageClass: "offer-card-image-pets",
    to: "/offers/domestic/pets",
  },
  {
    id: "items",
    title: "Transport of items",
    intro: "Large baggage, prams, pushchairs, and extra items for your trip.",
    details: "Add large baggage at checkout when your journey needs an extra baggage transport ticket.",
    imageClass: "offer-card-image-items",
    to: "/offers/domestic/items",
  },
];

function DomesticOffersPage() {
  return (
    <main className="trains-page offers-list-page">
      <section className="trains-content">
        <nav className="breadcrumb-nav trains-breadcrumb" aria-label="Breadcrumb">
          <Link to="/">Home</Link>
          <span>-&gt;</span>
          <Link to="/offers">Offers</Link>
          <span>-&gt;</span>
          <span>Domestic</span>
        </nav>

        <p className="eyebrow">Offers</p>
        <h1>Domestic</h1>

        <section className="train-category-grid offer-category-grid" aria-label="Domestic offer categories">
          {domesticOffers.map((offer) => (
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

export default DomesticOffersPage;
