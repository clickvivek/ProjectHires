import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface DirectCandidateDetails {
  headline?: string;
  summary?: string;
  visaId?: number;
  visaName?: string;
  visaExpiryDate?: string;
  totalYearsOfExp?: number;
  expectedAnnualSalary?: number;
  expectedHourlyRate?: number;
  preferredWorkType?: string;
  noticePeriodDays?: number;
  canRelocate?: boolean;
  remoteOnly?: boolean;
  isActivelyLooking?: boolean;
  gitHubUrl?: string;
  portfolioUrl?: string;
  isPublicProfileEnabled?: boolean;
  publicProfileSlug?: string;
  isShowCompensationPublic?: boolean;
  preferredLocations?: string;
}

export interface DirectCandidateResume {
  id: number;
  candidateUserId: number;
  fileName: string;
  blobUrl: string;
  fileSizeInKb?: number;
  isPrimary: boolean;
  uploadedDate: string;
}

export interface DirectCandidateExperience {
  id?: number;
  candidateUserId?: number;
  companyName: string;
  title: string;
  cityId?: number;
  cityName?: string;
  startDate: string;
  endDate?: string;
  isCurrent: boolean;
  description?: string;
}

export interface DirectCandidateEducation {
  id?: number;
  candidateUserId?: number;
  institution: string;
  degree: string;
  major?: string;
  startYear?: number;
  graduationYear?: number;
}

export interface CandidateAppliedJob {
  id: number;
  jobOpeningId: number;
  jobTitle: string;
  companyName: string;
  companyLogo?: string;
  jobLocation?: string;
  fromAmt?: number;
  toAmt?: number;
  statusId: number;
  statusName: string;
  resumeDoc?: string;
  comment?: string;
  appliedDate: string;
  isRead: boolean;
}

export interface CandidateFullProfile {
  userId: number;
  fname?: string;
  lname?: string;
  email?: string;
  phone?: string;
  linkedin?: string;
  cityId?: number;
  cityName?: string;
  profilePic?: string;
  details?: DirectCandidateDetails;
  resumes: DirectCandidateResume[];
  experiences: DirectCandidateExperience[];
  educations: DirectCandidateEducation[];
}

@Injectable({
  providedIn: 'root'
})
export class CandidateDirectService {
  private baseUrl = `${environment.rootUrl}/api/Candidate`;

  constructor(private http: HttpClient) {}

  getProfile(userId?: number): Observable<{ value: CandidateFullProfile }> {
    let params = new HttpParams();
    if (userId) params = params.set('userId', userId.toString());
    return this.http.get<{ value: CandidateFullProfile }>(`${this.baseUrl}/GetProfile`, { params });
  }

  saveDetails(details: DirectCandidateDetails, userId?: number): Observable<any> {
    let params = new HttpParams();
    if (userId) params = params.set('userId', userId.toString());
    return this.http.post<any>(`${this.baseUrl}/SaveDetails`, details, { params });
  }

  uploadResume(file: File, isPrimary: boolean = false, userId?: number): Observable<any> {
    const formData = new FormData();
    formData.append('File', file, file.name);
    formData.append('IsPrimary', isPrimary ? 'true' : 'false');

    let params = new HttpParams();
    if (userId) params = params.set('userId', userId.toString());
    return this.http.post<any>(`${this.baseUrl}/UploadResume`, formData, { params });
  }

  getResumes(userId?: number): Observable<{ value: DirectCandidateResume[] }> {
    let params = new HttpParams();
    if (userId) params = params.set('userId', userId.toString());
    return this.http.get<{ value: DirectCandidateResume[] }>(`${this.baseUrl}/GetResumes`, { params });
  }

  setPrimaryResume(resumeId: number, userId?: number): Observable<any> {
    let params = new HttpParams().set('resumeId', resumeId.toString());
    if (userId) params = params.set('userId', userId.toString());
    return this.http.put<any>(`${this.baseUrl}/SetPrimaryResume`, null, { params });
  }

  deleteResume(resumeId: number, userId?: number): Observable<any> {
    let params = new HttpParams();
    if (userId) params = params.set('userId', userId.toString());
    return this.http.delete<any>(`${this.baseUrl}/DeleteResume/${resumeId}`, { params });
  }

  addExperience(exp: DirectCandidateExperience, userId?: number): Observable<any> {
    let params = new HttpParams();
    if (userId) params = params.set('userId', userId.toString());
    return this.http.post<any>(`${this.baseUrl}/AddExperience`, exp, { params });
  }

  updateExperience(id: number, exp: DirectCandidateExperience, userId?: number): Observable<any> {
    let params = new HttpParams();
    if (userId) params = params.set('userId', userId.toString());
    return this.http.put<any>(`${this.baseUrl}/UpdateExperience/${id}`, exp, { params });
  }

  deleteExperience(id: number, userId?: number): Observable<any> {
    let params = new HttpParams();
    if (userId) params = params.set('userId', userId.toString());
    return this.http.delete<any>(`${this.baseUrl}/DeleteExperience/${id}`, { params });
  }

  addEducation(edu: DirectCandidateEducation, userId?: number): Observable<any> {
    let params = new HttpParams();
    if (userId) params = params.set('userId', userId.toString());
    return this.http.post<any>(`${this.baseUrl}/AddEducation`, edu, { params });
  }

  updateEducation(id: number, edu: DirectCandidateEducation, userId?: number): Observable<any> {
    let params = new HttpParams();
    if (userId) params = params.set('userId', userId.toString());
    return this.http.put<any>(`${this.baseUrl}/UpdateEducation/${id}`, edu, { params });
  }

  deleteEducation(id: number, userId?: number): Observable<any> {
    let params = new HttpParams();
    if (userId) params = params.set('userId', userId.toString());
    return this.http.delete<any>(`${this.baseUrl}/DeleteEducation/${id}`, { params });
  }

  getMyApplications(userId?: number): Observable<{ value: CandidateAppliedJob[] }> {
    let params = new HttpParams();
    if (userId) params = params.set('userId', userId.toString());
    return this.http.get<{ value: CandidateAppliedJob[] }>(`${this.baseUrl}/GetMyApplications`, { params });
  }

  applyDirect(applyData: { jobOpeningId: number; resumeId?: number; coverNote?: string }, userId?: number): Observable<any> {
    let params = new HttpParams();
    if (userId) params = params.set('userId', userId.toString());
    return this.http.post<any>(`${this.baseUrl}/ApplyDirect`, applyData, { params });
  }
}
