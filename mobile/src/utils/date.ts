import dayjs from "dayjs";
import "dayjs/locale/pt-br";

dayjs.locale("pt-br");
export const formatLongDate = (value: string | Date) => {
  const text = dayjs(value).format("dddd, D [de] MMMM [de] YYYY");
  return text.charAt(0).toUpperCase() + text.slice(1);
};
export const formatReceiptDate = (value: string | Date) =>
  dayjs(value).format("D [de] MMMM [de] YYYY");
export const formatTime = (value: string | Date) =>
  dayjs(value).format("HH:mm");
export const dateKey = (value: string | Date = new Date()) =>
  dayjs(value).format("YYYY-MM-DD");
export const normalizeTimestamp = (value?: string) => {
  const parsed = dayjs(value);
  return parsed.isValid() ? parsed.toISOString() : new Date().toISOString();
};
