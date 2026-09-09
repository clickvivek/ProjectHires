import { Component, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';

import { UserService } from 'src/app/api';
import { SessionService } from 'src/app/core/session/session.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'change-password',
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.scss']
})
export class ChangePasswordComponent {

  isLoaded:boolean = false;
  isError:boolean = false;
  error = '';

  showOldPassword:boolean = false
  showNewPassword:boolean = false
  showConfirmPassword:boolean = false

  formData = {
    oldPassword: "",
    newPassword: "",
    confirmPassword: ""
  };

  isFormSubmitted:boolean = false;

  @ViewChild('changePasswordForm', {static: false}) changePasswordForm: NgForm;

  constructor(
    private userService: UserService,
    private sessionService: SessionService,
    private toastr: ToastrService
  ) {

  }

  validatePasswords() {
    
    const newPassword = this.changePasswordForm.value.newPassword;
    const confirmPassword = this.changePasswordForm.value.confirmPassword;

    if (newPassword !== confirmPassword) {
      this.changePasswordForm.controls['confirmPassword'].setErrors({ passwordMismatch: true });
    } else {
      this.changePasswordForm.controls['confirmPassword'].setErrors(null);
    }
  }
  
  scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  changePassword() {

    this.isFormSubmitted = true

    for (const field in this.changePasswordForm.controls) {
      if (this.changePasswordForm.controls.hasOwnProperty(field)) {
        const control = this.changePasswordForm.controls[field];
        if (control.invalid) {
          console.log(`Invalid field: ${field}`);
        }
      }
    }


    if (!this.changePasswordForm.valid) {
      this.scrollToTop()
      this.isFormSubmitted = false
    }
    else {
      this.userService.apiUserUpdatePasswordPut(this.sessionService.userEmail, this.formData.newPassword).subscribe({
        next:() => {
          this.toastr.success( 'Password updated successfully', '' , {
            timeOut: 3000,
            positionClass: 'toast-top-center'
          });
          this.changePasswordForm.resetForm()
          this.fetchData()
          this.scrollToTop()
          this.isFormSubmitted = false
        },
        error:() => {
          this.isFormSubmitted = false
        }
      })
    }
  }

  fetchData() {

    this.userService.apiUserGetUserByUserNameGet(this.sessionService.userEmail).subscribe({
      next:(res:any) => {
        
        const newData = res.value[0]
        this.formData.oldPassword = newData.password

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

  }

  ngOnInit() {
    this.fetchData()
  }

}
