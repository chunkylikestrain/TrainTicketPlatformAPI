import { Link } from "react-router-dom";

type BookingExpiredModalProps = {
  isOpen: boolean;
};

function BookingExpiredModal({ isOpen }: BookingExpiredModalProps) {
  if (!isOpen) {
    return null;
  }

  return (
    <div className="expired-modal-backdrop" role="presentation">
      <section className="expired-modal" role="alertdialog" aria-modal="true" aria-labelledby="expired-title">
        <Link to="/search" className="expired-modal-close" aria-label="Close and go back to search">
          x
        </Link>
        <h2 id="expired-title">Confirmation</h2>
        <p>Time to buy a ticket has expired.</p>
        <Link to="/search" className="expired-modal-action">
          Go back to the search engine
        </Link>
      </section>
    </div>
  );
}

export default BookingExpiredModal;
