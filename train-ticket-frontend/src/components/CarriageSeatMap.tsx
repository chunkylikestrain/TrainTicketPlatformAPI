import type { TripSeatAvailability } from "../types/trip";

type CarriageTemplate =
  | "open-second"
  | "open-second-accessible"
  | "open-second-bike"
  | "open-first"
  | "combo-accessible"
  | "combo-second-wheelchair-bike"
  | "combo-first-second"
  | "first-compartment"
  | "second-compartment"
  | "mixed"
  | "emu-first-second"
  | "emu-second-open"
  | "emu-second-family-open"
  | "emu-dining-accessible"
  | "emu-second-quiet"
  | "restaurant";

type SeatPlanSlot =
  | { type: "seat"; classType?: "Class 1" | "Class 2" }
  | { type: "compartment"; seats: number; classType?: "Class 1" | "Class 2" }
  | { type: "facility"; label: string; tone?: "large" | "long" }
  | { type: "class"; label: string }
  | { type: "corridor"; label?: string }
  | { type: "divider" }
  | { type: "space"; size?: "wide" | "long" };

type PlannedSlot =
  | { type: "seat"; seat: TripSeatAvailability | null }
  | { type: "compartment"; seats: Array<TripSeatAvailability | null>; classType?: "Class 1" | "Class 2" }
  | Exclude<SeatPlanSlot, { type: "seat" | "compartment" }>;

type CarriageSeatMapProps = {
  coach: string;
  selectedClass: string;
  selectedSeat: TripSeatAvailability | null;
  seats: TripSeatAvailability[];
  template: CarriageTemplate;
  isSeatSelectable?: (seat: TripSeatAvailability) => boolean;
  onSelectSeat: (seat: TripSeatAvailability) => void;
};

function seatSlots(count: number, classType?: "Class 1" | "Class 2"): SeatPlanSlot[] {
  return Array.from({ length: count }, () => ({ type: "seat", classType }));
}

