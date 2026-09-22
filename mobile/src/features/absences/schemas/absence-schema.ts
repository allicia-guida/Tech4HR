import { z } from "zod";

export const absenceSchema = z
  .object({
    type: z.enum(["VACATION", "ABSENCE", "MEDICAL_LEAVE"]),
    startDate: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, "Use AAAA-MM-DD"),
    endDate: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, "Use AAAA-MM-DD"),
    reason: z
      .string()
      .trim()
      .min(5, "Informe pelo menos 5 caracteres")
      .max(500),
  })
  .refine((value) => value.endDate >= value.startDate, {
    path: ["endDate"],
    message: "A data final deve ser igual ou posterior à inicial",
  });

export type AbsenceForm = z.infer<typeof absenceSchema>;
