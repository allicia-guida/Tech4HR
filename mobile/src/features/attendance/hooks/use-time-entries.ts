import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { attendanceService } from "../services/attendance-service";
import { AttendanceLocation, TimeEntryType } from "../types/time-entry";

export const timeEntryKeys = { all: ["time-entries"] as const };
export function useTimeEntries() {
  return useQuery({
    queryKey: timeEntryKeys.all,
    queryFn: attendanceService.list,
  });
}
export function useCreateTimeEntry() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: ({
      type,
      location,
    }: {
      type: TimeEntryType;
      location: AttendanceLocation;
    }) => attendanceService.create(type, location),
    onSuccess: (entry) =>
      client.setQueryData(timeEntryKeys.all, (current: unknown) =>
        Array.isArray(current) ? [entry, ...current] : [entry],
      ),
  });
}
