import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface ReferralSkipDetailDto {
  email: string;
  reason: string;
}

export interface SubmitReferralRequestDto {
  emails: string[];
  customMessage?: string;
}

export interface SubmitReferralResponseDto {
  success: boolean;
  message: string;
  totalSubmitted: number;
  successfullyInvited: number;
  alreadyRegistered: number;
  alreadyInvited: number;
  invalidEmails: number;
  invitedEmails?: string[];
  skippedDetails?: ReferralSkipDetailDto[];
}

export interface UserReferralDto {
  id: number;
  referredEmail: string;
  referralCode: string;
  status: string;
  referredUserId?: number;
  rewardClaimed: boolean;
  createdDate: string;
  registeredDate?: string;
  rewardGrantedDate?: string;
}

export interface ReferralStatsDto {
  totalInvited: number;
  totalRegistered: number;
  freePostingsEarned: number;
  freeMonthsEarned: number;
  referralCode: string;
  referralLink: string;
  referrals: UserReferralDto[];
}

@Injectable({
  providedIn: 'root'
})
export class ReferralService {
  private baseUrl = `${environment.rootUrl}/api/Referral`;

  constructor(private http: HttpClient) {}

  submitReferrals(dto: SubmitReferralRequestDto): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/Submit`, dto);
  }

  getReferralStats(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Stats`);
  }

  processSignup(email: string, referralCode: string, newUserId: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/ProcessSignup?email=${encodeURIComponent(email)}&referralCode=${encodeURIComponent(referralCode)}&newUserId=${newUserId}`, {});
  }
}
