import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { picUrl, defaultProfilePic } from 'src/app/data/various';

import * as moment from 'moment';
import _ from 'underscore';

import { ConfirmApplyJobComponent } from '../../public/searchjobs/confirm-apply-job/confirm-apply-job.component';
import { LoginModalComponent } from 'src/app/modules/shared/components/login-modal/login-modal.component';

import { UserService } from 'src/app/api/api/user.service';
import { JobOpeningService } from 'src/app/api/api/job-opening.service';
import { AuthService } from 'src/app/core/auth/auth.service';
import { SharedService } from 'src/app/modules/shared/services/shared.service';
import { SessionService } from 'src/app/core/session/session.service';
import { TalkService } from 'src/app/modules/talk/talk.service';

@Component({
  selector: 'app-job-details-page',
  templateUrl: './job-details-page.component.html',
  styleUrls: ['./job-details-page.component.scss']
})
export class JobDetailsPageComponent implements OnInit {

  previousQueryParams: Params = {};

  skill: string = "";
  location: string = "";
  cityId: any = null;
  visasId: any = [];
  wmIds: any = [];
  expIds: any = [];
  dateString: string = "";

  jobId: string | null = null;
  selectedJob: any = null;

  chatUser: any;
  isUserOnline: boolean = false;
  profilePicUrl: string = "";
  isLoaded: boolean = false;

