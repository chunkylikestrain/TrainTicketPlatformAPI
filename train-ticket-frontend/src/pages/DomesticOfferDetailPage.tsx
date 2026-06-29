import { Link, Navigate, useParams } from "react-router-dom";
import TrainSearchForm from "../components/TrainSearchForm";

type DomesticOfferSection = {
  title: string;
  icon: string;
  body?: string[];
  bullets?: string[];
};

type DomesticOfferDetail = {
  title: string;
  lead: string;
  sections: DomesticOfferSection[];
};

const domesticOfferDetails: Record<string, DomesticOfferDetail> = {
  "big-family": {
    title: "Big Family Offer",
    lead:
      "Travelling with the entire family does not have to cost too much. Big-family style discounts help larger households plan domestic journeys at a friendlier price.",
    sections: [
      {
        title: "At RailBook, the Big Family Card means",
        icon: "30%",
        body: [
          "A 30% discount can apply to a joint domestic trip for at least two passengers holding the relevant family entitlement.",
          "The offer is intended for family travel on domestic trains and can be combined with statutory concessions where the passenger is entitled to them.",
        ],
      },
      {
        title: "Good to know",
        icon: "i",
        bullets: [
          "Children under 4 may travel free in second class when the proper free ticket is collected for them.",
          "Family-friendly trains may include compartments for carers with children or baby changing tables in toilets.",
          "Passengers should carry documents confirming entitlement, because staff may check them during travel.",
          "Choose the matching discount setup before checkout so the final price and ticket artifact are clear.",
        ],
      },
      {
        title: "What is a Big Family Card?",
        icon: "?",
        body: [
          "It is an eligibility document for families with at least three children. In RailBook, treat it like any other discount entitlement: select the right discount and keep the document with you for the journey.",
        ],
      },
    ],
  },
  senior: {
    title: "Senior's Ticket",
    lead: "Passengers over 60 can travel for less when they choose the correct senior entitlement before checkout.",
    sections: [
      {
        title: "Senior's Ticket",
        icon: "30%",
        body: [
          "A senior discount can apply to domestic journeys in first or second class, depending on the selected fare and train category.",
          "To confirm eligibility, the passenger should carry a photo document showing their identity and age.",
        ],
      },
      {
        title: "Statutory 37% discount",
        icon: "37%",
        body: [
          "Some retired passengers or pensioners may be entitled to a statutory 37% discount for a limited number of trips per year.",
          "Eligibility depends on the correct supporting document. The selected discount on the ticket should match the document carried during travel.",
        ],
      },
      {
        title: "Before you travel",
        icon: "ID",
        bullets: [
          "Select the senior or pensioner discount before checkout.",
          "Check that every passenger has their own matching document.",
          "If the entitlement cannot be verified during travel, the passenger may need to pay the difference or an additional charge.",
        ],
      },
    ],
  },
  "family-ticket": {
    title: "Family Ticket",
    lead:
      "A special offer for small groups travelling with a child. It is designed for domestic trips where adults and children travel together.",
    sections: [
      {
        title: "Offer details",
        icon: "%",
        body: [
          "The Family Ticket is intended for groups of 2 to 5 people travelling together, including at least one child under 16 years of age.",
          "Eligible groups can receive a 30% discount on the applicable domestic fare.",
        ],
      },
      {
        title: "How to use the Family Ticket",
        icon: "T",
        bullets: [
          "Set the number of adults and children before searching.",
          "Choose the correct child and family-related discount during discount selection.",
          "Carry a document confirming the child's age during the journey.",
          "If part of the group leaves earlier, the offer remains valid only while the remaining group still satisfies the offer conditions.",
        ],
      },
      {
        title: "Good to know",
        icon: "i",
        body: [
          "The offer is for domestic travel in regular seating classes. Sleeping cars and couchettes may require separate supplements when those services are available.",
        ],
      },
    ],
  },
  bicycles: {
    title: "Transport of bicycles",
    lead:
      "Taking a bicycle by train is possible on selected services. Always check train facilities and bike-space availability before completing the journey.",
    sections: [
      {
        title: "Is bicycle transport paid?",
        icon: "BI",
        body: [
          "Bicycle transport may require a separate ticket or designated reservation depending on train category and service rules.",
          "RailBook shows bicycle-friendly facilities through filters and carriage information where the train data includes them.",
        ],
      },
      {
        title: "Where to place your bicycle",
        icon: "↕",
        bullets: [
          "Use dedicated bicycle spaces where they are available.",
          "On some trains, a bicycle may be allowed in a vestibule only when the service permits it.",
          "Do not place bicycles in restaurant cars, sleeping cars, couchettes, aisles, or any area not designed for them.",
        ],
      },
      {
        title: "If you did not arrange bicycle transport",
        icon: "!",
        body: [
          "Inform staff as early as possible. If the train has no available bicycle space or the rules do not permit onboard sale, you may be asked to travel without the bicycle or pay an additional charge.",
        ],
      },
    ],
  },
  pets: {
    title: "Transporting dogs, cats and other pets",
    lead:
      "Pets can travel with you when they are transported safely and do not disturb other passengers. Dogs outside a carrier require an additional dog transport ticket.",
    sections: [
      {
        title: "Fees due",
        icon: "PLN",
        body: [
          "Small pets travelling in a secure carrier can usually travel without an extra ticket, provided they do not cause nuisance to other passengers.",
          "A dog travelling outside a carrier requires a dog transport ticket. RailBook applies a 15 PLN dog add-on at checkout and attaches it to the booking.",
        ],
      },
      {
        title: "Rules during travel",
        icon: "P",
        bullets: [
          "Keep the dog on a leash and muzzled for the whole trip.",
          "Carry a valid rabies vaccination certificate for the dog.",
          "Use a suitable carrier for smaller animals and keep it in a safe hand-luggage area.",
          "If other passengers object to an animal in a compartment, you may need to move to another suitable seat.",
        ],
      },
      {
        title: "Where animals are not allowed",
        icon: "X",
        bullets: [
          "Restaurant cars, except for guide dogs and assistance dogs.",
          "Seats, beds in sleeping cars, and berths in couchette cars.",
          "Any space where the animal blocks movement or creates a safety risk.",
        ],
      },
    ],
  },
  items: {
    title: "Transport of items",
    lead:
      "Large baggage, prams, pushchairs, and other carried items should fit safely into the available luggage space without blocking passengers or staff.",
    sections: [
      {
        title: "What counts as hand luggage",
        icon: "B",
        body: [
          "Items that fit on shelves, under the seat, or in other designated spaces can usually travel with no extra charge.",
          "Folded pushchairs, compact baby strollers, and easy-to-carry items are treated as hand luggage when they fit safely.",
        ],
      },
      {
        title: "When do I have to pay for luggage?",
        icon: "5",
        bullets: [
          "If an item is too large for the normal luggage areas, a carriage-of-goods ticket may be required.",
          "RailBook supports an extra large-baggage add-on at checkout for 5 PLN per large bag.",
          "Items must not block aisles, doors, emergency equipment, or other passengers' seats.",
        ],
      },
      {
        title: "If you do not have the correct ticket",
        icon: "!",
        body: [
          "You may be able to arrange the extra ticket with staff, but onboard charges can apply. It is better to add large baggage before payment so the booking and ticket artifact clearly show it.",
        ],
      },
    ],
  },
};

