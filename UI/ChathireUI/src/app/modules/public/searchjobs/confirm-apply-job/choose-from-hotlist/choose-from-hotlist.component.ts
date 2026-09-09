import { Component, Inject, Output, EventEmitter } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CandidateProfileService } from 'src/app/api/api/candidate-profile.service';
import { SessionService } from 'src/app/core/session/session.service';
import { ToastrService } from 'ngx-toastr';

import { JobOpeningService } from 'src/app/api';
import { JobOpeningCandidateProfileMapDtoForInsert } from 'src/app/api/model/job-opening-candidate-profile-map-dto-for-insert';
import _ from 'underscore';

@Component({
  selector: 'choose-from-hotlist',
  templateUrl: './choose-from-hotlist.component.html',
  styleUrls: ['./choose-from-hotlist.component.scss']
})
export class ChooseFromHotlistComponent {

  isLoaded:boolean = false;
  searchData: string = ""

  hotList:Array<any> =[];
  filteredHotList:Array<any> =[];

  ItemStartIndex:any = 0;
  ItemEndIndex:any = 10;
  itemLimit:any = 10;
  totalItems: any;

  selectedHotlistType: string = "";

  selectedCandidate:any;

  applyJobModel: JobOpeningCandidateProfileMapDtoForInsert;

  @Output() outParams = new EventEmitter();

  constructor(
    @Inject(MAT_DIALOG_DATA) public job: any,
    private dialogRef: MatDialogRef<ChooseFromHotlistComponent>,
    private candidateProfileService: CandidateProfileService,
    private jobOpeningService: JobOpeningService,
    private sessionService: SessionService,
    private toastr: ToastrService
  ) {

  }

  getIndexParams(event:any){
    this.ItemStartIndex = event.ItemStartIndex;
    this.ItemEndIndex = event.ItemEndIndex;
  }

  getRelocation(data) {

    let item = data.candidatePrefLocations

    let newData:any = []
    if(!_.isEmpty(item)) {
      item.forEach(listItem => {
        let name = listItem.cityName.split('-')
        newData.push(name[0])
      });
      return newData.join(', ')
    }
    else {
      return newData
    }
  }

  onSearchData() {
    this.filteredHotList = this.hotList.filter((candidate) =>
      this.isMatch(candidate, this.searchData)
    );
  }
  
  isMatch(candidate, term): boolean {
    term = term.toLowerCase();
    return (
      candidate.candidateName.toLowerCase().includes(term) ||
      candidate.title.toLowerCase().includes(term) ||
      candidate.candidatePrefLocations.some((item) =>
        item.cityName.toLowerCase().includes(term)
      )
    );
  }

  isNoLocation(data) {
    return _.isEmpty(data.candidatePrefLocations)
  }

  isNotResume(data) {
    return _.isEmpty(data)
  }

  handleCandidate(item) {
    this.selectedCandidate = item
  }

  clearData() {
    this.selectedHotlistType = '';
    this.outParams.emit(false)
  }

  applyJob() {
    
    this.applyJobModel = {
      jobOpeningId: parseInt(this.job.jobOpeningId),
      candidateProfileId: parseInt(this.selectedHotlistType),
      appliedDate: new Date().toISOString(),
      active: true,
      candidateProfileMappingStatusId: 1,
      comment: "",
      consultancyUserId: this.sessionService.consultancyUserId,
      //candidateUserId: this.selectedCandidate.id,
      candidateUserId: null,
      doc: this.selectedCandidate.candidateDocuments.length != 0 ? this.selectedCandidate.candidateDocuments[0].doc : ""
    }

    this.jobOpeningService.apiJobOpeningApplyPost(this.applyJobModel).subscribe({
      next: (res: any) => {
        
        this.selectedHotlistType = ""
        this.fetchData()
        setTimeout(() => {
          this.toastr.success('Job applied successfully', '', {
            timeOut: 2000,
            positionClass: 'toast-top-center'
          });
        }, 100);
        this.dialogRef.close()
      },
      error: (error:any) => {
        setTimeout(() => {
          this.toastr.error('Some error occured', '', {
            timeOut: 2000,
            positionClass: 'toast-top-center'
          });
        }, 100);
      }
    })

  }

  fetchData() {

    this.isLoaded = false;

    this.candidateProfileService.apiCandidateProfileGetByConsultancyUserSimplelistGet(this.sessionService.consultancyUserId, true, 2).subscribe({
      next: (res: any) => {

        this.isLoaded = true
        
        this.hotList = res.value;
        this.filteredHotList = this.hotList

        this.totalItems = this.hotList.length;

        if (this.totalItems > this.itemLimit) {
          this.ItemEndIndex = this.itemLimit;
        }
        else {
          this.ItemEndIndex = this.totalItems;
        }

      },
      error: (error:any) => {
        this.isLoaded = true
      }
    })

  }

  ngOnInit() {

    this.fetchData()

  }

}
