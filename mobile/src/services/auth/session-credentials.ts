import * as SecureStore from "expo-secure-store";

const TOKEN_KEY = "tech4hr.access-token";
const EXPIRATION_KEY = "tech4hr.access-token-expiration";

let accessToken: string | null = null;
let expiration: string | null = null;

const isExpired = (value: string | null) =>
  Boolean(value && new Date(value).getTime() <= Date.now());

export const sessionCredentials = {
  async hydrate() {
    const [storedToken, storedExpiration] = await Promise.all([
      SecureStore.getItemAsync(TOKEN_KEY),
      SecureStore.getItemAsync(EXPIRATION_KEY),
    ]);
    if (!storedToken || isExpired(storedExpiration)) {
      await this.clear();
      return false;
    }
    accessToken = storedToken;
    expiration = storedExpiration;
    return true;
  },
  getAccessToken() {
    if (isExpired(expiration)) {
      accessToken = null;
      expiration = null;
      return null;
    }
    return accessToken;
  },
  async set(token: string, expiresAt: string, persist: boolean) {
    accessToken = token;
    expiration = expiresAt;
    if (!persist) {
      await Promise.all([
        SecureStore.deleteItemAsync(TOKEN_KEY),
        SecureStore.deleteItemAsync(EXPIRATION_KEY),
      ]);
      return;
    }
    const options = {
      keychainAccessible: SecureStore.WHEN_UNLOCKED_THIS_DEVICE_ONLY,
    };
    await Promise.all([
      SecureStore.setItemAsync(TOKEN_KEY, token, options),
      SecureStore.setItemAsync(EXPIRATION_KEY, expiresAt, options),
    ]);
  },
  async clear() {
    accessToken = null;
    expiration = null;
    await Promise.all([
      SecureStore.deleteItemAsync(TOKEN_KEY),
      SecureStore.deleteItemAsync(EXPIRATION_KEY),
    ]);
  },
};
