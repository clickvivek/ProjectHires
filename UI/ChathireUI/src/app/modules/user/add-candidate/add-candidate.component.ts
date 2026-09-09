import { Component, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';

import { CandidateProfileService } from 'src/app/api/api/candidate-profile.service';
import { CandidateProfileForInsertDto } from '../../../api/model/candidate-profile-for-insert-dto';

import { SessionService } from 'src/app/core/session/session.service';
import { CommonService } from 'src/app/api';
import { ToastrService } from 'ngx-toastr';

import _ from 'underscore';

@Component({
  selector: 'app-add-candidate',
  templateUrl: './add-candidate.component.html',
  styleUrls: ['./add-candidate.component.scss']
})
export class AddCandidateComponent implements OnInit {

  formData:any = {
    candidateName: '',
    title: '',
    totalExp: null,
    billingRangeId: '',
    fromAmt: 0,
    toAmt: 0,
    linkedIn: '',
    visaId: null,
    cityId: null,
    skillId: 'null',
    availability: null,
    stateId: null,
    candidateProfileSkills: [],
    candidatePrefLocations: [],
  }

  selectRemoteLocation:string = ""

  selectLocationList:any;
  selectVisaList:any;
  selectSkillList:any;
  selectAvailabilityList:any;

  selectReLocationList:any;

  selectBillingList:Array<any> = [
    { id: 1, range: "Less than 50"},
    { id: 2, range: "50-60" },
    { id: 3, range: "60-70"},
    { id: 4, range: "70-80"},
    { id: 5, range: "80-90" },
    { id: 6, range: "90-100"},
    { id: 7, range: "100+"}
  ]

  locationPrefList: Array<any> = [
    { label: 'Remote Only', value: "1", id:'locationRemoteProject' },
    { label: 'Open to relocate anywhere', value: "2",  id:'locationRelocateAnywhere' },
    { label: 'Open to relocate specific states', value: '3',  id:'locationRelocateSpecific' }
  ]

  candidate: CandidateProfileForInsertDto = {};

  isEdit:boolean = false;
  candidateId: any = null;
  
  selectedFile: any | null = null;

  candidateDocuments = [];

  isFormSubmitted:boolean = false;

  isCandidatePosted:boolean = false;

  @ViewChild('addCandidateForm', {static: false}) addCandidateForm: NgForm;

  constructor(
    public router: Router,
    private route: ActivatedRoute,
    private sessionService: SessionService,
    private candidateProfileService: CandidateProfileService,
    private commonService: CommonService,
    private toastr: ToastrService
  ) {

    router.events.subscribe((event: any) => {
      if (event instanceof NavigationEnd) {
        this.route.params.subscribe((params) => {
          if(params['id']) {
            this.candidateId = params['id']
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

    this.candidateProfileService.apiCandidateProfileGetGet(this.candidateId)
    .subscribe({
      next: (res : any) => {

        let newData = res.value
        
        this.formData = {
          id: newData.id,
          userId: newData.userId,
          active: newData.active,
          consultingRoleId: newData.consultingRoleId,
          postedDate: newData.postedDate,
          statusId: newData.statusId,
          consultancyUserId: newData.consultancyUserId,
          
          backgroundchecked: true,
          comment: "",
          email: "",
          employerInfo: "",
          passportno: "",
          phone: "",
          resume: "",
          visaExpiryDate: newData.visaExpiryDate,
          firstArrivalDate: newData.firstArrivalDate,

          candidateName: newData.candidateName,
          title: newData.title,
          totalExp: newData.totalExp,
          fromAmt: newData.fromAmt,
          toAmt: newData.toAmt,
          linkedIn: newData.linkedIn,
          visaId: newData.visaId,
          city: newData.city,
          cityId: newData.cityId,
          skillId: null,
          availability: newData.availability,
          candidateProfileSkills: [],
          candidatePrefLocations: [],
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

        //location preference
        if(newData.canRelocate) {
          this.selectRemoteLocation = '3'
          this.formData.stateId = null
        }
        else if(newData.remoteOnly) {
          this.selectRemoteLocation = '1'
          this.formData.stateId = 1
        }
        else if(newData.anyLocation) {
          this.selectRemoteLocation = '2'
          this.formData.stateId = 2
        }

        //skills
        if(!_.isEmpty(newData.candidateProfileSkills)) {
          let arrData:any = []
          newData.candidateProfileSkills.forEach(item => {
            let itemData = {
              skillId: item.skillId,
              name: item.name
            }
            arrData.push(itemData)
          });
          this.formData.candidateProfileSkills = arrData
        }

        //preferrred locations
        if(!_.isEmpty(newData.candidatePrefLocations)) {
          let arrData:any = []
          newData.candidatePrefLocations.forEach(item => {
            let itemData = {
              cityId: item.cityId,
              city1: item.stateCode + '-' + item.stateName,
              cityName: item.cityName,
              stateName: item.stateName,
              stateCode: item.stateCode
            }
            arrData.push(itemData)
          });
          this.formData.candidatePrefLocations = arrData
        }

        this.candidateDocuments = newData.candidateDocuments
        

      },
      error: (error:any) => { }
    })

  }

  scrollToTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
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

  onVisaChange(event:any){
    this.formData.visaId = event.id
  }

  onAvailabilityChange(event:any) {
    this.formData.availability = event.id
  }

  onLocationQuery(event:any) {
    this.commonService.apiCommonCityGet(event,undefined,false).subscribe({
      next: (res : any) => {
        this.selectLocationList = res.value
      },
      error: (error:any) => { }
    })
  }

  onLocationChange(event:any){
    this.formData.cityId = event.id
  }

  onSkillQuery(event:any){
    this.commonService.apiCommonSkillsGet(event).subscribe({
      next: (res : any) => {
        this.selectSkillList = res.value
      },
      error: (error:any) => { }
    })
  }

  onRemoteChange(value): void {
    
    this.selectRemoteLocation = value
    if(value != '3') {
      this.formData.stateId = parseInt(value)
    }
    else {
      this.formData.stateId = null
    }
  }

  onReLocationQuery(event:any) {
    this.commonService.apiCommonCityGet(event,undefined,true).subscribe({
      next: (res : any) => {
        this.selectReLocationList = res.value
      },
      error: (error:any) => { }
    })
  }

  getRelocationRequired() {
    return this.selectRemoteLocation == '3' ? true : false
  }

  onSelectedSkills(event:any) {
    let newData:any = []
    if(!_.isEmpty(event)) {
      event.forEach((item, index) => {
        let itemData = {
            id: index,
            candidateProfileid: this.formData.id,
            skillId: item.skillId || item.id,
            active: true,
            proficiencyLevel: 0,
            yearsOfExp: 0
        }
        newData.push(itemData)
      });
      this.formData.candidateProfileSkills = newData;
      this.formData.skillId = event[0].id;
    }
    else {
      this.formData.candidateProfileSkills = []
      this.formData.skillId = null
    }
  }

  onSelectedLocations(event:any) {
    let newData:any = []
    if(!_.isEmpty(event)) {
      event.forEach((item, index) => {
        let itemData = {
            id: index,
            candidateId: this.formData.id,
            cityId: item.id || item.cityId
        }
        newData.push(itemData)
      });
      this.formData.candidatePrefLocations = newData;
      this.formData.stateId = event[0].id;
    }
    else {
      this.formData.candidatePrefLocations = []
      this.formData.stateId = null
    }
  }

  attachResume(event: any) {
    this.selectedFile = event.target.files[0];
  }

  deleteFile() {
    this.selectedFile = null;
  }

  uploadResume(id) {
    
    this.candidateProfileService.apiCandidateProfileUpdateDocumentPost(id, 1, this.selectedFile).subscribe({
      next: (res: any) => { 
        this.successToast()
      },
      error: (error: any) => { 
        this.errorToast()
      }
    })
  }

  successToast() {
    this.toastr.success(`Candidate ${this.isEdit ? 'updated' : 'added'} successfully`, '' , {
      timeOut: 3000,
      positionClass: 'toast-top-center'
    });
    this.scrollToTop();
  }

  errorToast() {
    this.toastr.error('Some error occured', '' , {
      timeOut: 3000,
      positionClass: 'toast-top-center'
    });
    this.scrollToTop();
  }

  addCandidate() {

    this.isFormSubmitted = true

    for (const field in this.addCandidateForm.controls) {
      if (this.addCandidateForm.controls.hasOwnProperty(field)) {
        const control = this.addCandidateForm.controls[field];
        if (control.invalid) {
          console.log(`Invalid field: ${field}`);
        }
      }
    }


    if (!this.addCandidateForm.valid) {
      this.scrollToTop()
      this.isFormSubmitted = false
    }
    else {

      if(!this.isEdit) {

        this.candidate = {
          userId: this.sessionService.userId,
          consultancyUserId: this.sessionService.consultancyUserId,
          candidateName: this.formData.candidateName,
          title: this.formData.title,
          active: true,
          postedDate: new Date().toISOString(),
          canRelocate: this.selectRemoteLocation === '3' ? true : false,
          totalExp: this.formData.totalExp,
          statusId: 2,
          linkedIn: this.formData.linkedIn,
          availability: this.formData.availability,
          visaId: this.formData.visaId,
          cityId: this.formData.cityId,
          remoteOnly: this.selectRemoteLocation === '1' ? true : false,
          anyLocation: this.selectRemoteLocation === '2' ? true : false,
          fromAmt: this.formData.fromAmt,
          toAmt: this.formData.toAmt,
          consultingRoleId: 1,
          candidateProfileSkills: this.formData.candidateProfileSkills,
          candidatePrefLocations: this.formData.candidatePrefLocations
        }


        this.candidateProfileService.apiCandidateProfileAddPost(this.candidate).subscribe({
          next: (res: any) => {
            this.isFormSubmitted = false
            this.isCandidatePosted = true;
            this.addCandidateForm.resetForm()
            if (this.selectedFile) {
              this.uploadResume(res.value.id)
            }
            else {
              this.successToast()
            }
          },
          error: (error:any) => {
            this.errorToast()
            this.isFormSubmitted = false
            this.isCandidatePosted = true;
          }
        })

      }
      else {

        this.candidate = {
          id: this.formData.id,
          userId: this.formData.userId,
          consultancyUserId: this.formData.consultancyUserId,
          postedDate: this.formData.postedDate,
          statusId: this.formData.statusId,
          
          backgroundchecked: this.formData.backgroundchecked,
          comment: this.formData.comment,
          email: this.formData.email,
          employerInfo: this.formData.employerInfo,
          passportno: this.formData.passportno,
          phone: this.formData.phone,
          resume: this.formData.resume,
          visaExpiryDate: this.formData.visaExpiryDate,
          firstArrivalDate: this.formData.firstArrivalDate,

          candidateName: this.formData.candidateName,
          title: this.formData.title,
          active: this.formData.active,
          canRelocate: this.selectRemoteLocation === '3' ? true : false,
          totalExp: this.formData.totalExp,
          linkedIn: this.formData.linkedIn,
          availability: this.formData.availability,
          visaId: this.formData.visaId,
          cityId: this.formData.cityId,
          remoteOnly: this.selectRemoteLocation === '1' ? true : false,
          anyLocation: this.selectRemoteLocation === '2' ? true : false,
          fromAmt: this.formData.fromAmt,
          toAmt: this.formData.toAmt,
          consultingRoleId: this.formData.consultingRoleId,
          candidateProfileSkills: this.formData.candidateProfileSkills,
          candidatePrefLocations: this.formData.candidatePrefLocations
        }

       this.candidateProfileService.apiCandidateProfileUpdatePut(this.candidate).subscribe({
          next: (res : any) => {
            this.isFormSubmitted = false
            this.isCandidatePosted = true;
            this.addCandidateForm.resetForm()
            if (this.selectedFile) {
              this.uploadResume(res.value.id)
            }
            else {
              this.successToast()
            }
          },
          error: (error:any) => {
            this.isFormSubmitted = false
            this.isCandidatePosted = true;
            this.errorToast()
          }
        })

      }

    }

  }

  ngOnInit() {

    this.commonService.apiCommonVisaGet().subscribe({
      next: (res : any) => {
        this.selectVisaList = res.value
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
