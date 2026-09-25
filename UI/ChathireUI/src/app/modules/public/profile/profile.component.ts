import { Component, OnInit, EventEmitter } from '@angular/core';
import { MatDialogRef } from "@angular/material/dialog";
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';

import { ConsultancyService, UserService } from 'src/app/api';
import { AuthService } from 'src/app/core/auth/auth.service';
import { SessionService } from 'src/app/core/session/session.service';
import { publicProfileUrlPrefix, picUrl } from 'src/app/data/various';

import _ from 'underscore';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss'],
  providers: [
    { provide: MatDialogRef, useValue: {} },
  ]
})
export class ProfileComponent implements OnInit {

  user:any;
  company: any = {};
  myJobs:any;
  myHotlist:any;

  selectedJobId:number = -1;
  selectedJob:any;

  isJobName:boolean = false;

  profileId:any;
  profileUrlPrefix = publicProfileUrlPrefix;
  profileUrl = ""
  isCopied: boolean = false;

  consultancyUserId:any;

  isLoaded:boolean = false;
  isProfileAvailable:boolean = false;
  isProfilePrivate:boolean = false;
  isCandidate:boolean = false;
  candidateData:any = null;
  
  activeTab: string = 'recruiters';
  isJobsLoaded: boolean = false;
  isCandidatesLoaded: boolean = false;
  hasJobs: boolean = false;
  hasCandidates: boolean = false;

  chatUser:any = {}

  jobDetailsChanged: EventEmitter<any> = new EventEmitter();

  constructor(
    public router: Router,
    private route: ActivatedRoute,
    private authService: AuthService,
    private userService: UserService,
    private consultancyService: ConsultancyService,
    private sessionService: SessionService
  ) { 
    router.events.subscribe((event: any) => {
      if (event instanceof NavigationEnd) {
        this.route.params.subscribe((params) => {
          if(params['id']) {
            this.profileId = params['id'];
            this.profileUrl = `${this.profileUrlPrefix}${this.profileId}`;
            this.fetchUser();
          }
        });
      }
    });
  }

  ngOnInit() {
    this.route.params.subscribe((params) => {
      if (params['id']) {
        this.profileId = params['id'];
        this.profileUrl = `${this.profileUrlPrefix}${this.profileId}`;
        this.fetchUser();
      }
    });
  }

  copyURL(text: string) {
    if (!text) return;
    navigator.clipboard.writeText(text);
    this.isCopied = true;
    setTimeout(() => {
      this.isCopied = false;
    }, 2000);
  }

  getRoleText(): string {
    if (!this.user) return 'Recruiter';
    if (this.user.roleBenchSales && this.user.roleRecruiter) return 'Recruiter / Bench Sales';
    if (this.user.roleBenchSales) return 'Bench Sales Recruiter / Manager';
    if (this.user.roleRecruiter) return 'Recruiter';
    return this.user.userTypeId === 2 ? 'Bench Sales Recruiter / Manager' : 'Recruiter';
  }

  getRoleBadgeText(): string {
    if (!this.user) return 'Recruiter';
    if (this.user.roleBenchSales && this.user.roleRecruiter) return 'Recruiter / Bench Sales';
    if (this.user.roleBenchSales) return 'Bench Sales';
    if (this.user.roleRecruiter) return 'Recruiter';
    return this.user.userTypeId === 2 ? 'Bench Sales' : 'Recruiter';
  }

  getTitleText(): string {
    if (!this.user) return 'Recruiter';
    if (this.user.title) return this.user.title;
    if (this.user.roleBenchSales && this.user.roleRecruiter) return 'Manager, Recruiter & Bench Sales';
    if (this.user.roleBenchSales) return 'Manager, Bench Sales';
    if (this.user.roleRecruiter) return 'Senior Technical Recruiter';
    return this.user.userTypeId === 2 ? 'Manager, Bench Sales' : 'Technical Recruiter';
  }

