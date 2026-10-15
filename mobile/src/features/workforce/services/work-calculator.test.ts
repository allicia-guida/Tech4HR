import { describe, expect, it } from "vitest";
import { initialTimeEntries } from "@/features/attendance/fixtures/time-entries";
import {
  TimeEntry,
  TimeEntryType,
} from "@/features/attendance/types/time-entry";
import { demoAbsences } from "../fixtures/absences";
import { defaultWorkPolicy } from "../fixtures/default-work-policy";
import { calculateDailyWork } from "./work-calculator";

describe("calculateDailyWork", () => {
  it("calcula jornada e hora extra", () => {
    const summary = calculateDailyWork(
      "2026-09-16",
      initialTimeEntries,
      defaultWorkPolicy,
      demoAbsences,
    );
    expect(summary.workedMinutes).toBe(553);
    expect(summary.overtimeMinutes).toBe(68);
    expect(summary.timeBankMinutes).toBe(68);
    expect(summary.status).toBe("COMPLETE");
  });

  it("calcula atraso considerando tolerância", () => {
    const summary = calculateDailyWork(
      "2026-09-15",
      initialTimeEntries,
      defaultWorkPolicy,
      demoAbsences,
    );
    expect(summary.lateMinutes).toBe(7);
    expect(summary.deficitMinutes).toBe(9);
  });

  it("reconhece férias aprovadas", () => {
    const summary = calculateDailyWork(
      "2026-09-07",
      [],
      defaultWorkPolicy,
      demoAbsences,
    );
    expect(summary.status).toBe("LEAVE");
  });

  it("reconhece feriado", () => {
    const summary = calculateDailyWork("2026-12-25", [], defaultWorkPolicy, []);
    expect(summary.status).toBe("HOLIDAY");
  });

  it("marca uma jornada aberta como incompleta", () => {
    const entry: TimeEntry = {
      id: "open-entry",
      employeeId: "employee",
      type: TimeEntryType.CLOCK_IN,
      timestamp: "2026-09-23T08:00:00-03:00",
      source: "APP_SMARTPHONE",
      deviceId: "device",
      createdAt: "2026-09-23T08:00:00-03:00",
    };
    const summary = calculateDailyWork(
      "2026-09-23",
      [entry],
      defaultWorkPolicy,
    );
    expect(summary.status).toBe("INCOMPLETE");
    expect(summary.openEntry).toBe(true);
  });

  it("usa o fuso da política", () => {
    const entries: TimeEntry[] = [
      {
        id: "tz-in",
        employeeId: "employee",
        type: TimeEntryType.CLOCK_IN,
        timestamp: "2026-09-22T11:00:00Z",
        source: "APP_SMARTPHONE",
        deviceId: "device",
        createdAt: "2026-09-22T11:00:00Z",
      },
      {
        id: "tz-out",
        employeeId: "employee",
        type: TimeEntryType.CLOCK_OUT,
        timestamp: "2026-09-22T20:00:00Z",
        source: "APP_SMARTPHONE",
        deviceId: "device",
        createdAt: "2026-09-22T20:00:00Z",
      },
    ];
    const summary = calculateDailyWork(
      "2026-09-22",
      entries,
      defaultWorkPolicy,
    );
    expect(summary.firstEntryMinute).toBe(480);
    expect(summary.lastExitMinute).toBe(1020);
  });
});
