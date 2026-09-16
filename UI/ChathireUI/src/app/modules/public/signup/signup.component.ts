import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';

import { UserDto } from 'src/app/api/';
import { AuthService } from 'src/app/core/auth/auth.service';
import { UserService } from 'src/app/api/api/user.service';
import { TokenService } from 'src/app/api/api/token.service';
import { SessionService } from 'src/app/core/session/session.service';

declare var google: any;

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.scss']
})
export class SignupComponent implements OnInit, OnDestroy {

  step: 'signup' | 'otp' = 'signup';

  formData = {
    email: '',
    password: '',
    agreeTerms: false
  };

  otpCode: string = '';
  showPassword: boolean = false;
  isFormSubmitted: boolean = false;
  isVerifyingOtp: boolean = false;
  isResendingOtp: boolean = false;
  error: string = '';
  successMessage: string = '';

  resendCountdown: number = 60;
  resendTimer: any = null;

  googleClientId: string = '262467975068-u0o6qtjog1o7e1p4jp5kuag34ibhfm1l.apps.googleusercontent.com';

  user: UserDto;

  @ViewChild('signupForm', { static: false }) signupForm: NgForm;
  @ViewChild('otpForm', { static: false }) otpForm: NgForm;

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private tokenService: TokenService,
    private sessionService: SessionService
  ) { }

  handleTogglePassword() {
    this.showPassword = !this.showPassword;
  }

  signupWithGoogle() {
    if (typeof google !== 'undefined' && google.accounts) {
      google.accounts.id.initialize({
        client_id: this.googleClientId,
        callback: (response: any) => this.handleGoogleResponse(response)
      });
      google.accounts.id.prompt();
    } else {
      this.error = "Google Auth service is initializing. Please try again in a moment.";
    }
  }

  handleGoogleResponse(response: any) {
    if (response && response.credential) {
      this.isFormSubmitted = true;
      this.error = "";

      this.tokenService.apiTokenGooglePost({ idToken: response.credential }).subscribe({
        next: (res: any) => {
          this.isFormSubmitted = false;

          let user = {
            userEmail: res.value.userName,
            userId: res.value.userId,
            token: res.value.token,
            userTypeId: res.value.userTypeId,
            consultancyId: res.value.consultancyId,
            consultancyUserId: res.value.consultancyUserId
          };
          this.authService.login(user);
        },
        error: (err: any) => {
          console.error('Google Signup Error:', err);
          this.isFormSubmitted = false;
          this.error = "Google Sign-Up failed. Please try again.";
        }
      });
    }
  }

  signup() {
    if (this.signupForm.valid) {
      this.isFormSubmitted = true;
      this.error = "";
      this.successMessage = "";

      this.user = {
        "email": this.formData.email,
        "password": this.formData.password,
        "active": false,
        "userTypeId": 1,
        "updated": new Date().toISOString()
      };

      this.userService.apiUserAddPost(this.user).subscribe({
        next: (res: any) => {
          this.isFormSubmitted = false;
          // Account created - proceed to OTP verification step
          this.step = 'otp';
          this.otpCode = '';
          this.error = '';
          this.successMessage = `Verification code sent to ${this.formData.email}. Please check your inbox.`;
          this.startResendTimer();
        },
        error: (error: any) => {
          console.error('Signup error:', error);
          this.isFormSubmitted = false;
          if (error.status === 422) {
            this.error = error.error?.errors?.[0]?.message || "Validation error";
          } else if (error.status === 0) {
            this.error = "Network Error. Please check your connection.";
          } else {
            this.error = error.error?.errors?.[0]?.message || error.message || "An error occurred during signup.";
          }
        }
      });
    }
  }

  verifyOtp() {
    if (!this.otpCode || this.otpCode.trim().length !== 6) {
      this.error = "Please enter the valid 6-digit OTP code.";
      return;
    }

    this.isVerifyingOtp = true;
    this.error = "";
    this.successMessage = "";

    this.userService.apiUserVerifyOtpPost({
      email: this.formData.email,
      otp: this.otpCode.trim()
    }).subscribe({
      next: (res: any) => {
        if (res && res.value === true) {
          // Account verified - now issue login token
          let loginData = {
            "eMail": this.formData.email,
            "pwd": this.formData.password
          };

          this.tokenService.apiTokenPost(loginData).subscribe({
            next: (tokenRes: any) => {
              this.isVerifyingOtp = false;

              let rememberedUseremail = this.sessionService.getRememberedUseremail();
              if (rememberedUseremail) {
                localStorage.removeItem('ch_remembered_useremail');
              }

              let user = {
                userEmail: this.formData.email,
                userId: tokenRes.value.userId,
                consultancyId: null,
                consultancyUserId: null,
                token: tokenRes.value.token,
                userTypeName: tokenRes.value.userTypeName,
                userTypeId: tokenRes.value.userTypeId
              };
              this.authService.login(user);
            },
            error: (loginErr: any) => {
              this.isVerifyingOtp = false;
              this.error = "Account verified successfully! Please click Log In to sign in.";
            }
          });
        } else {
          this.isVerifyingOtp = false;
          this.error = "Invalid verification code. Please try again.";
        }
      },
      error: (err: any) => {
        console.error('Verify OTP error:', err);
        this.isVerifyingOtp = false;
        if (err.error && err.error.errors && err.error.errors.length > 0) {
          this.error = err.error.errors[0].message;
        } else {
          this.error = "Failed to verify OTP code. Please check your code and try again.";
        }
      }
    });
  }

  resendOtp() {
    if (this.resendCountdown > 0 || this.isResendingOtp) return;

    this.isResendingOtp = true;
    this.error = "";
    this.successMessage = "";

    this.userService.apiUserResendOtpPost({ email: this.formData.email }).subscribe({
      next: (res: any) => {
        this.isResendingOtp = false;
        this.successMessage = "A new verification code has been sent to your email address.";
        this.startResendTimer();
      },
      error: (err: any) => {
        console.error('Resend OTP error:', err);
        this.isResendingOtp = false;
        if (err.error && err.error.errors && err.error.errors.length > 0) {
          this.error = err.error.errors[0].message;
        } else {
          this.error = "Failed to resend verification code. Please try again.";
        }
      }
    });
  }

  startResendTimer() {
    this.resendCountdown = 60;
    if (this.resendTimer) {
      clearInterval(this.resendTimer);
    }
    this.resendTimer = setInterval(() => {
      if (this.resendCountdown > 0) {
        this.resendCountdown--;
      } else {
        clearInterval(this.resendTimer);
      }
    }, 1000);
  }

  backToSignup() {
    this.step = 'signup';
    this.error = '';
    this.successMessage = '';
    this.otpCode = '';
    if (this.resendTimer) {
      clearInterval(this.resendTimer);
    }
  }

  ngOnInit() {
    setTimeout(() => {
      if (typeof google !== 'undefined' && google.accounts) {
        google.accounts.id.initialize({
          client_id: this.googleClientId,
          callback: (response: any) => this.handleGoogleResponse(response)
        });
      }
    }, 1000);
  }

  ngOnDestroy() {
    if (this.resendTimer) {
      clearInterval(this.resendTimer);
    }
  }

}