function buildTemplate(template: CarriageTemplate, seats: TripSeatAvailability[]): SeatPlanSlot[][] {
  const seatCount = Math.max(seats.length, defaultSeatCount(template));

  if (template === "first-compartment") {
    return buildCompartmentTemplate(Math.max(9, Math.ceil(seatCount / 6)), "1", "Class 1");
  }

  if (template === "second-compartment") {
    return buildCompartmentTemplate(Math.max(10, Math.ceil(seatCount / 6)), "2", "Class 2");
  }

  if (template === "open-first") {
    return buildOpenTemplate(Math.max(seatCount, 54), "1", "Class 1");
  }

  if (template === "open-second" || template === "emu-second-open") {
    return buildOpenTemplate(Math.max(seatCount, 88), "2", "Class 2");
  }

  if (template === "emu-second-family-open") {
    return [
      [
        { type: "facility", label: "Family", tone: "long" },
        { type: "divider" },
        ...seatSlots(18, "Class 2"),
        { type: "space" },
        ...seatSlots(16, "Class 2"),
        { type: "divider" },
        { type: "facility", label: "WC", tone: "large" },
      ],
      [
        { type: "space", size: "wide" },
        { type: "corridor", label: "2" },
        { type: "space", size: "wide" },
      ],
      [
        { type: "facility", label: "Bag", tone: "large" },
        { type: "divider" },
        ...seatSlots(24, "Class 2"),
        { type: "space" },
        ...seatSlots(Math.max(18, seatCount - 58), "Class 2"),
        { type: "facility", label: "WC", tone: "large" },
      ],
    ];
  }

  if (template === "emu-dining-accessible") {
    return [
      [
        { type: "facility", label: "Ramp", tone: "large" },
        { type: "facility", label: "WC", tone: "large" },
        { type: "divider" },
        ...seatSlots(2, "Class 2"),
        { type: "space" },
        { type: "facility", label: "Bar", tone: "long" },
        { type: "facility", label: "Dining", tone: "long" },
        { type: "facility", label: "Kitchen", tone: "long" },
      ],
      [
        { type: "space", size: "wide" },
        { type: "corridor", label: "WARS" },
        { type: "space", size: "wide" },
      ],
      [
        { type: "facility", label: "Wheel", tone: "large" },
        { type: "divider" },
        ...seatSlots(Math.max(10, seatCount - 2), "Class 2"),
        { type: "facility", label: "Crew", tone: "long" },
      ],
    ];
  }

  if (template === "emu-second-quiet") {
    return buildOpenTemplate(Math.max(seatCount, 88), "2", "Class 2", {
      start: [{ type: "facility", label: "Quiet", tone: "long" }],
      end: [{ type: "facility", label: "WC", tone: "large" }],
    });
  }

  if (template === "open-second-accessible") {
    return buildOpenTemplate(Math.max(seatCount, 88), "2", "Class 2", {
      start: [{ type: "facility", label: "Wheel", tone: "large" }],
      end: [{ type: "facility", label: "WC", tone: "large" }],
    });
  }

  if (template === "open-second-bike") {
    return buildOpenTemplate(Math.max(seatCount, 88), "2", "Class 2", {
      start: [{ type: "facility", label: "Bike", tone: "long" }],
      end: [{ type: "facility", label: "Bike", tone: "long" }],
    });
  }

  if (template === "combo-second-wheelchair-bike" || template === "combo-accessible") {
    return [
      [
        { type: "facility", label: "WC", tone: "large" },
        { type: "facility", label: "Wheel", tone: "large" },
        { type: "divider" },
        ...seatSlots(12, "Class 2"),
        { type: "space" },
        ...seatSlots(8, "Class 2"),
        { type: "facility", label: "Bike", tone: "long" },
        { type: "divider" },
        { type: "facility", label: "WC", tone: "large" },
      ],
      [
        { type: "space", size: "wide" },
        { type: "corridor", label: "2" },
        { type: "space", size: "wide" },
      ],
      [
        { type: "facility", label: "Family", tone: "large" },
        { type: "divider" },
        ...seatSlots(12, "Class 2"),
        { type: "space" },
        ...seatSlots(10, "Class 2"),
        { type: "facility", label: "Bike", tone: "long" },
      ],
    ];
  }

  if (template === "combo-first-second" || template === "mixed" || template === "emu-first-second") {
    return [
      [
        { type: "facility", label: "WC", tone: "large" },
        { type: "divider" },
        { type: "compartment", seats: 6, classType: "Class 1" },
        { type: "class", label: "1" },
        { type: "compartment", seats: 6, classType: "Class 1" },
        { type: "divider" },
        { type: "compartment", seats: 8, classType: "Class 2" },
        { type: "class", label: "2" },
        { type: "compartment", seats: 8, classType: "Class 2" },
        { type: "facility", label: "WC", tone: "large" },
      ],
      [
        { type: "space", size: "wide" },
        { type: "corridor", label: "1/2" },
        { type: "space", size: "wide" },
      ],
      [
        { type: "facility", label: "Bag", tone: "large" },
        { type: "divider" },
        ...seatSlots(8, "Class 1"),
        { type: "space" },
        ...seatSlots(Math.max(8, seatCount - 36), "Class 2"),
        { type: "facility", label: "Bike", tone: "long" },
      ],
    ];
  }

  if (template === "restaurant") {
    return [
      [
        { type: "facility", label: "Kitchen", tone: "long" },
        { type: "divider" },
        { type: "facility", label: "Bar", tone: "long" },
        { type: "space", size: "long" },
        { type: "facility", label: "Dining", tone: "long" },
        { type: "space", size: "long" },
        { type: "facility", label: "Tables", tone: "long" },
      ],
      [{ type: "corridor", label: "WARS" }],
    ];
  }

  return buildOpenTemplate(Math.max(seatCount, 88), "2", "Class 2");
}

function defaultSeatCount(template: CarriageTemplate) {
  if (template === "first-compartment") return 54;
  if (template === "second-compartment") return 60;
  if (template === "open-first") return 54;
  if (template === "combo-first-second" || template === "mixed" || template === "emu-first-second") return 64;
  if (template === "emu-second-family-open") return 98;
  if (template === "emu-dining-accessible") return 12;
  if (template === "emu-second-quiet") return 88;
  if (template === "combo-accessible" || template === "combo-second-wheelchair-bike") return 56;
  if (template === "restaurant") return 0;
  return 88;
}

