import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { SessionService } from 'src/app/core/session/session.service';
import { CandidateDirectService, CandidateFullProfile, DirectCandidateResume } from 'src/app/core/services/candidate-direct.service';
import { ConsultancyService } from 'src/app/api';

@Component({
  selector: 'app-confirm-apply-job',
  templateUrl: './confirm-apply-job.component.html',
  styleUrls: ['./confirm-apply-job.component.scss']
})
export class ConfirmApplyJobComponent implements OnInit {
  optionSelected: string = '';
  selectedHotlistType: string = '';
  isAppliedSuccess: boolean = false;
  appliedData: any = null;

  // Candidate specific
  isCandidateUser: boolean = false;
  isLoadingCandidateProfile: boolean = false;
  candidateProfile: CandidateFullProfile | null = null;
  selectedResumeId: number | null = null;
  coverNote: string = '';
  isSubmittingCandidateApply: boolean = false;
  candidateErrorMessage: string = '';

  // New resume upload toggle for candidate
  showUploadNewResume: boolean = false;
  newResumeFile: File | null = null;
  isUploadingNewResume: boolean = false;
  isDirectCompany: boolean = false;
  checkingDirectCompany: boolean = true;

  constructor(
    @Inject(MAT_DIALOG_DATA) public job: any,
    public dialogRef: MatDialogRef<ConfirmApplyJobComponent>,
    private sessionService: SessionService,
    private candidateService: CandidateDirectService,
    private consultancyService: ConsultancyService
  ) {}

  ngOnInit(): void {
    this.isCandidateUser = Number(this.sessionService.userTypeId) === 5;
    this.checkIfJobIsDirectCompany();
    if (this.isCandidateUser) {
      this.loadCandidateProfile();
    }
  }

  checkIfJobIsDirectCompany(): void {
    if (this.job?.isDirectCompany === true || this.job?.isDirectCompany === 1) {
      this.isDirectCompany = true;
      this.checkingDirectCompany = false;
      return;
    }

    const cid = this.job?.consultancyId || this.job?.consultancy?.id;
    if (cid && Number(cid) > 0) {
      this.consultancyService.apiConsultancyConsultancyByIdGet(Number(cid)).subscribe({
        next: (res: any) => {
          this.checkingDirectCompany = false;
          const comp = res?.value || res?.data || res;
          if (comp && (comp.isDirectCompany === true || comp.isDirectCompany === 1)) {
            this.isDirectCompany = true;
          }
        },
        error: () => {
          this.checkingDirectCompany = false;
        }
      });
    } else {
      this.checkingDirectCompany = false;
    }
  }

  loadCandidateProfile(): void {
    this.isLoadingCandidateProfile = true;
    this.candidateErrorMessage = '';
    this.candidateService.getProfile().subscribe({
      next: (res: any) => {
        this.isLoadingCandidateProfile = false;
        if (res && res.value) {
          const profile = res.value as CandidateFullProfile;
          this.candidateProfile = profile;
          const resumes = profile.resumes || [];
          const primaryResume = resumes.find(r => r.isPrimary);
          if (primaryResume) {
            this.selectedResumeId = primaryResume.id;
          } else if (resumes.length > 0) {
            this.selectedResumeId = resumes[0].id;
          } else {
            this.showUploadNewResume = true;
          }
        }
      },
      error: (err) => {
        this.isLoadingCandidateProfile = false;
        this.candidateErrorMessage = 'Failed to load your profile.';
        console.error(err);
      }
    });
  }

  onCandidateResumeFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.newResumeFile = file;
    }
  }

  applyWithCandidateProfile(): void {
    if (!this.selectedResumeId && !this.newResumeFile) {
      this.candidateErrorMessage = 'Please select or upload a resume to apply.';
      return;
    }

    this.isSubmittingCandidateApply = true;
    this.candidateErrorMessage = '';

    if (this.newResumeFile) {
      // First upload the new resume, then apply
      this.candidateService.uploadResume(this.newResumeFile, true).subscribe({
        next: (resumeRes: any) => {
          const uploadedResumeId = resumeRes.value?.id;
          this.submitCandidateApplication(uploadedResumeId);
        },
        error: (err) => {
          this.isSubmittingCandidateApply = false;
          this.candidateErrorMessage = 'Failed to upload new resume. Please try again.';
          console.error(err);
        }
      });
    } else {
      this.submitCandidateApplication(this.selectedResumeId || undefined);
    }
  }

  private submitCandidateApplication(resumeId?: number): void {
    const applyData = {
      jobOpeningId: this.job?.jobOpeningId || this.job?.id,
      resumeId: resumeId,
      coverNote: this.coverNote
    };

    this.candidateService.applyDirect(applyData).subscribe({
      next: (res: any) => {
        this.isSubmittingCandidateApply = false;
        this.appliedData = {
          candidateName: `${this.candidateProfile?.fname || ''} ${this.candidateProfile?.lname || ''}`.trim() || 'Candidate',
          jobTitle: this.job?.jobOpeningName || this.job?.name
        };
        this.isAppliedSuccess = true;
      },
      error: (err: any) => {
        this.isSubmittingCandidateApply = false;
        const msg = err.error?.errors?.[0]?.message || err.error?.message || err.message || 'Failed to submit application.';
        this.candidateErrorMessage = msg;
        console.error('Apply direct error', err);
      }
    });
  }

  handleOption(type: string): void {
    if (this.isDirectCompany) return;
    this.optionSelected = type;
  }

  onOptionTypeChange(event: any): void {
    this.optionSelected = '';
  }

  onAppliedSuccess(data: any): void {
    this.appliedData = data;
    this.isAppliedSuccess = true;
  }

  getSelectedResumeName(): string {
    if (this.candidateProfile?.resumes) {
      const res = this.candidateProfile.resumes.find(r => r.id === this.selectedResumeId);
      if (res) return res.fileName;
    }
    return '';
  }

  getLocationsText(): string {
    if (!this.job?.locations) return '';
    if (Array.isArray(this.job.locations)) {
      return this.job.locations
        .filter(loc => !!loc && String(loc).trim() !== '')
        .join(', ');
    }
    return String(this.job.locations);
  }
}
