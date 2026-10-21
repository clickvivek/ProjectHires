import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface JobOpeningAdminItemDto {
  id: number;
  jobTitle?: string;
  companyName?: string;
  companyId?: number;
  companyLogo?: string;
  postedByUserName?: string;
  postedByFullName?: string;
  postedByEmail?: string;
  postedByUserId?: number;
  consultancyUserId?: number;
  postedDate?: string;
  lastDate?: string;
  updated?: string;
  active?: boolean;
  isExpired?: boolean;
  jobLocation?: string;
  country?: string;
  totalExp?: number;
  numberOfOpening?: number;
  salaryDisplay?: string;
  applicationCount: number;
  source?: string;
  skills: string[];
}

export interface JobOpeningAdminResponseDto {
  totalCount: number;
  page: number;
  pageSize: number;
  items: JobOpeningAdminItemDto[];
}

@Injectable({
  providedIn: 'root'
})
export class JobPostingAdminService {
  private apiUrl = `${environment.rootUrl}/api/JobOpening`;

  constructor(private http: HttpClient) {}

  getAdminJobOpenings(search?: string, status?: string, source?: string, page: number = 1, pageSize: number = 500): Observable<{ value: JobOpeningAdminResponseDto; errors: any }> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search && search.trim().length > 0) {
      params = params.set('search', search.trim());
    }
    if (status && status.trim().length > 0 && status !== 'all') {
      params = params.set('status', status.trim());
    }
    if (source && source.trim().length > 0 && source !== 'all') {
      params = params.set('source', source.trim());
    }

    return this.http.get<{ value: JobOpeningAdminResponseDto; errors: any }>(`${this.apiUrl}/AdminJobOpenings`, { params });
  }
}
