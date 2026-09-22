import { describe, expect, it, vi } from "vitest";
import {
  nextTimeEntryType,
  TimeEntry,
  TimeEntryType,
} from "./time-entry";

const entry = (type: TimeEntryType, hour: number): TimeEntry => ({
  id: `${type}-${hour}`,
  employeeId: "1",
  type,
  timestamp: `2026-09-21T${String(hour).padStart(2, "0")}:00:00-03:00`,
  source: "APP_SMARTPHONE",
  deviceId: "test",
  createdAt: "2026-09-21T00:00:00-03:00",
});

describe("sequência de registro de ponto", () => {
  it("segue a ordem exigida pela API", () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date("2026-09-21T08:00:00-03:00"));
    const entries: TimeEntry[] = [];
    expect(nextTimeEntryType(entries)).toBe(TimeEntryType.CLOCK_IN);
    entries.push(entry(TimeEntryType.CLOCK_IN, 8));
    expect(nextTimeEntryType(entries)).toBe(TimeEntryType.BREAK_START);
    entries.push(entry(TimeEntryType.BREAK_START, 12));
    expect(nextTimeEntryType(entries)).toBe(TimeEntryType.BREAK_END);
    entries.push(entry(TimeEntryType.BREAK_END, 13));
    expect(nextTimeEntryType(entries)).toBe(TimeEntryType.CLOCK_OUT);
    entries.push(entry(TimeEntryType.CLOCK_OUT, 17));
    expect(nextTimeEntryType(entries)).toBeNull();
    vi.useRealTimers();
  });
});
