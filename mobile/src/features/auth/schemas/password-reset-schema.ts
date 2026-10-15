import { z } from "zod";

export const passwordResetSchema = z.object({
  email: z.string().email("Informe um e-mail válido.").max(254),
});

export type PasswordResetForm = z.infer<typeof passwordResetSchema>;
