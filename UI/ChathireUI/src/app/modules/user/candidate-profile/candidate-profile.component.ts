import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { CandidateDirectService, CandidateFullProfile, DirectCandidateDetails, DirectCandidateResume, DirectCandidateExperience, DirectCandidateEducation } from 'src/app/core/services/candidate-direct.service';
import { CommonService } from 'src/app/api/api/common.service';
import { UserService } from 'src/app/api';
import { SessionService } from 'src/app/core/session/session.service';
import { picUrl, publicProfileUrlPrefix } from 'src/app/data/various';

@Component({
  selector: 'app-candidate-profile',
  templateUrl: './candidate-profile.component.html',
  styleUrls: ['./candidate-profile.component.scss']
})
export class CandidateProfileComponent implements OnInit, OnDestroy {
  activeTab: 'details' | 'resumes' | 'experience' | 'education' = 'details';

  isLoading: boolean = true;
  isSavingDetails: boolean = false;
  isUploadingResume: boolean = false;
  isSavingExp: boolean = false;
  isSavingEdu: boolean = false;

  profile: CandidateFullProfile | null = null;
  visaList: any[] = [];

  detailsForm: FormGroup;
  expForm: FormGroup;
  eduForm: FormGroup;

  editingExpId: number | null = null;
  showExpModal: boolean = false;

  editingEduId: number | null = null;
  showEduModal: boolean = false;

  successMessage: string = '';
  errorMessage: string = '';

  // Public Profile URL state
  isSlugAvailable: 'true' | 'false' | null = null;
  isCheckingSlug: boolean = false;
  isCopiedUrl: boolean = false;
  publicProfileUrlPrefix = publicProfileUrlPrefix || 'www.chathire.com/profile/';

  // Preferred Locations state
  locationSearch$ = new Subject<string>();
  private locationSub: Subscription;
  selectPreferredLocationsList: any[] = [];
  selectedLocationsList: any[] = [];

  constructor(
    private candidateService: CandidateDirectService,
    private commonService: CommonService,
    private userService: UserService,
    private sessionService: SessionService,
    private fb: FormBuilder
  ) {
    this.initForms();
  }

  ngOnInit(): void {
    this.loadVisas();
    this.loadProfile();
    this.initLocationSearch();
  }

  ngOnDestroy(): void {
    if (this.locationSub) {
      this.locationSub.unsubscribe();
    }
  }

  initLocationSearch(): void {
    this.locationSub = this.locationSearch$
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe((query: string) => {
        const cleanTerm = (query || '').trim();
        if (cleanTerm && cleanTerm.length >= 2) {
          this.commonService.apiCommonCityGet(cleanTerm, undefined, true).subscribe({
            next: (res: any) => {
              this.selectPreferredLocationsList = res.value || [];
            },
            error: (err) => console.error('City search failed', err)
          });
        } else {
          this.selectPreferredLocationsList = [];
        }
      });
  }

  onPreferredLocationQuery(event: any): void {
    this.locationSearch$.next(event);
  }

  onSelectedLocationsChange(event: any): void {
    this.selectedLocationsList = event || [];
  }

  initForms(): void {
    this.detailsForm = this.fb.group({
      headline: ['', [Validators.maxLength(200)]],
      summary: [''],
      visaId: [null],
      visaExpiryDate: [''],
      totalYearsOfExp: [null, [Validators.min(0), Validators.max(50)]],
      expectedAnnualSalary: [null, [Validators.min(0)]],
      expectedHourlyRate: [null, [Validators.min(0)]],
      preferredWorkType: ['Full-time'],
      noticePeriodDays: [0, [Validators.min(0)]],
      canRelocate: [false],
      remoteOnly: [false],
      isActivelyLooking: [true],
      gitHubUrl: ['', [Validators.maxLength(300)]],
      portfolioUrl: ['', [Validators.maxLength(300)]],
      isPublicProfileEnabled: [false],
      publicProfileSlug: ['', [Validators.pattern('^[a-zA-Z0-9_-]+$')]],
      isShowCompensationPublic: [true],
      preferredLocations: ['']
    });

    this.expForm = this.fb.group({
      companyName: ['', [Validators.required, Validators.maxLength(200)]],
      title: ['', [Validators.required, Validators.maxLength(150)]],
      cityId: [null],
      startDate: ['', Validators.required],
      endDate: [''],
      isCurrent: [false],
      description: ['']
    });

    this.eduForm = this.fb.group({
      institution: ['', [Validators.required, Validators.maxLength(250)]],
      degree: ['', [Validators.required, Validators.maxLength(150)]],
      major: ['', [Validators.maxLength(150)]],
      startYear: [null],
      graduationYear: [null]
    });
  }

