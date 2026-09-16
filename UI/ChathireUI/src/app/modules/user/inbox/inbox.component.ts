import { Component, ViewChild, HostListener, ElementRef } from '@angular/core';
import {  Router, ActivatedRoute, NavigationEnd } from '@angular/router';
import * as moment from 'moment';
import _ from 'underscore';

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
export class InboxComponent {

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

    router.events.subscribe((event: any) => {
      if (event instanceof NavigationEnd) {
        this.fetchProfiles();
      }
    });

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

 
  fetchProfiles() {

    this.jobOpeningService.apiJobOpeningGetJobOpeningProfileMapSummaryGet(this.sessionService.consultancyUserId).subscribe({
      next:(res:any) => {


        this.isLoaded = true
        
        this.isError = false

        if(!_.isEmpty(res)) {

          this.initialData = res
          this.filteredData = this.initialData
  
          this.initialData.forEach((item:any) => {
            
            let profileFilteredCountSummary:any = []
            let existingProfileCountSummary = item.profileCountSummary

            this.profileInitialCountSummary.forEach((profileOne:any) => {
              let matchObj = existingProfileCountSummary.find(profileTwo => profileTwo.candidateProfileMappingStatusId === profileOne.candidateProfileMappingStatusId);
              if(matchObj) {
                profileFilteredCountSummary.push(matchObj);
              }
              else {
                profileFilteredCountSummary.push(profileOne);
              }
            })

            item.profileCountSummary = profileFilteredCountSummary

          })

          this.totalItems = this.filteredData.length;
        }
        else {
          this.totalItems = this.filteredData.length;
        }
       
      },
      error:(error:any) => {
        this.isError = true
        this.isLoaded = true
        this.error = 'Some error occured';
      }
    })

  }
 
  ngOnInit() {
    
    this.sortList = [
      { id: 1, name: "Posted Date" },
      { id: 2, name: "Title" }
    ]
    
    this.orderList = [
      { id: 1, name: "Ascending" },
      { id: 2, name: "Descending" }
    ]


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
      case 3: return 'Selected';
      case 4: return 'No Response';
      case 5: return 'Interview';
      case 6: return 'Rejected';
      default: return profile.candidateProfileMappingStatusName || '';
    }
  }

  getStatusTileClass(statusId: number, count: number): string {
    if (!count || count === 0) return 'tile-zero';
    switch (statusId) {
      case 1: return 'tile-new';
      case 2: return 'tile-on-hold';
      case 3: return 'tile-selected';
      case 4: return 'tile-no-response';
      case 5: return 'tile-interview';
      case 6: return 'tile-rejected';
      default: return 'tile-default';
    }
  }

}
