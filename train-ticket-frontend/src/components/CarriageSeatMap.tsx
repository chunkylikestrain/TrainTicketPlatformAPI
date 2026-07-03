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
  | "international-sleeper"
  | "domestic-sleeper"
  | "four-berth-couchette"
  | "six-berth-couchette"
  | "mixed"
  | "emu-first-second"
  | "emu-second-open"
  | "emu-second-family-open"
  | "emu-dining-accessible"
  | "emu-second-quiet"
  | "emu-dart-first-cab"
  | "emu-dart-first-accessible"
  | "emu-dart-restaurant"
  | "emu-dart-second-open"
  | "emu-dart-second-cab"
  | "restaurant";

type SeatClassType = "Class 1" | "Class 2" | "Sleeper" | "Couchette";

type SeatPlanSlot =
  | { type: "seat"; classType?: SeatClassType }
  | { type: "compartment"; seats: number; classType?: SeatClassType }
  | { type: "facility"; label: string; tone?: "large" | "long" }
  | { type: "class"; label: string }
  | { type: "corridor"; label?: string; size?: "short" }
  | { type: "divider" }
  | { type: "space"; size?: "wide" | "long" };

type PlannedSlot =
  | { type: "seat"; seat: TripSeatAvailability | null }
  | { type: "compartment"; seats: Array<TripSeatAvailability | null>; classType?: SeatClassType }
  | Exclude<SeatPlanSlot, { type: "seat" | "compartment" }>;

type CarriageSeatMapProps = {
  coach: string;
  selectedClass: string;
  selectedSeat: TripSeatAvailability | null;
  selectedSeats?: TripSeatAvailability[];
  seats: TripSeatAvailability[];
  template: CarriageTemplate;
  isSeatSelectable?: (seat: TripSeatAvailability) => boolean;
  onSelectSeat: (seat: TripSeatAvailability) => void;
};

function isSeatSelected(
  seat: TripSeatAvailability | null,
  selectedSeat: TripSeatAvailability | null,
  selectedSeats: TripSeatAvailability[],
) {
  return Boolean(
    seat &&
      (selectedSeat?.seatId === seat.seatId ||
        selectedSeats.some((selected) => selected.seatId === seat.seatId)),
  );
}

type GraphicSeatSlot = {
  number: string;
  x: number;
  y: number;
};

type GraphicFacilitySlot = {
  label: string;
  x: number;
  y: number;
  width?: number;
  height?: number;
};

type GraphicCompartmentSlot = {
  x: number;
  y: number;
};