  get resumes(): DirectCandidateResume[] {
    return this.profile?.resumes || [];
  }

  get experiences(): DirectCandidateExperience[] {
    return this.profile?.experiences || [];
  }

  get educations(): DirectCandidateEducation[] {
    return this.profile?.educations || [];
  }

  loadVisas(): void {
    this.commonService.apiCommonVisaGet().subscribe({
      next: (res: any) => {
        if (res && res.value) {
          this.visaList = res.value;
        }
      },
      error: (err) => console.error('Failed to load visas', err)
    });
  }

  loadProfile(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.candidateService.getProfile().subscribe({
      next: (res: any) => {
        this.isLoading = false;
        if (res && res.value) {
          this.profile = res.value;
          this.patchDetailsForm();
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'Failed to load profile. Please refresh.';
        console.error(err);
      }
    });
  }

  patchDetailsForm(): void {
    if (!this.profile || !this.profile.details) return;
    const d = this.profile.details;
    this.detailsForm.patchValue({
      headline: d.headline || '',
      summary: d.summary || '',
      visaId: d.visaId || null,
      visaExpiryDate: d.visaExpiryDate ? d.visaExpiryDate.substring(0, 10) : '',
      totalYearsOfExp: d.totalYearsOfExp || null,
      expectedAnnualSalary: d.expectedAnnualSalary || null,
      expectedHourlyRate: d.expectedHourlyRate || null,
      preferredWorkType: d.preferredWorkType || 'Full-time',
      noticePeriodDays: d.noticePeriodDays || 0,
      canRelocate: !!d.canRelocate,
      remoteOnly: !!d.remoteOnly,
      isActivelyLooking: d.isActivelyLooking !== false,
      gitHubUrl: d.gitHubUrl || '',
      portfolioUrl: d.portfolioUrl || '',
      isPublicProfileEnabled: !!d.isPublicProfileEnabled,
      publicProfileSlug: d.publicProfileSlug || '',
      isShowCompensationPublic: d.isShowCompensationPublic !== false,
      preferredLocations: d.preferredLocations || ''
    });

    if (d.preferredLocations) {
      try {
        this.selectedLocationsList = JSON.parse(d.preferredLocations);
      } catch (e) {
        this.selectedLocationsList = [];
      }
    } else {
      this.selectedLocationsList = [];
    }

    if (d.publicProfileSlug) {
      this.isSlugAvailable = 'true';
    }
  }

  onPublicToggleChange(): void {
    const isEnabled = this.detailsForm.get('isPublicProfileEnabled')?.value;
    const currentSlug = this.detailsForm.get('publicProfileSlug')?.value;
    if (isEnabled && !currentSlug) {
      this.generateDefaultSlug();
    }
  }

  onSlugInput(): void {
    this.isSlugAvailable = null;
  }

  generateDefaultSlug(force: boolean = false): void {
    const fname = (this.profile?.fname || '').trim().toLowerCase().replace(/[^a-z0-9]/g, '');
    const lname = (this.profile?.lname || '').trim().toLowerCase().replace(/[^a-z0-9]/g, '');
    let baseName = `${fname}${lname}`.trim();
    if (!baseName) {
      baseName = 'candidate';
    }

    const currentSlug = this.detailsForm.get('publicProfileSlug')?.value;
    if (!force && currentSlug) {
      return;
    }

    this.checkAndAssignSlug(baseName, 0);
  }

  checkAndAssignSlug(baseName: string, attempts: number = 0): void {
    const candidate = attempts === 0 
      ? baseName 
      : `${baseName}${Math.floor(10 + Math.random() * 90)}`;

    this.isCheckingSlug = true;
    this.userService.apiUserPublicProfileValidationGet(candidate).subscribe({
      next: (res: any) => {
        this.isCheckingSlug = false;
        if (res?.value && attempts < 5) {
          // Taken -> retry with random number
          this.checkAndAssignSlug(baseName, attempts + 1);
        } else {
          // Available or reached max attempts
          this.detailsForm.patchValue({ publicProfileSlug: candidate });
          this.isSlugAvailable = res?.value ? 'false' : 'true';
        }
      },
      error: () => {
        this.isCheckingSlug = false;
        this.detailsForm.patchValue({ publicProfileSlug: candidate });
      }
    });
  }

  checkSlugAvailability(): void {
    const slug = (this.detailsForm.get('publicProfileSlug')?.value || '').trim();
    if (!slug) return;

    // If current slug is identical to already saved profile slug
    if (this.profile?.details?.publicProfileSlug === slug) {
      this.isSlugAvailable = 'true';
      return;
    }

    this.isCheckingSlug = true;
    this.userService.apiUserPublicProfileValidationGet(slug).subscribe({
      next: (res: any) => {
        this.isCheckingSlug = false;
        // res.value === true means taken, false means available
        this.isSlugAvailable = res.value ? 'false' : 'true';
      },
      error: () => {
        this.isCheckingSlug = false;
      }
    });
  }

  getPublicProfileUrl(): string {
    const slug = this.detailsForm.get('publicProfileSlug')?.value || this.profile?.details?.publicProfileSlug || '';
    if (!slug) return '';
    return `${window.location.origin}/#/profile/${slug}`;
  }

  copyPublicUrl(): void {
    const url = this.getPublicProfileUrl();
    if (!url) return;
    navigator.clipboard.writeText(url);
    this.isCopiedUrl = true;
    setTimeout(() => {
      this.isCopiedUrl = false;
    }, 2000);
  }

  saveDetails(): void {
    if (this.detailsForm.invalid) return;
    this.isSavingDetails = true;
    this.clearAlerts();

    const formVal = this.detailsForm.value;
    const detailsData: DirectCandidateDetails = {
      ...formVal,
      isShowCompensationPublic: formVal.isShowCompensationPublic !== false,
      preferredLocations: this.selectedLocationsList.length > 0 ? JSON.stringify(this.selectedLocationsList) : null,
      publicProfileSlug: (formVal.publicProfileSlug || '').trim().toLowerCase(),
      visaExpiryDate: formVal.visaExpiryDate ? new Date(formVal.visaExpiryDate).toISOString() : null
    };

    this.candidateService.saveDetails(detailsData).subscribe({
      next: (res: any) => {
        this.isSavingDetails = false;
        this.showSuccess('Profile details saved successfully!');
        if (this.profile) {
          this.profile.details = res.value;
        }
        // Refresh session user details to update top-bar header
        this.sessionService.refreshUser();
      },
      error: (err) => {
        this.isSavingDetails = false;
        this.showError('Failed to save profile details.');
        console.error(err);
      }
    });
  }

  // Resume Management
  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (!file) return;

    const allowedExtensions = ['.pdf', '.doc', '.docx'];
    const ext = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
    if (!allowedExtensions.includes(ext)) {
      this.showError('Please upload a PDF or Word document (.pdf, .doc, .docx).');
      return;
    }

    if (file.size > 10 * 1024 * 1024) {
      this.showError('File size cannot exceed 10 MB.');
      return;
    }

    this.isUploadingResume = true;
    this.clearAlerts();

    const isPrimary = !this.profile?.resumes || this.profile.resumes.length === 0;
    this.candidateService.uploadResume(file, isPrimary).subscribe({
      next: (res: any) => {
        this.isUploadingResume = false;
        this.showSuccess('Resume uploaded successfully!');
        event.target.value = '';
        this.loadProfile();
      },
      error: (err) => {
        this.isUploadingResume = false;
        this.showError('Failed to upload resume. Please try again.');
        console.error(err);
      }
    });
  }

