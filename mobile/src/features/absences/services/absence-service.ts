import { secureJson } from "@/services/storage/secure-json";
import { AbsenceRequest, CreateAbsenceRequest } from "../types/absence-request";

const KEY = "tech4hr.absence-requests";

export const absenceService = {
  async list(employeeId: string) {
    const requests = await secureJson.read<AbsenceRequest[]>(KEY, []);
    return requests.filter((item) => item.employeeId === employeeId);
  },
  async create(input: CreateAbsenceRequest) {
    const requests = await secureJson.read<AbsenceRequest[]>(KEY, []);
    const request: AbsenceRequest = {
      ...input,
      id: `absence-${Date.now()}`,
      status: "PENDING",
      createdAt: new Date().toISOString(),
    };
    await secureJson.write(KEY, [request, ...requests]);
    return request;
  },
  async cancel(id: string, employeeId: string) {
    const requests = await secureJson.read<AbsenceRequest[]>(KEY, []);
    const updated = requests.map((item) =>
      item.id === id &&
      item.employeeId === employeeId &&
      item.status === "PENDING"
        ? { ...item, status: "CANCELED" as const }
        : item,
    );
    await secureJson.write(KEY, updated);
  },
};
