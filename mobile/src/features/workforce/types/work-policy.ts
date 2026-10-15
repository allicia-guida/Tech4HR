export type Weekday = 0 | 1 | 2 | 3 | 4 | 5 | 6;

export type DailySchedule = {
  weekday: Weekday;
  enabled: boolean;
  startMinute: number;
  endMinute: number;
  breakMinutes: number;
};

export type Holiday = {
  id: string;
  date: string;
  name: string;
  scope: "NATIONAL" | "STATE" | "CITY" | "COMPANY";
};

export type WorkPolicy = {
  id: string;
  name: string;
  timeZone: string;
  schedule: DailySchedule[];
  lateToleranceMinutes: number;
  overtimeToleranceMinutes: number;
  minimumBreakMinutes: number;
  allowNegativeTimeBank: boolean;
  holidays: Holiday[];
};

export type AbsenceType = "ABSENCE" | "VACATION" | "MEDICAL_LEAVE" | "DAY_OFF";

export type Absence = {
  id: string;
  employeeId: string;
  date: string;
  type: AbsenceType;
  status: "PENDING" | "APPROVED" | "REJECTED";
  reason?: string;
};

export type DailyWorkStatus =
  "COMPLETE" | "INCOMPLETE" | "ABSENT" | "HOLIDAY" | "LEAVE" | "DAY_OFF";

export type DailyWorkSummary = {
  date: string;
  status: DailyWorkStatus;
  scheduledMinutes: number;
  workedMinutes: number;
  breakMinutes: number;
  lateMinutes: number;
  overtimeMinutes: number;
  deficitMinutes: number;
  timeBankMinutes: number;
  firstEntryMinute: number | null;
  lastExitMinute: number | null;
  openEntry: boolean;
};