  setPrimaryResume(resumeId: number): void {
    this.candidateService.setPrimaryResume(resumeId).subscribe({
      next: () => {
        this.showSuccess('Primary resume updated!');
        if (this.profile?.resumes) {
          this.profile.resumes.forEach(r => r.isPrimary = (r.id === resumeId));
        }
      },
      error: (err) => this.showError('Failed to update primary resume.')
    });
  }

  deleteResume(resumeId: number): void {
    if (!confirm('Are you sure you want to delete this resume?')) return;
    this.candidateService.deleteResume(resumeId).subscribe({
      next: () => {
        this.showSuccess('Resume deleted successfully.');
        this.loadProfile();
      },
      error: (err) => this.showError('Failed to delete resume.')
    });
  }

  getResumeUrl(blobUrl: string): string {
    if (!blobUrl) return '';
    if (blobUrl.startsWith('http://') || blobUrl.startsWith('https://')) return blobUrl;
    return `${picUrl}${blobUrl}`;
  }

  // Experience CRUD
  openAddExpModal(): void {
    this.editingExpId = null;
    this.expForm.reset({
      companyName: '',
      title: '',
      cityId: null,
      startDate: '',
      endDate: '',
      isCurrent: false,
      description: ''
    });
    this.showExpModal = true;
  }

  openEditExpModal(exp: DirectCandidateExperience): void {
    this.editingExpId = exp.id || null;
    this.expForm.patchValue({
      companyName: exp.companyName,
      title: exp.title,
      cityId: exp.cityId || null,
      startDate: exp.startDate ? exp.startDate.substring(0, 10) : '',
      endDate: exp.endDate ? exp.endDate.substring(0, 10) : '',
      isCurrent: !!exp.isCurrent,
      description: exp.description || ''
    });
    this.showExpModal = true;
  }

