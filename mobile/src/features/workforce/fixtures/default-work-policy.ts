import { WorkPolicy } from "../types/work-policy";

export const defaultWorkPolicy: WorkPolicy = {
  id: "standard-40h",
  name: "Jornada padrão",
  timeZone: "America/Sao_Paulo",
  lateToleranceMinutes: 5,
  overtimeToleranceMinutes: 5,
  minimumBreakMinutes: 60,
  allowNegativeTimeBank: true,
  schedule: [
    {
      weekday: 0,
      enabled: false,
      startMinute: 0,
      endMinute: 0,
      breakMinutes: 0,
    },
    {
      weekday: 1,
      enabled: true,
      startMinute: 480,
      endMinute: 1020,
      breakMinutes: 60,
    },
    {
      weekday: 2,
      enabled: true,
      startMinute: 480,
      endMinute: 1020,
      breakMinutes: 60,
    },
    {
      weekday: 3,
      enabled: true,
      startMinute: 480,
      endMinute: 1020,
      breakMinutes: 60,
    },
    {
      weekday: 4,
      enabled: true,
      startMinute: 480,
      endMinute: 1020,
      breakMinutes: 60,
    },
    {
      weekday: 5,
      enabled: true,
      startMinute: 480,
      endMinute: 1020,
      breakMinutes: 60,
    },
    {
      weekday: 6,
      enabled: false,
      startMinute: 0,
      endMinute: 0,
      breakMinutes: 0,
    },
  ],
  holidays: [
    {
      id: "holiday-2026-01-01",
      date: "2026-01-01",
      name: "Confraternização Universal",
      scope: "NATIONAL",
    },
    {
      id: "holiday-2026-12-25",
      date: "2026-12-25",
      name: "Natal",
      scope: "NATIONAL",
    },
  ],
};
