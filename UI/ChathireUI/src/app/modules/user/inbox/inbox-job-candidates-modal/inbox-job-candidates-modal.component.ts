import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { JobOpeningService } from 'src/app/api';
import { SharedService } from 'src/app/modules/shared/services/shared.service';
import { ToastrService } from 'ngx-toastr';
import { profileInitialCountSummary } from 'src/app/data/various';
import { getMeaningfulErrorMessage } from 'src/app/modules/shared/utils/error-handler.util';

export interface InboxJobCandidatesModalData {
  job: any;
}

@Component({
  selector: 'app-inbox-job-candidates-modal',
  templateUrl: './inbox-job-candidates-modal.component.html',
  styleUrls: ['./inbox-job-candidates-modal.component.scss']
})
export class InboxJobCandidatesModalComponent implements OnInit {

  job: any;
  candidates: any[] = [];
  filteredCandidates: any[] = [];
  isLoading: boolean = true;
  errorMessage: string = '';

  selectedStatusFilter: number | null = null;
  statusTypeDataList = profileInitialCountSummary;

  // Drawer for comments/contact from inside modal if needed
  activeResumeForComments: any = null;
  activeResumeForContact: any = null;

  constructor(
    public dialogRef: MatDialogRef<InboxJobCandidatesModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: InboxJobCandidatesModalData,
    private jobOpeningService: JobOpeningService,
    private sharedService: SharedService,
    private toastr: ToastrService
  ) {
    this.job = data?.job;
  }

  ngOnInit(): void {
    this.loadCandidates();
  }

  loadCandidates(): void {
    if (!this.job?.jobOpeningId) {
      this.isLoading = false;
      this.errorMessage = 'No job selected.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.jobOpeningService.apiJobOpeningGetAllResumeReceivedByJobIdGet(this.job.jobOpeningId).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        if (res && res.value) {
          this.candidates = res.value;
        } else if (Array.isArray(res)) {
          this.candidates = res;
        } else {
          this.candidates = [];
        }
        this.applyFilter();
      },
      error: (error: any) => {
        this.isLoading = false;
        this.errorMessage = getMeaningfulErrorMessage(error, 'Failed to load candidates.');
      }
    });
  }

  setStatusFilter(statusId: number | null): void {
    this.selectedStatusFilter = statusId;
    this.applyFilter();
  }

  applyFilter(): void {
    if (this.selectedStatusFilter === null) {
      this.filteredCandidates = [...this.candidates];
    } else {
      this.filteredCandidates = this.candidates.filter(
        c => c.candidateProfileMappingStatusId === this.selectedStatusFilter
      );
    }
  }

  getCandidateCountByStatus(statusId: number): number {
    return this.candidates.filter(c => c.candidateProfileMappingStatusId === statusId).length;
  }

  getStatusName(statusId: number): string {
    const item = this.statusTypeDataList.find(s => s.candidateProfileMappingStatusId === statusId);
    return item ? item.candidateProfileMappingStatusName : 'Unknown';
  }

  getStatusBadgeClass(statusId: number): string {
    switch (statusId) {
      case 1: return 'badge-new';
      case 2: return 'badge-hold';
      case 3: return 'badge-shortlisted';
      case 4: return 'badge-noresponse';
      case 5: return 'badge-interview';
      case 6: return 'badge-rejected';
      case 7: return 'badge-submitted';
      default: return 'badge-default';
    }
  }

  getCandidateLocation(c: any): string {
    if (!c) return '';
    if (c.currentLocationCity) {
      const city = c.currentLocationCity.city1 || '';
      const state = c.currentLocationCity.stateCode || c.currentLocationCity.idStateNavigation?.stateCode || '';
      if (city && state) return `${city}, ${state}`;
      if (city) return city;
      if (state) return state;
    }
    if (c.candidateProfile && c.candidateProfile.city) {
      const city = c.candidateProfile.city.city1 || '';
      const state = c.candidateProfile.city.stateCode || c.candidateProfile.city.idStateNavigation?.stateCode || '';
      if (city && state) return `${city}, ${state}`;
      if (city) return city;
      if (state) return state;
    }
    return '';
  }

  handleChangeStatus(event: any, candidate: any): void {
    const selectedItem = event.data;
    const mapId = event.id || candidate.id;

    this.jobOpeningService.apiJobOpeningChangeCandidateProfileMappingStatusIdPut(
      mapId,
      selectedItem.candidateProfileMappingStatusId
    ).subscribe({
      next: () => {
        candidate.candidateProfileMappingStatusId = selectedItem.candidateProfileMappingStatusId;
        this.sharedService.triggerInboxRefresh();
        this.applyFilter();
        this.toastr.success('Candidate status updated successfully', '', {
          timeOut: 3000,
          positionClass: 'toast-top-center'
        });
      },
      error: (error: any) => {
        this.toastr.error(getMeaningfulErrorMessage(error, 'Failed to update candidate status.'), '', {
          timeOut: 3000,
          positionClass: 'toast-top-center'
        });
      }
    });
  }

  close(): void {
    this.dialogRef.close();
  }
}
