import { Component, ViewChild, HostListener, ElementRef, OnInit, OnDestroy } from '@angular/core';
import {  Router, ActivatedRoute } from '@angular/router';
import * as moment from 'moment';
import _ from 'underscore';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { profileInitialCountSummary } from 'src/app/data/various';
import { JobOpeningService } from 'src/app/api';
import { SessionService } from 'src/app/core/session/session.service';
import { ToastrService } from 'ngx-toastr';
import { SharedService } from '../../shared/services/shared.service';

@Component({
  selector: 'app-inbox',
  templateUrl: './inbox.component.html',
  styleUrls: ['./inbox.component.scss']
})
export class InboxComponent implements OnInit, OnDestroy {

  private destroy$ = new Subject<void>();

  isLoaded:boolean = false;
  isError:boolean = true;

  totalItems;
  searchData: string = ""

  sortList:any = [];
  orderList:any = [];

  initialData:any[] = [];
  filteredData: any[] = [];

  profileInitialCountSummary:any[] = profileInitialCountSummary;

  error: string = "";

  isActive:boolean = true;

  isOpened:boolean = false;

  selectedJob = null

  isJobDescriptionSelected:boolean = false;
  isResumeTypeSelected:boolean = false;

  @ViewChild('sideNav ') sideNav :ElementRef

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private sessionService: SessionService,
    private sharedService: SharedService,
    private toastr: ToastrService,
    private jobOpeningService: JobOpeningService
  ) {
  }

  getDate(date) {
    return moment(date).format('MMMM D, YYYY')
  }

  handleSearch(event:any) {
    this.searchData = event
    this.filterSearchData()
  }

  handleActiveList(event) {
    this.isActive = event.status
    this.filterSearchData()
  }

  handleActive(event) {
    
    let newItem = event.item

    this.jobOpeningService.apiJobOpeningActivateDeactivateJobPut(newItem.jobOpeningId, event.status).subscribe({
      next:(res) => {
        this.toastr.success(`Job ${res.value?.active ? "activated" : "deactivated"} successfully`, '' , {
          timeOut: 1000,
          positionClass: 'toast-top-center'
        });
      },
      error:(res) => {
        this.toastr.error('Some error occured', '' , {
          timeOut: 1000,
          positionClass: 'toast-top-center'
        });
      }
    })
  }

  filterSearchData() {
    
    this.filteredData = this.initialData.filter((candidate) =>
      this.isMatch(candidate, this.searchData)
    );

    this.filteredData = this.filteredData.filter((item:any) => {
      return item.active == this.isActive
    })
    this.totalItems = this.filteredData.length;
  }

  isMatch(candidate, term): boolean {
    term = term.toLowerCase();
    return (
      candidate.name.toLowerCase().includes(term) ||
      candidate.jobLocation?.toLowerCase().includes(term)
    );
  }

  handleResumeType(item, typeId) {
    this.sharedService.setSideNavData(item)
    this.router.navigate(['resumes', item.jobOpeningId, typeId], { relativeTo: this.route });
  }

  handleJob(item) {
    this.sharedService.setSideNavData(item)
    this.router.navigate(['jobdetails', item.jobOpeningId], { relativeTo: this.route });
  }

  isDataAvailable() {
    return this.totalItems != 0
  }

 
  fetchProfiles(showLoader: boolean = true) {
    if (showLoader) {
      this.isLoaded = false;
    }

    this.jobOpeningService.apiJobOpeningGetJobOpeningProfileMapSummaryGet(this.sessionService.consultancyUserId).subscribe({
      next:(res:any) => {
        this.isLoaded = true;
        this.isError = false;

        if(!_.isEmpty(res)) {
          this.initialData = res;
  
          this.initialData.forEach((item:any) => {
            let profileFilteredCountSummary:any = [];
            let existingProfileCountSummary = item.profileCountSummary;

            this.profileInitialCountSummary.forEach((profileOne:any) => {
              let matchObj = existingProfileCountSummary.find(profileTwo => profileTwo.candidateProfileMappingStatusId === profileOne.candidateProfileMappingStatusId);
              if(matchObj) {
                profileFilteredCountSummary.push(matchObj);
              }
              else {
                profileFilteredCountSummary.push(profileOne);
              }
            });

            item.profileCountSummary = profileFilteredCountSummary;
          });

          this.filterSearchData();
        }
        else {
          this.initialData = [];
          this.filteredData = [];
          this.totalItems = 0;
        }
      },
      error:(error:any) => {
        this.isError = true;
        this.isLoaded = true;
        this.error = 'Some error occured';
      }
    });
  }
 
  ngOnInit() {
    this.sortList = [
      { id: 1, name: "Posted Date" },
      { id: 2, name: "Title" }
    ];
    
    this.orderList = [
      { id: 1, name: "Ascending" },
      { id: 2, name: "Descending" }
    ];

    this.sharedService.refreshinboxcast
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.fetchProfiles(false);
      });

    if (this.sessionService.consultancyUserId) {
      this.fetchProfiles();
    } else {
      this.sessionService.userdetailscast
        .pipe(takeUntil(this.destroy$))
        .subscribe((user: any) => {
          if (user && this.sessionService.consultancyUserId) {
            this.fetchProfiles();
          }
        });
    }
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  getTotalCandidates(item: any): number {
    if (!item || !item.profileCountSummary) return 0;
    return item.profileCountSummary.reduce((acc: number, p: any) => acc + (p.count || 0), 0);
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

  getStatusTooltip(statusId: number | any): string {
    const id = typeof statusId === 'object' ? statusId?.candidateProfileMappingStatusId : statusId;
    switch (id) {
      case 1:
        return 'New candidate received and awaiting review';
      case 2:
        return 'Application is paused temporarily';
      case 3:
        return 'Candidate passed screening and is shortlisted';
      case 4:
        return 'Waiting for candidate response';
      case 6:
        return 'Candidate is not moving forward';
      case 7:
        return 'Profile submitted to client or hiring manager';
      case 5:
        return 'Interview is scheduled or in progress';
      default:
        return '';
    }
  }

  getActiveJobsCount(): number {
    if (!this.initialData) return 0;
    return this.initialData.filter((j: any) => j.active).length;
  }

  getNewCandidatesCount(): number {
    if (!this.initialData) return 0;
    return this.initialData.reduce((acc, job) => {
      const match = job.profileCountSummary?.find((p: any) => p.candidateProfileMappingStatusId === 1);
      return acc + (match?.count || 0);
    }, 0);
  }

  getShortlistedCount(): number {
    if (!this.initialData) return 0;
    return this.initialData.reduce((acc, job) => {
      const match = job.profileCountSummary?.find((p: any) => p.candidateProfileMappingStatusId === 3);
      return acc + (match?.count || 0);
    }, 0);
  }

  getSubmittedCount(): number {
    if (!this.initialData) return 0;
    return this.initialData.reduce((acc, job) => {
      const match = job.profileCountSummary?.find((p: any) => p.candidateProfileMappingStatusId === 7);
      return acc + (match?.count || 0);
    }, 0);
  }

  getTotalCandidatesCount(): number {
    if (!this.initialData) return 0;
    return this.initialData.reduce((acc, job) => {
      if (!job.profileCountSummary) return acc;
      return acc + job.profileCountSummary.reduce((subAcc: number, p: any) => subAcc + (p.count || 0), 0);
    }, 0);
  }

}
