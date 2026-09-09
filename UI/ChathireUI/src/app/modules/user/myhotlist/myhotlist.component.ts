import { Component, OnInit, HostListener, EventEmitter, ViewEncapsulation } from '@angular/core';
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';


import { MatDialog } from '@angular/material/dialog';
import { MatDialogRef } from "@angular/material/dialog";

import { CandidateDetailSheetComponent } from 'src/app/modules/shared/components/candidate-detail-sheet/candidate-detail-sheet.component';
import { HotlistSwitchConfirmationModalComponent } from './hotlist-switch-confirmation-modal/hotlist-switch-confirmation-modal.component';

import { CandidateProfileService } from 'src/app/api/api/candidate-profile.service';
import { SessionService } from 'src/app/core/session/session.service';
import { CommonService } from 'src/app/api';
import { ToastrService } from 'ngx-toastr';

import _ from 'underscore';
import { HotlistDeleteConfirmationModalComponent } from './hotlist-delete-confirmation-modal/hotlist-delete-confirmation-modal.component';

@Component({
  selector: 'app-myhotlist',
  templateUrl: './myhotlist.component.html',
  styleUrls: ['./myhotlist.component.scss'],
  providers: [
    { provide: MatDialogRef, useValue: {} },
  ]
})
export class MyhotlistComponent implements OnInit {

  isJobLoaded:boolean = false;
  isJobAvailable:boolean = false;
  isError:boolean = false;
  error:string = ""

  totalItems:any;

  skillSetData:any;
  initialHotListData:any[] = [];
  filteredHotListData:any[] = [];
  isActiveCandidate:any;
  totalExp:any;
  visaList:any;
  cityList:any;

  isMobile:boolean = false;

  selectedCandidateId:number = -1;
  selectedCandidateDetails: any;
  
  selectAvailabilityList: any[] = [];

  searchData: string = ""
  
  isListView: boolean = false;
  isTableView: boolean = false;

  profileId:any;

  candidateDetailsChanged: EventEmitter<any> = new EventEmitter();

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    public dialog: MatDialog,
    private commonService: CommonService,
    private sessionService: SessionService,
    private candidateProfileService: CandidateProfileService,
    private toastr: ToastrService
  ) { 

    router.events.subscribe((event: any) => {
      
      if (event instanceof NavigationEnd) {
        
        const params = this.route.snapshot.queryParams;

        if (_.isUndefined(params['view'])) {
          this.router.navigate([], { queryParamsHandling: 'merge', queryParams: { view: 'list' } });
          this.isListView = true
          this.isTableView = false
        }
        else {
          const type = params['view'];
          if (type == 'table') {
            this.isListView = false
            this.isTableView = true
          }
          else {
            this.isListView = true
            this.isTableView = false
          }
        }
      }
      
    });

  }

  getParams(event){
    this.isActiveCandidate = event.active;
    this.totalExp = event.experience;
  }

  handleSearch(event:any) {
    this.searchData = event
    this.onSearchData()
  }

  onSearchData() {

    this.filteredHotListData = this.initialHotListData.filter((candidate) =>
      this.isMatch(candidate, this.searchData)
    );

  }

  isMatch(candidate, term): boolean {
    term = term.toLowerCase();
    return (
      candidate.candidateName.toLowerCase().includes(term) ||
      candidate.title.toLowerCase().includes(term) ||
      candidate.candidateProfileSkills.some((item) =>
        item.name.toLowerCase().includes(term)
      )
    );
  }

  showCandidateDetaislModal(){

    const jobDialogRef = this.dialog.open(CandidateDetailSheetComponent, {
        panelClass: 'material',
        disableClose: true,
        data: this.selectedCandidateDetails
    });

  }

  @HostListener('window:resize', ['$event'])
    onResize(event){

    if (event.target.innerWidth <= 991) {
      this.isMobile = true;
      this.router.navigate([], { queryParamsHandling: 'merge', queryParams: { view: 'list' } });
    }
    else {
      this.isMobile = false;
    }

  }

  isSelected(id){
    return this.selectedCandidateId == id ? 'selected' : '';
  }

  showCandidateDescription(candidate){

    this.selectedCandidateId = candidate.id;
    this.selectedCandidateDetails = candidate;
    this.candidateDetailsChanged.emit(this.selectedCandidateDetails);

  }

  onStatusChange({statusId, item}) {
    this.switchConfirmation(statusId, item)
  }

  switchConfirmation(statusId, item) {


    const dialogRef = this.dialog.open(HotlistSwitchConfirmationModalComponent, {
      panelClass: 'material',
      disableClose: true,
      data: statusId
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {

        this.candidateProfileService.apiCandidateProfileActivateOrDeActivatePut(item.id, true, statusId).subscribe({
          next: (res : any) => {

            if(res.value) {

              this.filteredHotListData.map(listItem => {
                if(listItem.id == item.id) {
                  item.statusId = statusId
                  item.status = item.statusId == 2 ? true : false
                  return item
                }
                return item
              })

              setTimeout(() => {
                this.toastr.success(`Marketing ${statusId == 2 ? 'activated' : 'deactivated'} successfully`, '' , {
                  timeOut: 5000,
                  positionClass: 'toast-top-center'
                });
              }, 200)

            }


          },
          error: (error:any) => {
            this.toastr.error('Some error occured', '' , {
              timeOut: 5000,
              positionClass: 'toast-top-center'
            });
          }
        })

      }
    });

  }

