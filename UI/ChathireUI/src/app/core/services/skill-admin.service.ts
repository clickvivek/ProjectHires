import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';

export interface AdminSkillItemDto {
  id: number;
  name: string;
  active: boolean;
  isUserDefined: boolean;
  updated?: string | null;
  updatedBy?: number | null;
}

export interface SkillsAdminSummaryDto {
  totalCount: number;
  systemDefinedCount: number;
  userDefinedCount: number;
  activeCount: number;
  inactiveCount: number;
  skills: AdminSkillItemDto[];
}

export interface BulkAddSkillsRequestDto {
  skillsText: string;
  isUserDefined: boolean;
  active: boolean;
}

export interface BulkAddSkillsResultDto {
  totalProcessed: number;
  addedCount: number;
  skippedDuplicateCount: number;
  skippedCount?: number;
  addedSkills: string[];
  skippedSkills: string[];
}

export interface CreateSkillRequestDto {
  name: string;
  isUserDefined: boolean;
  active: boolean;
}

export interface UpdateSkillRequestDto {
  id: number;
  name: string;
  active?: boolean | null;
  isUserDefined?: boolean | null;
}

export interface DeleteSkillResultDto {
  success: boolean;
  wasDeactivated: boolean;
  deactivatedOnly?: boolean;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class SkillAdminService {
  private baseUrl = `${environment.rootUrl}/api/Common`;

  constructor(private http: HttpClient) {}

  getSkillsAdmin(isUserDefined?: boolean, active?: boolean, search?: string): Observable<SkillsAdminSummaryDto> {
    let params = new HttpParams();
    if (isUserDefined !== undefined && isUserDefined !== null) {
      params = params.set('isUserDefined', isUserDefined.toString());
    }
    if (active !== undefined && active !== null) {
      params = params.set('active', active.toString());
    }
    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<any>(`${this.baseUrl}/AdminSkills`, { params }).pipe(
      map(res => res?.value ?? res)
    );
  }

  getSkillsAdminSummary(isUserDefined?: boolean, active?: boolean, search?: string): Observable<SkillsAdminSummaryDto> {
    return this.getSkillsAdmin(isUserDefined, active, search);
  }

  bulkAddSkills(request: BulkAddSkillsRequestDto): Observable<BulkAddSkillsResultDto> {
    return this.http.post<any>(`${this.baseUrl}/BulkAddSkills`, request).pipe(
      map(res => res?.value ?? res)
    );
  }

  addSkill(request: CreateSkillRequestDto): Observable<AdminSkillItemDto> {
    return this.http.post<any>(`${this.baseUrl}/AddSkill`, request).pipe(
      map(res => res?.value ?? res)
    );
  }

  updateSkill(request: UpdateSkillRequestDto): Observable<AdminSkillItemDto> {
    return this.http.put<any>(`${this.baseUrl}/UpdateSkill`, request).pipe(
      map(res => res?.value ?? res)
    );
  }

  toggleSkillStatus(id: number, active: boolean): Observable<AdminSkillItemDto> {
    return this.http.post<any>(`${this.baseUrl}/ToggleSkillStatus`, { id, active }).pipe(
      map(res => res?.value ?? res)
    );
  }

  convertSkillUserDefined(id: number, isUserDefined: boolean): Observable<boolean> {
    return this.http.post<any>(`${this.baseUrl}/ConvertSkillUserDefined`, { id, isUserDefined }).pipe(
      map(res => res?.value ?? res)
    );
  }

  deleteSkill(id: number): Observable<DeleteSkillResultDto> {
    return this.http.delete<any>(`${this.baseUrl}/DeleteSkill/${id}`).pipe(
      map(res => res?.value ?? res)
    );
  }
}