function buildOpenTemplate(
  seatCount: number,
  classLabel: string,
  classType: "Class 1" | "Class 2",
  facilities?: { start?: SeatPlanSlot[]; end?: SeatPlanSlot[] },
): SeatPlanSlot[][] {
  const seatsPerSideRow = Math.ceil(seatCount / 4);
  return [
    [
      { type: "facility", label: "WC", tone: "large" },
      ...(facilities?.start ?? []),
      { type: "divider" },
      ...openRowSeats(seatsPerSideRow, classType),
      { type: "divider" },
      { type: "facility", label: "WC", tone: "large" },
    ],
    [
      { type: "space", size: "wide" },
      ...openRowSeats(seatsPerSideRow, classType),
      { type: "space", size: "wide" },
    ],
    [
      { type: "space", size: "wide" },
      { type: "corridor", label: classLabel },
      { type: "space", size: "wide" },
    ],
    [
      { type: "facility", label: "Bag" },
      { type: "divider" },
      ...openRowSeats(seatsPerSideRow, classType),
      { type: "divider" },
      ...(facilities?.end ?? [{ type: "facility", label: "Bag" }]),
    ],
    [
      { type: "space", size: "wide" },
      ...openRowSeats(seatsPerSideRow, classType),
      { type: "space", size: "wide" },
    ],
  ];
}

function openRowSeats(count: number, classType: "Class 1" | "Class 2"): SeatPlanSlot[] {
  return Array.from({ length: count }).flatMap((_, index) => [
    { type: "seat", classType } as SeatPlanSlot,
    ...(index % 6 === 5 ? [{ type: "space" } as SeatPlanSlot] : []),
  ]);
}

function buildCompartmentTemplate(
  compartmentCount: number,
  classLabel: string,
  classType: "Class 1" | "Class 2",
): SeatPlanSlot[][] {
  return [
    [
      { type: "facility", label: "WC", tone: "large" },
      { type: "divider" },
      ...Array.from({ length: compartmentCount }).map(() => ({ type: "compartment", seats: 6, classType }) as SeatPlanSlot),
      { type: "divider" },
      { type: "facility", label: "WC", tone: "large" },
    ],
    [
      { type: "space", size: "wide" },
      { type: "corridor", label: classLabel },
      { type: "space", size: "wide" },
    ],
  ];
}

function sortSeats(seats: TripSeatAvailability[]) {
  return [...seats].sort((first, second) => {
    const firstNumber = Number.parseInt(first.number, 10);
    const secondNumber = Number.parseInt(second.number, 10);

    if (Number.isNaN(firstNumber) || Number.isNaN(secondNumber)) {
      return first.number.localeCompare(second.number);
    }

    return firstNumber - secondNumber;
  });
}

function buildRows(template: CarriageTemplate, seats: TripSeatAvailability[]): PlannedSlot[][] {
  const sortedSeats = sortSeats(seats);
  const templateRows = buildTemplate(template, seats);
  const usedSeatIds = new Set<number>();

  return templateRows.map((row) => {
    const plannedRow: PlannedSlot[] = [];

    row.forEach((slot) => {
      if (slot.type === "seat") {
        const seat = takeNextSeat(sortedSeats, usedSeatIds, slot.classType);
        plannedRow.push({ type: "seat", seat });
        return;
      }

      if (slot.type === "compartment") {
        const compartmentSeats = Array.from({ length: slot.seats }).map(() =>
          takeNextSeat(sortedSeats, usedSeatIds, slot.classType));

        plannedRow.push({ type: "compartment", seats: compartmentSeats, classType: slot.classType });
        return;
      }

      plannedRow.push(slot);
    });

    return plannedRow;
  });
}

function takeNextSeat(
  seats: TripSeatAvailability[],
  usedSeatIds: Set<number>,
  classType?: "Class 1" | "Class 2",
) {
  const classNumber = classType?.includes("1") ? "1" : classType?.includes("2") ? "2" : "";
  const matchingIndex = seats.findIndex((seat) =>
    !usedSeatIds.has(seat.seatId) && (!classNumber || seat.classType.includes(classNumber)));
  const fallbackIndex = seats.findIndex((seat) => !usedSeatIds.has(seat.seatId));
  const seat = seats[matchingIndex >= 0 ? matchingIndex : fallbackIndex] ?? null;

  if (seat) {
    usedSeatIds.add(seat.seatId);
  }

  return seat;
}

function CarriageSeatMap({
  coach,
  selectedClass,
  selectedSeat,
  seats,
  template,
  isSeatSelectable,
  onSelectSeat,
}: CarriageSeatMapProps) {
  const rows = buildRows(template, seats);

  return (
    <div className={`real-coach-layout real-coach-${template}`} aria-label={`Car ${coach} seat map`}>
      <div className="coach-shell">
        {rows.map((row, rowIndex) => (
          <div className="coach-plan-row" key={`${template}-${rowIndex}`}>
            {row.map((slot, slotIndex) => renderSlot(slot, coach, slotIndex, selectedSeat, isSeatSelectable, onSelectSeat))}
          </div>
        ))}
      </div>
      <div className="coach-scrollbar" aria-hidden="true">
        <span />
      </div>
      <p className="coach-caption">
        Car {coach} - Class {selectedClass} - {templateLabel(template)}
      </p>
    </div>
  );
}

