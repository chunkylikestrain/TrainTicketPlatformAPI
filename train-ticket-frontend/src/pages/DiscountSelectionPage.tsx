import { useMemo, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import {
  copyPurchasePreferenceParams,
  discountOptions,
  formatDiscountSummary,
  formatPassengerSummary,
  getDiscountCodes,
  getDiscountOption,
  getPassengerCounts,
  getPassengerTotal,
  type DiscountCode,
} from "../utils/purchasePreferences";

function DiscountSelectionPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const initialCounts = useMemo(() => getPassengerCounts(searchParams), [searchParams]);
  const [adults, setAdults] = useState(initialCounts.adults);
  const [children, setChildren] = useState(initialCounts.children);
  const [discounts, setDiscounts] = useState<DiscountCode[]>(() => getDiscountCodes(searchParams, initialCounts));
  const returnTo = searchParams.get("returnTo") || "/";
  const counts = { adults, children };
  const totalPassengers = getPassengerTotal(counts);
  const visibleDiscounts = discounts.slice(0, totalPassengers);
  const canConfirm = visibleDiscounts.length === totalPassengers;

  function updateAdults(nextAdults: number) {
    const normalizedAdults = Math.max(1, Math.min(6 - children, nextAdults));
    setAdults(normalizedAdults);
    resizeDiscounts(normalizedAdults, children);
  }

  function updateChildren(nextChildren: number) {
    const normalizedChildren = Math.max(0, Math.min(6 - adults, nextChildren));
    setChildren(normalizedChildren);
    resizeDiscounts(adults, normalizedChildren);
  }

  function resizeDiscounts(nextAdults: number, nextChildren: number) {
    const nextTotal = Math.max(1, nextAdults + nextChildren);

    setDiscounts((current) =>
      Array.from({ length: nextTotal }, (_, index) => {
        if (current[index]) {
          return current[index];
        }

        return index < nextAdults ? "normal" : "child37";
      }),
    );
  }

  function updatePassengerDiscount(index: number, code: DiscountCode) {
    setDiscounts((current) => current.map((discount, itemIndex) => (itemIndex === index ? code : discount)));
  }

  function handleConfirm() {
    const returnUrl = new URL(returnTo, window.location.origin);
    const nextParams = new URLSearchParams(returnUrl.search);
    copyPurchasePreferenceParams(searchParams, nextParams);
    nextParams.set("adults", String(adults));
    nextParams.set("children", String(children));
    nextParams.set("discounts", visibleDiscounts.join(","));

    navigate(`${returnUrl.pathname}?${nextParams.toString()}`);
  }

  return (
    <main className="discount-page">
      <section className="discount-panel">
        <Link className="data-back-link" to={returnTo}>
          &lt; Select discounts
        </Link>

        <h1>Select a discount</h1>

        <div className="discount-counter-grid">
          <PassengerCounter label="Adults" value={adults} onChange={updateAdults} min={1} max={6 - children} />
          <PassengerCounter label="Children" value={children} onChange={updateChildren} min={0} max={6 - adults} />
        </div>

        <div className="discount-passenger-list">
          {Array.from({ length: totalPassengers }, (_, index) => {
            const passengerType = index < adults ? "adult" : "child";
            const currentDiscount = visibleDiscounts[index] ?? (passengerType === "child" ? "child37" : "normal");
            const allowedDiscounts = discountOptions.filter(
              (discount) => discount.appliesTo === "all" || discount.appliesTo === passengerType,
            );
            const currentOption = getDiscountOption(currentDiscount);

            return (
              <article className="discount-passenger-row" key={`${passengerType}-${index}`}>
                <div>
                  <span className="summary-passenger-icon" aria-hidden="true" />
                  <strong>Passenger {index + 1}: {passengerType === "adult" ? "Adult" : "Child"}</strong>
                </div>

                <label>
                  <span>Ticket</span>
                  <select
                    value={currentDiscount}
                    onChange={(event) => updatePassengerDiscount(index, event.target.value as DiscountCode)}
                  >
                    {allowedDiscounts.map((discount) => (
                      <option value={discount.code} key={discount.code}>
                        {discount.label}
                      </option>
                    ))}
                  </select>
                </label>

                <small>{currentOption.documentHint}</small>
              </article>
            );
          })}
        </div>

        <section className="discount-summary-box">
          <strong>{formatPassengerSummary(counts)}</strong>
          <span>{formatDiscountSummary(visibleDiscounts)}</span>
        </section>

        <button className="discount-confirm-button" type="button" disabled={!canConfirm} onClick={handleConfirm}>
          Confirm discounts
        </button>

        <Link className="discount-back-button" to={returnTo}>
          Go back
        </Link>
      </section>
    </main>
  );
}

type PassengerCounterProps = {
  label: string;
  value: number;
  min: number;
  max: number;
  onChange: (value: number) => void;
};

function PassengerCounter({ label, value, min, max, onChange }: PassengerCounterProps) {
  return (
    <div className="discount-counter-row">
      <strong>{label}</strong>
      <button type="button" disabled={value <= min} onClick={() => onChange(value - 1)} aria-label={`Remove ${label}`}>
        -
      </button>
      <span>{value}</span>
      <button type="button" disabled={value >= max} onClick={() => onChange(value + 1)} aria-label={`Add ${label}`}>
        +
      </button>
    </div>
  );
}

export default DiscountSelectionPage;
