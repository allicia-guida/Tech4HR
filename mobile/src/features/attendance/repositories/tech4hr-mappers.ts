import { TimeEntry, TimeEntryType } from "../types/time-entry";

// Tradução entre o modelo do aplicativo (uma batida = um registro) e o da
// API do Tech4Hr (um dia = uma linha com quatro horários).

export type Tech4hrRegistrationType =
  | "ENTRADA"
  | "SAIDA_ALMOCO"
  | "ENTRADA_ALMOCO"
  | "SAIDA";

// Uma linha de GET /api/pontos/meus-pontos e o campo "ponto" de /hoje.
// Os horários são de Brasília, sem deslocamento: "2026-10-05T08:03:00".
export type Tech4hrDayRecord = {
  idPonto: number;
  dataPonto: string;
  entrada: string | null;
  saidaAlmoco: string | null;
  entradaAlmoco: string | null;
  saida: string | null;
};

// Resposta de POST /api/pontos/registrar.
export type Tech4hrRegistration = {
  idPonto: number;
  tipoRegistro: Tech4hrRegistrationType;
  dataHora: string;
  mensagem: string;
};

export const appToTech4hrType: Record<TimeEntryType, Tech4hrRegistrationType> =
  {
    [TimeEntryType.CLOCK_IN]: "ENTRADA",
    [TimeEntryType.BREAK_START]: "SAIDA_ALMOCO",
    [TimeEntryType.BREAK_END]: "ENTRADA_ALMOCO",
    [TimeEntryType.CLOCK_OUT]: "SAIDA",
  };

export const tech4hrToAppType: Record<Tech4hrRegistrationType, TimeEntryType> =
  {
    ENTRADA: TimeEntryType.CLOCK_IN,
    SAIDA_ALMOCO: TimeEntryType.BREAK_START,
    ENTRADA_ALMOCO: TimeEntryType.BREAK_END,
    SAIDA: TimeEntryType.CLOCK_OUT,
  };

// Os quatro horários do dia, na ordem da jornada.
const slots: {
  field: keyof Pick<
    Tech4hrDayRecord,
    "entrada" | "saidaAlmoco" | "entradaAlmoco" | "saida"
  >;
  type: Tech4hrRegistrationType;
}[] = [
  { field: "entrada", type: "ENTRADA" },
  { field: "saidaAlmoco", type: "SAIDA_ALMOCO" },
  { field: "entradaAlmoco", type: "ENTRADA_ALMOCO" },
  { field: "saida", type: "SAIDA" },
];

const pad = (value: number) => String(value).padStart(2, "0");

// "2026-10-05T08:03:00" (Brasília) + -180 minutos => instante em UTC (ISO).
export const brasiliaLocalToIso = (
  local: string,
  offsetMinutes: number,
): string => {
  const sign = offsetMinutes < 0 ? "-" : "+";
  const absolute = Math.abs(offsetMinutes);
  const suffix = `${sign}${pad(Math.floor(absolute / 60))}:${pad(absolute % 60)}`;
  return new Date(`${local.slice(0, 19)}${suffix}`).toISOString();
};

const entry = (
  idPonto: number,
  type: Tech4hrRegistrationType,
  iso: string,
): TimeEntry => ({
  id: `${idPonto}-${type}`,
  employeeId: "current",
  type: tech4hrToAppType[type],
  timestamp: iso,
  source: "APP_SMARTPHONE",
  deviceId: "server",
  createdAt: iso,
});

// Um dia da API vira de zero a quatro batidas.
export const dayRecordToEntries = (
  record: Tech4hrDayRecord,
  offsetMinutes: number,
): TimeEntry[] =>
  slots.flatMap(({ field, type }) => {
    const local = record[field];
    return local
      ? [entry(record.idPonto, type, brasiliaLocalToIso(local, offsetMinutes))]
      : [];
  });

// Resposta do registro vira a batida confirmada. A localização é do aparelho:
// a API do Tech4Hr ainda não guarda coordenadas, então ela serve só para o
// comprovante exibido agora.
export const registrationToEntry = (
  response: Tech4hrRegistration,
  location?: {
    latitude: number;
    longitude: number;
    accuracyMeters: number;
    capturedAt: string;
  },
): TimeEntry => {
  const iso = new Date(response.dataHora).toISOString();
  return {
    ...entry(response.idPonto, response.tipoRegistro, iso),
    latitude: location?.latitude,
    longitude: location?.longitude,
    accuracyMeters: location?.accuracyMeters,
    locationCapturedAt: location?.capturedAt,
  };
};

export const sortNewestFirst = (entries: TimeEntry[]): TimeEntry[] =>
  [...entries].sort(
    (a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime(),
  );
