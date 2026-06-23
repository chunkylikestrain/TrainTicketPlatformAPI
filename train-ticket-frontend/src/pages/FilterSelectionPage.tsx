import { useMemo, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import {
  copyPurchasePreferenceParams,
  filterOptions,
  getFilterCodes,
} from "../utils/purchasePreferences";

type FilterSection = {
  key: "seat" | "car" | "changes" | "other" | "transport";
  title: string;
  note?: string;
  icon: string;
};

const sections: FilterSection[] = [
  {
    key: "seat",
    title: "Seat type",
    note: "At least one connection on the route",
    icon: "seat",
  },
  {
    key: "car",
    title: "Car type",
    note: "All connections on the route",
    icon: "car",
  },
  {
    key: "changes",
    title: "Changes",
    icon: "changes",
  },
  {
    key: "other",
    title: "Other",
    icon: "other",
  },
  {
    key: "transport",
    title: "Means of transport",
    icon: "train",
  },
];

function FilterSelectionPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [selectedFilters, setSelectedFilters] = useState<string[]>(() => getFilterCodes(searchParams));
  const returnTo = searchParams.get("returnTo") || "/";
  const optionsBySection = useMemo(() => {
    return sections.reduce<Record<string, typeof filterOptions>>((result, section) => {
      result[section.key] = filterOptions.filter((option) => option.section === section.key);
      return result;
    }, {});
  }, []);

  function toggleFilter(code: string) {
    setSelectedFilters((current) =>
      current.includes(code)
        ? current.filter((item) => item !== code)
        : [...current, code],
    );
  }

  function handleSave() {
    const returnUrl = new URL(returnTo, window.location.origin);
    const nextParams = new URLSearchParams(returnUrl.search);
    copyPurchasePreferenceParams(searchParams, nextParams);

    if (selectedFilters.length > 0) {
      nextParams.set("filters", selectedFilters.join(","));
    } else {
      nextParams.delete("filters");
    }

    navigate(`${returnUrl.pathname}?${nextParams.toString()}`);
  }

  return (
    <main className="filter-page">
      <section className="filter-panel">
        <div className="filter-header">
          <div>
            <span className="filter-header-icon filter-icon-train" aria-hidden="true" />
            <span className="filter-header-icon filter-icon-seat" aria-hidden="true" />
            <span className="filter-header-icon filter-icon-accessible" aria-hidden="true" />
            <h1>Filters</h1>
          </div>
          <Link to={returnTo} aria-label="Close filters">x</Link>
        </div>

        <div className="filter-grid">
          <div className="filter-column">
            {sections.slice(0, 2).map((section) => (
              <FilterGroup
                key={section.key}
                icon={section.icon}
                title={section.title}
                note={section.note}
                options={optionsBySection[section.key] ?? []}
                selectedFilters={selectedFilters}
                onToggle={toggleFilter}
              />
            ))}
          </div>

          <div className="filter-column">
            {sections.slice(2).map((section) => (
              <FilterGroup
                key={section.key}
                icon={section.icon}
                title={section.title}
                note={section.note}
                options={optionsBySection[section.key] ?? []}
                selectedFilters={selectedFilters}
                onToggle={toggleFilter}
              />
            ))}
          </div>
        </div>

        <button className="filter-save-button" type="button" onClick={handleSave}>
          Save
        </button>
      </section>
    </main>
  );
}

type FilterGroupProps = {
  icon: string;
  title: string;
  note?: string;
  options: typeof filterOptions;
  selectedFilters: string[];
  onToggle: (code: string) => void;
};

function FilterGroup({ icon, title, note, options, selectedFilters, onToggle }: FilterGroupProps) {
  return (
    <section className="filter-group">
      <h2>
        <span className={`filter-section-icon filter-icon-${icon}`} aria-hidden="true" />
        {title}
      </h2>
      {note && <p>{note}</p>}
      <div className="filter-option-list">
        {options.map((option) => (
          <label className="filter-option" key={option.code}>
            <input
              checked={selectedFilters.includes(option.code)}
              onChange={() => onToggle(option.code)}
              type="checkbox"
            />
            <span>{option.label}</span>
          </label>
        ))}
      </div>
    </section>
  );
}

export default FilterSelectionPage;
