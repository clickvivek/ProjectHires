import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import * as moment from 'moment';
import { SessionService } from 'src/app/core/session/session.service';
import { CandidateProfileService } from 'src/app/api/api/candidate-profile.service';
import { JobOpeningService } from 'src/app/api';
import { SharedService } from '../../../shared/services/shared.service';

@Component({
  selector: 'dashboard-highlights',
  templateUrl: './dashboard-highlights.component.html',
  styleUrls: ['./dashboard-highlights.component.scss']
})
export class DashboardHighlightsComponent implements OnInit {

  candidateList: any[] = [];
  filteredCandidateList: any[] = [];
  searchTerm: string = '';
  user: any;
  isCollapsed: boolean = false; // Expanded by default
  totalHotlistCandidates: number = 0;
  resumesSubmittedLast30Days: number = 0;
  jobRequirementPosted: number = 0;
  expiredPostings: number = 0;
  resumesReceived: number = 0;
  isLoadingStats: boolean = true;

  inboxJobs: any[] = [];
  filteredInboxJobs: any[] = [];
  inboxSearchTerm: string = '';
  isLoadingInbox: boolean = true;

  constructor(
    private router: Router,
    private sessionService: SessionService,
    private sharedService: SharedService,
    private candidateProfileService: CandidateProfileService,
    private jobOpeningService: JobOpeningService
  ) { }

  ngOnInit() {
    this.sharedService.refreshinboxcast.subscribe(() => {
      if (this.showRecruitmentSection()) {
        this.fetchRecruiterStats();
        this.fetchInboxResumes();
      }
      if (this.showBenchSalesSection()) {
        this.fetchBenchSalesStats();
      }
    });

    this.sessionService.userdetailscast.subscribe((res: any) => {
      this.user = res;
      if (this.user) {
        if (this.showBenchSalesSection()) {
          this.fetchBenchSalesStats();
        }
        if (this.showRecruitmentSection()) {
          this.fetchRecruiterStats();
          this.fetchInboxResumes();
        }
      }
    });
  }

  fetchBenchSalesStats() {
    const userId = this.user?.id || this.sessionService.userId;
    const consultancyUserId = this.user?.consultancyUserId || this.sessionService.consultancyUserId;

    if (userId || consultancyUserId) {
      this.isLoadingStats = true;
      this.candidateProfileService.apiCandidateProfileGetBenchSalesStatsGet(userId, consultancyUserId).subscribe({
        next: (res: any) => {
          this.isLoadingStats = false;
          if (res && res.value) {
            this.totalHotlistCandidates = res.value.totalHotlistCandidates ?? 0;
            this.resumesSubmittedLast30Days = res.value.resumesSubmittedLast30Days ?? 0;
            this.candidateList = res.value.candidates || [];
            this.filteredCandidateList = [...this.candidateList];
          }
        },
        error: (error: any) => {
          this.isLoadingStats = false;
          console.error('Error fetching bench sales stats:', error);
        }
      });
    } else {
      this.isLoadingStats = false;
    }
  }

  fetchRecruiterStats() {
    const consultancyUserId = this.user?.consultancyUserId || this.sessionService.consultancyUserId;

    if (consultancyUserId) {
      this.jobOpeningService.apiJobOpeningGetRecruiterStatsGet(consultancyUserId).subscribe({
        next: (res: any) => {
          if (res && res.value) {
            this.jobRequirementPosted = res.value.jobRequirementPosted ?? 0;
            this.expiredPostings = res.value.expiredPostings ?? 0;
            this.resumesReceived = res.value.resumesReceived ?? 0;
          }
        },
        error: (error: any) => {
          console.error('Error fetching recruiter stats:', error);
        }
      });
    }
  }

  getRoleBadgeText(): string {
    if (!this.user) return 'Recruiter';
    if (this.user.roleBenchSales && this.user.roleRecruiter) return 'Recruiter / Bench Sales';
    if (this.user.roleBenchSales) return 'Bench Sales Recruiter / Manager';
    if (this.user.roleRecruiter) return 'Recruiter';
    if (this.user.userTypeId === 2) return 'Bench Sales Recruiter / Manager';
    return 'Recruiter';
  }

