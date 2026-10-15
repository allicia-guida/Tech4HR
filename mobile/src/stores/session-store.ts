import * as SecureStore from "expo-secure-store";
import { create } from "zustand";
import { Employee } from "@/features/profile/types/employee";
import type { SignInResult } from "@/features/auth/services/auth-service";
import { getEnvironment } from "@/config/environment";
import { sessionCredentials } from "@/services/auth/session-credentials";

type SessionState = {
  user: Employee | null;
  hydrated: boolean;
  hydrate: () => Promise<void>;
  signIn: (session: SignInResult, remember: boolean) => Promise<void>;
  signOut: () => Promise<void>;
  updateProfile: (values: Partial<Employee>) => Promise<void>;
};

const SESSION_KEY = "tech4hr.session";

const isEmployee = (value: unknown): value is Employee => {
  if (!value || typeof value !== "object") return false;
  const employee = value as Record<string, unknown>;
  return (
    typeof employee.id === "string" &&
    typeof employee.name === "string" &&
    typeof employee.email === "string" &&
    typeof employee.registration === "string" &&
    typeof employee.initials === "string" &&
    typeof employee.phone === "string" &&
    typeof employee.department === "string" &&
    typeof employee.jobTitle === "string" &&
    typeof employee.hireDate === "string" &&
    typeof employee.workPolicyName === "string"
  );
};

export const useSessionStore = create<SessionState>((set) => ({
  user: null,
  hydrated: false,
  hydrate: async () => {
    try {
      const serialized = await SecureStore.getItemAsync(SESSION_KEY);
      if (!serialized) return;
      const parsed: unknown = JSON.parse(serialized);
      if (!isEmployee(parsed)) {
        await SecureStore.deleteItemAsync(SESSION_KEY);
        return;
      }
      const requiresToken = true;
      const hasToken = await sessionCredentials.hydrate();
      if (!getEnvironment().EXPO_PUBLIC_API_URL || (requiresToken && !hasToken)) {
        await SecureStore.deleteItemAsync(SESSION_KEY);
        return;
      }
      set({ user: parsed });
    } catch {
      await SecureStore.deleteItemAsync(SESSION_KEY).catch(() => undefined);
    } finally {
      set({ hydrated: true });
    }
  },
  signIn: async (session, remember) => {
    const { user, accessToken, expiresAt } = session;
    set({ user });
    if (accessToken && expiresAt)
      await sessionCredentials.set(accessToken, expiresAt, remember);
    else await sessionCredentials.clear();
    if (!remember) {
      await SecureStore.deleteItemAsync(SESSION_KEY);
      return;
    }
    await SecureStore.setItemAsync(SESSION_KEY, JSON.stringify(user), {
      keychainAccessible: SecureStore.WHEN_UNLOCKED_THIS_DEVICE_ONLY,
    });
  },
  signOut: async () => {
    set({ user: null });
    await Promise.all([
      SecureStore.deleteItemAsync(SESSION_KEY),
      sessionCredentials.clear(),
    ]);
  },
  updateProfile: async (values) => {
    const current = useSessionStore.getState().user;
    if (!current) return;
    const updated = { ...current, ...values };
    set({ user: updated });
    await SecureStore.setItemAsync(SESSION_KEY, JSON.stringify(updated), {
      keychainAccessible: SecureStore.WHEN_UNLOCKED_THIS_DEVICE_ONLY,
    });
  },
}));
