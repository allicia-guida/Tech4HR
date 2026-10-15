import { apiRequest } from "@/services/api/http-client";
import { AttendanceRepository } from "./attendance-repository";
import {
  ApiTimeEntryType,
  AttendanceLocation,
  TimeEntry,
  TimeEntryType,
} from "../types/time-entry";
import { dateKey } from "@/utils/date";

type ApiLocation = {
  latitude: number;
  longitude: number;
  accuracyMeters: number;
};

type ApiTimeEntry = {
  id: string;
  type: ApiTimeEntryType;
  recordedAtUtc: string;
  location: ApiLocation | null;
  deviceReference?: string | null;
};

const apiToAppType: Record<ApiTimeEntryType, TimeEntryType> = {
  ENTRADA: TimeEntryType.CLOCK_IN,
  INICIO_INTERVALO: TimeEntryType.BREAK_START,
  FIM_INTERVALO: TimeEntryType.BREAK_END,
  SAIDA: TimeEntryType.CLOCK_OUT,
};

const sequence: ApiTimeEntryType[] = [
  "ENTRADA",
  "INICIO_INTERVALO",
  "FIM_INTERVALO",
  "SAIDA",
];

const toEntry = (entry: ApiTimeEntry): TimeEntry => ({
  id: entry.id,
  employeeId: "current",
  type: apiToAppType[entry.type],
  apiType: entry.type,
  timestamp: entry.recordedAtUtc,
  source: "APP_SMARTPHONE",
  deviceId: entry.deviceReference ?? "server",
  createdAt: entry.recordedAtUtc,
  latitude: entry.location?.latitude,
  longitude: entry.location?.longitude,
  accuracyMeters: entry.location?.accuracyMeters,
  locationCapturedAt: entry.recordedAtUtc,
});

const list = async () => {
  const entries = await apiRequest<ApiTimeEntry[]>(
    "/api/v1/points/history",
  );
  return entries
    .map(toEntry)
    .sort(
      (a, b) =>
        new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime(),
    );
};

export const apiAttendanceRepository: AttendanceRepository = {
  list,
  create: async (type: TimeEntryType, location: AttendanceLocation) => {
    const entries = await list();
    const today = dateKey();
    const todayCount = entries.filter(
      (entry) => dateKey(entry.timestamp) === today,
    ).length;
    const apiType = sequence[todayCount];
    if (!apiType || apiToAppType[apiType] !== type)
      throw new Error("INVALID_TIME_ENTRY_SEQUENCE");
    const response = await apiRequest<ApiTimeEntry>("/api/v1/points", {
      method: "POST",
      body: JSON.stringify({
        type: apiType,
        latitude: location.latitude,
        longitude: location.longitude,
        accuracyMeters: location.accuracyMeters,
        deviceReference: "APP_SMARTPHONE",
        idempotencyKey: `${apiType}-${location.capturedAt}`,
      }),
    });
    return toEntry(response);
  },
};
