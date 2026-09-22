import { TimeEntry, TimeEntryType } from "../types/time-entry";
import { demoEmployee } from "@/features/profile/fixtures/demo-employee";

export const EMPLOYEE_ID = demoEmployee.registration;
const rawEntries: [string, TimeEntryType, string][] = [
  ["entry-01", TimeEntryType.CLOCK_IN, "2026-09-14T07:58:00-03:00"],
  ["entry-02", TimeEntryType.BREAK_START, "2026-09-14T12:01:00-03:00"],
  ["entry-03", TimeEntryType.BREAK_END, "2026-09-14T13:00:00-03:00"],
  ["entry-04", TimeEntryType.CLOCK_OUT, "2026-09-14T17:05:00-03:00"],
  ["entry-05", TimeEntryType.CLOCK_IN, "2026-09-15T08:12:00-03:00"],
  ["entry-06", TimeEntryType.BREAK_START, "2026-09-15T12:00:00-03:00"],
  ["entry-07", TimeEntryType.BREAK_END, "2026-09-15T13:02:00-03:00"],
  ["entry-08", TimeEntryType.CLOCK_OUT, "2026-09-15T17:00:00-03:00"],
  ["entry-09", TimeEntryType.CLOCK_IN, "2026-09-16T08:00:00-03:00"],
  ["entry-10", TimeEntryType.BREAK_START, "2026-09-16T12:04:00-03:00"],
  ["entry-11", TimeEntryType.BREAK_END, "2026-09-16T13:01:00-03:00"],
  ["entry-12", TimeEntryType.CLOCK_OUT, "2026-09-16T18:10:00-03:00"],
  ["entry-13", TimeEntryType.CLOCK_IN, "2026-09-17T08:03:00-03:00"],
  ["entry-14", TimeEntryType.BREAK_START, "2026-09-17T12:00:00-03:00"],
  ["entry-15", TimeEntryType.BREAK_END, "2026-09-17T13:00:00-03:00"],
  ["entry-16", TimeEntryType.CLOCK_OUT, "2026-09-17T17:02:00-03:00"],
  ["entry-17", TimeEntryType.CLOCK_IN, "2026-09-18T08:01:00-03:00"],
  ["entry-18", TimeEntryType.BREAK_START, "2026-09-18T12:01:00-03:00"],
  ["entry-19", TimeEntryType.BREAK_END, "2026-09-18T13:04:00-03:00"],
  ["entry-20", TimeEntryType.CLOCK_OUT, "2026-09-18T17:00:00-03:00"],
];

export const initialTimeEntries: TimeEntry[] = rawEntries.map(
  ([id, type, timestamp]) => ({
    id,
    employeeId: EMPLOYEE_ID,
    type,
    timestamp,
    source: "APP_SMARTPHONE",
    deviceId: "demo-device",
    createdAt: timestamp,
  }),
);
