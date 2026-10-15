import { z } from "zod";

export const correctionSchema = z.object({
  date: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, "Use o formato AAAA-MM-DD"),
  reason: z.enum([
    "FORGOT_ENTRY",
    "FORGOT_EXIT",
    "WRONG_TIME",
    "MEDICAL",
    "OTHER",
  ]),
  proposedType: z.enum([
    "CLOCK_IN",
    "BREAK_START",
    "BREAK_END",
    "CLOCK_OUT",
  ]),
  proposedTime: z
    .string()
    .regex(/^([01]\d|2[0-3]):[0-5]\d$/, "Use o formato HH:mm"),
  selectedEntryId: z.string(),
  justification: z
    .string()
    .trim()
    .min(10, "Informe pelo menos 10 caracteres")
    .max(500, "Use no máximo 500 caracteres"),
});

export type CorrectionForm = z.infer<typeof correctionSchema>;
