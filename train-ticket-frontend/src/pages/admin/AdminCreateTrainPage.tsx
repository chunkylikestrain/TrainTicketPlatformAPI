import {
  ArrowDownOutlined,
  ArrowLeftOutlined,
  ArrowUpOutlined,
  CheckCircleOutlined,
  CloseOutlined,
  PlusOutlined,
  ProfileOutlined,
  SaveOutlined,
} from "@ant-design/icons";
import axios from "axios";
import { useEffect, useMemo, useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { createAdminTrain, getAdminRollingStockOptions, getAdminTrain, updateAdminTrain } from "../../api/adminApi";
import AdminLayout from "../../components/AdminLayout";
import type { AdminRollingStockOption, AdminTrain, AdminTrainCarriage } from "../../types/admin";

type TrainCarriageForm = Omit<AdminTrainCarriage, "id" | "seatCount"> & {
  id?: number;
  seatCount: string;
};

type CarriagePreset = {
  label: string;
  classType: string;
  layoutType: string;
  vehicleType: string;
  seatCount: string;
  hasBikeSpace?: boolean;
  hasAccessibleSpace?: boolean;
  hasFamilyCompartment?: boolean;
  hasDiningSection?: boolean;
  notes?: string;
};

type EmuPreset = {
  label: string;
  model: string;
  codePrefix: string;
  carriages: CarriagePreset[];
};

const locomotiveOptions = [
  "EP05",
  "EU07",
  "EP07",
  "EU07A",
  "EP08",
  "EP09",
  "EU44 Husarz",
  "EU160 Griffin",
  "EU200 Griffin",
  "SM42",
  "SM42 6D",
  "SM42 18D",
  "SU42",
  "SU160 Gama",
  "SM60",
];

const carriagePresets: CarriagePreset[] = [
  {
    label: "1st class compartment",
    classType: "Class 1",
    layoutType: "FirstCompartment",
    vehicleType: "A9nouz",
    seatCount: "54",
    notes: "9 compartments, 6 seats each",
  },
  {
    label: "1st class open-space",
    classType: "Class 1",
    layoutType: "OpenFirst",
    vehicleType: "152A-Pesa A9mnopuz",
    seatCount: "54",
    notes: "First-class open-space coach",
  },
  {
    label: "2nd class open-space",
    classType: "Class 2",
    layoutType: "OpenSecond",
    vehicleType: "B9nopuvz",
    seatCount: "88",
    notes: "Open coach with 2+2 seating",
  },
  {
    label: "2nd class open-space accessible",
    classType: "Class 2",
    layoutType: "OpenSecondAccessible",
    vehicleType: "111Ainw B8bnopuz",
    seatCount: "82",
    hasAccessibleSpace: true,
    notes: "Open coach with wheelchair places and accessible toilet",
  },
  {
    label: "2nd class open-space bicycle",
    classType: "Class 2",
    layoutType: "OpenSecondBike",
    vehicleType: "111Arow B7nopuvz",
    seatCount: "72",
    hasBikeSpace: true,
    notes: "Open coach with bicycle racks",
  },
  {
    label: "2nd class compartment",
    classType: "Class 2",
    layoutType: "SecondCompartment",
    vehicleType: "B10nouz",
    seatCount: "66",
    notes: "10 compartments",
  },
  {
    label: "Mixed 1st/2nd compartment",
    classType: "Class 1/2",
    layoutType: "ComboFirstSecond",
    vehicleType: "112At AB9nou-v2",
    seatCount: "64",
    notes: "Mixed first and second class coach; second-class compartments use 8 seats",
  },
  {
    label: "Combo accessible/bike",
    classType: "Class 2",
    layoutType: "ComboAccessible",
    vehicleType: "B7bnopuz",
    seatCount: "56",
    hasBikeSpace: true,
    hasAccessibleSpace: true,
    hasFamilyCompartment: true,
    notes: "Accessible, bicycle, and family spaces",
  },
  {
    label: "Combo wheelchair/bike/open",
    classType: "Class 2",
    layoutType: "ComboSecondWheelchairBike",
    vehicleType: "111A-30 B6bnouvz",
    seatCount: "64",
    hasBikeSpace: true,
    hasAccessibleSpace: true,
    hasFamilyCompartment: true,
    notes: "Wheelchair spaces, open seats, compartment section, and bike racks",
  },
  {
    label: "Restaurant car",
    classType: "Dining",
    layoutType: "Restaurant",
    vehicleType: "WRnouz",
    seatCount: "0",
    hasDiningSection: true,
    notes: "Restaurant car, no bookable seats",
  },
];

const emuPresets: EmuPreset[] = [
  {
    label: "ED74 Bydgostia - 4-unit set",
    model: "ED74 Bydgostia",
    codePrefix: "ED74",
    carriages: [
      { label: "Unit A", classType: "Class 1/2", layoutType: "EmuFirstSecond", vehicleType: "ED74 unit A", seatCount: "72", notes: "Fixed end unit with first and second class sections" },
      { label: "Unit B", classType: "Class 2", layoutType: "EmuSecondOpen", vehicleType: "ED74 unit B", seatCount: "88", notes: "Fixed second-class open unit" },
      { label: "Unit C", classType: "Class 2", layoutType: "EmuSecondOpen", vehicleType: "ED74 unit C", seatCount: "88", notes: "Fixed second-class open unit" },
      { label: "Unit D", classType: "Class 2", layoutType: "OpenSecondAccessible", vehicleType: "ED74 unit D", seatCount: "64", hasAccessibleSpace: true, notes: "Fixed accessible end unit" },
    ],
  },
  {
    label: "ED160 FLIRT3 - 8-unit set",
    model: "ED160 FLIRT3",
    codePrefix: "ED160",
    carriages: [
      { label: "Unit 1", classType: "Class 1/2", layoutType: "EmuFirstSecond", vehicleType: "ED160 unit 1", seatCount: "64", notes: "Fixed mixed-class end unit" },
      ...Array.from({ length: 5 }, (_, index) => ({ label: `Unit ${index + 2}`, classType: "Class 2", layoutType: "EmuSecondOpen", vehicleType: `ED160 unit ${index + 2}`, seatCount: "88", notes: "Fixed second-class open unit" })),
      { label: "Unit 7", classType: "Class 2", layoutType: "OpenSecondBike", vehicleType: "ED160 unit 7", seatCount: "72", hasBikeSpace: true, notes: "Fixed bicycle unit" },
      { label: "Unit 8", classType: "Class 2", layoutType: "OpenSecondAccessible", vehicleType: "ED160 unit 8", seatCount: "64", hasAccessibleSpace: true, notes: "Fixed accessible end unit" },
    ],
  },
  {
    label: "ED250 Pendolino - 7-unit EIP set",
    model: "ED250 Pendolino",
    codePrefix: "EIP",
    carriages: [
      { label: "Unit 1", classType: "Class 1", layoutType: "OpenFirst", vehicleType: "ED250-1 first class cab unit", seatCount: "54", notes: "Fixed first-class Pendolino unit" },
      { label: "Unit 2", classType: "Class 2", layoutType: "EmuSecondFamilyOpen", vehicleType: "ED250-2 family and open second class", seatCount: "98", hasFamilyCompartment: true, notes: "Second-class unit with family compartment and open-space seating" },
      { label: "Unit 3", classType: "Class 2", layoutType: "EmuDiningAccessible", vehicleType: "ED250-3 accessible dining unit", seatCount: "12", hasAccessibleSpace: true, hasDiningSection: true, notes: "Accessible WARS dining unit with wheelchair spaces" },
      ...Array.from({ length: 3 }, (_, index) => ({ label: `Unit ${index + 4}`, classType: "Class 2", layoutType: "EmuSecondOpen", vehicleType: `ED250-${index + 4} second class open unit`, seatCount: "88", notes: "Fixed second-class open unit" })),
      { label: "Unit 7", classType: "Class 2", layoutType: "EmuSecondQuiet", vehicleType: "ED250-7 quiet second class cab unit", seatCount: "88", notes: "Dedicated quiet second-class end unit, not an accessible coach" },
    ],
  },
];

const premiumTrainType = "Express InterCity Premium";

function AdminCreateTrainPage() {
  const navigate = useNavigate();
  const { trainId } = useParams();
  const editingTrainId = Number(trainId);
  const isEditing = Number.isInteger(editingTrainId) && editingTrainId > 0;
  const [code, setCode] = useState("IC-");
  const [name, setName] = useState("");
  const [locomotive, setLocomotive] = useState("EU160");
  const [trainType, setTrainType] = useState("InterCity");
  const [emuModel, setEmuModel] = useState(emuPresets[0].label);
  const [status, setStatus] = useState("Active");
  const [presetLabel, setPresetLabel] = useState(carriagePresets[1].label);
  const [carriages, setCarriages] = useState<TrainCarriageForm[]>([
    createCarriageFromPreset(carriagePresets[0], "11", 1),
    createCarriageFromPreset(carriagePresets[1], "12", 2),
    createCarriageFromPreset(carriagePresets[3], "13", 3),
    createCarriageFromPreset(carriagePresets[4], "14", 4),
  ]);
  const [saved, setSaved] = useState(false);
  const [loadError, setLoadError] = useState("");
  const [saveError, setSaveError] = useState("");
  const [rollingStockOptions, setRollingStockOptions] = useState<AdminRollingStockOption[]>([]);

  useEffect(() => {
    getAdminRollingStockOptions()
      .then(setRollingStockOptions)
      .catch(() => setRollingStockOptions([]));
  }, []);

  useEffect(() => {
    if (!isEditing)
      return;

    async function loadTrain() {
      try {
        const train = await getAdminTrain(editingTrainId);
        setCode(train.code || "");
        setName(train.name || "");
        setLocomotive(train.locomotive || "");
        const loadedTrainType = train.type || "InterCity";
        setTrainType(loadedTrainType);
        if (loadedTrainType === premiumTrainType || train.locomotive === "ED250 Pendolino") {
          setEmuModel("ED250 Pendolino - 7-unit EIP set");
        } else if (loadedTrainType === "EMU") {
          const matchingPreset = emuPresets.find((preset) => preset.model === train.locomotive);
          setEmuModel(matchingPreset?.label ?? emuPresets[0].label);
        }
        setStatus(train.status || "Active");
        setCarriages(toCarriageForms(train));
      } catch {
        setLoadError("Could not load train details from the API.");
      }
    }

    loadTrain();
  }, [editingTrainId, isEditing]);

  const passengerCarriages = carriages.filter((carriage) => carriage.layoutType !== "Restaurant");
  const isEmu = trainType === "EMU";
  const isPremium = trainType === premiumTrainType;
  const isFixedSet = isEmu || isPremium;
  const totalSeats = passengerCarriages.reduce((sum, carriage) => sum + Number(carriage.seatCount || 0), 0);
  const selectedPreset = useMemo(
    () => carriagePresets.find((preset) => preset.label === presetLabel) ?? carriagePresets[1],
    [presetLabel]);
  const selectedEmuPreset = useMemo(
    () => emuPresets.find((preset) => preset.label === emuModel) ?? emuPresets[0],
    [emuModel]);
  const ed250Preset = useMemo(
    () => emuPresets.find((preset) => preset.model === "ED250 Pendolino") ?? emuPresets[2],
    []);
  const rollingStockDatalistOptions = useMemo(() => {
    const apiOptions = rollingStockOptions
      .filter((option) => option.status === "Active")
      .map((option) => option.displayName);

    return Array.from(new Set([
      ...apiOptions,
      ...locomotiveOptions,
      ...emuPresets.map((preset) => preset.model),
    ])).sort((first, second) => first.localeCompare(second));
  }, [rollingStockOptions]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSaveError("");
    setSaved(false);

    const duplicateCoach = findDuplicateCoach(carriages);
    if (duplicateCoach) {
      setSaveError(`Coach ${duplicateCoach} is used more than once.`);
      return;
    }

    const payload = toAdminTrainPayload({
      id: editingTrainId,
      code,
      name,
      locomotive,
      trainType,
      status,
      carriages,
    });

    try {
      if (isEditing) {
        await updateAdminTrain(payload);
      } else {
        await createAdminTrain(payload);
      }

      setSaved(true);
      window.setTimeout(() => navigate("/admin/trains"), 900);
    } catch (error) {
      setSaveError(getTrainSaveError(error));
    }
  }

  function addCarriage() {
    if (isFixedSet)
      return;

    setCarriages((current) => [
      ...current,
      createCarriageFromPreset(selectedPreset, nextCoachNumber(current), current.length + 1),
    ]);
  }

  function updateCarriage(index: number, patch: Partial<TrainCarriageForm>) {
    setCarriages((current) => current.map((carriage, currentIndex) =>
      currentIndex === index ? { ...carriage, ...patch } : carriage));
  }

  function moveCarriage(index: number, direction: -1 | 1) {
    if (isFixedSet)
      return;

    const nextIndex = index + direction;
    if (nextIndex < 0 || nextIndex >= carriages.length)
      return;

    setCarriages((current) => {
      const next = [...current];
      const [selected] = next.splice(index, 1);
      next.splice(nextIndex, 0, selected);
      return next.map((carriage, currentIndex) => ({ ...carriage, position: currentIndex + 1 }));
    });
  }

  function removeCarriage(index: number) {
    if (isFixedSet)
      return;

    setCarriages((current) =>
      current
        .filter((_, currentIndex) => currentIndex !== index)
        .map((carriage, currentIndex) => ({ ...carriage, position: currentIndex + 1 })));
  }

  function applyEmuPreset(preset = selectedEmuPreset, nextTrainType = preset.model === "ED250 Pendolino" ? premiumTrainType : "EMU") {
    setTrainType(nextTrainType);
    setLocomotive(preset.model);
    if (!code || code === "IC-") {
      setCode(`${preset.codePrefix}-`);
    }
    setCarriages(preset.carriages.map((carriage, index) =>
      createCarriageFromPreset(carriage, String(index + 1), index + 1)));
  }

  function handleTrainTypeChange(value: string) {
    setTrainType(value);
    if (value === "EMU") {
      const genericPreset = selectedEmuPreset.model === "ED250 Pendolino" ? emuPresets[0] : selectedEmuPreset;
      setEmuModel(genericPreset.label);
      applyEmuPreset(genericPreset, "EMU");
    }
    if (value === premiumTrainType) {
      setEmuModel(ed250Preset.label);
      applyEmuPreset(ed250Preset, premiumTrainType);
    }
  }

  return (
    <AdminLayout>
      <section className="admin-page-heading">
        <div>
          <Link className="admin-back-link" to="/admin/trains"><ArrowLeftOutlined /> Back to trains</Link>
          <h1><ProfileOutlined /> {isEditing ? "Edit Train" : "New Train"}</h1>
          <p>{isEditing ? "Adjust locomotive, car order, and coach details." : "Build a physical train consist before assigning it to schedules."}</p>
        </div>
      </section>

      {loadError && <div className="admin-save-banner">{loadError}</div>}
      {saveError && <div className="admin-save-banner admin-danger-panel">{saveError}</div>}
      {saved && (
        <div className="admin-save-banner">
          <CheckCircleOutlined /> Train saved.
        </div>
      )}

      <section className="admin-form-layout">
        <form className="admin-detail-form" onSubmit={handleSubmit}>
          <fieldset>
            <legend>Train identity</legend>
            <label>Train code<input value={code} onChange={(event) => setCode(event.target.value)} required /></label>
            <label>Display name<input value={name} onChange={(event) => setName(event.target.value)} placeholder="Example: Baltic Express" required /></label>
            <label>{isFixedSet ? "Fixed unit set" : "Locomotive"}<input value={locomotive} onChange={(event) => setLocomotive(event.target.value)} list="locomotive-options" placeholder="Example: EU160" disabled={isFixedSet} /></label>
            <datalist id="locomotive-options">
              {rollingStockDatalistOptions.map((option) => <option key={option} value={option} />)}
            </datalist>
            <label>Train type<select value={trainType} onChange={(event) => handleTrainTypeChange(event.target.value)}>
              <option>InterCity</option>
              <option>Express InterCity</option>
              <option>{premiumTrainType}</option>
              <option>EMU</option>
              <option>Regional</option>
              <option>Night train</option>
            </select></label>
            <label>Status<select value={status} onChange={(event) => setStatus(event.target.value)}>
              <option>Active</option>
              <option>Maintenance</option>
              <option>Retired</option>
            </select></label>
          </fieldset>

          <fieldset className="admin-consist-fieldset">
            <legend>Consist</legend>
            <div className="admin-consist-toolbar">
              <label>Car preset<select value={presetLabel} onChange={(event) => setPresetLabel(event.target.value)}>
                {carriagePresets.map((preset) => <option key={preset.label}>{preset.label}</option>)}
              </select></label>
              <button type="button" onClick={addCarriage} disabled={isFixedSet}><PlusOutlined /> Add car</button>
            </div>
            <div className="admin-emu-toolbar">
              <label>Fixed unit set<select value={emuModel} onChange={(event) => {
                const nextPreset = emuPresets.find((preset) => preset.label === event.target.value) ?? emuPresets[0];
                setEmuModel(nextPreset.label);
                if (isFixedSet) {
                  applyEmuPreset(nextPreset);
                }
              }}>
                {emuPresets.map((preset) => <option key={preset.label}>{preset.label}</option>)}
              </select></label>
              <button type="button" onClick={() => applyEmuPreset(selectedEmuPreset)}>Load fixed unit consist</button>
              {isFixedSet && <span>Fixed unit sets are locked to their real consist, so car add/remove/reorder controls are disabled.</span>}
            </div>

            <ol className="admin-carriage-list">
              {carriages.map((carriage, index) => (
                <li key={`${carriage.coach}-${index}`}>
                  <div className="admin-carriage-order">
                    <b>{index + 1}</b>
                    <button type="button" onClick={() => moveCarriage(index, -1)} disabled={isFixedSet || index === 0} aria-label={`Move coach ${carriage.coach} up`}><ArrowUpOutlined /></button>
                    <button type="button" onClick={() => moveCarriage(index, 1)} disabled={isFixedSet || index === carriages.length - 1} aria-label={`Move coach ${carriage.coach} down`}><ArrowDownOutlined /></button>
                    <button type="button" onClick={() => removeCarriage(index)} disabled={isFixedSet} aria-label={`Remove coach ${carriage.coach}`}><CloseOutlined /></button>
                  </div>

                  <div className="admin-carriage-fields">
                    <label>Coach<input value={carriage.coach} onChange={(event) => updateCarriage(index, { coach: event.target.value })} required /></label>
                    <label>Class<select value={carriage.classType} onChange={(event) => updateCarriage(index, { classType: event.target.value })}>
                      <option>Class 1</option>
                      <option>Class 2</option>
                      <option>Class 1/2</option>
                      <option>Dining</option>
                    </select></label>
                    <label>Layout<select value={carriage.layoutType} onChange={(event) => updateCarriage(index, { layoutType: event.target.value, hasDiningSection: event.target.value === "Restaurant" })}>
                      <option>FirstCompartment</option>
                      <option>OpenFirst</option>
                      <option>SecondCompartment</option>
                      <option>OpenSecond</option>
                      <option>OpenSecondAccessible</option>
                      <option>OpenSecondBike</option>
                      <option>ComboAccessible</option>
                      <option>ComboSecondWheelchairBike</option>
                      <option>ComboFirstSecond</option>
                      <option>EmuFirstSecond</option>
                      <option>EmuSecondOpen</option>
                      <option>EmuSecondFamilyOpen</option>
                      <option>EmuDiningAccessible</option>
                      <option>EmuSecondQuiet</option>
                      <option>Restaurant</option>
                    </select></label>
                    <label>Vehicle type<input value={carriage.vehicleType} onChange={(event) => updateCarriage(index, { vehicleType: event.target.value })} placeholder="Example: B9nopuvz" /></label>
                    <label>Bookable seats<input value={carriage.seatCount} onChange={(event) => updateCarriage(index, { seatCount: event.target.value })} min="0" type="number" disabled={carriage.layoutType === "Restaurant"} /></label>
                    <label>Notes<input value={carriage.notes} onChange={(event) => updateCarriage(index, { notes: event.target.value })} /></label>
                  </div>

                  <div className="admin-carriage-flags">
                    <label><input checked={carriage.hasBikeSpace} onChange={(event) => updateCarriage(index, { hasBikeSpace: event.target.checked })} type="checkbox" /> Bike space</label>
                    <label><input checked={carriage.hasAccessibleSpace} onChange={(event) => updateCarriage(index, { hasAccessibleSpace: event.target.checked })} type="checkbox" /> Accessible</label>
                    <label><input checked={carriage.hasFamilyCompartment} onChange={(event) => updateCarriage(index, { hasFamilyCompartment: event.target.checked })} type="checkbox" /> Family</label>
                    <label><input checked={carriage.hasDiningSection} onChange={(event) => updateCarriage(index, { hasDiningSection: event.target.checked })} type="checkbox" /> Dining area</label>
                  </div>
                </li>
              ))}
            </ol>
          </fieldset>

          <div className="admin-form-actions">
            <button className="admin-primary-button" type="submit"><SaveOutlined /> Save train</button>
            <Link className="admin-secondary-button" to="/admin/trains">Cancel</Link>
          </div>
        </form>

        <aside className="admin-preview-card">
          <span className="admin-preview-icon"><ProfileOutlined /></span>
          <small>Preview</small>
          <h2>{code || "Train code"}</h2>
          <p>{name || "Train name"}</p>
          <ol className="admin-preview-stops">
            {locomotive && <li>Locomotive: {locomotive}</li>}
            {carriages.map((carriage) => (
              <li key={`${carriage.coach}-${carriage.position}`}>
                {carriage.coach} - {carriage.layoutType} ({carriage.seatCount || 0} seats)
              </li>
            ))}
          </ol>
          <dl>
            <div><dt>Type</dt><dd>{trainType}</dd></div>
            <div><dt>Status</dt><dd>{status}</dd></div>
            <div><dt>Cars</dt><dd>{carriages.length}</dd></div>
            <div><dt>Total seats</dt><dd>{totalSeats}</dd></div>
          </dl>
        </aside>
      </section>
    </AdminLayout>
  );
}

function createCarriageFromPreset(preset: CarriagePreset, coach: string, position: number): TrainCarriageForm {
  return {
    coach,
    position,
    classType: preset.classType,
    layoutType: preset.layoutType,
    vehicleType: preset.vehicleType,
    seatCount: preset.seatCount,
    hasBikeSpace: Boolean(preset.hasBikeSpace),
    hasAccessibleSpace: Boolean(preset.hasAccessibleSpace),
    hasFamilyCompartment: Boolean(preset.hasFamilyCompartment),
    hasDiningSection: Boolean(preset.hasDiningSection),
    notes: preset.notes ?? "",
  };
}

function toCarriageForms(train: AdminTrain): TrainCarriageForm[] {
  if (train.carriages?.length) {
    return train.carriages
      .sort((left, right) => left.position - right.position)
      .map((carriage, index) => ({
        ...carriage,
        position: index + 1,
        seatCount: String(carriage.seatCount),
      }));
  }

  return Array.from({ length: Math.max(train.carriageCount, 1) }, (_, index) =>
    createCarriageFromPreset(carriagePresets[1], String(index + 1), index + 1));
}

function nextCoachNumber(carriages: TrainCarriageForm[]) {
  const numbers = carriages.map((carriage) => Number(carriage.coach)).filter(Number.isFinite);
  const next = numbers.length > 0 ? Math.max(...numbers) + 1 : carriages.length + 1;
  return String(next);
}

function findDuplicateCoach(carriages: TrainCarriageForm[]) {
  const seen = new Set<string>();
  for (const carriage of carriages) {
    const coach = carriage.coach.trim().toLowerCase();
    if (!coach)
      continue;

    if (seen.has(coach))
      return carriage.coach.trim();

    seen.add(coach);
  }

  return "";
}

function toAdminTrainPayload(form: {
  id: number;
  code: string;
  name: string;
  locomotive: string;
  trainType: string;
  status: string;
  carriages: TrainCarriageForm[];
}): AdminTrain {
  const normalizedCarriages = form.carriages.map((carriage, index) => {
    const isRestaurantOnly = carriage.layoutType === "Restaurant";
    const hasDiningSection = carriage.hasDiningSection || isRestaurantOnly || carriage.layoutType === "EmuDiningAccessible";
    return {
      id: carriage.id ?? 0,
      coach: carriage.coach.trim(),
      position: index + 1,
      classType: isRestaurantOnly ? "Dining" : carriage.classType,
      layoutType: carriage.layoutType,
      vehicleType: carriage.vehicleType,
      seatCount: isRestaurantOnly ? 0 : Number(carriage.seatCount || 0),
      hasBikeSpace: carriage.hasBikeSpace,
      hasAccessibleSpace: carriage.hasAccessibleSpace,
      hasFamilyCompartment: carriage.hasFamilyCompartment,
      hasDiningSection,
      notes: carriage.notes,
    };
  });

  const passengerSeatCounts = normalizedCarriages
    .filter((carriage) => carriage.layoutType !== "Restaurant")
    .map((carriage) => carriage.seatCount);

  return {
    id: form.id,
    code: form.code.trim(),
    name: form.name.trim(),
    type: form.trainType,
    locomotive: form.locomotive.trim(),
    carriageCount: normalizedCarriages.length,
    seatsPerCarriage: passengerSeatCounts.length > 0 ? Math.max(...passengerSeatCounts) : 0,
    status: form.status,
    departureStation: "",
    arrivalStation: "",
    departureTime: new Date().toISOString(),
    arrivalTime: new Date().toISOString(),
    carriages: normalizedCarriages,
  };
}

function getTrainSaveError(error: unknown) {
  if (!axios.isAxiosError(error))
    return "Train could not be saved. Please try again.";

  if (error.response?.status === 409)
    return typeof error.response.data === "string" ? error.response.data : "A train with this code already exists.";

  if (error.response?.status === 400)
    return typeof error.response.data === "string" ? error.response.data : "Train details are invalid.";

  if (error.response?.status === 401 || error.response?.status === 403)
    return "Your admin session expired. Please log in again.";

  if (error.response)
    return `Train could not be saved. API returned ${error.response.status}.`;

  return "Train could not be saved. Check that the API is running.";
}

export default AdminCreateTrainPage;
