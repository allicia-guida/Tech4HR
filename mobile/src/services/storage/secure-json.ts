import * as SecureStore from "expo-secure-store";

export const secureJson = {
  async read<T>(key: string, fallback: T): Promise<T> {
    try {
      const value = await SecureStore.getItemAsync(key);
      return value ? (JSON.parse(value) as T) : fallback;
    } catch {
      await SecureStore.deleteItemAsync(key).catch(() => undefined);
      return fallback;
    }
  },
  async write<T>(key: string, value: T): Promise<void> {
    await SecureStore.setItemAsync(key, JSON.stringify(value), {
      keychainAccessible: SecureStore.WHEN_UNLOCKED_THIS_DEVICE_ONLY,
    });
  },
  async remove(key: string): Promise<void> {
    await SecureStore.deleteItemAsync(key);
  },
};
