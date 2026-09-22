import { secureJson } from "@/services/storage/secure-json";
import { PendingOperation } from "../types/pending-operation";

const STORAGE_KEY = "tech4hr.pending-operations";

export const offlineQueue = {
  list: () => secureJson.read<PendingOperation[]>(STORAGE_KEY, []),
  async add(kind: PendingOperation["kind"], payload: Record<string, unknown>) {
    const items = await this.list();
    const operation: PendingOperation = {
      id: `pending-${Date.now()}`,
      kind,
      payload,
      createdAt: new Date().toISOString(),
      status: "WAITING_CONNECTION",
    };
    await secureJson.write(STORAGE_KEY, [operation, ...items]);
    return operation;
  },
  async clear() {
    await secureJson.remove(STORAGE_KEY);
  },
};
