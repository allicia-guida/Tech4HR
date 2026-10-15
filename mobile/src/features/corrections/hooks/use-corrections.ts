import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { correctionService } from "../services/correction-service";
import { CreateCorrectionRequest } from "../types/correction-request";

export const correctionKeys = {
  byEmployee: (employeeId: string) => ["corrections", employeeId] as const,
  pending: ["corrections", "pending"] as const,
};

export const useCorrections = (employeeId: string) =>
  useQuery({
    queryKey: correctionKeys.byEmployee(employeeId),
    queryFn: () => correctionService.list(employeeId),
  });

export const useCreateCorrection = (employeeId: string) => {
  const client = useQueryClient();
  return useMutation({
    mutationFn: (input: CreateCorrectionRequest) =>
      correctionService.create(input),
    onSuccess: (request) =>
      client.setQueryData(
        correctionKeys.byEmployee(employeeId),
        (current: unknown) =>
          Array.isArray(current) ? [request, ...current] : [request],
      ),
  });
};

export const usePendingCorrections = () =>
  useQuery({
    queryKey: correctionKeys.pending,
    queryFn: correctionService.listPending,
  });

export const useCorrection = (id: string) =>
  useQuery({
    queryKey: ["corrections", "detail", id],
    queryFn: () => correctionService.get(id),
    enabled: Boolean(id),
  });

export const useCancelCorrection = (employeeId: string) => {
  const client = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => correctionService.cancel(id, employeeId),
    onSuccess: () =>
      client.invalidateQueries({
        queryKey: correctionKeys.byEmployee(employeeId),
      }),
  });
};

export const useReviewCorrection = () => {
  const client = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      decision,
      note,
    }: {
      id: string;
      decision: "APPROVED" | "REJECTED";
      note?: string;
    }) => correctionService.review(id, decision, "demo-manager", note),
    onSuccess: () => client.invalidateQueries({ queryKey: ["corrections"] }),
  });
};