  getCleanWebsite(url: string): string {
    if (!url) return '';
    return url.replace(/^https?:\/\//i, '').replace(/\/$/, '');
  }

  getLinkedInUrl(): string {
    const link = this.user?.linkedin || this.company?.linkedin;
    if (!link) return '';
    if (link.startsWith('http://') || link.startsWith('https://')) {
      return link;
    }
    return `https://${link}`;
  }

  getUserInitials(): string {
    if (!this.user) return 'CH';
    const first = (this.user.fname || '').charAt(0).toUpperCase();
    const last = (this.user.lname || '').charAt(0).toUpperCase();
    return `${first}${last}` || 'CH';
  }

  getCompanyLogoUrl(logo: string | null | undefined): string {
    if (!logo) {
      return '';
    }
    if (logo.startsWith('http://') || logo.startsWith('https://') || logo.startsWith('data:image')) {
      return logo;
    }
    return `${picUrl}${logo}`;
  }

  getCompanyInitial(): string {
    const name = this.company?.name || 'C';
    return name.charAt(0).toUpperCase();
  }

  isLoggedIn() {
    return this.authService.isLoggedIn();
  }

  isPublic() {
    return this.sessionService.userId != this.user?.id;
  }

  isSelected(id){
    return this.selectedJobId == id ? 'selected' : '';
  }

  setActiveTab(tab: string) {
    if (tab === 'recruiters' && this.isJobsLoaded && !this.hasJobs) return;
    if (tab === 'benchsales' && this.isCandidatesLoaded && !this.hasCandidates) return;
    this.activeTab = tab;
  }

  handleJobs(event: any) {
    this.hasJobs = typeof event === 'object' ? !!event.hasData : !!event;
    this.isJobsLoaded = true;
    this.adjustActiveTab();
  }

  handleCandidates(event: any) {
    this.hasCandidates = typeof event === 'object' ? !!event.hasData : !!event;
    this.isCandidatesLoaded = true;
    this.adjustActiveTab();
  }

  adjustActiveTab() {
    if (this.activeTab === 'recruiters' && this.isJobsLoaded && !this.hasJobs && (!this.isCandidatesLoaded || this.hasCandidates)) {
      this.activeTab = 'benchsales';
    } else if (this.activeTab === 'benchsales' && this.isCandidatesLoaded && !this.hasCandidates && (!this.isJobsLoaded || this.hasJobs)) {
      this.activeTab = 'recruiters';
    }
  }

  showJobDescription(job){
    if(this.selectedJobId != job.id){
      this.selectedJobId = job.id;
    }
    else {
      this.selectedJobId = -1;
    }
    this.selectedJob = job;
    this.jobDetailsChanged.emit(this.selectedJob);
  }

  fetchProfile() {
    this.fetchUser();
  }

  fetchCompanyDetails(id) {
    this.consultancyService.apiConsultancyConsultancyByIdGet(id).subscribe({
      next: (res:any) => {
        this.company = res.value;
        this.chatUser.companyName = this.company?.name || '';
      },
      error: (error:any) => {
        console.log(error);
      }
    });
  }

  fetchUser() {
    this.isLoaded = false;
    this.isJobsLoaded = false;
    this.isCandidatesLoaded = false;
    this.hasJobs = false;
    this.hasCandidates = false;
    this.isProfilePrivate = false;

    this.userService.apiUserGetUserByPublicProfileIdGet(this.profileId).subscribe({
      next: (res: any) => {
        if (!_.isEmpty(res.value)) {
          let publicUser = res.value[0];

          if (publicUser.userTypeId === 5 || publicUser.directCandidateDetail) {
            this.isCandidate = true;
            this.candidateData = publicUser;
            this.user = publicUser;

            const isOptedIn = publicUser.directCandidateDetail?.isPublicProfileEnabled;
            const isOwner = this.sessionService.userId == publicUser.id;

            if (!isOptedIn && !isOwner) {
              this.isProfileAvailable = false;
              this.isProfilePrivate = true;
              this.isLoaded = true;
              return;
            }

            this.chatUser = {
              userId: publicUser.id,
              userFName: publicUser.fname,
              userLName: publicUser.lname,
              profilePic: publicUser.profilePic,
              profileUserName: publicUser.directCandidateDetail?.publicProfileSlug
            };

            this.isLoaded = true;
            this.isProfileAvailable = true;
            return;
          }

          this.isCandidate = false;
          let consultancyId = publicUser.consultancyUsers && publicUser.consultancyUsers[0]?.consultancyId;
          this.consultancyUserId = publicUser.consultancyUsers && publicUser.consultancyUsers[0]?.id;
          this.user = publicUser.consultancyUsers && publicUser.consultancyUsers[0]?.user;
          if (consultancyId) {
            this.fetchCompanyDetails(consultancyId);
          }

          const profileUserName = publicUser?.consultancyUsers && publicUser?.consultancyUsers[0]?.publicProfileUserName;

          this.chatUser = {
            userId: publicUser.id,
            userFName: publicUser.fname,
            userLName: publicUser.lname,
            profilePic: publicUser.profilePic,
            profileUserName: profileUserName
          };

          if (this.user?.roleBenchSales && !this.user?.roleRecruiter) {
            this.activeTab = 'benchsales';
          } else {
            this.activeTab = 'recruiters';
          }

          this.isLoaded = true;
          this.isProfileAvailable = true;
        } else {
          this.user = null;
          this.isCandidate = false;
          this.isLoaded = true;
          this.isProfileAvailable = false;
        }
      },
      error: (error: any) => {
        this.isLoaded = true;
        this.isProfileAvailable = false;
      },
    });
  }

  getCandidateLocation(): string {
    if (!this.candidateData) return '';
    if (this.candidateData.city) {
      return `${this.candidateData.city.cityName || this.candidateData.city.city1 || ''}${this.candidateData.city.stateCode ? ', ' + this.candidateData.city.stateCode : ''}`;
    }
    return this.candidateData.location || '';
  }

  getCandidatePreferredLocations(): string[] {
    const raw = this.candidateData?.directCandidateDetail?.preferredLocations;
    if (!raw) return [];
    try {
      const parsed = JSON.parse(raw);
      if (Array.isArray(parsed)) {
        return parsed.map((item: any) => {
          if (typeof item === 'string') return item;
          return item.city1 ? `${item.city1}${item.stateCode ? ', ' + item.stateCode : ''}` : (item.cityName || item.name || '');
        }).filter(Boolean);
      }
    } catch (e) {
      return raw.split(',').map((s: string) => s.trim()).filter(Boolean);
    }
    return [];
  }

  isCandidateOwner(): boolean {
    return this.sessionService.userId == this.candidateData?.id;
  }

  getCandidateResumeUrl(): string {
    const resumes = this.candidateData?.directCandidateResumes;
    if (resumes && resumes.length > 0) {
      const primary = resumes.find((r: any) => r.isPrimary) || resumes[0];
      return primary.blobUrl;
    }
    return '';
  }

}
