import { AttachmentReference } from "@/features/corrections/types/correction-request";

export type AbsenceRequest = {
  id: string;
  employeeId: string;
  type: "VACATION" | "ABSENCE" | "MEDICAL_LEAVE";
  startDate: string;
  endDate: string;
  reason: string;
  attachments: AttachmentReference[];
  status: "PENDING" | "APPROVED" | "REJECTED" | "CANCELED";
  createdAt: string;
};

export type CreateAbsenceRequest = Omit<
  AbsenceRequest,
  "id" | "status" | "createdAt"
>;
