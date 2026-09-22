import { dateKey } from "@/utils/date";

export enum TimeEntryType {
  CLOCK_IN = "CLOCK_IN",
  BREAK_START = "BREAK_START",
  BREAK_END = "BREAK_END",
  CLOCK_OUT = "CLOCK_OUT",
}

export type ApiTimeEntryType =
  | "ENTRADA"
  | "INICIO_INTERVALO"
  | "FIM_INTERVALO"
  | "SAIDA";

export type TimeEntry = {
  id: string;
  employeeId: string;
  type: TimeEntryType;
  timestamp: string;
  source: "APP_SMARTPHONE";
  deviceId: string;
  latitude?: number;
  longitude?: number;
  accuracyMeters?: number;
  locationCapturedAt?: string;
  createdAt: string;
  apiType?: ApiTimeEntryType;
};

export type AttendanceLocation = {
  latitude: number;
  longitude: number;
  accuracyMeters: number;
  capturedAt: string;
  mocked: boolean;
};

export const timeEntryLabel: Record<TimeEntryType, string> = {
  [TimeEntryType.CLOCK_IN]: "Entrada",
  [TimeEntryType.BREAK_START]: "Início do intervalo",
  [TimeEntryType.BREAK_END]: "Retorno do intervalo",
  [TimeEntryType.CLOCK_OUT]: "Saída",
};

export const nextTimeEntryType = (entries: TimeEntry[]) => {
  const today = dateKey();
  const count = entries.filter((entry) => {
    const entryDate = dateKey(entry.timestamp);
    return entryDate === today;
  }).length;
  return [
    TimeEntryType.CLOCK_IN,
    TimeEntryType.BREAK_START,
    TimeEntryType.BREAK_END,
    TimeEntryType.CLOCK_OUT,
  ][count] ?? null;
};
