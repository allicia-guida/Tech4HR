import * as SecureStore from "expo-secure-store";
import { create } from "zustand";

export type ThemePreference = "system" | "light" | "dark";

type ThemeState = {
  preference: ThemePreference;
  hydrated: boolean;
  hydrate: () => Promise<void>;
  setPreference: (preference: ThemePreference) => Promise<void>;
};

const THEME_KEY = "tech4hr.theme";

export const useThemeStore = create<ThemeState>((set) => ({
  preference: "system",
  hydrated: false,
  hydrate: async () => {
    try {
      const value = await SecureStore.getItemAsync(THEME_KEY);
      if (value === "system" || value === "light" || value === "dark")
        set({ preference: value });
    } finally {
      set({ hydrated: true });
    }
  },
  setPreference: async (preference) => {
    set({ preference });
    await SecureStore.setItemAsync(THEME_KEY, preference, {
      keychainAccessible: SecureStore.WHEN_UNLOCKED_THIS_DEVICE_ONLY,
    });
  },
}));
