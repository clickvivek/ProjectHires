import { Component, Input, SimpleChanges } from '@angular/core';
import { Subscription } from 'rxjs';

import {  Router, ActivatedRoute } from '@angular/router';
import { JobOpeningService } from 'src/app/api';
import { SharedService } from 'src/app/modules/shared/services/shared.service';
import { ToastrService } from 'ngx-toastr';

import { profileInitialCountSummary } from 'src/app/data/various';
import * as moment from 'moment';

@Component({
  selector: 'inbox-resume-types',
  templateUrl: './inbox-resume-types.component.html',
  styleUrls: ['./inbox-resume-types.component.scss']
})
export class InboxResumeTypesComponent {

  selectedJob;
  isLoaded:boolean = false;

  jobDetails: any[];

  isExpanded:boolean = false

  itemStartIndex:any = 0;
  itemEndIndex:any = 5;
  itemLimit:any = 10;
  totalItems: any;

  isOpened:boolean = false;
  
  resume:any = null

  isComment:boolean = false;
  isContact:boolean = false;

  statusTypeDataList = profileInitialCountSummary
  subscription: Subscription;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private jobOpeningService: JobOpeningService,
    private sharedService: SharedService,
    private toastr: ToastrService
  ) {

  }

  getDate(date) {
    return moment(date).format('MMMM D, YYYY')
  }

  showAll() {
    this.isExpanded = !this.isExpanded
    if(this.isExpanded) {
      this.itemLimit = this.totalItems
    }
    else {
      this.itemLimit = 10;
    }
  }

  onSidenavClose() {
    this.sharedService.setSideNavData(null)
    this.router.navigate(['/inbox']);
  }

  handleCommentDrawer(job) {
    this.resume = job
    this.isComment = true
    this.isContact = false
  }

  handleContactDrawer(job) {
    this.resume = job
    this.isContact = true
    this.isComment = false
  }

  onCloseDrawer(event) {
    this.resume = null
    this.isComment = false
    this.isContact = false
  }

  handleChangeStatus(event:any) {

    let selectedItem = event.data
    let jobOpeningId = event.id

    this.jobOpeningService.apiJobOpeningChangeCandidateProfileMappingStatusIdPut(jobOpeningId, selectedItem.candidateProfileMappingStatusId)
    .subscribe({
      next:(res:any) => {
        this.sharedService.triggerInboxRefresh();
        this.fetchResumes()
        this.toastr.success('Resume status updated successfully', '' , {
          timeOut: 3000,
          positionClass: 'toast-top-center'
        });
        this.router.navigate(['/inbox']);
      },
      error:(error:any) => {
        this.toastr.error('Some error occured', '' , {
          timeOut: 3000,
          positionClass: 'toast-top-center'
        });
      }
    })

  }

  fetchResumes() {

    this.jobOpeningService.apiJobOpeningGetAllResumeReceivedByJobIdGet(this.selectedJob.jobOpeningId).subscribe({
      next:((res:any) => {

        this.jobDetails = res.value
        this.isLoaded = true;

        const typeId = this.route.snapshot.params['typeid'];

        this.jobDetails = this.jobDetails.filter((item:any) => {
          return item.candidateProfileMappingStatusId == typeId
        })

        

        this.totalItems = this.jobDetails.length

        if (this.totalItems > this.itemLimit) {
          this.itemEndIndex = this.itemLimit;
        }
        else {
          this.itemEndIndex = this.totalItems;
        }

      }),
      error:((error:any) => {
        this.isLoaded = true;
      })
    })

  }

  ngOnInit() {

    this.subscription = this.sharedService.sidenavdatacast.subscribe((res:any) => {
      if(res) {
        setTimeout(() => {
          this.isOpened = true
          this.selectedJob = res
          this.fetchResumes()
        }, 100);
      }
      else {
        this.isOpened = false
        this.selectedJob = null
      }

    })

  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }


}
