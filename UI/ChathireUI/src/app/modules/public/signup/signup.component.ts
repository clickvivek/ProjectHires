import { Component, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';

import { UserDto } from 'src/app/api/'
import { AuthService } from 'src/app/core/auth/auth.service';
import { UserService } from 'src/app/api/api/user.service';
import { TokenService } from 'src/app/api/api/token.service';
import { SessionService } from 'src/app/core/session/session.service';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.scss']
})
export class SignupComponent implements OnInit {

  formData = {
    email: '',
    password: '',
    agreeTerms: false
  }

  showPassword:boolean = false
  isFormSubmitted:boolean = false;
  error:string = "";

  user: UserDto;

  @ViewChild('signupForm', {static: false}) signupForm: NgForm;

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private tokenService: TokenService,
    private sessionService: SessionService
  ) { }

  handleTogglePassword() {
    this.showPassword = !this.showPassword
  }

  signup() {

    if(this.signupForm.valid) {

      this.isFormSubmitted = true
      this.error = ""

      this.user = {
        "email": this.formData.email,
        "password": this.formData.password,
        "active": true,
        "userTypeId": 1,
        "updated": new Date().toISOString()
      }

      this.userService.apiUserAddPost(this.user).subscribe({
        next: (res : any) => {

          this.isFormSubmitted = false

          if(res.value) {

            let loginData = {
              "eMail":res.value.email,
              "pwd":res.value.password
            }

            this.tokenService.apiTokenPost(loginData).subscribe({
              next: (res : any) => {

                this.isFormSubmitted = false

                let rememberedUseremail = this.sessionService.getRememberedUseremail()
                if (rememberedUseremail) {
                  localStorage.removeItem('ch_remembered_useremail');
                }

                let user = {
                  userEmail : this.formData.email,
                  userId: res.value.userId,
                  consultancyId: null,
                  consultancyUserId: null,
                  token: res.value.token,
                  userTypeName: res.value.userTypeName,
                  userTypeId: res.value.userTypeId
                }
                this.authService.login(user)

              },
              error: (error:any) => {

                console.log(error)

                this.isFormSubmitted = false

                if(error.status == 422) {
                  this.error = error.error?.errors[0].message
                }
                else if(error.status == 0) {
                  this.error = "Network Error"
                }

              }
            })

          }

        },
        error: (error:any) => {

          console.log(error)

          this.isFormSubmitted = false

          if(error.status == 422) {
            this.error = error.error?.errors[0].message
          }
          else if(error.status == 0) {
            this.error = "Network Error"
          }
          else {
            this.error = error.errors[0].message
          }
        }
      })

    }

  }

  ngOnInit() {
  }

}
