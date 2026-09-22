import * as Network from "expo-network";
import { getEnvironment } from "@/config/environment";
import { offlineQueue } from "@/features/offline/services/offline-queue";
import { apiAttendanceRepository } from "../repositories/api-attendance-repository";
import { AttendanceRepository } from "../repositories/attendance-repository";
import {
  AttendanceLocation,
  TimeEntry,
  TimeEntryType,
} from "../types/time-entry";

const repository = (): AttendanceRepository => {
  if (!getEnvironment().EXPO_PUBLIC_API_URL)
    throw new Error("API_NOT_CONFIGURED");
  return apiAttendanceRepository;
};

export const attendanceService = {
  list(): Promise<TimeEntry[]> {
    return repository().list();
  },
  async create(
    type: TimeEntryType,
    location: AttendanceLocation,
  ): Promise<TimeEntry> {
    const network = await Network.getNetworkStateAsync();
    if (
      network.isConnected === false ||
      network.isInternetReachable === false
    ) {
      await offlineQueue.add("TIME_ENTRY", { type, location });
      throw new Error("OFFLINE_NOT_CONFIRMED");
    }
    return repository().create(type, location);
  },
};
