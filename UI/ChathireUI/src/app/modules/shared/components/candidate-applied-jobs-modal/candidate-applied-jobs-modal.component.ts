import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import * as moment from 'moment';
import { CandidateProfileService } from 'src/app/api/api/candidate-profile.service';
import { CandidateProfileAppliedJobDto } from 'src/app/api/model/models';
import { picUrl } from 'src/app/data/various';

export interface CandidateAppliedJobsModalData {
  candidateProfileId: number;
  candidateName: string;
  candidateRole?: string;
}

@Component({
  selector: 'app-candidate-applied-jobs-modal',
  templateUrl: './candidate-applied-jobs-modal.component.html',
  styleUrls: ['./candidate-applied-jobs-modal.component.scss']
})
export class CandidateAppliedJobsModalComponent implements OnInit {

  appliedJobs: CandidateProfileAppliedJobDto[] = [];
  isLoading: boolean = true;
  errorMessage: string = '';

  constructor(
    public dialogRef: MatDialogRef<CandidateAppliedJobsModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CandidateAppliedJobsModalData,
    private candidateProfileService: CandidateProfileService
  ) { }

  ngOnInit(): void {
    this.loadAppliedJobs();
  }

  loadAppliedJobs(): void {
    if (!this.data?.candidateProfileId) {
      this.isLoading = false;
      this.errorMessage = 'No candidate specified.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.candidateProfileService.apiCandidateProfileGetCandidateAppliedJobsGet(this.data.candidateProfileId).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        if (res?.value) {
          this.appliedJobs = res.value;
        } else if (Array.isArray(res)) {
          this.appliedJobs = res;
        } else {
          this.appliedJobs = [];
        }
      },
      error: (err: any) => {
        this.isLoading = false;
        this.errorMessage = 'Failed to load applied jobs history. Please try again.';
        console.error('Error fetching candidate applied jobs:', err);
      }
    });
  }

  close(): void {
    this.dialogRef.close();
  }

  formatDate(dateStr?: string | Date): string {
    if (!dateStr) return '';
    return moment(dateStr).format('MMM D, YYYY');
  }

  getRelativeTime(dateStr?: string | Date): string {
    if (!dateStr) return '';
    return moment(dateStr).fromNow();
  }

  getCompanyLogo(logo?: string): string {
    if (!logo) return '';
    if (logo.startsWith('http://') || logo.startsWith('https://')) return logo;
    return `${picUrl}${logo}`;
  }

  getResumeUrl(doc?: string): string {
    if (!doc) return '';
    if (doc.startsWith('http://') || doc.startsWith('https://')) return doc;
    return `${picUrl}${doc}`;
  }

  openResume(doc?: string, event?: Event): void {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    const url = this.getResumeUrl(doc);
    if (url) {
      window.open(url, '_blank');
    }
  }

  getStatusBadgeClass(statusId?: number, statusName?: string): string {
    const name = (statusName || '').toLowerCase();
    if (name.includes('hire') || name.includes('offer') || name.includes('select')) {
      return 'status-badge-success';
    }
    if (name.includes('interview') || name.includes('shortlist')) {
      return 'status-badge-interview';
    }
    if (name.includes('review') || name.includes('screen')) {
      return 'status-badge-review';
    }
    if (name.includes('reject') || name.includes('decline')) {
      return 'status-badge-rejected';
    }
    return 'status-badge-default';
  }

  formatCompensation(fromAmt?: number, toAmt?: number): string {
    if (fromAmt && toAmt) return `$${fromAmt} - $${toAmt}/hr`;
    if (fromAmt) return `From $${fromAmt}/hr`;
    if (toAmt) return `Up to $${toAmt}/hr`;
    return '';
  }
}
