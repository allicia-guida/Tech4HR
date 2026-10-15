import { describe, expect, it } from "vitest";
import { absenceSchema } from "./absence-schema";

describe("absenceSchema", () => {
  it("aceita um período válido", () => {
    const result = absenceSchema.safeParse({
      type: "VACATION",
      startDate: "2026-10-01",
      endDate: "2026-10-10",
      reason: "Férias programadas",
    });
    expect(result.success).toBe(true);
  });

  it("rejeita período invertido", () => {
    const result = absenceSchema.safeParse({
      type: "MEDICAL_LEAVE",
      startDate: "2026-10-10",
      endDate: "2026-10-01",
      reason: "Atestado médico",
    });
    expect(result.success).toBe(false);
  });
});
