import { demoEmployee } from "@/features/profile/fixtures/demo-employee";
import { Absence } from "../types/work-policy";

export const demoAbsences: Absence[] = [
  {
    id: "absence-2026-09-07",
    employeeId: demoEmployee.id,
    date: "2026-09-07",
    type: "VACATION",
    status: "APPROVED",
    reason: "Férias programadas",
  },
  {
    id: "absence-2026-09-08",
    employeeId: demoEmployee.id,
    date: "2026-09-08",
    type: "VACATION",
    status: "APPROVED",
    reason: "Férias programadas",
  },
  {
    id: "absence-2026-09-11",
    employeeId: demoEmployee.id,
    date: "2026-09-11",
    type: "MEDICAL_LEAVE",
    status: "APPROVED",
    reason: "Afastamento médico",
  },
];
