import { EMPLOYEE_ID, initialTimeEntries } from "../fixtures/time-entries";
import {
  AttendanceLocation,
  TimeEntry,
  TimeEntryType,
} from "../types/time-entry";
import { AttendanceRepository } from "./attendance-repository";

let entries = [...initialTimeEntries];
const wait = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const localAttendanceRepository: AttendanceRepository = {
  async list(): Promise<TimeEntry[]> {
    await wait(250);
    return [...entries];
  },
  async create(
    type: TimeEntryType,
    location: AttendanceLocation,
  ): Promise<TimeEntry> {
    await wait(450);
    const timestamp = new Date().toISOString();
    const entry: TimeEntry = {
      id: `entry-${Date.now()}`,
      employeeId: EMPLOYEE_ID,
      type,
      timestamp,
      source: "APP_SMARTPHONE",
      deviceId: "local-device",
      latitude: location.latitude,
      longitude: location.longitude,
      accuracyMeters: location.accuracyMeters,
      locationCapturedAt: location.capturedAt,
      createdAt: timestamp,
    };
    entries = [entry, ...entries];
    return entry;
  },
};