handleViewType(type) {
  this.router.navigate([], { queryParamsHandling: 'merge', queryParams: { view: type } });
}

 deleteCandidate(item) {

  const dialogRef = this.dialog.open(HotlistDeleteConfirmationModalComponent, {
    panelClass: 'material',
    disableClose: true
  });

  dialogRef.afterClosed().subscribe((result) => {

    if (result) {

      this.candidateProfileService.apiCandidateProfileActivateOrDeActivatePut(item.id, false, item.statusId).subscribe({
        next: (res : any) => {

          if(res.value) {

            this.filteredHotListData = this.filteredHotListData.filter(listItem => {
              return listItem.id != item.id
            })

            this.toastr.success(`Candidate deleted successfully`, '' , {
              timeOut: 5000,
              positionClass: 'toast-top-center'
            });

          }

        },
        error: (error:any) => {
          this.toastr.error('Some error occured', '' , {
            timeOut: 5000,
            positionClass: 'toast-top-center'
          });
        }

      })

    }

  });


 }

 fetchData() {
  this.candidateProfileService.apiCandidateProfileGetByConsultancyUserGet(this.sessionService.consultancyUserId, this.profileId).subscribe({
    next: (res : any) => {

      this.initialHotListData = res.value.filter(item => {
        return item.active
      });

      this.initialHotListData.map(item => {
        item.status = item.statusId == 2 ? true : false
      });

      this.filteredHotListData = this.initialHotListData

      if(this.filteredHotListData.length > 0){
        this.isJobAvailable = true;
        this.isJobLoaded = true;
      }
      else {
        this.isJobAvailable = false;
        this.isJobLoaded = true;
      }

    },
    error: (error:any) => {

      this.isJobLoaded = true;
      this.isJobLoaded = true;
      this.isError = true
      this.error = "Some error occurred"

     }
  })
 }

  ngOnInit() {

    if(window.innerWidth <= 991)
      this.isMobile = true;
    else
      this.isMobile = false;

      this.sessionService.userdetailscast.subscribe((res: any) => {
        let user = res
        this.profileId = user?.consultancyUsers[0].publicProfileUserName
        this.fetchData()
      })

    this.candidateDetailsChanged.subscribe( jobDetails => {
      if(this.isMobile){
        this.showCandidateDetaislModal();
      }
    })

    this.commonService.apiCommonVisaGet().subscribe({
      next: (res : any) => {
        this.visaList = res.value
      },
      error: (error:any) => { }
    })

    this.commonService.apiCommonCandidateAvailabilityGet().subscribe({
      next: (res: any) => {
        this.selectAvailabilityList = res.value
      },
      error: (error:any) => { }
    })

  }

}