function DomesticOfferDetailPage() {
  const { offerId = "" } = useParams();
  const offer = domesticOfferDetails[offerId];

  if (!offer) {
    return <Navigate to="/offers/domestic" replace />;
  }

  return (
    <main className="offer-page domestic-offer-detail-page">
      <section className="offer-layout domestic-offer-detail-layout">
        <article className="offer-content">
          <nav className="breadcrumb-nav" aria-label="Breadcrumb">
            <Link to="/">Home</Link>
            <span>-&gt;</span>
            <Link to="/offers">Offers</Link>
            <span>-&gt;</span>
            <Link to="/offers/domestic">Domestic</Link>
            <span>-&gt;</span>
            <span>{offer.title}</span>
          </nav>

          <p className="eyebrow">Domestic offer</p>
          <h1>{offer.title}</h1>
          <p className="offer-lead">{offer.lead}</p>

          <div className="domestic-offer-sections">
            {offer.sections.map((section) => (
              <section className="domestic-offer-section" key={section.title}>
                <span className="domestic-offer-icon" aria-hidden="true">
                  {section.icon}
                </span>
                <div>
                  <h2>{section.title}</h2>
                  {section.body?.map((paragraph) => (
                    <p key={paragraph}>{paragraph}</p>
                  ))}
                  {section.bullets && (
                    <ul>
                      {section.bullets.map((bullet) => (
                        <li key={bullet}>{bullet}</li>
                      ))}
                    </ul>
                  )}
                </div>
              </section>
            ))}
          </div>
        </article>

        <aside className="offer-search-card" aria-label={`Search tickets for ${offer.title}`}>
          <h2>Search a connection</h2>
          <TrainSearchForm compact />
        </aside>
      </section>
    </main>
  );
}

export default DomesticOfferDetailPage;
