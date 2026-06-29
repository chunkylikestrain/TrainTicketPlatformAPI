import { Link } from "react-router-dom";

type FaqItem = {
  question: string;
  answer: string;
};

type FaqSection = {
  title: string;
  items: FaqItem[];
};

const faqSections: FaqSection[] = [
  {
    title: "Buying a ticket",
    items: [
      {
        question: "Do I need an account to buy a ticket?",
        answer:
          "No. You can continue as a guest by entering passenger details, an email address, and the required consents at checkout. If you are logged in, the ticket is also saved to My tickets so you can download it again later.",
      },
      {
        question: "How do I search for a journey?",
        answer:
          "Enter the origin, destination, date, and departure time on the home page. You can choose one way or round trip, adjust the number of travelers, pick discounts, and set filters before searching.",
      },
      {
        question: "Can I book a journey with transfers?",
        answer:
          "Yes. RailBook shows direct journeys and non-direct itineraries with up to two transfers across three trains. Each itinerary keeps its own segment details through class selection, seat selection, checkout, and the final ticket.",
      },
      {
        question: "Can I book a return journey?",
        answer:
          "Yes. Select round trip on the search form, enter the return date and time, and choose both outbound and return connections. The order keeps both directions together during payment and ticket viewing.",
      },
    ],
  },
  {
    title: "Seats and passengers",
    items: [
      {
        question: "Can I choose my own seat?",
        answer:
          "Yes. The seat map shows available seats in green, selected seats in orange, and unavailable seats in grey. Seats from another class remain visible in the train layout but cannot be selected for the chosen class.",
      },
      {
        question: "How does seat availability work on transfer or partial-route trips?",
        answer:
          "Availability is checked for the exact trip date and route segment. The same physical seat can be sold again only when the booked route sections do not overlap.",
      },
      {
        question: "Can I buy tickets for several passengers at once?",
        answer:
          "Yes. Add adults or children in the traveler box, choose a discount for each passenger, select a seat for each passenger where required, and pay for the whole order with one payment.",
      },
      {
        question: "Can I add a dog or large baggage ticket?",
        answer:
          "Yes. Dog transport and large baggage can be added during checkout. RailBook adds the fee to the total and stores the add-on on the booking and ticket PDF so it can be checked during travel.",
      },
    ],
  },
  {
    title: "Discounts and prices",
    items: [
      {
        question: "Which discounts can I choose?",
        answer:
          "RailBook supports normal tickets plus student, child, senior, Big Family, and Family Ticket style offers where they are available in the app. Some offers depend on age, passenger count, or supporting documents.",
      },
      {
        question: "Do I need to prove my discount?",
        answer:
          "The app lets you select the discount, but eligibility documents are checked outside RailBook during travel. The discount on the ticket should match the document carried by the passenger.",
      },
      {
        question: "Why can the final price differ from the first result I saw?",
        answer:
          "The final checkout summary includes the selected class, passengers, discounts, add-ons, and any loyalty point redemption. Always review the summary before payment.",
      },
      {
        question: "How do My IC points work?",
        answer:
          "Logged-in passengers earn points after eligible ticket purchases. During payment, points can be redeemed against the order total at the rate shown in the checkout flow.",
      },
    ],
  },
  {
    title: "Payment and ticket documents",
    items: [
      {
        question: "What happens after payment?",
        answer:
          "After payment is confirmed, RailBook creates the ticket number, QR code, ticket PDF, and email delivery status. Logged-in users can also find the ticket later in My tickets.",
      },
      {
        question: "Where can I download my ticket PDF?",
        answer:
          "Use the Download PDF button on the success page or open My tickets and download the ticket again from the active ticket card.",
      },
      {
        question: "Can I show the ticket on my phone?",
        answer:
          "Yes. The QR code and ticket PDF are designed to be available digitally. Make sure the screen is readable and the ticket details match the passenger and journey.",
      },
      {
        question: "What if the PDF or email is not available immediately?",
        answer:
          "Open My tickets and try the download again. If the issue is still there, contact support with the booking reference, ticket number, payment status, and email address used for checkout.",
      },
    ],
  },
  {
    title: "Refunds, invoices, and account history",
    items: [
      {
        question: "How do I refund a ticket?",
        answer:
          "Open My tickets, find an active confirmed ticket, and use the Refund action. Returned tickets move to the Returned tab and remain visible as account history.",
      },
      {
        question: "Where do old trips go?",
        answer:
          "Tickets whose arrival time has already passed are shown in Travel history. Active tickets stay in the Tickets tab until the journey is complete or returned.",
      },
      {
        question: "Can I generate an invoice?",
        answer:
          "Yes. Choose the invoice option during checkout or from the ticket where invoice generation is available. Generated invoices are shown in My invoices and can be downloaded later.",
      },
      {
        question: "Can I change passenger data after buying a ticket?",
        answer:
          "Account details can be changed in My data, but already issued tickets keep the passenger data from the time of purchase unless a ticket-specific change option is available.",
      },
    ],
  },
  {
    title: "Disruptions and current trip",
    items: [
      {
        question: "Where can I see delay or platform changes?",
        answer:
          "Operational messages are shown on affected active tickets and trip details when the schedule has a delay, cancellation, platform change, or passenger-facing disruption note.",
      },
      {
        question: "What is the current trip page?",
        answer:
          "The current trip page puts the QR code first, then the route, train details, and upcoming stops. Passed stops disappear from the calling pattern as the journey progresses.",
      },
      {
        question: "Can a train arrive early?",
        answer:
          "RailBook can show an early or delayed arrival where schedule data supports it. Departure is not shown earlier than planned, because a passenger-facing timetable should not ask people to leave before the published time.",
      },
    ],
  },
  {
    title: "Services not covered",
    items: [
      {
        question: "Does RailBook sell tickets through ticket offices or a mobile app?",
        answer:
          "No. RailBook focuses on the web booking flow. Ticket office, call-centre, and separate mobile-app processes are outside this application.",
      },
      {
        question: "Does RailBook manage external railway cards or paper passes?",
        answer:
          "No. The app can apply supported discount choices during checkout, but external card issuing, paper pass management, and onboard document verification are outside the system.",
      },
      {
        question: "Can I order restaurant-car food through RailBook?",
        answer:
          "No. The meal information page explains the onboard dining idea, but food ordering is not part of the RailBook checkout flow.",
      },
    ],
  },
];

