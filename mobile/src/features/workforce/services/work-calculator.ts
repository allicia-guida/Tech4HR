import {
  TimeEntry,
  TimeEntryType,
} from "@/features/attendance/types/time-entry";
import { Absence, DailyWorkSummary, WorkPolicy } from "../types/work-policy";

const dateParts = (value: string, timeZone: string) => {
  const parts = new Intl.DateTimeFormat("en-CA", {
    timeZone,
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    hourCycle: "h23",
  }).formatToParts(new Date(value));
  const part = (type: Intl.DateTimeFormatPartTypes) =>
    parts.find((item) => item.type === type)?.value ?? "0";
  return {
    date: `${part("year")}-${part("month")}-${part("day")}`,
    minute: Number(part("hour")) * 60 + Number(part("minute")),
  };
};

const weekdayForDate = (date: string) =>
  new Date(`${date}T12:00:00Z`).getUTCDay() as 0 | 1 | 2 | 3 | 4 | 5 | 6;

export const calculateDailyWork = (
  date: string,
  entries: TimeEntry[],
  policy: WorkPolicy,
  absences: Absence[] = [],
): DailyWorkSummary => {
  const schedule = policy.schedule.find(
    (item) => item.weekday === weekdayForDate(date),
  );
  const holiday = policy.holidays.some((item) => item.date === date);
  const absence = absences.find(
    (item) => item.date === date && item.status === "APPROVED",
  );
  const dayEntries = entries
    .map((entry) => ({ entry, ...dateParts(entry.timestamp, policy.timeZone) }))
    .filter((item) => item.date === date)
    .sort(
      (a, b) =>
        new Date(a.entry.timestamp).getTime() -
        new Date(b.entry.timestamp).getTime(),
    );
  let workedMinutes = 0;
  let openMinute: number | null = null;
  for (const item of dayEntries) {
    const startsWork =
      item.entry.type === TimeEntryType.CLOCK_IN ||
      item.entry.type === TimeEntryType.BREAK_END;
    const stopsWork =
      item.entry.type === TimeEntryType.BREAK_START ||
      item.entry.type === TimeEntryType.CLOCK_OUT;
    if (startsWork && openMinute === null) {
      openMinute = item.minute;
    }
    if (stopsWork && openMinute !== null) {
      workedMinutes += Math.max(0, item.minute - openMinute);
      openMinute = null;
    }
  }
  const firstEntryMinute =
    dayEntries.find((item) => item.entry.type === TimeEntryType.CLOCK_IN)
      ?.minute ?? null;
  const exits = dayEntries.filter((item) =>
    [TimeEntryType.BREAK_START, TimeEntryType.CLOCK_OUT].includes(
      item.entry.type,
    ),
  );
  const lastExitMinute = exits.length ? exits[exits.length - 1].minute : null;
  const enabled = Boolean(schedule?.enabled);
  const scheduledMinutes = enabled
    ? Math.max(
        0,
        (schedule?.endMinute ?? 0) -
          (schedule?.startMinute ?? 0) -
          (schedule?.breakMinutes ?? 0),
      )
    : 0;
  const spanMinutes =
    firstEntryMinute !== null && lastExitMinute !== null
      ? Math.max(0, lastExitMinute - firstEntryMinute)
      : workedMinutes;
  const breakMinutes = Math.max(0, spanMinutes - workedMinutes);
  const lateMinutes =
    enabled && firstEntryMinute !== null
      ? Math.max(
          0,
          firstEntryMinute -
            (schedule?.startMinute ?? 0) -
            policy.lateToleranceMinutes,
        )
      : 0;
  const overtimeMinutes = Math.max(
    0,
    workedMinutes - scheduledMinutes - policy.overtimeToleranceMinutes,
  );
  const deficitMinutes = enabled
    ? Math.max(
        0,
        scheduledMinutes - workedMinutes - policy.lateToleranceMinutes,
      )
    : 0;
  const timeBankMinutes = overtimeMinutes - deficitMinutes;
  let status: DailyWorkSummary["status"] = "COMPLETE";
  if (holiday) status = "HOLIDAY";
  else if (absence) status = absence.type === "VACATION" ? "LEAVE" : "LEAVE";
  else if (!enabled) status = "DAY_OFF";
  else if (!dayEntries.length) status = "ABSENT";
  else if (openMinute !== null) status = "INCOMPLETE";
  return {
    date,
    status,
    scheduledMinutes,
    workedMinutes,
    breakMinutes,
    lateMinutes,
    overtimeMinutes,
    deficitMinutes,
    timeBankMinutes,
    firstEntryMinute,
    lastExitMinute,
    openEntry: openMinute !== null,
  };
};
