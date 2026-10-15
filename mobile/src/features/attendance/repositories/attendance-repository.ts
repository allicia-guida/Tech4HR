import {
  AttendanceLocation,
  TimeEntry,
  TimeEntryType,
} from "../types/time-entry";

export interface AttendanceRepository {
  list(): Promise<TimeEntry[]>;
  create(type: TimeEntryType, location: AttendanceLocation): Promise<TimeEntry>;
}
