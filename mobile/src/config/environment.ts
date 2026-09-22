import { z } from "zod";

const environmentSchema = z.object({
  EXPO_PUBLIC_APP_ENV: z
    .enum(["development", "staging", "production"])
    .default("development"),
  EXPO_PUBLIC_API_URL: z.preprocess(
    (value) => (value === "" ? undefined : value),
    z
      .string()
      .url()
      .refine((value) => value.startsWith("https://"))
      .optional(),
  ),
  EXPO_PUBLIC_MAINTENANCE_MODE: z.enum(["true", "false"]).default("false"),
});

export const getEnvironment = () =>
  environmentSchema.parse({
    EXPO_PUBLIC_APP_ENV: process.env.EXPO_PUBLIC_APP_ENV,
    EXPO_PUBLIC_API_URL: process.env.EXPO_PUBLIC_API_URL,
    EXPO_PUBLIC_MAINTENANCE_MODE: process.env.EXPO_PUBLIC_MAINTENANCE_MODE,
  });