  closeExpModal(): void {
    this.showExpModal = false;
    this.editingExpId = null;
  }

  saveExperience(): void {
    if (this.expForm.invalid) return;
    this.isSavingExp = true;
    this.clearAlerts();

    const val = this.expForm.value;
    const expData: DirectCandidateExperience = {
      ...val,
      startDate: new Date(val.startDate).toISOString(),
      endDate: val.endDate && !val.isCurrent ? new Date(val.endDate).toISOString() : null
    };

    if (this.editingExpId) {
      this.candidateService.updateExperience(this.editingExpId, expData).subscribe({
        next: () => {
          this.isSavingExp = false;
          this.closeExpModal();
          this.showSuccess('Experience updated successfully.');
          this.loadProfile();
        },
        error: (err) => {
          this.isSavingExp = false;
          this.showError('Failed to update experience.');
        }
      });
    } else {
      this.candidateService.addExperience(expData).subscribe({
        next: () => {
          this.isSavingExp = false;
          this.closeExpModal();
          this.showSuccess('Experience added successfully.');
          this.loadProfile();
        },
        error: (err) => {
          this.isSavingExp = false;
          this.showError('Failed to add experience.');
        }
      });
    }
  }

  deleteExperience(id?: number): void {
    if (!id) return;
    if (!confirm('Are you sure you want to delete this experience entry?')) return;
    this.candidateService.deleteExperience(id).subscribe({
      next: () => {
        this.showSuccess('Experience deleted.');
        this.loadProfile();
      },
      error: () => this.showError('Failed to delete experience.')
    });
  }

  // Education CRUD
  openAddEduModal(): void {
    this.editingEduId = null;
    this.eduForm.reset({
      institution: '',
      degree: '',
      major: '',
      startYear: null,
      graduationYear: null
    });
    this.showEduModal = true;
  }

  openEditEduModal(edu: DirectCandidateEducation): void {
    this.editingEduId = edu.id || null;
    this.eduForm.patchValue({
      institution: edu.institution,
      degree: edu.degree,
      major: edu.major || '',
      startYear: edu.startYear || null,
      graduationYear: edu.graduationYear || null
    });
    this.showEduModal = true;
  }

  closeEduModal(): void {
    this.showEduModal = false;
    this.editingEduId = null;
  }

  saveEducation(): void {
    if (this.eduForm.invalid) return;
    this.isSavingEdu = true;
    this.clearAlerts();

    const eduData: DirectCandidateEducation = this.eduForm.value;

    if (this.editingEduId) {
      this.candidateService.updateEducation(this.editingEduId, eduData).subscribe({
        next: () => {
          this.isSavingEdu = false;
          this.closeEduModal();
          this.showSuccess('Education updated successfully.');
          this.loadProfile();
        },
        error: () => {
          this.isSavingEdu = false;
          this.showError('Failed to update education.');
        }
      });
    } else {
      this.candidateService.addEducation(eduData).subscribe({
        next: () => {
          this.isSavingEdu = false;
          this.closeEduModal();
          this.showSuccess('Education added successfully.');
          this.loadProfile();
        },
        error: () => {
          this.isSavingEdu = false;
          this.showError('Failed to add education.');
        }
      });
    }
  }

  deleteEducation(id?: number): void {
    if (!id) return;
    if (!confirm('Are you sure you want to delete this education entry?')) return;
    this.candidateService.deleteEducation(id).subscribe({
      next: () => {
        this.showSuccess('Education deleted.');
        this.loadProfile();
      },
      error: () => this.showError('Failed to delete education.')
    });
  }

  getProfilePic(): string {
    if (this.profile?.profilePic) {
      if (this.profile.profilePic.startsWith('http') || this.profile.profilePic.startsWith('data:')) {
        return this.profile.profilePic;
      }
      return `${picUrl}${this.profile.profilePic}`;
    }
    return '';
  }

  getUserInitial(fName?: string, lName?: string): string {
    if (fName && fName.trim().length > 0) {
      return fName.trim().charAt(0).toUpperCase();
    }
    if (lName && lName.trim().length > 0) {
      return lName.trim().charAt(0).toUpperCase();
    }
    return 'U';
  }

  showSuccess(msg: string): void {
    this.successMessage = msg;
    this.errorMessage = '';
    setTimeout(() => this.successMessage = '', 4000);
  }

  showError(msg: string): void {
    this.errorMessage = msg;
    this.successMessage = '';
  }

  clearAlerts(): void {
    this.successMessage = '';
    this.errorMessage = '';
  }
}
