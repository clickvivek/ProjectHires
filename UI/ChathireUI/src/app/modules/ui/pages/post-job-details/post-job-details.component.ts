import { Component, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { NgForm } from '@angular/forms';

import { JobOpeningService } from 'src/app/api';
import { JobOpeningDto } from 'src/app/api/model/job-opening-dto';

import { CommonService } from 'src/app/api';
import { SessionService } from 'src/app/core/session/session.service';
import { SharedService } from 'src/app/modules/shared/services/shared.service';
import { ToastrService } from 'ngx-toastr';

import { defaultPostJobVisas } from 'src/app/data/various';

import _ from 'underscore';

import { AuthService } from 'src/app/core/auth/auth.service';
import { TokenService } from 'src/app/api/api/token.service';

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

  formData: any = {
    name: "",
    country: "",
    joiningdays: "",
    description: "",
    totalExp: null,
    postedDate: null,
    lastDate: null,
    jobTypeId: null,
    employmentTypeId: null,
    visaId: null,
    cityId: null,
    jobOpeningJobTypes: [],
    jobOpeningEmploymentTypes: [],
    jobOpeningVisaMaps: [],
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
    private tokenService: TokenService
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
              employmentTypeId: item.employmentTypeId,
              description: item.employmentType.description
            }
            arrData.push(itemData)
          });
         this.formData.jobOpeningEmploymentTypes = arrData
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
              city1: city.city1,
              stateName: city.stateName,
              stateCode: city.stateCode,
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
              name: item.skill.name
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

  onPositionTypeChange(event: any) {

    let newData: any = []
    
    if(!_.isEmpty(event)) {
      event.forEach(item => {
        let itemData = {
            jobTypeId: item.jobTypeId || item.id,
            active: true
        }
        newData.push(itemData)
      });
      this.formData.jobOpeningJobTypes = newData;
      this.formData.jobTypeId = event[0].id;
    }
    else {
      this.formData.jobOpeningJobTypes = []
      this.formData.jobTypeId = null
    }

  }

  onEmploymentTypeChange(event: any) {

    let newData: any = []

    if(!_.isEmpty(event)) {
      event.forEach(item => {
        let itemData = {
            employmentTypeId: item.employmentTypeId || item.id,
            active: true
        }
        newData.push(itemData)
      });
      this.formData.jobOpeningEmploymentTypes = newData;
      this.formData.employmentTypeId = event[0].id;
    }
    else {
      this.formData.jobOpeningEmploymentTypes = []
      this.formData.employmentTypeId = null
    }

  }

  onVisaMapChange(event: any) {
   
    this.formData.jobOpeningVisaMaps = event
    this.formData.visaId = event.id

    let newData:any = []
    if(!_.isEmpty(event)) {
      event.forEach(item => {
        let itemData = {
            visaId: item.visaId || item.id
        }
        newData.push(itemData)
      });
      this.formData.jobOpeningVisaMaps = newData;
      this.formData.visaId = event[0].id;
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
          id: index,
          cityId: item.cityId || item.id,
          numberOfOpenings: item.numberOfOpenings
        }
        newData.push(itemData)
      });
      this.formData.jobOpeningLocations = newData;
      this.formData.cityId = event[0].id;
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
            skillId: item.skillId || item.id,
            isMandate: true,
            active: true
        }
        newData.push(itemData)
      });
      this.formData.jobOpeningSkills = newData;
      this.formData.skillId = event[0].id;
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
    
     if(!this.postJobsForm.valid) {
      this.scrollToTop()
      this.isFormSubmitted = false
     }
     else {

      if(!this.isEdit) {

        this.job = {
          name: this.formData.name,
          country: "USA", //hard coded
          joiningdays: 0,
          description: this.formData.description,
          totalExp: parseInt(this.formData.totalExp),
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
          country: this.formData.country,
          joiningdays: this.formData.joiningdays,
          description: this.formData.description,
          totalExp: parseInt(this.formData.totalExp),
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
       
      this.jobOpeningService.apiJobOpeningAddPost(this.job).subscribe({
          next: (res : any) => {
            this.toastr.success(`Job ${this.isEdit ? 'reposted' : 'posted'} successfully`, '' , {
              timeOut: 3000,
              positionClass: 'toast-top-center'
            });
           this.scrollToTop();
           
           if(this.isEdit) {
            this.outputparams.emit(true)
           }
           else {
            this.postparams.emit(true)
           }

           this.isFormSubmitted = false

          },
          error: (error:any) => {
            this.toastr.error('Some error occured', '' , {
              timeOut: 3000,
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

    this.formData.jobOpeningVisaMaps = defaultPostJobVisas

    this.commonService.apiCommonJobTypeGet().subscribe({
      next: (res:any) => {
        this.selectJobPositionTypeList = res.value
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
        this.selectVisaMapsList = res.value
      },
      error: (error:any) => {
        
      }
    })

    this.sharedService.skillsetdatacast.subscribe(data => {
      this.selectSkillList = data;
    })

  }

}