function seatSlots(count: number, classType?: SeatClassType): SeatPlanSlot[] {
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

  if (template === "international-sleeper") {
    return buildInternationalSleeperTemplate();
  }

  if (template === "domestic-sleeper") {
    return buildDomesticSleeperTemplate();
  }

  if (template === "four-berth-couchette") {
    return buildFourBerthCouchetteTemplate();
  }

  if (template === "six-berth-couchette") {
    return buildSixBerthCouchetteTemplate();
  }

  if (template === "open-first") {
    return buildFirstOpenTemplate(Math.max(seatCount, 54));
  }

  if (template === "open-second" || template === "emu-second-open") {
    return buildOpenTemplate(Math.max(seatCount, 88), "2", "Class 2");
  }

  if (template === "emu-second-family-open") {
    return buildSecondFamilyOpenTemplate(Math.max(seatCount, 98));
  }

  if (template === "emu-dart-first-cab") {
    return buildFirstOpenTemplate(Math.max(seatCount, 54));
  }

  if (template === "emu-dart-first-accessible") {
    return buildDartFirstAccessibleTemplate(Math.max(seatCount, 42));
  }

  if (template === "emu-dart-restaurant") {
    return buildDartRestaurantTemplate(Math.max(seatCount, 16));
  }

  if (template === "emu-dart-second-open") {
    return buildOpenTemplate(Math.max(seatCount, 76), "2", "Class 2");
  }

  if (template === "emu-dart-second-cab") {
    return buildDartSecondCabTemplate(Math.max(seatCount, 76));
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
  if (template === "international-sleeper") return 22;
  if (template === "domestic-sleeper") return 30;
  if (template === "four-berth-couchette") return 30;
  if (template === "six-berth-couchette") return 44;
  if (template === "open-first") return 54;
  if (template === "combo-first-second" || template === "mixed" || template === "emu-first-second") return 64;
  if (template === "emu-second-family-open") return 98;
  if (template === "emu-dining-accessible") return 12;
  if (template === "emu-second-quiet") return 88;
  if (template === "emu-dart-first-cab") return 54;
  if (template === "emu-dart-first-accessible") return 42;
  if (template === "emu-dart-restaurant") return 16;
  if (template === "emu-dart-second-open" || template === "emu-dart-second-cab") return 76;
  if (template === "combo-accessible" || template === "combo-second-wheelchair-bike") return 56;
  if (template === "restaurant") return 0;
  return 88;
}

function buildInternationalSleeperTemplate(): SeatPlanSlot[][] {
  const tripleCompartment = (): SeatPlanSlot => ({ type: "compartment", seats: 3, classType: "Sleeper" });
  const deluxeCompartment = (): SeatPlanSlot => ({ type: "compartment", seats: 2, classType: "Sleeper" });

  return [
    [
      { type: "facility", label: "WC", tone: "large" },
      { type: "facility", label: "Attendant", tone: "long" },
      { type: "divider" },
      tripleCompartment(),
      tripleCompartment(),
      tripleCompartment(),
      { type: "facility", label: "Shower", tone: "large" },
      deluxeCompartment(),
      deluxeCompartment(),
      { type: "facility", label: "Shower", tone: "large" },
      tripleCompartment(),
      tripleCompartment(),
      tripleCompartment(),
      { type: "divider" },
      { type: "facility", label: "WC", tone: "large" },
    ],
    [
      { type: "space", size: "wide" },
      { type: "corridor", label: "WLAB international sleeper" },
      { type: "space", size: "wide" },
    ],
    [
      { type: "facility", label: "3-berth compartments", tone: "long" },
      { type: "space", size: "long" },
      { type: "facility", label: "Deluxe 2-berth with shower", tone: "long" },
      { type: "space", size: "long" },
      { type: "facility", label: "3-berth compartments", tone: "long" },
    ],
  ];
}

function buildDomesticSleeperTemplate(): SeatPlanSlot[][] {
  const tripleCompartment = (): SeatPlanSlot => ({ type: "compartment", seats: 3, classType: "Sleeper" });

  return [
    [
      { type: "facility", label: "WC", tone: "large" },
      { type: "facility", label: "Attendant", tone: "long" },
      { type: "divider" },
      tripleCompartment(),
      tripleCompartment(),
      tripleCompartment(),
      tripleCompartment(),
      tripleCompartment(),
      tripleCompartment(),
      tripleCompartment(),
      tripleCompartment(),
      tripleCompartment(),
      tripleCompartment(),
      { type: "divider" },
      { type: "facility", label: "WC", tone: "large" },
    ],
    [
      { type: "space", size: "wide" },
      { type: "corridor", label: "WLAB domestic sleeper" },
      { type: "space", size: "wide" },
    ],
    [
      { type: "facility", label: "10 compartments", tone: "long" },
      { type: "space", size: "long" },
      { type: "facility", label: "3 berths per compartment", tone: "long" },
    ],
  ];
}

function buildFourBerthCouchetteTemplate(): SeatPlanSlot[][] {
  const accessibleCompartment = (): SeatPlanSlot => ({ type: "compartment", seats: 2, classType: "Couchette" });
  const couchetteCompartment = (): SeatPlanSlot => ({ type: "compartment", seats: 4, classType: "Couchette" });

  return [
    [
      { type: "facility", label: "Accessible WC", tone: "long" },
      { type: "facility", label: "Lift", tone: "large" },
      accessibleCompartment(),
      { type: "facility", label: "Attendant", tone: "long" },
      { type: "divider" },
      couchetteCompartment(),
      couchetteCompartment(),
      couchetteCompartment(),
      couchetteCompartment(),
      couchetteCompartment(),
      couchetteCompartment(),
      couchetteCompartment(),
      { type: "divider" },
      { type: "facility", label: "WC", tone: "large" },
    ],
    [
      { type: "space", size: "wide" },
      { type: "corridor", label: "Bc 4-berth couchette" },
      { type: "space", size: "wide" },
    ],
    [
      { type: "facility", label: "Accessible compartment", tone: "long" },
      { type: "space", size: "long" },
      { type: "facility", label: "7 compartments", tone: "long" },
      { type: "space", size: "long" },
      { type: "facility", label: "4 berths per compartment", tone: "long" },
    ],
  ];
}

function buildSixBerthCouchetteTemplate(): SeatPlanSlot[][] {
  const accessibleCompartment = (): SeatPlanSlot => ({ type: "compartment", seats: 2, classType: "Couchette" });
  const couchetteCompartment = (): SeatPlanSlot => ({ type: "compartment", seats: 6, classType: "Couchette" });

  return [
    [
      { type: "facility", label: "Accessible WC", tone: "long" },
      { type: "facility", label: "Lift", tone: "large" },
      accessibleCompartment(),
      { type: "facility", label: "Attendant", tone: "long" },
      { type: "divider" },
      couchetteCompartment(),
      couchetteCompartment(),
      couchetteCompartment(),
      couchetteCompartment(),
      couchetteCompartment(),
      couchetteCompartment(),
      couchetteCompartment(),
      { type: "divider" },
      { type: "facility", label: "WC", tone: "large" },
    ],
    [
      { type: "space", size: "wide" },
      { type: "corridor", label: "Bc 6-berth couchette" },
      { type: "space", size: "wide" },
    ],
    [
      { type: "facility", label: "Accessible compartment", tone: "long" },
      { type: "space", size: "long" },
      { type: "facility", label: "7 compartments", tone: "long" },
      { type: "space", size: "long" },
      { type: "facility", label: "6 berths per compartment", tone: "long" },
    ],
  ];
}

function splitAcrossRows(total: number, rowCount: number) {
  const base = Math.floor(total / rowCount);
  const remainder = total % rowCount;
  return Array.from({ length: rowCount }, (_, index) => base + (index < remainder ? 1 : 0));
}

function buildFirstOpenTemplate(seatCount: number): SeatPlanSlot[][] {
  const seatsPerRow = Math.ceil(seatCount / 3);
  const bottomRowSeats = Math.max(0, seatCount - seatsPerRow * 2);

  return [
    [
      { type: "facility", label: "WC", tone: "large" },
      { type: "divider" },
      ...openRowSeats(seatsPerRow, "Class 1"),
      { type: "facility", label: "Bag" },
    ],
    [
      { type: "facility", label: "Cabin", tone: "large" },
      { type: "divider" },
      ...openRowSeats(seatsPerRow, "Class 1"),
      { type: "divider" },
      { type: "facility", label: "WC", tone: "large" },
    ],
    [
      { type: "space", size: "wide" },
      { type: "corridor", label: "1" },
      { type: "space", size: "wide" },
    ],
    [
      { type: "facility", label: "Bag", tone: "large" },
      { type: "divider" },
      ...openRowSeats(bottomRowSeats, "Class 1"),
      { type: "divider" },
      { type: "facility", label: "Bag" },
    ],
  ];
}

function buildSecondFamilyOpenTemplate(seatCount: number): SeatPlanSlot[][] {
  const familyCompartmentCount = 6;
  const familySeats = familyCompartmentCount * 4;
  const openSeatCount = Math.max(64, seatCount - familySeats);
  const rowSeats = splitAcrossRows(openSeatCount, 4);
  const familyCompartments = Array.from({ length: familyCompartmentCount }, () =>
    ({ type: "compartment", seats: 4, classType: "Class 2" }) as SeatPlanSlot);
  const firstFamilyBlock = familyCompartments.slice(0, 3);
  const secondFamilyBlock = familyCompartments.slice(3);

  return [
    [
      { type: "facility", label: "WC", tone: "large" },
      { type: "divider" },
      { type: "corridor", size: "short" },
      { type: "facility", label: "Family", tone: "long" },
      { type: "divider" },
      ...openRowSeats(rowSeats[0], "Class 2"),
    ],
    [
      { type: "facility", label: "Cabin", tone: "large" },
      { type: "divider" },
      ...firstFamilyBlock,
      { type: "space", size: "long" },
      ...openRowSeats(rowSeats[1], "Class 2"),
      { type: "divider" },
      { type: "facility", label: "WC", tone: "large" },
    ],
    [
      { type: "space", size: "wide" },
      { type: "divider" },
      ...secondFamilyBlock,
      { type: "space", size: "wide" },
      { type: "corridor", label: "2" },
    ],
    [
      { type: "facility", label: "Bag" },
      { type: "divider" },
      { type: "space", size: "long" },
      { type: "space", size: "wide" },
      ...openRowSeats(rowSeats[2], "Class 2"),
      { type: "divider" },
      { type: "facility", label: "Bag" },
    ],
    [
      { type: "facility", label: "Stroller", tone: "long" },
      { type: "divider" },
      { type: "space", size: "long" },
      { type: "space", size: "wide" },
      ...openRowSeats(rowSeats[3], "Class 2"),
      { type: "divider" },
      { type: "facility", label: "WC", tone: "large" },
    ],
  ];
}

function buildDartFirstAccessibleTemplate(seatCount: number): SeatPlanSlot[][] {
  const standardSeats = Math.max(24, seatCount - 4);
  const rowSeats = splitAcrossRows(standardSeats, 3);

  return [
    [
      { type: "facility", label: "Wheel", tone: "large" },
      { type: "facility", label: "WC", tone: "large" },
      { type: "divider" },
      ...openRowSeats(rowSeats[0], "Class 1"),
      { type: "facility", label: "Bag" },
    ],
    [
      { type: "facility", label: "Ramp", tone: "large" },
      { type: "divider" },
      ...openRowSeats(rowSeats[1], "Class 1"),
      { type: "facility", label: "WC", tone: "large" },
    ],
    [
      { type: "space", size: "wide" },
      { type: "corridor", label: "1" },
      { type: "space", size: "wide" },
    ],
    [
      { type: "facility", label: "Companion", tone: "long" },
      { type: "divider" },
      ...openRowSeats(rowSeats[2], "Class 1"),
    ],
  ];
}

function buildDartRestaurantTemplate(seatCount: number): SeatPlanSlot[][] {
  const rowSeats = splitAcrossRows(Math.max(12, seatCount), 2);

  return [
    [
      { type: "facility", label: "Bar", tone: "long" },
      { type: "facility", label: "Kitchen", tone: "long" },
      { type: "facility", label: "Staff", tone: "large" },
      { type: "divider" },
      ...openRowSeats(rowSeats[0], "Class 2"),
    ],
    [
      { type: "space", size: "wide" },
      { type: "corridor", label: "WARS" },
      { type: "space", size: "wide" },
    ],
    [
      { type: "facility", label: "Dining", tone: "long" },
      { type: "facility", label: "WC", tone: "large" },
      { type: "divider" },
      ...openRowSeats(rowSeats[1], "Class 2"),
    ],
  ];
}

function buildDartSecondCabTemplate(seatCount: number): SeatPlanSlot[][] {
  return buildOpenTemplate(Math.max(seatCount, 76), "2", "Class 2", {
    start: [{ type: "facility", label: "Cab", tone: "large" }],
    end: [{ type: "facility", label: "WC", tone: "large" }],
  });
}

function buildOpenTemplate(
  seatCount: number,
  classLabel: string,
  classType: SeatClassType,
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

function openRowSeats(count: number, classType: SeatClassType): SeatPlanSlot[] {
  return Array.from({ length: count }).flatMap((_, index) => [
    { type: "seat", classType } as SeatPlanSlot,
    ...(index % 6 === 5 ? [{ type: "space" } as SeatPlanSlot] : []),
  ]);
}

function buildCompartmentTemplate(
  compartmentCount: number,
  classLabel: string,
  classType: SeatClassType,
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
  classType?: SeatClassType,
) {
  const classNumber = classType?.includes("1") ? "1" : classType?.includes("2") ? "2" : "";
  const accommodationClass = classType === "Sleeper" || classType === "Couchette" ? classType.toLowerCase() : "";
  const matchingIndex = seats.findIndex((seat) => {
    if (usedSeatIds.has(seat.seatId)) {
      return false;
    }

    const normalizedSeatClass = seat.classType.toLowerCase();
    if (accommodationClass) {
      return normalizedSeatClass === accommodationClass;
    }

    return !classNumber || normalizedSeatClass.includes(classNumber);
  });
  const fallbackIndex = seats.findIndex((seat) => !usedSeatIds.has(seat.seatId));
  const seat = seats[matchingIndex >= 0 ? matchingIndex : fallbackIndex] ?? null;

  if (seat) {
    usedSeatIds.add(seat.seatId);
  }

  return seat;
}

function numberRange(start: number, end: number) {
  return Array.from({ length: end - start + 1 }, (_, index) => String(start + index));
}

function graphicSplitRow(numbers: string[], x: number, y: number): GraphicSeatSlot[] {
  return numbers.map((number, index) => ({
    number,
    x: x + index * 54 + (index >= 6 ? 54 : 0),
    y,
  }));
}

function graphicCompartment(numbers: string[], x: number, y: number): GraphicSeatSlot[] {
  const positions = [
    { x, y },
    { x: x + 54, y },
    { x, y: y + 54 },
    { x: x + 54, y: y + 54 },
  ];

  return numbers.slice(0, 4).map((number, index) => ({
    number,
    x: positions[index].x,
    y: positions[index].y,
  }));
}

function buildFamilyOpenGraphicSeats(): GraphicSeatSlot[] {
  return [
    ...graphicCompartment(numberRange(1, 4), 190, 144),
    ...graphicCompartment(numberRange(5, 8), 312, 144),
    ...graphicCompartment(numberRange(9, 12), 434, 144),
    ...graphicSplitRow(numberRange(13, 23), 640, 74),
    ...graphicSplitRow(numberRange(24, 34), 640, 130),
    ...graphicSplitRow(numberRange(35, 45), 640, 250),
    ...graphicSplitRow(numberRange(46, 56), 640, 306),
  ];
}

const familyOpenFacilities: GraphicFacilitySlot[] = [
  { label: "WC", x: 26, y: 72, width: 82, height: 48 },
  { label: "Cabin", x: 26, y: 184, width: 82, height: 48 },
  { label: "Bag", x: 26, y: 298, width: 72, height: 48 },
  { label: "Family", x: 556, y: 126, width: 94, height: 48 },
  { label: "WC", x: 1214, y: 72, width: 82, height: 48 },
  { label: "WC", x: 1214, y: 130, width: 82, height: 48 },
];

const familyOpenCompartments: GraphicCompartmentSlot[] = [
  { x: 180, y: 128 },
  { x: 302, y: 128 },
  { x: 424, y: 128 },
];

function CarriageSeatMap({
  coach,
  selectedClass,
  selectedSeat,
  selectedSeats = [],
  seats,
  template,
  isSeatSelectable,
  onSelectSeat,
}: CarriageSeatMapProps) {
  if (template === "emu-second-family-open") {
    return (
      <GraphicFamilyOpenCoach
        coach={coach}
        selectedClass={selectedClass}
        selectedSeat={selectedSeat}
        selectedSeats={selectedSeats}
        seats={seats}
        template={template}
        isSeatSelectable={isSeatSelectable}
        onSelectSeat={onSelectSeat}
      />
    );
  }

  const rows = buildRows(template, seats);

  return (
    <div className={`real-coach-layout real-coach-${template}`} aria-label={`Car ${coach} seat map`}>
      <div className="coach-shell">
        {rows.map((row, rowIndex) => (
          <div className="coach-plan-row" key={`${template}-${rowIndex}`}>
            {row.map((slot, slotIndex) => renderSlot(slot, coach, slotIndex, selectedSeat, selectedSeats, isSeatSelectable, onSelectSeat))}
          </div>
        ))}
      </div>
      <p className="coach-caption">
        Car {coach} - {getAccommodationLabel(template, selectedClass)} - {templateLabel(template)}
      </p>
    </div>
  );
}

function GraphicFamilyOpenCoach({
  coach,
  selectedClass,
  selectedSeat,
  selectedSeats = [],
  seats,
  template,
  isSeatSelectable,
  onSelectSeat,
}: CarriageSeatMapProps) {
  const seatsByNumber = new Map(seats.map((seat) => [seat.number.trim(), seat]));
  const slots = buildFamilyOpenGraphicSeats();

  return (
    <div className={`real-coach-layout real-coach-${template} graphic-coach-layout`} aria-label={`Car ${coach} seat map`}>
      <div className="graphic-coach-shell">
        <div className="graphic-coach-canvas">
          <svg className="graphic-coach-lines" viewBox="0 0 1320 392" aria-hidden="true">
            <line className="graphic-coach-outline" x1="0" y1="24" x2="1300" y2="24" />
            <line className="graphic-coach-outline" x1="0" y1="368" x2="1300" y2="368" />
            <path className="graphic-corridor-band" d="M 0 220 H 150 V 70 H 608 V 220 H 1300" />
            <path className="graphic-corridor-spine" d="M 0 220 H 150 V 70 H 608 V 220 H 1300" />
            <line className="graphic-transition-line" x1="150" y1="62" x2="150" y2="340" />
            <line className="graphic-transition-line" x1="608" y1="62" x2="608" y2="340" />
            <line className="graphic-transition-line" x1="1168" y1="62" x2="1168" y2="178" />
            <line className="graphic-transition-line" x1="1168" y1="250" x2="1168" y2="340" />
            <line className="graphic-open-row-guide" x1="626" y1="194" x2="1156" y2="194" />
            <line className="graphic-open-row-guide" x1="626" y1="348" x2="1156" y2="348" />
          </svg>

          {familyOpenCompartments.map((slot, index) => (
            <span className="graphic-compartment-box" style={{ left: slot.x, top: slot.y }} key={`family-compartment-${index}`} />
          ))}

          {familyOpenFacilities.map((facility) => (
            <span
              className="graphic-facility"
              style={{ left: facility.x, top: facility.y, width: facility.width, height: facility.height }}
              key={`${facility.label}-${facility.x}-${facility.y}`}
            >
              {facility.label}
            </span>
          ))}

          <span className="graphic-class-marker" style={{ left: 878, top: 194 }}>
            2
          </span>

          {slots.map((slot) =>
            renderGraphicSeatCell(
              seatsByNumber.get(slot.number) ?? null,
              slot,
              selectedSeat,
              selectedSeats,
              isSeatSelectable,
              onSelectSeat,
              `${coach}-graphic-seat-${slot.number}`,
            ),
          )}
        </div>
      </div>
      <p className="coach-caption">
        Car {coach} - {getAccommodationLabel(template, selectedClass)} - {templateLabel(template)}
      </p>
    </div>
  );
}

function renderSlot(
  slot: PlannedSlot,
  coach: string,
  slotIndex: number,
  selectedSeat: TripSeatAvailability | null,
  selectedSeats: TripSeatAvailability[],
  isSeatSelectable: ((seat: TripSeatAvailability) => boolean) | undefined,
  onSelectSeat: (seat: TripSeatAvailability) => void,
) {
  if (slot.type === "seat") {
    return renderSeatCell(slot.seat, selectedSeat, selectedSeats, isSeatSelectable, onSelectSeat, `${coach}-seat-${slot.seat?.seatId ?? slotIndex}`);
  }

  if (slot.type === "compartment") {
    return (
      <span className={`coach-compartment ${slot.classType?.includes("1") ? "coach-compartment-first" : ""}`} key={`${coach}-compartment-${slotIndex}`}>
        {slot.seats.map((seat, seatIndex) =>
          renderSeatCell(
            seat,
            selectedSeat,
            selectedSeats,
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
      <span className={`coach-corridor ${slot.size === "short" ? "coach-corridor-short" : ""}`} key={`${coach}-corridor-${slotIndex}`}>
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

function renderGraphicSeatCell(
  seat: TripSeatAvailability | null,
  slot: GraphicSeatSlot,
  selectedSeat: TripSeatAvailability | null,
  selectedSeats: TripSeatAvailability[],
  isSeatSelectable: ((seat: TripSeatAvailability) => boolean) | undefined,
  onSelectSeat: (seat: TripSeatAvailability) => void,
  key: string,
) {
  const isSelected = isSeatSelected(seat, selectedSeat, selectedSeats);
  const canSelect = Boolean(seat?.isAvailable && (!isSeatSelectable || isSeatSelectable(seat)));
  const canClick = canSelect || isSelected;

  return (
    <button
      type="button"
      className={`graphic-seat-cell seat-cell ${canSelect ? "seat-available" : "seat-unavailable"} ${isSelected ? "seat-selected" : ""}`}
      style={{ left: slot.x, top: slot.y }}
      disabled={!canClick}
      onClick={() => canClick && seat && onSelectSeat(seat)}
      key={key}
    >
      {seat?.number ?? slot.number}
    </button>
  );
}

function renderSeatCell(
  seat: TripSeatAvailability | null,
  selectedSeat: TripSeatAvailability | null,
  selectedSeats: TripSeatAvailability[],
  isSeatSelectable: ((seat: TripSeatAvailability) => boolean) | undefined,
  onSelectSeat: (seat: TripSeatAvailability) => void,
  key: string,
) {
  const isSelected = isSeatSelected(seat, selectedSeat, selectedSeats);
  const canSelect = Boolean(seat?.isAvailable && (!isSeatSelectable || isSeatSelectable(seat)));
  const canClick = canSelect || isSelected;

  return (
    <button
      type="button"
      className={`seat-cell ${canSelect ? "seat-available" : "seat-unavailable"} ${isSelected ? "seat-selected" : ""}`}
      disabled={!canClick}
      onClick={() => canClick && seat && onSelectSeat(seat)}
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

  if (template === "emu-dart-first-cab") {
    return "first class cab unit";
  }

  if (template === "emu-dart-first-accessible") {
    return "first class accessible unit";
  }

  if (template === "emu-dart-restaurant") {
    return "restaurant unit with passenger seating";
  }

  if (template === "emu-dart-second-open") {
    return "second class open-space unit";
  }

  if (template === "emu-dart-second-cab") {
    return "second class cab unit";
  }

  if (template === "first-compartment") {
    return "9-compartment first class coach";
  }

  if (template === "second-compartment") {
    return "10-compartment second class coach";
  }

  if (template === "international-sleeper") {
    return "international sleeper with deluxe shower compartments";
  }

  if (template === "domestic-sleeper") {
    return "domestic sleeper with 10 triple-berth compartments";
  }

  if (template === "four-berth-couchette") {
    return "4-berth couchette with accessible compartment";
  }

  if (template === "six-berth-couchette") {
    return "6-berth couchette with accessible compartment";
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

function getAccommodationLabel(template: CarriageTemplate, selectedClass: string) {
  if (template === "international-sleeper" || template === "domestic-sleeper") {
    return "Sleeper";
  }

  if (template === "four-berth-couchette" || template === "six-berth-couchette") {
    return "Couchette";
  }

  return `Class ${selectedClass}`;
}

export type { CarriageTemplate };
export default CarriageSeatMap;
