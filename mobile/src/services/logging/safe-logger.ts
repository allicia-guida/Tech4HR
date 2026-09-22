const sanitize = (value: unknown): unknown => {
  if (!value || typeof value !== "object") return value;
  if (Array.isArray(value)) return value.map(sanitize);
  return Object.fromEntries(
    Object.entries(value as Record<string, unknown>).map(([key, item]) => [
      key,
      /token|password|email|phone|name|document|attachment/i.test(key)
        ? "[REDACTED]"
        : sanitize(item),
    ]),
  );
};

export const safeLogger = {
  error(event: string, details?: Record<string, unknown>) {
    if (__DEV__) console.error(event, sanitize(details));
  },
};
