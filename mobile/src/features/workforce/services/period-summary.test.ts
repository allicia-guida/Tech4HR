import { describe, expect, it } from "vitest";
import { dateRange, shiftPeriod } from "./period-summary";

describe("period summary dates", () => {
  it("gera a semana de segunda a domingo", () => {
    const range = dateRange("2026-09-22", "WEEK");
    expect(range[0]).toBe("2026-09-21");
    expect(range[6]).toBe("2026-09-27");
  });

  it("navega entre meses", () => {
    expect(shiftPeriod("2026-09-22", "MONTH", 1)).toBe("2026-10-22");
    expect(dateRange("2026-02-10", "MONTH")).toHaveLength(28);
  });
});
