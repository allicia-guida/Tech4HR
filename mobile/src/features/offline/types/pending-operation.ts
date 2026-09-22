export type PendingOperation = {
  id: string;
  kind: "TIME_ENTRY" | "CORRECTION" | "ABSENCE";
  createdAt: string;
  payload: Record<string, unknown>;
  status: "WAITING_CONNECTION" | "WAITING_API" | "FAILED";
};
