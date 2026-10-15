export const formatMinutes = (total: number) => {
  const sign = total < 0 ? "-" : "";
  const absolute = Math.abs(total);
  const hours = Math.floor(absolute / 60);
  const minutes = absolute % 60;
  return `${sign}${hours}h ${String(minutes).padStart(2, "0")}min`;
};

export const formatMinuteOfDay = (value: number) =>
  `${String(Math.floor(value / 60)).padStart(2, "0")}:${String(value % 60).padStart(2, "0")}`;
