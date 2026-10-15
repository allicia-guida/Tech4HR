import { secureJson } from "@/services/storage/secure-json";
import {
  CorrectionRequest,
  CreateCorrectionRequest,
} from "../types/correction-request";

const KEY = "tech4hr.correction-requests";

const read = () => secureJson.read<CorrectionRequest[]>(KEY, []);

export const correctionService = {
  async list(employeeId: string): Promise<CorrectionRequest[]> {
    const requests = await read();
    return requests.filter((item) => item.employeeId === employeeId);
  },
  async listPending(): Promise<CorrectionRequest[]> {
    const requests = await read();
    return requests.filter((item) => item.status === "PENDING");
  },
  async get(id: string): Promise<CorrectionRequest | null> {
    const requests = await read();
    return requests.find((item) => item.id === id) ?? null;
  },
  async create(input: CreateCorrectionRequest): Promise<CorrectionRequest> {
    const requests = await read();
    const createdAt = new Date().toISOString();
    const request: CorrectionRequest = {
      ...input,
      id: `correction-${Date.now()}`,
      status: "PENDING",
      createdAt,
      history: [
        {
          id: `event-${Date.now()}`,
          action: "CREATED",
          at: createdAt,
          actor: input.employeeId,
        },
      ],
    };
    await secureJson.write(KEY, [request, ...requests]);
    return request;
  },
  async cancel(id: string, employeeId: string): Promise<CorrectionRequest> {
    const requests = await read();
    const current = requests.find((item) => item.id === id);
    if (
      !current ||
      current.employeeId !== employeeId ||
      current.status !== "PENDING"
    ) {
      throw new Error("INVALID_CORRECTION_REQUEST");
    }
    const at = new Date().toISOString();
    const canceled: CorrectionRequest = {
      ...current,
      status: "CANCELED",
      history: [
        ...current.history,
        {
          id: `event-${Date.now()}`,
          action: "CANCELED",
          at,
          actor: employeeId,
        },
      ],
    };
    await secureJson.write(
      KEY,
      requests.map((item) => (item.id === id ? canceled : item)),
    );
    return canceled;
  },
  async review(
    id: string,
    decision: "APPROVED" | "REJECTED",
    reviewerId: string,
    reviewerNote?: string,
  ): Promise<CorrectionRequest> {
    const requests = await read();
    const current = requests.find((item) => item.id === id);
    if (!current || current.status !== "PENDING") {
      throw new Error("INVALID_CORRECTION_REQUEST");
    }
    const reviewedAt = new Date().toISOString();
    const reviewed: CorrectionRequest = {
      ...current,
      status: decision,
      reviewerId,
      reviewerNote,
      reviewedAt,
      history: [
        ...current.history,
        {
          id: `event-${Date.now()}`,
          action: decision,
          at: reviewedAt,
          actor: reviewerId,
          note: reviewerNote,
        },
      ],
    };
    await secureJson.write(
      KEY,
      requests.map((item) => (item.id === id ? reviewed : item)),
    );
    return reviewed;
  },
};
