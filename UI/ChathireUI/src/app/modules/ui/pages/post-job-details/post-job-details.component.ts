import { Component, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { NgForm } from '@angular/forms';

import { JobOpeningService } from 'src/app/api';
import { JobOpeningDto } from 'src/app/api/model/job-opening-dto';

import { CommonService } from 'src/app/api';
import { SessionService } from 'src/app/core/session/session.service';
import { SharedService } from 'src/app/modules/shared/services/shared.service';
import { ToastrService } from 'ngx-toastr';

import { defaultPostJobVisas, defaultPostJobPositionTypes } from 'src/app/data/various';

import _ from 'underscore';

import { AuthService } from 'src/app/core/auth/auth.service';
import { TokenService } from 'src/app/api/api/token.service';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';

export interface UserQuotaStatus {
  cycleStartDate: string;
  cycleEndDate: string;
  daysRemainingInCycle: number;
  maxJobPostings: number;
  usedJobPostings: number;
  remainingJobPostings: number;
  maxDownloads: number;
  usedDownloads: number;
  remainingDownloads: number;
  dailyChatLimit: number;
  usedChatsToday: number;
  remainingChatsToday: number;
  isFreeTier: boolean;
  planName: string;
  isLimitReached: boolean;
  postingsPercentage: number;
  downloadsPercentage: number;
}

declare var google: any;

@Component({
  selector: 'post-job-details',
  templateUrl: './post-job-details.component.html',
  styleUrls: ['./post-job-details.component.scss']
})
export class PostJobDetailsComponent {

  @Input() isJobPosted;

  @Output() outputparams = new EventEmitter();
  @Output() postparams = new EventEmitter();

  showAuthPromptModal: boolean = false;
  googleClientId: string = '262467975068-u0o6qtjog1o7e1p4jp5kuag34ibhfm1l.apps.googleusercontent.com';

  quotaStatus: UserQuotaStatus | null = null;
  isLoadingQuota: boolean = false;
  isQuotaCollapsed: boolean = true;

  formData: any = {
    name: "",
    country: "",
    joiningdays: "",
    description: "",
    totalExp: null,
    postedDate: null,
    lastDate: null,
    jobTypeId: 7,
    employmentTypeId: null,
    visaId: 4,
    jobOpeningJobTypes: [...defaultPostJobPositionTypes],
    jobOpeningEmploymentTypes: [],
    jobOpeningVisaMaps: [...defaultPostJobVisas],
    jobOpeningLocations: [],
    jobOpeningSkills: [],
    directClient: null,
    billingRangeId: '',
    fromAmt: 0,
    toAmt: 0
  }

  selectJobPositionTypeList: any;
  selectEmploymentTypeList: any;
  selectVisaMapsList: any;
  selectLocationList: any;

  selectCountryList: Array<any> = [
    { id: 1, name: 'USA', description: 'USA' },
    { id: 2, name: 'Canada', description: 'Canada' },
    { id: 3, name: 'India', description: 'India' }
  ];
  selectedCountryItems: any[] = [];
  

  selectBillingList:Array<any> = [
    { id: 1, range: "Less than 50"},
    { id: 2, range: "50-60" },
    { id: 3, range: "60-70"},
    { id: 4, range: "70-80"},
    { id: 5, range: "75-85"},
    { id: 6, range: "80-90" },
    { id: 7, range: "90-100"},
    { id: 8, range: "100+"}
  ]

  selectSkillList: any;
  
  directClientList: Array<any> = [
    { label: 'Yes', value: true, id:'directClientYes' },
    { label: 'No', value: false,  id:'directClientNo' },
    { label: 'Not Disclosed', value: 'null',  id:'directClientNotDisclosed' }
  ]

  selectDirectClient: any = "null";

  editorPlacehorder:string = 'Text here...';

  postedDate = new Date();
  lastDate = new Date();

  isFormSubmitted: boolean = false;

  job: JobOpeningDto = {}

  jobOpeningId;
  isEdit: boolean = false;

  isLoaded:boolean = true

  modules = {
    toolbar: [
      ['bold', 'italic', 'underline'],
      [{ 'list': 'ordered'}, { 'list': 'bullet' }],
      [{ 'color': [] }, { 'background': [] }]
    ]
  };
    
  @ViewChild('postJobsForm', {static: false}) postJobsForm: NgForm;
  @ViewChild('quillEditor') quillEditor;


  constructor(
    public router: Router,
    private route: ActivatedRoute,
    private commonService: CommonService,
    private sessionService: SessionService,
    private sharedService: SharedService,
    private toastr: ToastrService,
    private jobOpeningService: JobOpeningService,
    private authService: AuthService,
    private tokenService: TokenService,
    private http: HttpClient
  ) { 

    router.events.subscribe((event: any) => {
      if (event instanceof NavigationEnd) {
        this.route.params.subscribe((params) => {
          if(params['id']) {
            this.jobOpeningId = params['id']
            this.isEdit = true
            this.editForm()
          }
          else {
            this.isEdit = false
          }
        });
      }
    });

  }

  editForm() {

    this.isLoaded = false

    this.jobOpeningService.apiJobOpeningJobOpeningByIdGet(this.jobOpeningId).subscribe({
      next:(res:any) => {

        this.isLoaded = true
        let newData = res.value

        this.formData = newData;

        if (newData.country) {
          const names = newData.country.split(',').map((s: string) => s.trim().toLowerCase());
          this.selectedCountryItems = this.selectCountryList.filter(c => names.includes(c.name.toLowerCase()));
          this.formData.country = newData.country;
        }

        //Position type
        if(!_.isEmpty(newData.jobOpeningJobTypes)) {
          let arrData:any = []
          newData.jobOpeningJobTypes.forEach(item => {
            let itemData = {
              jobTypeId: item.jobTypeId,
              description: item.jobType.description
            }
            arrData.push(itemData)
          });
         this.formData.jobOpeningJobTypes = arrData
        }

        //Employment type
        if(!_.isEmpty(newData.jobOpeningEmploymentTypes)) {
          let arrData:any = []
          newData.jobOpeningEmploymentTypes.forEach(item => {
            let itemData = {
              id: item.employmentTypeId,
              employmentTypeId: item.employmentTypeId,
              description: item.employmentType?.description || ''
            }
            arrData.push(itemData)
          });
          this.formData.jobOpeningEmploymentTypes = arrData;
          this.formData.employmentTypeId = arrData[0]?.id;
        }

        //Visa type
        if(!_.isEmpty(newData.jobOpeningVisaMaps)) {
          let arrData:any = []
          newData.jobOpeningVisaMaps.forEach(item => {
            let itemData = {
              visaId: item.visaId,
              name: item.visa.name
            }
            arrData.push(itemData)
          });
         this.formData.jobOpeningVisaMaps = arrData
        }

        //locations
        if(!_.isEmpty(newData.jobOpeningLocations)) {
          let arrData:any = []
          newData.jobOpeningLocations.forEach(item => {
            let city = item.city
            let itemData = {
              cityId: item.cityId,
              city1: city?.city1 || '',
              stateName: city?.stateName || '',
              stateCode: city?.stateCode || '',
              zip: city?.zip || '',
              countryName: city?.countryName || '',
              numberOfOpenings: item.numberOfOpenings
            }
            arrData.push(itemData)
          });
          this.formData.jobOpeningLocations = arrData
        }

        //skills
        if(!_.isEmpty(newData.jobOpeningSkills)) {
          let arrData:any = []
          newData.jobOpeningSkills.forEach(item => {
            let itemData = {
              skillId: item.skillId,
              name: item.skill?.name || item.name || ''
            }
            arrData.push(itemData)
          });
          this.formData.jobOpeningSkills = arrData
        }

         //billing range
         if((newData.fromAmt == 0 && newData.toAmt == 0) || (newData.fromAmt == 0 && newData.toAmt == 50)) {
          this.formData.billingRangeId = 1
        }
        else {
          const billingData = newData.fromAmt.toString()+'-'+newData.toAmt.toString()
          const filteredData = this.selectBillingList.filter(item => {
            return item.range == billingData
          })
          this.formData.billingRangeId = filteredData[0].id
        }

        //Direct Client
        if(!newData.directClient == null) {
          this.selectDirectClient = 'null'
        }
        else {
          this.selectDirectClient = newData.directClient
        }

      },
      error:(error:any) => {
        this.isLoaded = true
      }
    })

  }

  togglePositionType(item: any) {
    const id = item.jobTypeId || item.id;
    const exists = (this.formData.jobOpeningJobTypes || []).some((j: any) => (j.jobTypeId || j.id) === id);

    if (exists) {
      this.formData.jobOpeningJobTypes = this.formData.jobOpeningJobTypes.filter((j: any) => (j.jobTypeId || j.id) !== id);
    } else {
      this.formData.jobOpeningJobTypes = [
        ...(this.formData.jobOpeningJobTypes || []),
        {
          ...item,
          id: id,
          jobTypeId: id,
          description: item.description,
          active: true
        }
      ];
    }

    if (this.formData.jobOpeningJobTypes.length > 0) {
      this.formData.jobTypeId = this.formData.jobOpeningJobTypes[0].id || this.formData.jobOpeningJobTypes[0].jobTypeId;
    } else {
      this.formData.jobTypeId = null;
    }
  }

  isPositionTypeSelected(item: any): boolean {
    const id = item.jobTypeId || item.id;
    return (this.formData.jobOpeningJobTypes || []).some((j: any) => (j.jobTypeId || j.id) === id);
  }

  isPositionTypeRequired(): boolean {
    return !this.formData.jobOpeningJobTypes || this.formData.jobOpeningJobTypes.length === 0;
  }

  onPositionTypeChange(event: any) {

    let newData: any = []
    
    if(!_.isEmpty(event)) {
      event.forEach(item => {
        let itemData = {
            ...item,
            id: item.id || item.jobTypeId,
            jobTypeId: item.jobTypeId || item.id,
            active: true
        }
        newData.push(itemData)
      });
      this.formData.jobOpeningJobTypes = newData;
      this.formData.jobTypeId = event[0].id || event[0].jobTypeId;
    }
    else {
      this.formData.jobOpeningJobTypes = []
      this.formData.jobTypeId = null;
    }

  }

  isRemoteJob(): boolean {
    if (this.formData.employmentTypeId) {
      const selected = this.selectEmploymentTypeList?.find((item: any) => item.id == this.formData.employmentTypeId);
      if (selected) {
        const desc = (selected.description || selected.name || '').trim().toLowerCase();
        return desc === 'remote';
      }
    }
    if (this.formData.jobOpeningEmploymentTypes?.length) {
      const selected = this.formData.jobOpeningEmploymentTypes[0];
      const desc = (selected.description || selected.name || '').trim().toLowerCase();
      return desc === 'remote';
    }
    return false;
  }

  onEmploymentTypeChange(event: any) {

    if (event && (event.id || event.employmentTypeId)) {
      const id = event.id || event.employmentTypeId;
      this.formData.employmentTypeId = id;
      this.formData.jobOpeningEmploymentTypes = [
        {
          ...event,
          id: id,
          employmentTypeId: id,
          active: true
        }
      ];
      if (this.isRemoteJob()) {
        this.formData.jobOpeningLocations = [];
        this.formData.cityId = null;
        if (!this.selectedCountryItems || this.selectedCountryItems.length === 0) {
          this.selectedCountryItems = [{ id: 1, name: 'USA', description: 'USA' }];
          this.formData.country = 'USA';
        }
      } else {
        this.selectedCountryItems = [];
        this.formData.country = '';
      }
    } else {
      this.formData.jobOpeningEmploymentTypes = [];
      this.formData.employmentTypeId = null;
      this.selectedCountryItems = [];
      this.formData.country = '';
    }

  }

  onCountryChange(event: any) {
    this.selectedCountryItems = event || [];
    if (!_.isEmpty(event)) {
      const names = event.map((item: any) => item.name || item.description).filter(Boolean);
      this.formData.country = names.join(', ');
    } else {
      this.formData.country = '';
    }
  }

  toggleVisa(item: any) {
    const id = item.visaId || item.id;
    const exists = (this.formData.jobOpeningVisaMaps || []).some((v: any) => (v.visaId || v.id) === id);

    if (exists) {
      this.formData.jobOpeningVisaMaps = this.formData.jobOpeningVisaMaps.filter((v: any) => (v.visaId || v.id) !== id);
    } else {
      this.formData.jobOpeningVisaMaps = [
        ...(this.formData.jobOpeningVisaMaps || []),
        {
          ...item,
          id: id,
          visaId: id,
          name: item.name,
          active: true
        }
      ];
    }

    if (this.formData.jobOpeningVisaMaps.length > 0) {
      this.formData.visaId = this.formData.jobOpeningVisaMaps[0].id || this.formData.jobOpeningVisaMaps[0].visaId;
    } else {
      this.formData.visaId = null;
    }
  }

  isVisaSelected(item: any): boolean {
    const id = item.visaId || item.id;
    return (this.formData.jobOpeningVisaMaps || []).some((v: any) => (v.visaId || v.id) === id);
  }

  isVisaRequired(): boolean {
    return !this.formData.jobOpeningVisaMaps || this.formData.jobOpeningVisaMaps.length === 0;
  }

  onVisaMapChange(event: any) {
   
    let newData:any = []
    if(!_.isEmpty(event)) {
      event.forEach(item => {
        let itemData = {
            ...item,
            id: item.id || item.visaId,
            visaId: item.visaId || item.id
        }
        newData.push(itemData)
      });
      this.formData.jobOpeningVisaMaps = newData;
      this.formData.visaId = event[0].id || event[0].visaId;
    }
    else {
      this.formData.jobOpeningVisaMaps = []
      this.formData.visaId = null
    }


  }

  onLocationQuery(event:any) {
    this.commonService.apiCommonCityGet(event,undefined,false).subscribe({
      next: (res : any) => {
        this.selectLocationList = res.value
      },
      error: (error:any) => { }
    })
  }


  onLocationChange(event:any) {
    let newData: any = []
    
    if(!_.isEmpty(event)) {
      event.forEach((item, index) => {
        let itemData = {
          ...item,
          id: index,
          cityId: item.cityId || item.id,
          numberOfOpenings: item.numberOfOpenings || 1
        }
        newData.push(itemData)
      });
      this.formData.jobOpeningLocations = newData;
      this.formData.cityId = event[0].id || event[0].cityId;
    }
    else {
      this.formData.jobOpeningLocations = []
      this.formData.cityId = null
    }
  }

  onSkillQuery(event:any){
    this.commonService.apiCommonSkillsGet(event).subscribe({
      next: (res : any) => {
        this.selectSkillList = res.value
      },
      error: (error:any) => { }
    })
  }

  onSelectedSkills(event:any) {
    let newData:any = []
    if(!_.isEmpty(event)) {
      event.forEach(item => {
        let itemData = {
            skillId: item.skillId || item.id || 0,
            name: item.name || '',
            isMandate: true,
            active: true
        }
        newData.push(itemData)
      });
      this.formData.jobOpeningSkills = newData;
      this.formData.skillId = event[0].skillId || event[0].id || 0;
    }
    else {
      this.formData.jobOpeningSkills = []
      this.formData.skillId = null
    }
  }

  onBillingListChange(event:any){
    let data = event.range
    let finalData = data.split('-')
    if(finalData.length != 1) {
      this.formData.fromAmt = finalData[0]
      this.formData.toAmt = finalData[1]
    }
    else {
      this.formData.fromAmt = 0
      this.formData.toAmt = 50
    }
  }

  onDirectClientChange(value): void {
    this.selectDirectClient = value

  }

  onContentChanged(event) {

  }

  scrollToTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  onCancel() {
    this.outputparams.emit(true)
  }

  onExpKeyPress(event: KeyboardEvent) {
    const charCode = event.which ? event.which : event.keyCode;
    if (charCode < 48 || charCode > 57) {
      event.preventDefault();
      return;
    }
    const input = event.target as HTMLInputElement;
    if (input.value && input.value.length >= 2) {
      event.preventDefault();
    }
  }

  onExpInput(event: any) {
    const input = event.target as HTMLInputElement;
    let val = input.value;
    if (val) {
      if (val.length > 2) {
        val = val.slice(0, 2);
      }
      if (parseInt(val, 10) > 50) {
        val = '50';
      }
      input.value = val;
      this.formData.totalExp = val ? parseInt(val, 10) : null;
    }
  }

  postJob() {
    
    this.isFormSubmitted = true
    
    for (const field in this.postJobsForm.controls) {
      if (this.postJobsForm.controls.hasOwnProperty(field)) {
        const control = this.postJobsForm.controls[field];
        if (control.invalid) {
          console.log(`Invalid field: ${field}`);
        }
      }
    }
    
     if(!this.postJobsForm.valid || this.isPositionTypeRequired() || this.isVisaRequired()) {
      this.scrollToTop()
      this.isFormSubmitted = false
     }
     else {

      const parsedTotalExp = (this.formData.totalExp != null && this.formData.totalExp !== '') ? parseInt(this.formData.totalExp, 10) : 0;
      const candidateCountry = this.isRemoteJob()
        ? (this.formData.country || 'USA')
        : (this.formData.jobOpeningLocations?.[0]?.countryName || this.formData.country || 'USA');

      if(!this.isEdit) {

        this.job = {
          name: this.formData.name,
          country: candidateCountry,
          joiningdays: 0,
          description: this.formData.description,
          totalExp: parsedTotalExp,
          postedDate: new Date().toISOString(),
          lastDate: new Date().toISOString(),
          numberOfOpening: 0,
          active: true,
          consultancyUserId: this.sessionService.consultancyUserId,
          priorityId: 0,
          categoryId: 1, //hard coded
          jobLocation: "string",
          postalcode: "string",
          projectStartId: 8, //USA
          directClient: this.selectDirectClient == 'null' ? null : this.selectDirectClient,
          isReviewed: true,
          fromAmt: this.formData.fromAmt,
          toAmt: this.formData.toAmt,
          notifyOnCandidateProfileMap: this.formData.notifyOnCandidateProfileMap,
          notifyWithResume: this.formData.notifyWithResume,
          localCandidatePref: this.formData.localCandidatePref,
          localCandidateOnly: true,
          isExpired: true,
          jobOpeningSkills: this.formData.jobOpeningSkills,
          jobOpeningVisaMaps: this.formData.jobOpeningVisaMaps,
          jobOpeningLocations: this.formData.jobOpeningLocations,
          jobOpeningEmploymentTypes: this.formData.jobOpeningEmploymentTypes,
          jobOpeningJobTypes: this.formData.jobOpeningJobTypes,
          
        }

      }
      else {

        this.job = {
          id: this.formData.id,
          name: this.formData.name,
          country: candidateCountry,
          joiningdays: this.formData.joiningdays,
          description: this.formData.description,
          totalExp: parsedTotalExp,
          postedDate: new Date().toISOString(),
          lastDate: new Date().toISOString(),
          numberOfOpening: this.formData.numberOfOpening,
          active: this.formData.active,
          consultancyUserId: this.sessionService.consultancyUserId,
          priorityId: this.formData.priorityId,
          categoryId: this.formData.categoryId,
          jobLocation: this.formData.jobLocation,
          postalcode: this.formData.postalcode,
          projectStartId: this.formData.projectStartId,
          directClient: this.formData.directClient,
          isReviewed: this.formData.isReviewed,
          fromAmt: this.formData.fromAmt,
          toAmt: this.formData.toAmt,
          notifyOnCandidateProfileMap: this.formData.notifyOnCandidateProfileMap,
          notifyWithResume: this.formData.notifyWithResume,
          localCandidatePref: this.formData.localCandidatePref,
          localCandidateOnly: this.formData.localCandidateOnly,
          isExpired: this.formData.isExpired,
          jobOpeningSkills: this.formData.jobOpeningSkills,
          jobOpeningVisaMaps: this.formData.jobOpeningVisaMaps,
          jobOpeningLocations: this.formData.jobOpeningLocations,
          jobOpeningEmploymentTypes: this.formData.jobOpeningEmploymentTypes,
          jobOpeningJobTypes: this.formData.jobOpeningJobTypes,
          
        }

      }

      if (!this.authService.isLoggedIn()) {
        sessionStorage.setItem('ch_pending_job_post', JSON.stringify(this.job));
        this.showAuthPromptModal = true;
        this.isFormSubmitted = false;
        return;
      }

      if (!this.isEdit && this.quotaStatus && this.quotaStatus.isLimitReached) {
        this.toastr.error(`You have reached your 30-day limit of ${this.quotaStatus.maxJobPostings} job postings. Quota resets on ${new Date(this.quotaStatus.cycleEndDate).toLocaleDateString()} (${this.quotaStatus.daysRemainingInCycle} days remaining).`, 'Limit Reached', {
          timeOut: 6000,
          positionClass: 'toast-top-center'
        });
        this.scrollToTop();
        this.isFormSubmitted = false;
        return;
      }
       
      this.jobOpeningService.apiJobOpeningAddPost(this.job).subscribe({
          next: (res : any) => {
            this.toastr.success(`Job ${this.isEdit ? 'reposted' : 'posted'} successfully`, '' , {
              timeOut: 3000,
              positionClass: 'toast-top-center'
            });
           this.scrollToTop();
           this.fetchQuotaStatus();
           
           if(this.isEdit) {
            this.outputparams.emit(true)
           }
           else {
            this.postparams.emit(true)
           }

           this.isFormSubmitted = false

          },
          error: (error:any) => {
            const errMsg = error?.error?.errors?.[0]?.message || error?.message || 'Some error occured';
            this.toastr.error(errMsg, 'Job Posting Failed' , {
              timeOut: 6000,
              positionClass: 'toast-top-center'
            });
            this.scrollToTop();
            this.isFormSubmitted = false
          }
        })
       

     }

  }

  loginWithGoogleFromModal() {
    if (typeof google !== 'undefined' && google.accounts) {
      google.accounts.id.initialize({
        client_id: this.googleClientId,
        callback: (response: any) => this.handleGoogleResponse(response)
      });
      google.accounts.id.prompt();
    } else {
      this.toastr.error('Google Auth service is loading. Please try again.');
    }
  }

  handleGoogleResponse(response: any) {
    if (response && response.credential) {
      this.isFormSubmitted = true;
      this.tokenService.apiTokenGooglePost({ idToken: response.credential }).subscribe({
        next: (res: any) => {
          let user = {
            userEmail: res.value.userName,
            userId: res.value.userId,
            token: res.value.token,
            userTypeId: res.value.userTypeId,
            consultancyId: res.value.consultancyId,
            consultancyUserId: res.value.consultancyUserId
          };
          this.authService.login(user);
          this.showAuthPromptModal = false;
          this.checkAndAutoSubmitPendingJob();
        },
        error: (err: any) => {
          this.isFormSubmitted = false;
          this.toastr.error('Google Authentication failed. Please try again.');
        }
      });
    }
  }

  checkAndAutoSubmitPendingJob() {
    const pendingJobStr = sessionStorage.getItem('ch_pending_job_post');
    if (pendingJobStr && this.authService.isLoggedIn()) {
      try {
        const pendingJob = JSON.parse(pendingJobStr);
        pendingJob.consultancyUserId = this.sessionService.consultancyUserId;

        this.isFormSubmitted = true;
        this.jobOpeningService.apiJobOpeningAddPost(pendingJob).subscribe({
          next: (res: any) => {
            sessionStorage.removeItem('ch_pending_job_post');
            this.toastr.success('Your job post has been published successfully!', '', {
              timeOut: 4000,
              positionClass: 'toast-top-center'
            });
            this.scrollToTop();
            this.postparams.emit(true);
            this.isFormSubmitted = false;
          },
          error: (err: any) => {
            sessionStorage.removeItem('ch_pending_job_post');
            this.toastr.error('Failed to post job. Please try again.');
            this.isFormSubmitted = false;
          }
        });
      } catch (e) {
        sessionStorage.removeItem('ch_pending_job_post');
      }
    }
  }

  ngOnInit() {

    this.checkAndAutoSubmitPendingJob();

    this.formData.jobOpeningVisaMaps = [...defaultPostJobVisas];
    this.formData.jobOpeningJobTypes = [...defaultPostJobPositionTypes];

    this.commonService.apiCommonJobTypeGet().subscribe({
      next: (res:any) => {
        this.selectJobPositionTypeList = res.value;
        if (!this.isEdit) {
          const defaultNames = ['c2c - contract', 'w2 - contract', 'w2 - full time'];
          const defaultSelected = (res.value || []).filter((item: any) => {
            const desc = (item.description || item.name || '').trim().toLowerCase();
            return defaultNames.includes(desc);
          });
          this.formData.jobOpeningJobTypes = defaultSelected.length > 0 ? defaultSelected.map((item: any) => ({
            ...item,
            id: item.id || item.jobTypeId,
            jobTypeId: item.jobTypeId || item.id,
            active: true
          })) : [...defaultPostJobPositionTypes];
          this.formData.jobTypeId = this.formData.jobOpeningJobTypes[0]?.id || 7;
        }
      },
      error: (error:any) => {
        
      }
    })

    this.commonService.apiCommonEmploymentTypeGet().subscribe({
      next: (res:any) => {
        this.selectEmploymentTypeList = res.value
      },
      error: (error:any) => {
        
      }
    })

    this.commonService.apiCommonVisaGet().subscribe({
      next: (res:any) => {
        const activeVisas = (res.value || []).filter((v: any) => v.name?.trim().toUpperCase() !== 'ANY');
        this.selectVisaMapsList = activeVisas;
        if (!this.isEdit) {
          const defaultSelected = activeVisas.filter((v: any) => {
            const name = v.name?.trim().toUpperCase();
            return name === 'GC' || name === 'USC';
          });
          this.formData.jobOpeningVisaMaps = defaultSelected.length > 0 ? defaultSelected : [...defaultPostJobVisas];
          this.formData.visaId = this.formData.jobOpeningVisaMaps[0]?.id || 4;
        }
      },
      error: (error:any) => {
        
      }
    })

    this.sharedService.skillsetdatacast.subscribe(data => {
      this.selectSkillList = data;
    })

    this.fetchQuotaStatus();

  }

  fetchQuotaStatus() {
    this.isLoadingQuota = true;
    this.http.get<any>(`${environment.rootUrl}/api/Subscription/QuotaStatus`).subscribe({
      next: (res) => {
        this.isLoadingQuota = false;
        if (res && res.value) {
          this.quotaStatus = res.value;
        }
      },
      error: (err) => {
        this.isLoadingQuota = false;
        console.error('Failed to fetch quota status:', err);
      }
    });
  }

  toggleQuotaCollapse() {
    this.isQuotaCollapsed = !this.isQuotaCollapsed;
  }

}
