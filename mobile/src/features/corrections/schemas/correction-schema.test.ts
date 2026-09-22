import { describe, expect, it } from "vitest";
import { correctionSchema } from "./correction-schema";

describe("correctionSchema", () => {
  it("aceita uma solicitação válida", () => {
    const result = correctionSchema.safeParse({
      date: "2026-09-22",
      reason: "WRONG_TIME",
      proposedType: "CLOCK_IN",
      proposedTime: "08:05",
      selectedEntryId: "entry-25",
      justification: "O horário correto está no comprovante.",
    });
    expect(result.success).toBe(true);
  });

  it("rejeita horário e justificativa inválidos", () => {
    const result = correctionSchema.safeParse({
      date: "22/09/2026",
      reason: "OTHER",
      proposedType: "CLOCK_IN",
      proposedTime: "28:99",
      selectedEntryId: "",
      justification: "curta",
    });
    expect(result.success).toBe(false);
  });
});
