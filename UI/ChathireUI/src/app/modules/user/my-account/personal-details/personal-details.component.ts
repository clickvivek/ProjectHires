import { Component, ViewChild } from '@angular/core';
import { UserService } from 'src/app/api';
import { UserDetailsDtoForUpdate } from 'src/app/api';
import { SessionService } from 'src/app/core/session/session.service';
import { AuthService } from 'src/app/core/auth/auth.service';
import { SharedService } from 'src/app/modules/shared/services/shared.service';
import { ConsultancyService } from 'src/app/api';
import { ToastrService } from 'ngx-toastr';
import { publicProfileUrlPrefix } from 'src/app/data/various';
import _ from 'underscore';

@Component({
  selector: 'personal-details',
  templateUrl: './personal-details.component.html',
  styleUrls: ['./personal-details.component.scss']
})
export class PersonalDetailsComponent {


isLoaded:boolean = false;
isError:boolean = false;
error = '';

userData: UserDetailsDtoForUpdate = {};
formData: any;
companyData:any = {};
userPhone = '';
userTypeId;

isEdit:boolean = false;

selectCompanyList: any;

publicProfileUrlPrefix = publicProfileUrlPrefix;

isProfileIdSubmitted:boolean = false
isCheckAvailability:boolean = false
isProfileIdAvailable:any = null;

initialProfileId = ""

isFormSubmitted:boolean = false;

  @ViewChild('myProfileForm') myProfileForm:any;
  @ViewChild('telInput') telInput;

  constructor(
    private userService: UserService,
    private consultancyService: ConsultancyService,
    private authService: AuthService,
    private sessionService: SessionService,
    private sharedService: SharedService,
    private toastr: ToastrService
  ) {

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

  clearProfileId() {
    this.formData.publicProfileUserName = this.initialProfileId
  }

  handleProfileId() {
    this.isCheckAvailability = true
    this.isProfileIdSubmitted = false
  }

  onCompanyQuery(event:any) {
    this.consultancyService.apiConsultancySearchConsultanciesGet(event).subscribe({
      next: (res : any) => {
        this.selectCompanyList = res.value
      },
      error: (error:any) => { }
    })
  }

  onCompanyChange(event:any){
    this.userData.consultancyID = event.id
  }

  onJobRoleChange() {
    this.formData.userTypeId = this.userTypeId
  }

  scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  handleCancel() {
    this.isEdit = !this.isEdit
    if(this.myProfileForm.dirty) {
      this.reFetchData()
    }
  }

  refreshUser() {

    let user = {
      userEmail : this.formData.email,
      userId: this.sessionService.userId,
      token: this.authService.token,
      userTypeId: this.formData.userTypeId,
      consultancyId: this.userData.consultancyID,
      consultancyUserId: this.sessionService.consultancyUserId
    }
    this.authService.login(user)

  }

  submitMyProfileForm() {

    this.isFormSubmitted = true

    for (const field in this.myProfileForm.controls) {
      if (this.myProfileForm.controls.hasOwnProperty(field)) {
        const control = this.myProfileForm.controls[field];
        if (control.invalid) {
          console.log(`Invalid field: ${field}`);
        }
      }
    }


    if (!this.myProfileForm.valid) {
      this.scrollToTop()
      this.isFormSubmitted = false
    }
    else {
      
      if(!this.isProfileIdSubmitted) {
        this.formData.publicProfileUserName = this.initialProfileId
      }

      this.formData.updated = new Date().toISOString()
      this.formData.phone = this.userPhone
      this.userData.publicProfileUserName = this.formData.publicProfileUserName

      const { ['publicProfileUserName']: _, ...tempObject } = this.formData;
      this.userData.userDtoForUpdate = tempObject
      
     this.userService.apiUserUpdatePut(this.userData).subscribe({
        next:(res:any) => {

          this.toastr.success( 'Personal details updated successfully', '' , {
            timeOut: 3000,
            positionClass: 'toast-top-center'
          });
          this.scrollToTop()
          this.sharedService.setUserUpdate(true)
          this.refreshUser()
          this.isFormSubmitted = false

        },
        error:(error:any) => {
          this.isFormSubmitted = false
        },
      })

    }

  }

  reFetchData() {

    this.userService.apiUserGetUserByUserNameGet(this.sessionService.userEmail).subscribe({
      next:(res:any) => {
        
        this.formData = res.value[0]
        this.userPhone = this.formData.phone ? this.formData.phone : '';
        this.userTypeId = this.formData.userTypeId?.toString()
        
      },
      error:(error:any) => {
 
      }
    })

  }

  ngOnInit() {

    this.userService.apiUserGetUserByUserNameGet(this.sessionService.userEmail).subscribe({
      next:(res:any) => {
        
        this.formData = res.value[0]
        this.initialProfileId = res.value[0].consultancyUsers[0].publicProfileUserName
        this.formData.publicProfileUserName = this.initialProfileId
        
        this.userPhone = this.formData.phone ? this.formData.phone : '';
        this.userTypeId = this.formData.userTypeId?.toString()
        console.log(this.formData)
        this.isLoaded = true
        this.isError = false
        

      },
      error:(error:any) => {
        console.log(error)
        this.isLoaded = true
        this.isError = true
        this.error = 'Some error occured'
      }
    })

    this.consultancyService.apiConsultancyConsultancyByIdGet(this.sessionService.consultancyId).subscribe({
      next: (res:any) => {
        this.companyData = res.value;
      },
      error: (error:any) => {
        console.log(error);
      }
    })

  }

}
