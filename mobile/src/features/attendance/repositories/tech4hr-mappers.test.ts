import { afterEach, describe, expect, it, vi } from "vitest";
import { serverClock } from "@/utils/server-clock";
import { nextTimeEntryType, TimeEntryType } from "../types/time-entry";
import {
  appToTech4hrType,
  brasiliaLocalToIso,
  dayRecordToEntries,
  registrationToEntry,
  sortNewestFirst,
  tech4hrToAppType,
  Tech4hrDayRecord,
} from "./tech4hr-mappers";

afterEach(() => {
  serverClock.reset();
  vi.useRealTimers();
});

const fullDay: Tech4hrDayRecord = {
  idPonto: 7,
  dataPonto: "2026-10-05T00:00:00",
  entrada: "2026-10-05T08:03:00",
  saidaAlmoco: "2026-10-05T12:01:00",
  entradaAlmoco: "2026-10-05T13:03:00",
  saida: "2026-10-05T17:06:00",
};

describe("horário de Brasília para instante", () => {
  it("soma o deslocamento do fuso", () => {
    expect(brasiliaLocalToIso("2026-10-05T08:03:00", -180)).toBe(
      "2026-10-05T11:03:00.000Z",
    );
  });

  it("vira o dia quando a hora local passa das 21h", () => {
    expect(brasiliaLocalToIso("2026-10-05T22:30:00", -180)).toBe(
      "2026-10-06T01:30:00.000Z",
    );
  });
});

describe("tipos de batida", () => {
  it("cada tipo do app tem o seu da API e a volta é a mesma", () => {
    for (const type of Object.values(TimeEntryType)) {
      expect(tech4hrToAppType[appToTech4hrType[type]]).toBe(type);
    }
  });

  it("usa os nomes que a procedure aceita", () => {
    expect(appToTech4hrType[TimeEntryType.BREAK_START]).toBe("SAIDA_ALMOCO");
    expect(appToTech4hrType[TimeEntryType.BREAK_END]).toBe("ENTRADA_ALMOCO");
  });
});

describe("dia da API para batidas do app", () => {
  it("um dia completo vira quatro batidas na ordem da jornada", () => {
    const entries = dayRecordToEntries(fullDay, -180);

    expect(entries.map((item) => item.type)).toEqual([
      TimeEntryType.CLOCK_IN,
      TimeEntryType.BREAK_START,
      TimeEntryType.BREAK_END,
      TimeEntryType.CLOCK_OUT,
    ]);
    expect(entries[0].timestamp).toBe("2026-10-05T11:03:00.000Z");
    expect(entries[3].timestamp).toBe("2026-10-05T20:06:00.000Z");
    expect(entries[0].id).toBe("7-ENTRADA");
  });

  it("só gera as batidas que já aconteceram", () => {
    const entries = dayRecordToEntries(
      { ...fullDay, entradaAlmoco: null, saida: null },
      -180,
    );

    expect(entries).toHaveLength(2);
  });

  it("um dia sem batidas não gera nada", () => {
    const entries = dayRecordToEntries(
      {
        ...fullDay,
        entrada: null,
        saidaAlmoco: null,
        entradaAlmoco: null,
        saida: null,
      },
      -180,
    );

    expect(entries).toEqual([]);
  });

  it("a próxima batida respeita o dia de Brasília mesmo com o aparelho em UTC", () => {
    // Aparelho em UTC: já é 06/10. Em Brasília ainda é 05/10, 22:30.
    vi.useFakeTimers();
    vi.setSystemTime(new Date("2026-10-06T01:30:00Z"));

    const entries = dayRecordToEntries(
      { ...fullDay, entradaAlmoco: null, saida: null },
      -180,
    );

    // Entrada e saída para o almoço já foram: falta o retorno.
    expect(nextTimeEntryType(entries)).toBe(TimeEntryType.BREAK_END);
  });
});

describe("registro confirmado pela API", () => {
  it("vira a batida com o horário em instante e a localização do aparelho", () => {
    const entry = registrationToEntry(
      {
        idPonto: 7,
        tipoRegistro: "ENTRADA",
        dataHora: "2026-10-05T21:55:00-03:00",
        mensagem: "Ponto registrado com sucesso.",
      },
      {
        latitude: -23.55,
        longitude: -46.63,
        accuracyMeters: 12,
        capturedAt: "2026-10-06T00:54:50.000Z",
      },
    );

    expect(entry.type).toBe(TimeEntryType.CLOCK_IN);
    expect(entry.timestamp).toBe("2026-10-06T00:55:00.000Z");
    expect(entry.latitude).toBe(-23.55);
    expect(entry.id).toBe("7-ENTRADA");
  });
});

describe("ordem do histórico", () => {
  it("mostra a batida mais recente primeiro", () => {
    const entries = sortNewestFirst(dayRecordToEntries(fullDay, -180));

    expect(entries[0].type).toBe(TimeEntryType.CLOCK_OUT);
    expect(entries[3].type).toBe(TimeEntryType.CLOCK_IN);
  });
});
