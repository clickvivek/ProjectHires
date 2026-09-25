import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface ParsedJobDataDto {
  name: string;
  description?: string;
  jobLocation?: string;
  isRemote?: boolean;
  country?: string;
  postalcode?: string;
  totalExp?: number;
  fromAmt?: number;
  toAmt?: number;
  numberOfOpening?: number;
  skills?: string[];
  employmentTypes?: string[];
  visas?: string[];
  locations?: string[];
}

export interface EmailJobPostingQueueDto {
  id: number;
  senderEmail: string;
  senderName?: string;
  emailSubject?: string;
  rawEmailBodyText?: string;
  rawEmailBodyHtml?: string;
  userId?: number;
  recruiterName?: string;
  companyName?: string;
  consultancyId?: number;
  matchedEmailType?: 'Primary' | 'Alternate';
  status: 'Received' | 'Parsed' | 'Published' | 'Failed' | 'QuotaExceeded' | 'Rejected';
  parsedJobJson?: string;
  parsedJob?: ParsedJobDataDto;
  createdJobOpeningId?: number;
  errorMessage?: string;
  retryCount: number;
  receivedDate: string;
  processedDate?: string;
  updatedBy?: number;
}

export interface EmailJobPostingStatsDto {
  totalReceived: number;
  publishedCount: number;
  parsedCount: number;
  failedCount: number;
  quotaExceededCount: number;
  rejectedCount: number;
}

export interface SimulateInboundEmailDto {
  senderEmail: string;
  senderName?: string;
  emailSubject: string;
  emailBody: string;
  autoPublish?: boolean;
}

export interface ApproveEmailJobPostingDto {
  queueId: number;
  name: string;
  description: string;
  jobLocation?: string;
  totalExp?: number;
  fromAmt?: number;
  toAmt?: number;
  numberOfOpening?: number;
  skills?: string[];
  employmentTypes?: string[];
  visas?: string[];
  locations?: string[];
}

export interface RejectEmailJobPostingDto {
  queueId: number;
  reason?: string;
}

@Injectable({
  providedIn: 'root'
})
export class EmailJobPostingService {
  private baseUrl = `${environment.rootUrl}/api/EmailJobPosting`;

  constructor(private http: HttpClient) {}

  getAll(status?: string, search?: string, page: number = 1, pageSize: number = 20): Observable<{ value: EmailJobPostingQueueDto[] }> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (status && status !== 'all') {
      params = params.set('status', status);
    }
    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<{ value: EmailJobPostingQueueDto[] }>(`${this.baseUrl}/GetAll`, { params });
  }

  getCount(status?: string, search?: string): Observable<{ value: number }> {
    let params = new HttpParams();
    if (status && status !== 'all') {
      params = params.set('status', status);
    }
    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }
    return this.http.get<{ value: number }>(`${this.baseUrl}/Count`, { params });
  }

  getStats(): Observable<{ value: EmailJobPostingStatsDto }> {
    return this.http.get<{ value: EmailJobPostingStatsDto }>(`${this.baseUrl}/Stats`);
  }

  getById(id: number): Observable<{ value: EmailJobPostingQueueDto }> {
    return this.http.get<{ value: EmailJobPostingQueueDto }>(`${this.baseUrl}/${id}`);
  }

  process(id: number): Observable<{ value: EmailJobPostingQueueDto }> {
    return this.http.post<{ value: EmailJobPostingQueueDto }>(`${this.baseUrl}/Process/${id}`, {});
  }

  approve(dto: ApproveEmailJobPostingDto): Observable<{ value: any }> {
    return this.http.post<{ value: any }>(`${this.baseUrl}/Approve`, dto);
  }

  reject(dto: RejectEmailJobPostingDto): Observable<{ value: boolean }> {
    return this.http.post<{ value: boolean }>(`${this.baseUrl}/Reject`, dto);
  }

  delete(id: number): Observable<{ value: boolean }> {
    return this.http.delete<{ value: boolean }>(`${this.baseUrl}/${id}`);
  }

  simulate(dto: SimulateInboundEmailDto): Observable<{ value: EmailJobPostingQueueDto }> {
    return this.http.post<{ value: EmailJobPostingQueueDto }>(`${this.baseUrl}/Simulate`, dto);
  }
}
