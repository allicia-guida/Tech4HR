import dayjs, { Dayjs } from "dayjs";
import utc from "dayjs/plugin/utc";
import "dayjs/locale/pt-br";
import { serverClock } from "./server-clock";

dayjs.extend(utc);
dayjs.locale("pt-br");

// Datas sem hora ("2026-10-05") são datas de calendário, não instantes.
// Convertê-las de fuso mudaria o dia, então ficam como estão.
const isDateOnly = (value: string | Date) =>
  typeof value === "string" && /^\d{4}-\d{2}-\d{2}$/.test(value);

// Todo horário mostrado ao funcionário é o de Brasília, qualquer que seja o
// fuso do aparelho. O deslocamento vem da API (veja server-clock).
const inBrasilia = (value: string | Date): Dayjs =>
  isDateOnly(value)
    ? dayjs(value)
    : dayjs(value).utcOffset(serverClock.offsetMinutes());

export const formatLongDate = (value: string | Date) => {
  const text = inBrasilia(value).format("dddd, D [de] MMMM [de] YYYY");
  return text.charAt(0).toUpperCase() + text.slice(1);
};
export const formatReceiptDate = (value: string | Date) =>
  inBrasilia(value).format("D [de] MMMM [de] YYYY");
export const formatTime = (value: string | Date) =>
  inBrasilia(value).format("HH:mm");
export const dateKey = (value: string | Date = serverClock.now()) =>
  inBrasilia(value).format("YYYY-MM-DD");
export const normalizeTimestamp = (value?: string) => {
  const parsed = dayjs(value);
  return parsed.isValid()
    ? parsed.toISOString()
    : serverClock.now().toISOString();
};
