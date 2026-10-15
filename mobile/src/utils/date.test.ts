import { afterEach, describe, expect, it } from "vitest";
import {
  dateKey,
  formatLongDate,
  formatReceiptDate,
  formatTime,
} from "./date";
import { serverClock } from "./server-clock";

afterEach(() => serverClock.reset());

// 01:30 UTC de 06/10 é 22:30 de 05/10 em Brasília. É o caso em que o dia do
// aparelho (se estiver em UTC) e o dia do ponto são diferentes.
const NIGHT_UTC = "2026-10-06T01:30:00Z";

describe("datas no horário de Brasília", () => {
  it("mostra a hora de Brasília, não a do aparelho", () => {
    expect(formatTime(NIGHT_UTC)).toBe("22:30");
  });

  it("usa o dia de Brasília para agrupar o ponto", () => {
    expect(dateKey(NIGHT_UTC)).toBe("2026-10-05");
  });

  it("escreve a data por extenso em português", () => {
    expect(formatLongDate(NIGHT_UTC)).toBe(
      "Segunda-feira, 5 de outubro de 2026",
    );
    expect(formatReceiptDate(NIGHT_UTC)).toBe("5 de outubro de 2026");
  });

  it("não mexe em datas de calendário sem hora", () => {
    expect(dateKey("2026-10-05")).toBe("2026-10-05");
  });

  it("segue o deslocamento que a API informar", () => {
    serverClock.sync("2026-10-05T10:00:00+09:00");

    expect(dateKey("2026-10-05T20:00:00Z")).toBe("2026-10-06");
    expect(formatTime("2026-10-05T20:00:00Z")).toBe("05:00");
  });
});
