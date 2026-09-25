import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface PromocodeDto {
  id: number;
  promocode: string;
  description?: string;
  noOfFreeDownloads?: number;
  noOfFreeJobPosting?: number;
  discountAmount?: number;
  dailyChatLimit?: number;
  startDate?: string;
  endDate?: string;
  active?: boolean;
  updated?: string;
  updatedBy?: number;
  redemptionCount?: number;
  isSingleUse?: boolean;
  maxRedemptions?: number | null;
  isExpired?: boolean;
}

export interface CreatePromocodeDto {
  promocode: string;
  description?: string | null;
  startDate: string;
  endDate: string;
  noOfFreeDownloads?: number | null;
  noOfFreeJobPosting?: number | null;
  discountAmount?: number | null;
  dailyChatLimit?: number | null;
  isSingleUse?: boolean | null;
  maxRedemptions?: number | null;
  active?: boolean | null;
}

export interface RedeemPromocodeDto {
  promocode: string;
  userId?: number;
}

export interface RedeemPromocodeResponseDto {
  success: boolean;
  message: string;
  promocode?: string;
  freeDownloadsGranted: number;
  freeJobPostingGranted: number;
  dailyChatLimitGranted: number;
  discountAmount: number;
  startDate?: string;
  endDate?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PromocodeService {
  private baseUrl = `${environment.rootUrl}/api/Promocode`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/All`);
  }

  create(dto: CreatePromocodeDto): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/Create`, dto);
  }

  delete(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/${id}`);
  }

  toggleStatus(id: number): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}/toggle-status`, {});
  }

  redeem(dto: RedeemPromocodeDto): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/Redeem`, dto);
  }
}
