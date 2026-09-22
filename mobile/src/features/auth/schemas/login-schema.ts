import { z } from "zod";
export const loginSchema = z.object({
  email: z.string().email("Informe um e-mail válido.").max(254),
  password: z
    .string()
    .min(8, "A senha deve ter ao menos 8 caracteres.")
    .max(128),
});
export type LoginForm = z.infer<typeof loginSchema>;