function FaqPage() {
  return (
    <main className="faq-page">
      <section className="faq-shell">
        <nav className="content-breadcrumb" aria-label="Breadcrumb">
          <Link to="/">Home</Link>
          <span aria-hidden="true">&gt;</span>
          <Link to="/help">Help</Link>
          <span aria-hidden="true">&gt;</span>
          <span>FAQ</span>
        </nav>

        <p className="eyebrow">Customer service</p>
        <h1>Frequently Asked Questions</h1>
        <p className="faq-intro">
          Quick answers for buying tickets online, choosing seats and discounts, downloading ticket PDFs,
          handling refunds, and finding account documents later.
        </p>

        <section className="faq-index" aria-label="FAQ sections">
          {faqSections.map((section) => (
            <a href={`#${slugify(section.title)}`} key={section.title}>
              {section.title}
            </a>
          ))}
        </section>

        <div className="faq-content">
          {faqSections.map((section) => (
            <section className="faq-section" id={slugify(section.title)} key={section.title}>
              <h2>{section.title}</h2>
              {section.items.map((item) => (
                <details className="faq-item" key={item.question}>
                  <summary>{item.question}</summary>
                  <p>{item.answer}</p>
                </details>
              ))}
            </section>
          ))}
        </div>

        <section className="faq-followup">
          <h2>Still need help?</h2>
          <p>
            Contact support with your booking reference, ticket number, payment status, and the email
            address used during checkout.
          </p>
          <Link to="/contact">Open contact details</Link>
        </section>
      </section>
    </main>
  );
}

function slugify(value: string) {
  return value.toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/(^-|-$)/g, "");
}

export default FaqPage;
