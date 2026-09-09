import { Component, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Router } from '@angular/router';

import { AuthRequestModel } from 'src/app/api';
import { TokenService } from 'src/app/api/api/token.service';
import { SessionService } from 'src/app/core/session/session.service';
import { AuthService } from 'src/app/core/auth/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  formData = {
    email: '',
    password: '',
    rememberMe: false
  }

  showPassword:boolean = false
  isFormSubmitted:boolean = false;
  error:string = ""

  loginData:AuthRequestModel = {};

  @ViewChild('loginForm', {static: false}) loginForm: NgForm;

  constructor(
    public router: Router,
    private authService: AuthService,
    private tokenService: TokenService,
    private sessionService: SessionService
    ) {

      this.formData.rememberMe = false;
      let rememberedUseremail = this.sessionService.getRememberedUseremail()
      if (rememberedUseremail) {
        this.formData.email = rememberedUseremail;
        this.formData.rememberMe = true;
      }

  }


  handleTogglePassword() {
    this.showPassword = !this.showPassword
  }

  login() {

    if(this.loginForm.valid) {

      this.isFormSubmitted = true
      this.error = ""

      this.loginData = {
        'eMail':this.formData.email,
        'pwd':this.formData.password
      }

      this.tokenService.apiTokenPost(this.loginData).subscribe({
        next: (res: any) => {
          
          this.isFormSubmitted = false

          if (this.formData.rememberMe) {
            this.sessionService.setRememberedUseremail(this.formData.email)
          }
          else {
            localStorage.removeItem('ch_remembered_useremail');
          }

          let user = {
            userEmail : this.formData.email,
            userId: res.value.userId,
            token: res.value.token,
            //userTypeName: res.value.userTypeName, //not used anywhere as of now
            userTypeId: res.value.userTypeId,
            consultancyId: res.value.consultancyId,
            consultancyUserId: res.value.consultancyUserId
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

  }


  ngOnInit() {



  }

}
