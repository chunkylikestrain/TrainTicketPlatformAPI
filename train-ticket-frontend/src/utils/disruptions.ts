type DisruptionLike = {
  hasDisruption?: boolean;
  hasPlatformChange?: boolean;
  delayMinutes?: number;
  status?: string;
  platform?: string;
  track?: string;
  originalPlatform?: string;
  originalTrack?: string;
  disruptionMessage?: string;
  disruptionSeverity?: string;
  cancellationReason?: string | null;
  tripCancellationReason?: string | null;
};

export function hasDisruption(item: DisruptionLike) {
  return Boolean(
    item.hasDisruption ||
      item.delayMinutes ||
      item.hasPlatformChange ||
      item.disruptionMessage ||
      item.cancellationReason ||
      item.tripCancellationReason ||
      (item.status && item.status !== "Scheduled" && item.status !== "On time"),
  );
}

export function getDisruptionSeverity(item: DisruptionLike) {
  if (item.disruptionSeverity) {
    return item.disruptionSeverity.toLowerCase();
  }

  if (item.status?.toLowerCase() === "cancelled") {
    return "critical";
  }

  if ((item.delayMinutes ?? 0) >= 30) {
    return "major";
  }

  return hasDisruption(item) ? "notice" : "";
}

export function getDisruptionMessage(item: DisruptionLike) {
  if (item.disruptionMessage) {
    return item.disruptionMessage;
  }

  if (item.status?.toLowerCase() === "cancelled") {
    const reason = item.cancellationReason || item.tripCancellationReason;
    return reason ? `This train has been cancelled: ${reason}` : "This train has been cancelled.";
  }

  if ((item.delayMinutes ?? 0) > 0) {
    return `This train is delayed by ${item.delayMinutes} minutes.`;
  }

  if (item.hasPlatformChange) {
    return `Platform changed. Please use platform ${item.platform || "-"}, track ${item.track || "-"}.`;
  }

  return "";
}
