import { Component, Inject, Optional, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CandidateProfileService } from 'src/app/api/api/candidate-profile.service';
import { SessionService } from 'src/app/core/session/session.service';
import { ToastrService } from 'ngx-toastr';

import { JobOpeningService } from 'src/app/api';
import { JobOpeningCandidateProfileMapDtoForInsert } from 'src/app/api/model/job-opening-candidate-profile-map-dto-for-insert';
import { getMeaningfulErrorMessage } from 'src/app/modules/shared/utils/error-handler.util';
import _ from 'underscore';
import { formatRelocation } from 'src/app/data/various';

@Component({
  selector: 'choose-from-hotlist',
  templateUrl: './choose-from-hotlist.component.html',
  styleUrls: ['./choose-from-hotlist.component.scss']
})
export class ChooseFromHotlistComponent implements OnInit {

  @Input() job: any;

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
  @Output() appliedSuccess = new EventEmitter<any>();

  isSubmitting: boolean = false;

  constructor(
    @Optional() @Inject(MAT_DIALOG_DATA) public dialogData: any,
    @Optional() private dialogRef: MatDialogRef<ChooseFromHotlistComponent>,
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
    return formatRelocation(data);
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
    if (this.isNotResume(item.candidateDocuments)) return;
    this.selectedCandidate = item;
    this.selectedHotlistType = String(item.id);
  }

  clearData() {
    this.selectedHotlistType = '';
    this.selectedCandidate = null;
    this.outParams.emit(false);
  }

  clearSearch() {
    this.searchData = '';
    this.filteredHotList = [...this.hotList];
    this.totalItems = this.filteredHotList.length;
    this.ItemStartIndex = 0;
    this.ItemEndIndex = Math.min(this.itemLimit, this.totalItems);
  }

  getInitials(name: string): string {
    if (!name) return 'C';
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return parts[0].substring(0, 2).toUpperCase();
  }

  getLocationDisplay(candidate: any): string {
    if (!candidate) return '';
    if (candidate.remoteOnly) return 'Remote only';
    const reloc = formatRelocation(candidate);
    if (reloc && reloc.trim().length > 0 && reloc.toLowerCase() !== 'any location') return reloc;
    if (candidate.candidatePrefLocations && candidate.candidatePrefLocations.length > 0) {
      const cities = candidate.candidatePrefLocations.map((l: any) => l.cityName).filter(Boolean);
      if (cities.length > 0) return cities.join(', ');
    }
    return 'Any location';
  }

  hasDocument(candidate: any): boolean {
    return Array.isArray(candidate?.candidateDocuments) && candidate.candidateDocuments.length > 0 && !!candidate.candidateDocuments[0]?.doc;
  }

  applyJob() {
    if (this.isSubmitting || !this.selectedHotlistType) return;
    this.isSubmitting = true;

    const targetJob = this.job || this.dialogData;
    const jobOpeningId = Number(targetJob?.jobOpeningId || targetJob?.id);
    const candidateProfileId = Number(this.selectedHotlistType || this.selectedCandidate?.id);
    const consultancyUserId = Number(this.sessionService.consultancyUserId) > 0
      ? Number(this.sessionService.consultancyUserId)
      : (Number(this.selectedCandidate?.consultancyUserId) > 0 ? Number(this.selectedCandidate.consultancyUserId) : null);

    this.applyJobModel = {
      jobOpeningId: jobOpeningId,
      candidateProfileId: candidateProfileId,
      appliedDate: new Date().toISOString(),
      active: true,
      candidateProfileMappingStatusId: 1,
      comment: "",
      consultancyUserId: consultancyUserId,
      candidateUserId: null,
      doc: this.selectedCandidate?.candidateDocuments?.length ? this.selectedCandidate.candidateDocuments[0].doc : "",
      candidateName: this.selectedCandidate?.candidateName || ''
    };

    this.jobOpeningService.apiJobOpeningApplyPost(this.applyJobModel).subscribe({
      next: (res: any) => {
        this.isSubmitting = false;
        const candidateName = this.selectedCandidate?.candidateName || '';
        this.selectedHotlistType = "";
        this.appliedSuccess.emit({
          candidateName: candidateName,
          jobTitle: targetJob?.jobOpeningName || targetJob?.name,
          companyName: targetJob?.companyName
        });
      },
      error: (error: any) => {
        this.isSubmitting = false;
        const msg = getMeaningfulErrorMessage(error, 'Failed to submit candidate from hotlist. Please try again.');
        setTimeout(() => {
          this.toastr.error(msg, '', {
            timeOut: 4500,
            positionClass: 'toast-top-center'
          });
        }, 100);
      }
    });

  }

  fetchData() {
    this.isLoaded = false;

    this.candidateProfileService.apiCandidateProfileGetByConsultancyUserSimplelistGet(this.sessionService.consultancyUserId, true, 2).subscribe({
      next: (res: any) => {
        this.isLoaded = true;
        this.hotList = res.value || [];
        this.filteredHotList = this.hotList;
        this.totalItems = this.hotList.length;

        if (this.totalItems > this.itemLimit) {
          this.ItemEndIndex = this.itemLimit;
        } else {
          this.ItemEndIndex = this.totalItems;
        }
      },
      error: (error: any) => {
        this.isLoaded = true;
      }
    });
  }

  ngOnInit() {
    if (!this.job && this.dialogData) {
      this.job = this.dialogData;
    }
    this.fetchData();
  }

}
