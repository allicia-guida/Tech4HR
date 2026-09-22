import { create } from "zustand";
import { secureJson } from "@/services/storage/secure-json";
import { notificationService } from "@/features/notifications/services/notification-service";

const KEY = "tech4hr.notifications-enabled";

type NotificationState = {
  enabled: boolean;
  hydrated: boolean;
  hydrate: () => Promise<void>;
  setEnabled: (enabled: boolean) => Promise<void>;
};

export const useNotificationStore = create<NotificationState>((set) => ({
  enabled: false,
  hydrated: false,
  hydrate: async () => {
    const enabled = await secureJson.read(KEY, false);
    set({ enabled, hydrated: true });
  },
  setEnabled: async (enabled) => {
    if (enabled) await notificationService.enable();
    else await notificationService.disable();
    await secureJson.write(KEY, enabled);
    set({ enabled });
  },
}));
