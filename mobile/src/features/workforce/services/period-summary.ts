import { TimeEntry } from "@/features/attendance/types/time-entry";
import { calculateDailyWork } from "./work-calculator";
import { Absence, DailyWorkSummary, WorkPolicy } from "../types/work-policy";

export type HistoryPeriod = "DAY" | "WEEK" | "MONTH";

const isoDate = (date: Date) => date.toISOString().slice(0, 10);

export const shiftDate = (date: string, amount: number) => {
  const value = new Date(`${date}T12:00:00Z`);
  value.setUTCDate(value.getUTCDate() + amount);
  return isoDate(value);
};

export const dateRange = (anchor: string, period: HistoryPeriod) => {
  if (period === "DAY") return [anchor];
  const value = new Date(`${anchor}T12:00:00Z`);
  if (period === "WEEK") {
    const weekday = value.getUTCDay();
    const distance = weekday === 0 ? -6 : 1 - weekday;
    value.setUTCDate(value.getUTCDate() + distance);
    return Array.from({ length: 7 }, (_, index) => {
      const day = new Date(value);
      day.setUTCDate(value.getUTCDate() + index);
      return isoDate(day);
    });
  }
  const year = value.getUTCFullYear();
  const month = value.getUTCMonth();
  const days = new Date(Date.UTC(year, month + 1, 0)).getUTCDate();
  return Array.from({ length: days }, (_, index) =>
    isoDate(new Date(Date.UTC(year, month, index + 1))),
  );
};

export const shiftPeriod = (
  anchor: string,
  period: HistoryPeriod,
  direction: -1 | 1,
) => {
  const value = new Date(`${anchor}T12:00:00Z`);
  if (period === "DAY") value.setUTCDate(value.getUTCDate() + direction);
  if (period === "WEEK") value.setUTCDate(value.getUTCDate() + direction * 7);
  if (period === "MONTH") value.setUTCMonth(value.getUTCMonth() + direction);
  return isoDate(value);
};

export const summarizePeriod = (
  anchor: string,
  period: HistoryPeriod,
  entries: TimeEntry[],
  policy: WorkPolicy,
  absences: Absence[],
) => {
  const days = dateRange(anchor, period).map((date) =>
    calculateDailyWork(date, entries, policy, absences),
  );
  return {
    days,
    scheduledMinutes: days.reduce((sum, day) => sum + day.scheduledMinutes, 0),
    workedMinutes: days.reduce((sum, day) => sum + day.workedMinutes, 0),
    lateMinutes: days.reduce((sum, day) => sum + day.lateMinutes, 0),
    overtimeMinutes: days.reduce((sum, day) => sum + day.overtimeMinutes, 0),
    timeBankMinutes: days.reduce((sum, day) => sum + day.timeBankMinutes, 0),
  };
};

export const accumulatedTimeBank = (
  throughDate: string,
  entries: TimeEntry[],
  policy: WorkPolicy,
  absences: Absence[],
) => {
  const start = `${throughDate.slice(0, 4)}-01-01`;
  const days: DailyWorkSummary[] = [];
  let cursor = start;
  while (cursor <= throughDate) {
    days.push(calculateDailyWork(cursor, entries, policy, absences));
    cursor = shiftDate(cursor, 1);
  }
  return days.reduce((sum, day) => sum + day.timeBankMinutes, 0);
};
