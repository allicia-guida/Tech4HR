import { z } from "zod";

const emptyToUndefined = (value: unknown) => (value === "" ? undefined : value);

const environmentSchema = z
  .object({
    EXPO_PUBLIC_APP_ENV: z
      .enum(["development", "staging", "production"])
      .default("development"),
    EXPO_PUBLIC_API_URL: z.preprocess(
      emptyToUndefined,
      z.string().url().optional(),
    ),
    // Qual API o aplicativo usa:
    // "tech4hr": a API do Tech4Hr (Tech4Hr.API), a mesma do site e do banco
    //            que já existe. Rotas em /api/auth e /api/pontos.
    // "public":  a Tech4Hr.PublicApi independente (rotas em /api/v1).
    EXPO_PUBLIC_API_TARGET: z.preprocess(
      emptyToUndefined,
      z.enum(["tech4hr", "public"]).default("public"),
    ),
    EXPO_PUBLIC_MAINTENANCE_MODE: z.enum(["true", "false"]).default("false"),
  })
  .refine(
    (env) =>
      !env.EXPO_PUBLIC_API_URL ||
      env.EXPO_PUBLIC_APP_ENV === "development" ||
      env.EXPO_PUBLIC_API_URL.startsWith("https://"),
    {
      message:
        "Fora do ambiente de desenvolvimento a URL da API precisa usar HTTPS.",
      path: ["EXPO_PUBLIC_API_URL"],
    },
  );

export const getEnvironment = () =>
  environmentSchema.parse({
    EXPO_PUBLIC_APP_ENV: process.env.EXPO_PUBLIC_APP_ENV,
    EXPO_PUBLIC_API_URL: process.env.EXPO_PUBLIC_API_URL,
    EXPO_PUBLIC_API_TARGET: process.env.EXPO_PUBLIC_API_TARGET,
    EXPO_PUBLIC_MAINTENANCE_MODE: process.env.EXPO_PUBLIC_MAINTENANCE_MODE,
  });
