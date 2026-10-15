import { afterEach, describe, expect, it, vi } from "vitest";
import { parseOffsetMinutes, serverClock } from "./server-clock";

afterEach(() => {
  serverClock.reset();
  vi.useRealTimers();
});

describe("deslocamento do fuso", () => {
  it("lê o deslocamento que a API informa", () => {
    expect(parseOffsetMinutes("2026-10-05T21:55:09-03:00")).toBe(-180);
    expect(parseOffsetMinutes("2026-10-05T21:55:09+05:30")).toBe(330);
    expect(parseOffsetMinutes("2026-10-05T21:55:09Z")).toBe(0);
  });

  it("devolve null quando o texto não traz deslocamento", () => {
    expect(parseOffsetMinutes("2026-10-05T21:55:09")).toBeNull();
  });
});

describe("relógio oficial", () => {
  it("antes de sincronizar usa o aparelho e o fuso de Brasília", () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date("2026-10-05T12:00:00Z"));

    expect(serverClock.now().toISOString()).toBe("2026-10-05T12:00:00.000Z");
    expect(serverClock.offsetMinutes()).toBe(-180);
  });

  it("corrige um aparelho com a hora errada", () => {
    vi.useFakeTimers();
    // O aparelho acha que são 12:00 UTC. A API diz que são 00:55 UTC do dia seguinte.
    vi.setSystemTime(new Date("2026-10-05T12:00:00Z"));
    serverClock.sync("2026-10-05T21:55:09-03:00");

    expect(serverClock.now().toISOString()).toBe("2026-10-06T00:55:09.000Z");
  });

  it("acompanha a passagem do tempo depois de sincronizar", () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date("2026-10-05T12:00:00Z"));
    serverClock.sync("2026-10-05T21:55:09-03:00");

    vi.advanceTimersByTime(5_000);

    expect(serverClock.now().toISOString()).toBe("2026-10-06T00:55:14.000Z");
  });

  it("guarda o deslocamento que a API informou", () => {
    serverClock.sync("2026-10-05T21:55:09-02:00");

    expect(serverClock.offsetMinutes()).toBe(-120);
  });

  it("ignora uma resposta com data inválida", () => {
    serverClock.sync("isso não é uma data");

    expect(serverClock.offsetMinutes()).toBe(-180);
  });
});
