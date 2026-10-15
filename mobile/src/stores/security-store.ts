import * as SecureStore from "expo-secure-store";
import { create } from "zustand";

const KEY = "tech4hr.biometric-enabled";

type SecurityState = {
  biometricEnabled: boolean;
  hydrated: boolean;
  hydrate: () => Promise<void>;
  setBiometricEnabled: (enabled: boolean) => Promise<void>;
};

export const useSecurityStore = create<SecurityState>((set) => ({
  biometricEnabled: false,
  hydrated: false,
  hydrate: async () => {
    const value = await SecureStore.getItemAsync(KEY);
    set({ biometricEnabled: value === "true", hydrated: true });
  },
  setBiometricEnabled: async (enabled) => {
    await SecureStore.setItemAsync(KEY, String(enabled));
    set({ biometricEnabled: enabled });
  },
}));
