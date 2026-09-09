import { Component, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import _ from 'underscore';

import { publicProfileUrlPrefix } from 'src/app/data/various';

import { CommonService } from 'src/app/api';
import { ConsultancyService } from 'src/app/api';
import { UserService } from 'src/app/api';
import { AuthService } from 'src/app/core/auth/auth.service';
import { SessionService } from 'src/app/core/session/session.service';
import { SharedService } from '../../shared/services/shared.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {

  user:any;
  
  formData:any = {
    fname: '',
    consultancyId: null,
    cityId: null,
    phone: '',
    linkedin: "",
    lname: "",
    alternateEmail: "",
    publicProfileUserName: ""
  }

  selectLocationList: any;
  selectCompanyList: any;

  isProfileIdSubmitted:boolean = false;
  isCheckAvailability:boolean = false
  isProfileIdAvailable:any = null;

  publicProfileUrlPrefix = publicProfileUrlPrefix;

  uneadCount = 0;
  
  @ViewChild('addUserForm', { static: false }) addUserForm: NgForm;
  @ViewChild('telInput') telInput;

  constructor(
    private commonService: CommonService,
    private consultancyService: ConsultancyService,
    private userService: UserService,
  	private authService: AuthService,
    private sessionService: SessionService,
    private sharedService: SharedService,
    private toastr: ToastrService
  ) { }

  isLoggedIn() {
    return this.authService.isLoggedIn()
  }

  isConsultancyId() {
    return this.sessionService.consultancyId
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

  onCompanyQuery(event:any) {
    this.consultancyService.apiConsultancySearchConsultanciesGet(event).subscribe({
      next: (res : any) => {
        this.selectCompanyList = res.value
      },
      error: (error:any) => { }
    })
  }

  handleProfileId() {
    this.isCheckAvailability = true
    this.isProfileIdSubmitted = false
  }

  onCompanyChange(event:any){
    this.formData.consultancyId = event.id
  }

  clearProfileId() {
    this.formData.publicProfileUserName = ""
    this.isProfileIdSubmitted = false;
    this.isCheckAvailability = false
  }

  checkProfileId() {
    this.userService.apiUserPublicProfileValidationGet(this.formData.publicProfileUserName).subscribe({
      next:(res:any) => {
        this.isProfileIdSubmitted = true
        if(res.value) {
          this.isProfileIdAvailable = 'false'
        }
        else {
          this.isProfileIdAvailable = 'true'
        }
      },
      error:() => {
        
      }
    })
  }

  addUser() {

    for (const field in this.addUserForm.controls) {
      if (this.addUserForm.controls.hasOwnProperty(field)) {
        const control = this.addUserForm.controls[field];
        if (control.invalid) {
          console.log(`Invalid field: ${field}`);
        }
      }
    }
    if(this.addUserForm.valid) {

      let data = {
        userDtoForUpdate: {
        id: this.sessionService.userId,
        fname: this.formData.fname,
        email: this.sessionService.userEmail,
        cityId: this.formData.cityId,
        phone: this.formData.phone,
        linkedin: this.formData.linkedin,
        alternateEmail: "",
        lname: this.formData.lname
        },
        consultancyID: this.formData.consultancyId,
        publicProfileUserName: this.isProfileIdSubmitted ? this.formData.publicProfileUserName : ''
      }

      this.userService.apiUserUpdatePut(data).subscribe({
        next: (res: any) => {
          this.sessionService.consultancyId = this.formData.consultancyId
          this.sessionService.consultancyUserId = res.value.consultancyUserId
          this.toastr.success('User info updated successfully', '' , {
              timeOut: 3000,
              positionClass: 'toast-top-center'
          });
          this.sessionService.refreshUser()
        },
        error: (error:any) => {
          this.toastr.error('Some error occured', '' , {
              timeOut: 3000,
              positionClass: 'toast-top-center'
          });
        }
      })

    }

    

  }

  ngOnInit() {

    this.sessionService.userdetailscast.subscribe((res: any) => {
      this.user = res
    })

    this.sharedService.inboxunreadcountcast.subscribe((res:any) => {
      if(!_.isEmpty(res)) {
        this.uneadCount = res[0]?.unreadMessageCount
      }
      else {
        this.uneadCount = 0
      }
    })

  }

}
