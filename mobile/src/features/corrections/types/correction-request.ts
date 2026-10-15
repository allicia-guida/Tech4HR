export type CorrectionReason =
  "FORGOT_ENTRY" | "FORGOT_EXIT" | "WRONG_TIME" | "MEDICAL" | "OTHER";

export type AttachmentReference = {
  id: string;
  name: string;
  mimeType: string;
  size: number;
  localUri?: string;
  remoteUrl?: string;
};

export type CorrectionRequest = {
  id: string;
  employeeId: string;
  date: string;
  reason: CorrectionReason;
  justification: string;
  proposedEntries: {
    type: "CLOCK_IN" | "BREAK_START" | "BREAK_END" | "CLOCK_OUT";
    timestamp: string;
  }[];
  attachments: AttachmentReference[];
  status: "PENDING" | "APPROVED" | "REJECTED" | "CANCELED";
  reviewerId?: string;
  reviewerNote?: string;
  createdAt: string;
  reviewedAt?: string;
  history: {
    id: string;
    action: "CREATED" | "APPROVED" | "REJECTED" | "CANCELED";
    at: string;
    actor: string;
    note?: string;
  }[];
};

export type CreateCorrectionRequest = Pick<
  CorrectionRequest,
  | "employeeId"
  | "date"
  | "reason"
  | "justification"
  | "proposedEntries"
  | "attachments"
>;