  showRecruitmentSection(): boolean {
    if (!this.user) return true;
    if (this.user.roleBenchSales && !this.user.roleRecruiter) return false;
    return true;
  }

  showBenchSalesSection(): boolean {
    if (!this.user) return true;
    if (this.user.roleRecruiter && !this.user.roleBenchSales) return false;
    return true;
  }

  isBenchSalesOnly(): boolean {
    return !!this.user?.roleBenchSales && !this.user?.roleRecruiter;
  }

  toggleCollapse() {
    this.isCollapsed = !this.isCollapsed;
  }

  fetchInboxResumes() {
    const consultancyUserId = this.user?.consultancyUserId || this.sessionService.consultancyUserId;
    if (!consultancyUserId) return;

    this.isLoadingInbox = true;
    this.jobOpeningService.apiJobOpeningGetJobOpeningProfileMapSummaryGet(consultancyUserId).subscribe({
      next: (res: any) => {
        this.isLoadingInbox = false;
        if (res && Array.isArray(res)) {
          this.inboxJobs = res;
          this.resumesReceived = this.inboxJobs.reduce((acc, job) => acc + this.getTotalCandidates(job), 0);
          this.filterInboxJobs();
        }
      },
      error: (err: any) => {
        this.isLoadingInbox = false;
        console.error('Error fetching inbox summary for dashboard:', err);
      }
    });
  }

  filterInboxJobs() {
    if (!this.inboxJobs || this.inboxJobs.length === 0) {
      this.filteredInboxJobs = [];
      return;
    }

    // Only show job postings with count more than 0 in NEW status
    let list = this.inboxJobs.filter(job => this.getNewCandidatesCount(job) > 0);

    if (this.inboxSearchTerm) {
      const term = this.inboxSearchTerm.toLowerCase().trim();
      list = list.filter(job => 
        (job.name && job.name.toLowerCase().includes(term)) ||
        (job.jobLocation && job.jobLocation.toLowerCase().includes(term))
      );
    }

    this.filteredInboxJobs = list;
  }

  getTotalCandidates(item: any): number {
    if (!item || !item.profileCountSummary) return 0;
    return item.profileCountSummary.reduce((acc: number, p: any) => acc + (p.count || 0), 0);
  }

  getNewCandidatesCount(item: any): number {
    if (!item || !item.profileCountSummary) return 0;
    const newProfile = item.profileCountSummary.find((p: any) => p.candidateProfileMappingStatusId === 1);
    return newProfile?.count || 0;
  }

  getDate(date: any): string {
    return moment(date).format('MMMM D, YYYY');
  }

  getStatusDisplayName(profile: any): string {
    if (!profile) return '';
    const id = profile.candidateProfileMappingStatusId;
    switch (id) {
      case 1: return 'New';
      case 2: return 'On Hold';
      case 3: return 'Shortlisted';
      case 4: return 'No Response';
      case 5: return 'Interview';
      case 6: return 'Rejected';
      case 7: return 'Submitted';
      default: return profile.candidateProfileMappingStatusName || '';
    }
  }

  getStatusTileClass(statusId: number, count: number): string {
    if (!count || count === 0) return 'tile-zero';
    switch (statusId) {
      case 1: return 'tile-new';
      case 2: return 'tile-on-hold';
      case 3: return 'tile-shortlisted';
      case 4: return 'tile-no-response';
      case 5: return 'tile-interview';
      case 6: return 'tile-rejected';
      case 7: return 'tile-submitted';
      default: return 'tile-default';
    }
  }

  handleJobNavigate(item: any, statusId?: number) {
    this.sharedService.setSideNavData(item);
    if (statusId) {
      this.router.navigate(['inbox', 'resumes', item.jobOpeningId, statusId]);
    } else {
      this.router.navigate(['inbox', 'jobdetails', item.jobOpeningId]);
    }
  }

  filterCandidates() {
    if (!this.searchTerm) {
      this.filteredCandidateList = [...this.candidateList];
      return;
    }
    const term = this.searchTerm.toLowerCase().trim();
    this.filteredCandidateList = this.candidateList.filter(c => 
      (c.name && c.name.toLowerCase().includes(term)) ||
      (c.title && c.title.toLowerCase().includes(term))
    );
  }

}
