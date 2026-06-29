import { BankOutlined, ClockCircleOutlined, IdcardOutlined, SmileOutlined } from "@ant-design/icons";
import { Link } from "react-router-dom";
import TrainSearchForm from "../components/TrainSearchForm";

const benefits = [
  {
    title: "Savings",
    icon: <BankOutlined />,
    text: "With a valid student document, students can use a 51% statutory discount on national RailBook journeys.",
  },
  {
    title: "Speed",
    icon: <ClockCircleOutlined />,
    text: "Fast intercity trains help students move between university cities, home, and weekend plans without wasting half the day.",
  },
  {
    title: "Comfort",
    icon: <SmileOutlined />,
    text: "Choose your seat, travel with air conditioning, and bring your study notes, laptop, snacks, and questionable deadlines with you.",
  },
];

function StudentOfferPage() {
  return (
    <main className="offer-page">
      <section className="offer-ribbon" aria-hidden="true">
        <span />
        <strong>Travel to <b>MY</b></strong>
        <span />
      </section>

      <section className="offer-layout">
        <article className="offer-content">
          <nav className="breadcrumb-nav" aria-label="Breadcrumb">
            <Link to="/">Home</Link>
            <span>-&gt;</span>
            <Link to="/#passenger-info">For Passengers</Link>
            <span>-&gt;</span>
            <Link to="/#offers">Offers</Link>
            <span>-&gt;</span>
            <span>Student offer</span>
          </nav>

          <p className="eyebrow">Domestic offer</p>
          <h1>Student offer</h1>

          <p className="offer-lead">Students, get to know RailBook trains.</p>
          <p className="offer-kicker"><strong>Buy your ticket right now!</strong> Student travel is full of benefits:</p>

          <div className="student-benefit-list">
            {benefits.map((benefit) => (
              <section className="student-benefit" key={benefit.title}>
                <span className="student-benefit-icon" aria-hidden="true">{benefit.icon}</span>
                <div>
                  <h2>{benefit.title}</h2>
                  <p>{benefit.text}</p>
                </div>
              </section>
            ))}
          </div>

          <section className="student-discount-panel">
            <div className="student-phone-card" aria-hidden="true">
              <div className="phone-speaker" />
              <div className="phone-screen">
                <span>Buy ticket</span>
                <span>From</span>
                <span>To</span>
                <span>Date</span>
                <button type="button" tabIndex={-1}>Search</button>
              </div>
            </div>

            <div>
              <p>We honour the student discount in national train categories available in RailBook.</p>
              <h2>The 51% discount applies to:</h2>
              <ol>
                <li>Students up to the age of 26 with a valid student card issued by a Polish higher education institution.</li>
                <li>Polish citizens studying abroad up to the age of 26 with an accepted student document.</li>
                <li>Students of teacher training colleges up to the age of 26.</li>
                <li>Doctoral students up to the age of 35 with a valid doctoral student card.</li>
              </ol>
              <p className="student-document-note">
                <IdcardOutlined /> The discount is selected during checkout, but the passenger must carry the proper document during travel.
              </p>
            </div>
          </section>
        </article>

        <aside className="offer-search-card" aria-label="Search student tickets">
          <h2>Find a student ticket</h2>
          <TrainSearchForm compact />
        </aside>
      </section>
    </main>
  );
}

export default StudentOfferPage;