  constructor(
    public dialog: MatDialog,
    public _router: Router,
    private route: ActivatedRoute,
    private userService: UserService,
    private talkService: TalkService,
    private authService: AuthService,
    private sharedService: SharedService,
    private jobOpeningService: JobOpeningService,
    private sessionService: SessionService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.loadJobDetails(params);
    });
  }

  parseJobId(rawId: any): string | null {
    if (!rawId) return null;
    let strId = String(rawId).trim();

    try {
      strId = decodeURIComponent(strId);
    } catch (e) {}

    // 1. Plain numeric ID
    if (/^\d+$/.test(strId)) {
      return strId;
    }

    // 2. Direct 'chathire_1040'
    if (strId.startsWith('chathire_')) {
      const extracted = strId.replace('chathire_', '');
      if (/^\d+$/.test(extracted)) return extracted;
    }

    // 3. Base64 decoded (e.g. 'Y2hhdGhpcmVfMTA0MA==' -> 'chathire_1040')
    try {
      const decoded = atob(strId);
      if (decoded.startsWith('chathire_')) {
        const extracted = decoded.replace('chathire_', '');
        if (/^\d+$/.test(extracted)) return extracted;
      }
      if (/^\d+$/.test(decoded)) {
        return decoded;
      }
    } catch (e) {
      // not valid base64
    }

    // 4. Legacy hyphenated format (e.g. 3OTb-JZOA-6wrN-W1042z-OZKnemklKt-T1sk-UOE2-tx7N)
    if (strId.includes('-')) {
      const parts = strId.split('-');
      if (parts.length >= 4 && parts[3].length > 2) {
        const inner = parts[3].substring(1, parts[3].length - 1);
        if (/^\d+$/.test(inner)) {
          return inner;
        }
      }
      for (const part of parts) {
        const match = part.match(/\d+/);
        if (match && match[0].length >= 1) {
          return match[0];
        }
      }
    }

    return null;
  }

  loadJobDetails(params: Params): void {
    if (!params || _.isEmpty(params)) {
      this.isLoaded = true;
      return;
    }

    this.previousQueryParams = params;
    this.skill = params['skill'] || "";
    this.location = params['location'] || "";
    this.cityId = !_.isUndefined(params['cid']) ? params['cid'] : null;
    this.visasId = !_.isUndefined(params['visas']) ? params['visas']?.split(',') : [];
    this.wmIds = !_.isUndefined(params['wm']) ? params['wm']?.split(',') : [];
    this.expIds = !_.isUndefined(params['exp']) ? params['exp']?.split(',') : [];
    this.dateString = !_.isUndefined(params['date']) ? params['date'] : "";

    const rawId = params['id'];
    this.jobId = this.parseJobId(rawId);

    this.handleJobOpenings(this.skill, this.cityId, this.visasId, this.wmIds, this.expIds, this.dateString);
  }

  handleJobOpenings(skill: string, cityIdArr: any, visasArr: any[], employmentTypesArr: any[], expArr: any[], dateString: string): void {
    this.isLoaded = false;

    const searchStrings = skill && skill.trim().length > 0 ? [skill.trim()] : undefined;
    const cityIds = cityIdArr ? [cityIdArr] : undefined;
    const visas = visasArr && visasArr.length > 0 ? visasArr : undefined;
    const employmentTypes = employmentTypesArr && employmentTypesArr.length > 0 ? employmentTypesArr : undefined;

    this.jobOpeningService.apiJobOpeningSearchJobOpeningsGet(
      searchStrings,
      cityIds,
      undefined,
      visas,
      employmentTypes,
      undefined,
      undefined,
      undefined,
      undefined,
      undefined,
      undefined,
      undefined
    ).subscribe({
      next: (res: any[]) => {
        let matchedJob = null;
        if (this.jobId && Array.isArray(res)) {
          matchedJob = res.find(item => String(item.jobOpeningId) === String(this.jobId));
        }

        if (matchedJob) {
          this.selectedJob = matchedJob;
          this.isLoaded = true;
          this.loadUserPresence(this.selectedJob);
        } else if (this.jobId) {
          // If not found in filtered search, try searching all jobs or direct by ID
          this.fetchJobFallback(this.jobId);
        } else if (Array.isArray(res) && res.length > 0) {
          this.selectedJob = res[0];
          this.isLoaded = true;
          this.loadUserPresence(this.selectedJob);
        } else {
          this.selectedJob = null;
          this.isLoaded = true;
        }
      },
      error: (error: any) => {
        console.error('Error searching jobs:', error);
        if (this.jobId) {
          this.fetchJobFallback(this.jobId);
        } else {
          this.selectedJob = null;
          this.isLoaded = true;
        }
      }
    });
  }

  fetchJobFallback(jobId: string): void {
    // 1. First try search without any filter
    this.jobOpeningService.apiJobOpeningSearchJobOpeningsGet().subscribe({
      next: (res: any[]) => {
        const found = Array.isArray(res) ? res.find(item => String(item.jobOpeningId) === String(jobId)) : null;
        if (found) {
          this.selectedJob = found;
          this.isLoaded = true;
          this.loadUserPresence(this.selectedJob);
        } else {
          // 2. Direct by ID fallback
          this.fetchJobOpeningById(Number(jobId));
        }
      },
      error: () => {
        this.fetchJobOpeningById(Number(jobId));
      }
    });
  }

  fetchJobOpeningById(jobId: number): void {
    if (isNaN(jobId)) {
      this.selectedJob = null;
      this.isLoaded = true;
      return;
    }

    this.jobOpeningService.apiJobOpeningJobOpeningByIdGet(jobId).subscribe({
      next: (res: any) => {
        this.isLoaded = true;
        const job = res?.value || res;
        if (job && (job.id || job.jobOpeningId)) {
          this.selectedJob = {
            jobOpeningId: String(job.id || job.jobOpeningId),
            jobOpeningName: job.name || job.jobOpeningName,
            companyName: job.country || job.companyName || '',
            companyLogo: job.companyLogo || null,
            jobDescription: job.description || job.jobDescription,
            postedDate: job.postedDate,
            lastDate: job.lastDate,
            totalExp: job.totalExp ? String(job.totalExp) : '',
            numberOfOpening: job.numberOfOpening ? String(job.numberOfOpening) : '',
            userId: job.consultancyUserId ? String(job.consultancyUserId) : (job.userId ? String(job.userId) : ''),
            userName: job.userName || '',
            userFName: job.userFName || '',
            userLName: job.userLName || '',
            profilePic: job.profilePic || '',
            skills: Array.isArray(job.skills) ? job.skills : (job.jobOpeningSkills?.map((s: any) => s.skill?.name || s.skillId).filter(Boolean) || []),
            locations: Array.isArray(job.locations) ? job.locations : (job.jobOpeningLocations?.map((l: any) => l.city?.cityName ? `${l.city.cityName}, ${l.city.idStateNavigation?.stateName || ''}` : '').filter(Boolean) || []),
            visas: Array.isArray(job.visas) ? job.visas : (job.jobOpeningVisaMaps?.map((v: any) => v.visa?.name || v.visaId).filter(Boolean) || []),
            employmentTypes: Array.isArray(job.employmentTypes) ? job.employmentTypes : (job.jobOpeningEmploymentTypes?.map((e: any) => e.employmentType?.name || e.employmentTypeId).filter(Boolean) || []),
            jobTypes: Array.isArray(job.jobTypes) ? job.jobTypes : (job.jobOpeningJobTypes?.map((j: any) => j.jobType?.name || j.jobTypeId).filter(Boolean) || []),
          };
          if (this.selectedJob.userName) {
            this.loadUserPresence(this.selectedJob);
          }
        } else {
          this.selectedJob = null;
        }
      },
      error: (err: any) => {
        console.error('Error fetching job by ID:', err);
        this.selectedJob = null;
        this.isLoaded = true;
      }
    });
  }

  loadUserPresence(job: any): void {
    if (!job || !job.userName) return;

    this.userService.apiUserGetUserByUserNameGet(job.userName).subscribe({
      next: (res: any) => {
        const user = res?.value?.[0];
        const profileUserName = user?.consultancyUsers?.[0]?.publicProfileUserName;
        this.chatUser = { ...job, profileUserName };

        if (this.chatUser?.userId) {
          this.talkService.fetchTalkUserPresence(this.chatUser.userId).subscribe({
            next: (res: any) => {
              const newData = res?.data;
              const userData = newData?.[this.chatUser.userId];
              this.isUserOnline = userData?.status === 'online';
            },
            error: () => {
              this.isUserOnline = false;
            }
          });
        }
      },
      error: () => {}
    });
  }

  isChatDisabled(): boolean {
    return this.selectedJob?.userId == this.sessionService.userId;
  }

  getProfilePic(url: string | null | undefined): string {
    if (url) return `${picUrl}${url}`;
    return defaultProfilePic;
  }

  getFormattedLocations(locations: any): string {
    if (!locations || !Array.isArray(locations) || locations.length === 0) {
      return '';
    }
    const formatted = locations
      .filter(loc => loc && typeof loc === 'string' && loc.trim().length > 0)
      .map(loc => {
        let trimmed = loc.trim().replace(/[,;\s]+$/, '');
        if (trimmed.includes('-')) {
          const parts = trimmed.split('-');
          return parts.map(p => p.trim()).filter(p => p).join(', ');
        }
        if (trimmed.includes(',')) {
          const parts = trimmed.split(',');
          return parts.map(p => p.trim()).filter(p => p).join(', ');
        }
        return trimmed;
      })
      .filter(str => str.length > 0);

    return formatted.join(' ; ');
  }

  copyURL(): void {
    navigator.clipboard.writeText(window.location.href);
  }

  handleApplyJobConfirmModal(): void {
    if (!this.selectedJob) return;

    if (this.authService.isLoggedIn()) {
      this.dialog.open(ConfirmApplyJobComponent, {
        panelClass: ['material', 'confirm-apply-modal-panel'],
        maxHeight: '90vh',
        disableClose: true,
        data: this.selectedJob
      });
    } else {
      const loginDialogRef = this.dialog.open(LoginModalComponent, {
        width: '440px',
        panelClass: 'login-modal-panel',
        data: { actionText: 'apply for this job' }
      });

      loginDialogRef.afterClosed().subscribe(res => {
        if (res && res.success) {
          this.dialog.open(ConfirmApplyJobComponent, {
            panelClass: ['material', 'confirm-apply-modal-panel'],
            maxHeight: '90vh',
            disableClose: true,
            data: this.selectedJob
          });
        }
      });
    }
  }

  getPostedDays(date: any): string {
    if (!date) return '';
    const target = moment(date);
    if (!target.isValid()) return '';
    const current = moment();
    const differenceInDays = current.clone().startOf('day').diff(target.clone().startOf('day'), 'days');
    if (differenceInDays <= 0) {
      return 'Posted today';
    } else if (differenceInDays > 30) {
      const differenceInMonths = current.diff(target, 'months');
      return `Posted ${differenceInMonths} months ago`;
    } else if (differenceInDays === 1) {
      const differenceInHours = current.diff(target, 'hours');
      if (differenceInHours > 0 && differenceInHours < 24) {
        return `Posted ${differenceInHours} hours ago`;
      }
      return `Posted 1 day ago`;
    } else {
      return `Posted ${differenceInDays} days ago`;
    }
  }

  getCompanyLogoUrl(logo: string | null | undefined): string {
    if (!logo) return '';
    if (logo.startsWith('http://') || logo.startsWith('https://') || logo.startsWith('data:image')) return logo;
    return `${picUrl}${logo}`;
  }

  getCompanyInitial(name: string | null | undefined): string {
    if (!name) return 'C';
    return name.trim().charAt(0).toUpperCase();
  }
}

