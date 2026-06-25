import { useEffect, useMemo, useState } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom";
import axios from "axios";
import { createBookingHold, createBookingOrderHold } from "../api/bookingApi";
import { getTripById, getTripSeats } from "../api/tripApi";
import CarriageSeatMap, { type CarriageTemplate } from "../components/CarriageSeatMap";
import type { TripDetails, TripItinerarySegment, TripSeatAvailability } from "../types/trip";
import { getDiscountCodes, getPassengerCounts, getPassengerTotal } from "../utils/purchasePreferences";

function formatDate(value?: string) {
  if (!value) {
    return "Selected date";
  }

  return new Intl.DateTimeFormat("en", {
    weekday: "long",
    day: "numeric",
    month: "long",
  }).format(new Date(value));
}

function formatTime(value?: string) {
  if (!value) {
    return "--:--";
  }

  return new Intl.DateTimeFormat("en", {
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function decodeItinerarySegments(value: string | null): TripItinerarySegment[] {
  if (!value) {
    return [];
  }

  try {
    const decoded = decodeURIComponent(window.atob(value));
    const parsed = JSON.parse(decoded);
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}

function SeatMapPage() {
  const { tripId = "" } = useParams();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const selectedClass = searchParams.get("class") === "2" ? "2" : "1";
  const fromStationId = searchParams.get("fromStationId");
  const toStationId = searchParams.get("toStationId");
  const segmentDepartureName = searchParams.get("fromStation");
  const segmentArrivalName = searchParams.get("toStation");
  const passengerCounts = getPassengerCounts(searchParams);
  const passengerTotal = getPassengerTotal(passengerCounts);
  const discountCodes = getDiscountCodes(searchParams, passengerCounts);
  const itinerarySegments = useMemo(
    () => decodeItinerarySegments(searchParams.get("itinerarySegments")),
    [searchParams],
  );
  const isItineraryMode = itinerarySegments.length > 1;
  const segmentCount = isItineraryMode ? itinerarySegments.length : 1;
  const [activeSegmentIndex, setActiveSegmentIndex] = useState(0);
  const activeItinerarySegment = isItineraryMode ? itinerarySegments[activeSegmentIndex] : null;
  const activeTripId = activeItinerarySegment ? String(activeItinerarySegment.tripId) : tripId;
  const activeFromStationId = activeItinerarySegment ? String(activeItinerarySegment.departureStationId) : fromStationId;
  const activeToStationId = activeItinerarySegment ? String(activeItinerarySegment.arrivalStationId) : toStationId;
  const activeDepartureName = activeItinerarySegment?.departureStationName ?? segmentDepartureName;
  const activeArrivalName = activeItinerarySegment?.arrivalStationName ?? segmentArrivalName;
  const segmentSignature = isItineraryMode
    ? itinerarySegments.map((segment) => `${segment.tripId}:${segment.departureStationId}:${segment.arrivalStationId}`).join("|")
    : "direct";
  const [trip, setTrip] = useState<TripDetails | null>(null);
  const [seats, setSeats] = useState<TripSeatAvailability[]>([]);
  const [activeCoach, setActiveCoach] = useState("");
  const [activePassengerIndex, setActivePassengerIndex] = useState(0);
  const [selectedSeatsBySegment, setSelectedSeatsBySegment] = useState<Array<Array<TripSeatAvailability | null>>>(() =>
    [Array.from({ length: passengerTotal }, () => null)],
  );
  const [itineraryCompletionMessage, setItineraryCompletionMessage] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isCreatingHold, setIsCreatingHold] = useState(false);
  const [error, setError] = useState("");
  const selectedSeats = selectedSeatsBySegment[activeSegmentIndex] ?? [];
  const selectedSeat = selectedSeats[activePassengerIndex] ?? null;
  const allPassengersHaveSeats = selectedSeats.length === passengerTotal && selectedSeats.every(Boolean);
  const allItinerarySeatsHaveSeats = selectedSeatsBySegment.length === segmentCount
    && selectedSeatsBySegment.every((segmentSeats) => segmentSeats.length === passengerTotal && segmentSeats.every(Boolean));
  const usedSeatIds = selectedSeats
    .map((seat, index) => (index === activePassengerIndex ? null : seat?.seatId))
    .filter((seatId): seatId is number => Boolean(seatId));

  useEffect(() => {
    if (!activeTripId) {
      return;
    }

    setIsLoading(true);
    setError("");

    Promise.all([getTripById(activeTripId), getTripSeats(activeTripId, { fromStationId: activeFromStationId, toStationId: activeToStationId })])
      .then(([tripDetails, seatList]) => {
        setTrip(tripDetails);
        setSeats(seatList);
      })
      .catch(() => {
        setError("Could not load live seat availability. Check that the API is running and the trip exists.");
      })
      .finally(() => setIsLoading(false));
  }, [activeFromStationId, activeToStationId, activeTripId]);

  const coachOptions = useMemo(() => {
    return [...new Set(seats.map((seat) => seat.coach))].sort((first, second) => {
      return getCoachPosition(seats, first) - getCoachPosition(seats, second);
    });
  }, [seats]);

  useEffect(() => {
    if (coachOptions.length === 0) {
      setActiveCoach("");
      return;
    }

    setActiveCoach((currentCoach) => {
      if (currentCoach && coachOptions.includes(currentCoach)) {
        return currentCoach;
      }

      return coachOptions.find((coach) =>
        seats.some((seat) => seat.coach === coach && matchesSelectedClass(seat.classType, selectedClass)),
      ) ?? coachOptions[0];
    });
  }, [coachOptions, seats, selectedClass]);

  const activeCoachIndex = Math.max(0, coachOptions.indexOf(activeCoach));
  const activeCoachSeats = useMemo(() => {
    return seats.filter((seat) => seat.coach === activeCoach);
  }, [activeCoach, seats]);
  const activeTemplate = getTemplateForCoach(activeCoachSeats, selectedClass, activeCoachIndex, coachOptions.length);
  const flowParams = new URLSearchParams(searchParams);
  flowParams.set("class", selectedClass);

  useEffect(() => {
    setSelectedSeatsBySegment((current) => {
      return Array.from({ length: segmentCount }, (_, segmentIndex) => {
        const row = (current[segmentIndex] ?? []).slice(0, passengerTotal);
        while (row.length < passengerTotal) {
          row.push(null);
        }
        return row;
      });
    });
    setActiveSegmentIndex((current) => Math.min(current, segmentCount - 1));
    setActivePassengerIndex((current) => Math.min(current, passengerTotal - 1));
    setItineraryCompletionMessage("");
  }, [passengerTotal, segmentCount, segmentSignature]);

  function handleSelectSeat(seat: TripSeatAvailability) {
    setItineraryCompletionMessage("");
    let nextMissingIndex = -1;

    setSelectedSeatsBySegment((current) => {
      const next = current.map((segmentSeats) => segmentSeats.slice());
      const row = next[activeSegmentIndex] ?? Array.from({ length: passengerTotal }, () => null);
      row[activePassengerIndex] = seat;
      next[activeSegmentIndex] = row;
      nextMissingIndex = row.findIndex((item, index) => index !== activePassengerIndex && !item);
      return next;
    });

    if (nextMissingIndex >= 0) {
      setActivePassengerIndex(nextMissingIndex);
    }
  }

  async function handleConfirmSeat() {
    if (!trip || !allPassengersHaveSeats) {
      return;
    }

    if (isItineraryMode) {
      if (activeSegmentIndex < segmentCount - 1) {
        setActiveSegmentIndex((current) => current + 1);
        setActivePassengerIndex(0);
        setItineraryCompletionMessage("");
        return;
      }

      const firstIncompleteSegmentIndex = selectedSeatsBySegment.findIndex((segmentSeats) =>
        segmentSeats.length !== passengerTotal || segmentSeats.some((seat) => !seat),
      );

      if (firstIncompleteSegmentIndex >= 0) {
        setActiveSegmentIndex(firstIncompleteSegmentIndex);
        setActivePassengerIndex(0);
        setItineraryCompletionMessage(`Complete segment ${firstIncompleteSegmentIndex + 1} before finishing the itinerary.`);
        return;
      }

      if (allItinerarySeatsHaveSeats) {
        setIsCreatingHold(true);
        setError("");

        try {
          const order = await createBookingOrderHold({
            trainId: itinerarySegments[0].trainId,
            tripId: itinerarySegments[0].tripId,
            travelDate: itinerarySegments[0].departureTime,
            itineraryId: searchParams.get("itineraryId") ?? undefined,
            passengers: [],
            segments: itinerarySegments.map((segment, segmentIndex) => ({
              segmentIndex,
              trainId: segment.trainId,
              tripId: segment.tripId,
              segmentDepartureStationId: segment.departureStationId,
              segmentArrivalStationId: segment.arrivalStationId,
              travelDate: segment.departureTime,
              passengers: (selectedSeatsBySegment[segmentIndex] ?? []).map((seat, passengerIndex) => ({
                seatId: seat?.seatId ?? 0,
                passengerName: `Passenger ${passengerIndex + 1}`,
                passengerType: passengerIndex < passengerCounts.adults ? "Adult" : "Child",
                discountCode: discountCodes[passengerIndex],
              })),
            })),
          });

          const params = new URLSearchParams(flowParams);
          const allSelectedSeats = selectedSeatsBySegment.flat();
          params.set("orderId", String(order.id));
          params.set("bookingIds", order.bookings.map((booking) => String(booking.id)).join(","));
          params.set("seats", allSelectedSeats.map((seat) => `${seat?.coach}-${seat?.number}`).join(","));
          params.set("amount", String(order.amount));
          params.set("currency", order.bookings[0]?.currency || "PLN");
          params.set("fromStationId", String(itinerarySegments[0].departureStationId));
          params.set("toStationId", String(itinerarySegments[itinerarySegments.length - 1].arrivalStationId));
          params.set("fromStation", itinerarySegments[0].departureStationName);
          params.set("toStation", itinerarySegments[itinerarySegments.length - 1].arrivalStationName);
          params.delete("bookingId");

          const firstSeat = allSelectedSeats[0];
          if (firstSeat) {
            params.set("car", firstSeat.coach);
            params.set("seat", firstSeat.number);
          }

          navigate(`/summary/${itinerarySegments[0].tripId}?${params.toString()}`);
        } catch (reserveError) {
          const message = getReservationErrorMessage(reserveError);
          setError(message);

          if (message.toLowerCase().includes("seat")) {
            getTripSeats(activeTripId, { fromStationId: activeFromStationId, toStationId: activeToStationId })
              .then(setSeats)
              .catch(() => undefined);
          }
        } finally {
          setIsCreatingHold(false);
        }
      }
      return;
    }

    setIsCreatingHold(true);
    setError("");

    try {
      if (passengerTotal > 1) {
        const order = await createBookingOrderHold({
          trainId: trip.trainId,
          tripId: trip.tripId,
          travelDate: trip.departureTime,
          segmentDepartureStationId: fromStationId ? Number(fromStationId) : undefined,
          segmentArrivalStationId: toStationId ? Number(toStationId) : undefined,
          passengers: selectedSeats.map((seat, index) => ({
            seatId: seat?.seatId ?? 0,
            passengerName: `Passenger ${index + 1}`,
            passengerType: index < passengerCounts.adults ? "Adult" : "Child",
            discountCode: discountCodes[index],
          })),
        });

        const params = new URLSearchParams(flowParams);
        params.set("orderId", String(order.id));
        params.set("seats", selectedSeats.map((seat) => `${seat?.coach}-${seat?.number}`).join(","));
        params.set("bookingIds", order.bookings.map((booking) => String(booking.id)).join(","));
        params.set("amount", String(order.amount));
        params.set("currency", order.bookings[0]?.currency || "PLN");
        params.delete("bookingId");

        const firstSeat = selectedSeats[0];
        if (firstSeat) {
          params.set("car", firstSeat.coach);
          params.set("seat", firstSeat.number);
        }

        navigate(`/summary/${tripId}?${params.toString()}`);
        return;
      }

      const firstSeat = selectedSeats[0];
      if (!firstSeat) {
        return;
      }

      const booking = await createBookingHold({
        trainId: trip.trainId,
        tripId: trip.tripId,
        seatId: firstSeat.seatId,
        travelDate: trip.departureTime,
        segmentDepartureStationId: fromStationId ? Number(fromStationId) : undefined,
        segmentArrivalStationId: toStationId ? Number(toStationId) : undefined,
        passengerType: passengerCounts.adults > 0 ? "Adult" : "Child",
        discountCode: discountCodes[0],
      });

      const params = new URLSearchParams(flowParams);
      params.set("car", firstSeat.coach);
      params.set("seat", firstSeat.number);
      params.set("bookingId", String(booking.id));
      params.set("amount", String(booking.amount));
      params.set("currency", booking.currency || "PLN");
      params.delete("orderId");
      params.delete("bookingIds");
      params.delete("seats");

      navigate(`/summary/${tripId}?${params.toString()}`);
    } catch (reserveError) {
      const message = getReservationErrorMessage(reserveError);
      setError(message);

      if (message.toLowerCase().includes("seat")) {
        setSelectedSeatsBySegment((current) =>
          current.map((segmentSeats, segmentIndex) =>
            segmentIndex === activeSegmentIndex
              ? segmentSeats.map((seat, index) => (index === activePassengerIndex ? null : seat))
              : segmentSeats,
          ),
        );
        getTripSeats(activeTripId, { fromStationId: activeFromStationId, toStationId: activeToStationId }).then(setSeats).catch(() => undefined);
      }
    } finally {
      setIsCreatingHold(false);
    }
  }

  function moveCoach(direction: -1 | 1) {
    if (coachOptions.length <= 1) {
      return;
    }

    const nextIndex = (activeCoachIndex + direction + coachOptions.length) % coachOptions.length;
    setActiveCoach(coachOptions[nextIndex]);
  }

  return (
    <main className="seat-map-page">
      <section className="seat-map-panel">
        <div className="seat-map-header">
          <h1>Choose your seat on the plan</h1>
          <Link to={`/summary/${tripId}?${flowParams.toString()}`} aria-label="Close seat map">
            x
          </Link>
        </div>

        <div className="seat-list-toggle">
          <strong>I want to select a place from the list</strong>
          <span aria-hidden="true" />
        </div>

        <section className="seat-trip-meta">
          <strong>{activeItinerarySegment?.trainName ?? trip?.trainName ?? "Train"}</strong>
          <span>{formatDate(activeItinerarySegment?.departureTime ?? trip?.departureTime)}</span>
          <span>{formatTime(activeItinerarySegment?.departureTime ?? trip?.departureTime)} &gt; {formatTime(activeItinerarySegment?.arrivalTime ?? trip?.arrivalTime)}</span>
          <span>
            {activeDepartureName ?? trip?.departureStationName ?? "Departure"} &gt;{" "}
            {activeArrivalName ?? trip?.arrivalStationName ?? "Arrival"}
          </span>
        </section>

        {isItineraryMode && (
          <section className="itinerary-segment-strip" aria-label="Itinerary segments">
            {itinerarySegments.map((segment, index) => {
              const segmentSeats = selectedSeatsBySegment[index] ?? [];
              const isComplete = segmentSeats.length === passengerTotal && segmentSeats.every(Boolean);
              return (
                <button
                  type="button"
                  className={[
                    "itinerary-segment-tab",
                    index === activeSegmentIndex ? "itinerary-segment-tab-active" : "",
                    isComplete ? "itinerary-segment-tab-complete" : "",
                  ].join(" ")}
                  onClick={() => {
                    setActiveSegmentIndex(index);
                    setActivePassengerIndex(0);
                    setItineraryCompletionMessage("");
                  }}
                  key={`${segment.tripId}-${segment.departureStationId}-${segment.arrivalStationId}`}
                >
                  <strong>Segment {index + 1}</strong>
                  <span>{segment.departureStationName} -&gt; {segment.arrivalStationName}</span>
                  <small>{formatTime(segment.departureTime)} - {formatTime(segment.arrivalTime)}</small>
                </button>
              );
            })}
          </section>
        )}

        <section className="car-strip" aria-label="Train cars">
          <div className="train-direction">
            <span aria-hidden="true">-&gt;</span>
            <strong>Train direction</strong>
          </div>
          {coachOptions.map((coach, index) => {
            const coachSeats = seats.filter((seat) => seat.coach === coach);
            const hasSelectedClass = coachSeats.some((seat) => matchesSelectedClass(seat.classType, selectedClass));
            return (
            <button
              type="button"
              className={[
                "car-tab",
                coach === activeCoach ? "car-tab-active" : "",
                hasSelectedClass ? "" : "car-tab-muted",
              ].join(" ")}
              onClick={() => setActiveCoach(coach)}
              key={coach}
            >
              <span>{getCarBadge(seats, coach, selectedClass, index, coachOptions.length)}</span>
              <small>Car {coach}</small>
            </button>
            );
          })}
          <span className="car-tab car-tab-locomotive" aria-hidden="true">
            <span />
            <small>Locomotive</small>
          </span>
        </section>

        {isLoading && <div className="seat-map-notice">Loading live seats...</div>}
        {error && <div className="seat-map-notice">{error}</div>}

        {!isLoading && activeCoachSeats.length > 0 && (
          <section className="seat-map-stage">
            <button type="button" className="seat-nav" aria-label="Previous car" onClick={() => moveCoach(-1)}>
              &lt;
            </button>
            <CarriageSeatMap
              coach={activeCoach}
              selectedClass={selectedClass}
              selectedSeat={selectedSeat}
              seats={activeCoachSeats}
              template={activeTemplate}
              isSeatSelectable={(seat) =>
                matchesSelectedClass(seat.classType, selectedClass) && !usedSeatIds.includes(seat.seatId)
              }
              onSelectSeat={handleSelectSeat}
            />
            <button type="button" className="seat-nav" aria-label="Next car" onClick={() => moveCoach(1)}>
              &gt;
            </button>
          </section>
        )}

        {!isLoading && seats.length > 0 && activeCoachSeats.length === 0 && (
          <div className="seat-map-notice">No seats are available for this class on the selected car.</div>
        )}

        <section className="seat-legend">
          <span>Legend</span>
          <span><b className="legend-available" /> available</span>
          <span><b className="legend-unavailable" /> unavailable</span>
          <span><b className="legend-selected" /> selected</span>
        </section>

        <section className="seat-location-card">
          <span>Location</span>
          {selectedSeats.map((seat, index) => (
            <button
              className={index === activePassengerIndex ? "seat-passenger-active" : ""}
              type="button"
              onClick={() => setActivePassengerIndex(index)}
              key={`passenger-${index}`}
            >
              <strong>{isItineraryMode ? `Segment ${activeSegmentIndex + 1}, passenger ${index + 1}` : `Passenger ${index + 1}`}</strong>
              <span>{seat ? `Class ${selectedClass}, car ${seat.coach}, seat ${seat.number}` : "Choose seat"}</span>
            </button>
          ))}
        </section>

        <div className="seat-map-notice">
          {isItineraryMode
            ? `Choose ${passengerTotal} ${passengerTotal === 1 ? "seat" : "seats"} for segment ${activeSegmentIndex + 1} of ${segmentCount}.`
            : `Choose ${passengerTotal} ${passengerTotal === 1 ? "seat" : "seats"} to activate the confirm your choice button`}
        </div>

        {itineraryCompletionMessage && <div className="seat-map-notice seat-map-success">{itineraryCompletionMessage}</div>}

        <button
          className={`seat-confirm ${allPassengersHaveSeats ? "seat-confirm-active" : ""}`}
          type="button"
          disabled={!allPassengersHaveSeats || isCreatingHold}
          onClick={handleConfirmSeat}
        >
          {isCreatingHold
            ? "Reserving..."
            : isItineraryMode && activeSegmentIndex < segmentCount - 1
              ? `Confirm segment ${activeSegmentIndex + 1}`
              : isItineraryMode
                ? "Confirm all segment seats"
                : "I confirm my choice"}
        </button>
      </section>
    </main>
  );
}

function matchesSelectedClass(classType: string, selectedClass: string) {
  return classType.toLowerCase().includes(selectedClass);
}

function getReservationErrorMessage(error: unknown) {
  if (axios.isAxiosError(error)) {
    if (!error.response) {
      return "Could not reserve this seat because the API is unavailable.";
    }

    if (typeof error.response.data === "string" && error.response.data.trim()) {
      return error.response.data;
    }

    if (error.response.status >= 500) {
      return "Could not reserve this seat because the API returned a server error. The database may need the latest migration.";
    }

    return `Could not reserve this seat. API returned ${error.response.status}.`;
  }

  return "Could not reserve this seat. Please try again.";
}

function getCoachPosition(seats: TripSeatAvailability[], coach: string) {
  const seat = seats.find((item) => item.coach === coach);
  if (seat?.carriagePosition) {
    return seat.carriagePosition;
  }

  const parsed = Number.parseInt(coach, 10);
  return Number.isNaN(parsed) ? Number.MAX_SAFE_INTEGER : parsed;
}

function getTemplateForCoach(
  coachSeats: TripSeatAvailability[],
  selectedClass: string,
  coachIndex: number,
  coachCount: number,
): CarriageTemplate {
  const layoutType = coachSeats[0]?.layoutType?.toLowerCase();

  if (layoutType === "opensecond") {
    return "open-second";
  }

  if (layoutType === "opensecondaccessible") {
    return "open-second-accessible";
  }

  if (layoutType === "opensecondbike") {
    return "open-second-bike";
  }

  if (layoutType === "openfirst") {
    return "open-first";
  }

  if (layoutType === "comboaccessible") {
    return "combo-accessible";
  }

  if (layoutType === "combosecondwheelchairbike") {
    return "combo-second-wheelchair-bike";
  }

  if (layoutType === "combofirstsecond") {
    return "combo-first-second";
  }

  if (layoutType === "firstcompartment") {
    return "first-compartment";
  }

  if (layoutType === "secondcompartment") {
    return "second-compartment";
  }

  if (layoutType === "mixedfirstsecond") {
    return "mixed";
  }

  if (layoutType === "emufirstsecond") {
    return "emu-first-second";
  }

  if (layoutType === "emusecondopen") {
    return "emu-second-open";
  }

  if (layoutType === "emusecondfamilyopen") {
    return "emu-second-family-open";
  }

  if (layoutType === "emudiningaccessible") {
    return "emu-dining-accessible";
  }

  if (layoutType === "emusecondquiet") {
    return "emu-second-quiet";
  }

  if (layoutType === "emudartfirstcab") {
    return "emu-dart-first-cab";
  }

  if (layoutType === "emudartfirstaccessible") {
    return "emu-dart-first-accessible";
  }

  if (layoutType === "emudartrestaurant") {
    return "emu-dart-restaurant";
  }

  if (layoutType === "emudartsecondopen") {
    return "emu-dart-second-open";
  }

  if (layoutType === "emudartsecondcab") {
    return "emu-dart-second-cab";
  }

  if (layoutType === "restaurant") {
    return "restaurant";
  }

  if (selectedClass === "1") {
    return coachCount > 1 && coachIndex === 0 ? "mixed" : "first-compartment";
  }

  if (coachIndex === 1) {
    return "combo-accessible";
  }

  if (coachIndex === 2) {
    return "second-compartment";
  }

  if (coachIndex === 0 && coachCount > 3) {
    return "mixed";
  }

  return "open-second";
}

function getCarBadge(
  seats: TripSeatAvailability[],
  coach: string,
  selectedClass: string,
  coachIndex: number,
  coachCount: number,
) {
  const layoutType = seats.find((seat) => seat.coach === coach)?.layoutType?.toLowerCase() ?? "";
  const carriageClass = seats.find((seat) => seat.coach === coach)?.carriageClass?.toLowerCase() ?? "";
  if (layoutType === "combofirstsecond" || layoutType === "mixedfirstsecond" || layoutType === "emufirstsecond") {
    return "1 2";
  }

  if (layoutType === "emudiningaccessible") {
    return "2 WARS";
  }

  if (layoutType === "emusecondquiet") {
    return "2 quiet";
  }

  if (layoutType === "emudartfirstcab" || layoutType === "emudartfirstaccessible") {
    return "1";
  }

  if (layoutType === "emudartrestaurant") {
    return "2 WARS";
  }

  if (layoutType === "emudartsecondopen" || layoutType === "emudartsecondcab") {
    return "2";
  }

  if (carriageClass.includes("1") && carriageClass.includes("2")) {
    return "1 2";
  }

  if (carriageClass.includes("1")) {
    return "1";
  }

  if (carriageClass.includes("2")) {
    return "2";
  }

  if (selectedClass === "1" && (coachIndex === 0 || coachCount === 1)) {
    return "1";
  }

  if (selectedClass === "2" && coachIndex === 0 && coachCount > 3) {
    return "1 2";
  }

  return "2";
}

export default SeatMapPage;