function renderSlot(
  slot: PlannedSlot,
  coach: string,
  slotIndex: number,
  selectedSeat: TripSeatAvailability | null,
  isSeatSelectable: ((seat: TripSeatAvailability) => boolean) | undefined,
  onSelectSeat: (seat: TripSeatAvailability) => void,
) {
  if (slot.type === "seat") {
    return renderSeatCell(slot.seat, selectedSeat, isSeatSelectable, onSelectSeat, `${coach}-seat-${slot.seat?.seatId ?? slotIndex}`);
  }

  if (slot.type === "compartment") {
    return (
      <span className={`coach-compartment ${slot.classType?.includes("1") ? "coach-compartment-first" : ""}`} key={`${coach}-compartment-${slotIndex}`}>
        {slot.seats.map((seat, seatIndex) =>
          renderSeatCell(
            seat,
            selectedSeat,
            isSeatSelectable,
            onSelectSeat,
            `${coach}-compartment-${slotIndex}-seat-${seat?.seatId ?? seatIndex}`,
          ),
        )}
      </span>
    );
  }

  if (slot.type === "facility") {
    return (
      <span
        className={[
          "coach-facility",
          slot.tone === "large" ? "coach-facility-large" : "",
          slot.tone === "long" ? "coach-facility-long" : "",
        ].join(" ")}
        key={`${coach}-facility-${slotIndex}`}
      >
        {slot.label}
      </span>
    );
  }

  if (slot.type === "class") {
    return (
      <span className="coach-class-marker" key={`${coach}-class-${slotIndex}`}>
        {slot.label}
      </span>
    );
  }

  if (slot.type === "corridor") {
    return (
      <span className="coach-corridor" key={`${coach}-corridor-${slotIndex}`}>
        {slot.label && <b>{slot.label}</b>}
      </span>
    );
  }

  if (slot.type === "divider") {
    return <span className="coach-divider" aria-hidden="true" key={`${coach}-divider-${slotIndex}`} />;
  }

  return (
    <span
      className={[
        "coach-space",
        slot.size === "wide" ? "coach-space-wide" : "",
        slot.size === "long" ? "coach-space-long" : "",
      ].join(" ")}
      aria-hidden="true"
      key={`${coach}-space-${slotIndex}`}
    />
  );
}

function renderSeatCell(
  seat: TripSeatAvailability | null,
  selectedSeat: TripSeatAvailability | null,
  isSeatSelectable: ((seat: TripSeatAvailability) => boolean) | undefined,
  onSelectSeat: (seat: TripSeatAvailability) => void,
  key: string,
) {
  const isSelected = Boolean(seat && selectedSeat?.seatId === seat.seatId);
  const canSelect = Boolean(seat?.isAvailable && (!isSeatSelectable || isSeatSelectable(seat)));

  return (
    <button
      type="button"
      className={`seat-cell ${canSelect ? "seat-available" : "seat-unavailable"} ${isSelected ? "seat-selected" : ""}`}
      disabled={!canSelect}
      onClick={() => canSelect && seat && onSelectSeat(seat)}
      key={key}
    >
      {seat?.number ?? "X"}
    </button>
  );
}

function templateLabel(template: CarriageTemplate) {
  if (template === "combo-accessible" || template === "combo-second-wheelchair-bike") {
    return "accessible, bicycle, family, and open-space coach";
  }

  if (template === "combo-first-second" || template === "mixed" || template === "emu-first-second") {
    return "mixed first and second class coach";
  }

  if (template === "emu-second-family-open") {
    return "family and open second class unit";
  }

  if (template === "emu-dining-accessible") {
    return "accessible dining unit";
  }

  if (template === "emu-second-quiet") {
    return "quiet second class unit";
  }

  if (template === "first-compartment") {
    return "9-compartment first class coach";
  }

  if (template === "second-compartment") {
    return "10-compartment second class coach";
  }

  if (template === "open-first") {
    return "first class open-space coach";
  }

  if (template === "open-second-accessible") {
    return "second class open-space coach with wheelchair spaces";
  }

  if (template === "open-second-bike") {
    return "second class open-space coach with bicycle spaces";
  }

  if (template === "restaurant") {
    return "restaurant car";
  }

  return "second class open-space coach";
}

export type { CarriageTemplate };
export default CarriageSeatMap;
