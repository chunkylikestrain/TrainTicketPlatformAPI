import { Link } from "react-router-dom";
import warsDiningCar from "../../../docs/MyTrainImages/WARSdiningcar.jpg";

function MealOfferPage() {
  return (
    <main className="offer-page meal-offer-page">
      <section className="offer-layout meal-offer-layout">
        <article className="offer-content meal-offer-content">
          <nav className="breadcrumb-nav" aria-label="Breadcrumb">
            <Link to="/">Home</Link>
            <span>-&gt;</span>
            <Link to="/#passenger-info">For Passengers</Link>
            <span>-&gt;</span>
            <Link to="/offers">Offers</Link>
            <span>-&gt;</span>
            <span>Meal while travelling</span>
          </nav>

          <p className="eyebrow">Offers</p>
          <h1>Meal while travelling</h1>

          <p className="offer-lead">
            A delicious meal while travelling by train? Visit the WARS dining car on selected services and make the
            journey a little more comfortable.
          </p>

          <section className="meal-offer-section">
            <h2>Travelling full of taste</h2>
            <p>
              Restaurant cars are available on selected premium and intercity trains. Other services may offer minibar
              trolley service with hot and cold drinks, snacks, and light refreshments.
            </p>
            <p>
              The menu varies depending on train category and availability. RailBook keeps this page as a local
              summary so passengers can review catering information without leaving the site.
            </p>
          </section>

          <div className="wars-pattern" aria-hidden="true" />

          <section className="meal-offer-section">
            <h2>Everybody will find something for themselves</h2>
            <p>
              Passengers can usually find coffee, tea, cold drinks, sweet snacks, simple meals, and seasonal menu items.
              Some trains support seated restaurant-car dining, while others offer service from a minibar trolley.
            </p>
          </section>

          <img
            className="meal-offer-image"
            src={warsDiningCar}
            alt="WARS dining car interior on a train"
          />

          <section className="meal-offer-section">
            <h2>Offer for the youngest generation</h2>
            <p>
              Children can choose smaller, balanced meal options where the service is available. It is a useful option
              for families on longer daytime journeys.
            </p>
          </section>

          <section className="meal-offer-section">
            <h2>Additional services</h2>
            <p>
              For passenger comfort, large baggage and pets should not be taken into the restaurant car, except for
              guide dogs and assistance dogs. Alcohol purchased on the train should be consumed only where permitted by
              train service rules.
            </p>
            <p>
              Card payment is generally expected in restaurant cars. For minibar trolley service, passengers should be
              ready for more limited payment options depending on the train.
            </p>
          </section>
        </article>
      </section>
    </main>
  );
}

export default MealOfferPage;
