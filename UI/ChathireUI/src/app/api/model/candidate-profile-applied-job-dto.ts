export interface CandidateProfileAppliedJobDto {
    id?: number;
    jobOpeningId?: number;
    jobTitle?: string;
    companyName?: string;
    companyLogo?: string;
    jobLocation?: string;
    fromAmt?: number;
    toAmt?: number;
    statusId?: number;
    statusName?: string;
    resumeDoc?: string;
    comment?: string;
    appliedDate?: string;
    isRead?: boolean;
}
