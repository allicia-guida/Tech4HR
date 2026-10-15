import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { absenceService } from "../services/absence-service";
import { CreateAbsenceRequest } from "../types/absence-request";

const key = (employeeId: string) => ["absences", employeeId] as const;

export const useAbsences = (employeeId: string) =>
  useQuery({
    queryKey: key(employeeId),
    queryFn: () => absenceService.list(employeeId),
  });

export const useCreateAbsence = (employeeId: string) => {
  const client = useQueryClient();
  return useMutation({
    mutationFn: (input: CreateAbsenceRequest) => absenceService.create(input),
    onSuccess: () => client.invalidateQueries({ queryKey: key(employeeId) }),
  });
};

export const useCancelAbsence = (employeeId: string) => {
  const client = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => absenceService.cancel(id, employeeId),
    onSuccess: () => client.invalidateQueries({ queryKey: key(employeeId) }),
  });
};
