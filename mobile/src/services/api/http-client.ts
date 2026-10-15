import { getEnvironment } from "@/config/environment";
import { sessionCredentials } from "@/services/auth/session-credentials";
import { parseErrorMessage } from "./error-message";

const REQUEST_TIMEOUT_MS = 15_000;
let unauthorizedHandler: (() => void) | null = null;

export const setUnauthorizedHandler = (handler: (() => void) | null) => {
  unauthorizedHandler = handler;
};

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message = "Não foi possível concluir a solicitação.",
  ) {
    super(message);
    this.name = "ApiError";
  }
}

const responseMessage = async (response: Response) => {
  const fallback =
    response.status === 401
      ? "Sessão expirada. Entre novamente."
      : "Não foi possível concluir a solicitação.";
  try {
    return parseErrorMessage(await response.text(), fallback);
  } catch {
    return fallback;
  }
};

export const apiRequest = async <T>(
  path: string,
  init: RequestInit = {},
): Promise<T> => {
  const { EXPO_PUBLIC_API_URL, EXPO_PUBLIC_APP_ENV } = getEnvironment();
  if (!EXPO_PUBLIC_API_URL) throw new ApiError(0);
  const url = new URL(path, EXPO_PUBLIC_API_URL);
  // HTTPS sempre. HTTP só em desenvolvimento, para testar contra a API na rede
  // local (por exemplo http://192.168.0.10:5287).
  const insecureAllowed =
    EXPO_PUBLIC_APP_ENV === "development" && url.protocol === "http:";
  if (url.protocol !== "https:" && !insecureAllowed) throw new ApiError(0);
  const accessToken = sessionCredentials.getAccessToken();
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), REQUEST_TIMEOUT_MS);
  try {
    const response = await fetch(url, {
      ...init,
      signal: controller.signal,
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
        ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
        ...init.headers,
      },
    });
    if (!response.ok) {
      if (response.status === 401) {
        await sessionCredentials.clear();
        unauthorizedHandler?.();
      }
      throw new ApiError(response.status, await responseMessage(response));
    }
    if (response.status === 204) return undefined as T;
    const body = await response.text();
    return (body ? JSON.parse(body) : undefined) as T;
  } finally {
    clearTimeout(timeout);
  }
};
