type PassengerLegalFooterProps = {
  className?: string;
};

function PassengerLegalFooter({ className = "summary-legal" }: PassengerLegalFooterProps) {
  return (
    <section className={className}>
      <div>
        <h2>Technological break.</h2>
        <p>
          Please remember about the technological break in the online sales system from 11:45pm - 0:30 am.
          You cannot buy any tickets during this break.
        </p>
        <a href="#accessibility">Declaration of Accessibility</a>
      </div>
      <div>
        <p>
          The prices presented are <strong>indicative</strong>, published for informational purposes, and do not
          constitute an offer. The final prices are available in the purchase summary.
        </p>
        <p>
          <strong>
            The Controller of personal data provided in connection with voluntary registration on this service
            has its registered office in Warsaw.
          </strong>{" "}
          v
        </p>
      </div>
    </section>
  );
}

export default PassengerLegalFooter;
