import type { BayAvailabilityDto } from "../../lib/api";
import { StatusBadge } from "./StatusBadge";

type Props = {
  bays: BayAvailabilityDto[];
  selectedBayId?: string | null;
  onSelect: (bay: BayAvailabilityDto) => void;
};

function getBayState(bay: BayAvailabilityDto) {
  if (bay.isUnderMaintenance) {
    return { label: "Maintenance", tone: "danger", reason: "Unavailable for operations work." } as const;
  }

  if (bay.isOccupied) {
    return { label: "Occupied", tone: "warning", reason: bay.occupiedByLicensePlate ? `Occupied by ${bay.occupiedByLicensePlate}` : "Vehicle already present." } as const;
  }

  if (bay.isReserved) {
    return { label: "Reserved", tone: "warning", reason: "Already reserved for this time window." } as const;
  }

  return { label: "Available", tone: "success", reason: "Ready to book." } as const;
}

export function BayMap({ bays, selectedBayId, onSelect }: Props) {
  return (
    <div className="bay-map" role="list">
      {bays.map((bay) => {
        const state = getBayState(bay);

        return (
          <button
            key={bay.bayId}
            aria-pressed={selectedBayId === bay.bayId}
            className={`bay-tile bay-${state.label.toLowerCase()} ${selectedBayId === bay.bayId ? "selected" : ""}`}
            disabled={!bay.isAvailable}
            onClick={() => onSelect(bay)}
            type="button"
          >
            <div className="bay-tile-head">
              <strong>{bay.bayNumber}</strong>
              <StatusBadge tone={state.tone}>{state.label}</StatusBadge>
            </div>
            <span className="bay-tile-type">{bay.bayType}</span>
            <small>{state.reason}</small>
          </button>
        );
      })}
    </div>
  );
}

