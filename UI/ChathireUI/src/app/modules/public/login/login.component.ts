import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { AuthRequestModel } from 'src/app/api';
import { TokenService } from 'src/app/api/api/token.service';
import { UserService } from 'src/app/api/api/user.service';
import { SessionService } from 'src/app/core/session/session.service';
import { AuthService } from 'src/app/core/auth/auth.service';

declare var google: any;

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {

  // Current view step: 'login' | 'forgot_email' | 'forgot_otp_password'
  step: 'login' | 'forgot_email' | 'forgot_otp_password' = 'login';

  formData = {
    email: '',
    password: '',
    rememberMe: false
  };

  showPassword: boolean = false;
  isFormSubmitted: boolean = false;
  error: string = "";

  // Forgot password state
  forgotEmail: string = '';
  forgotOtp: string = '';
  newPassword: string = '';
  confirmPassword: string = '';
  showNewPassword: boolean = false;
  showConfirmPassword: boolean = false;
  isSendingOtp: boolean = false;
  isResettingPassword: boolean = false;
  forgotErrorMsg: string = '';
  forgotSuccessMsg: string = '';
  resendCountdown: number = 0;
  private resendInterval: any = null;

  googleClientId: string = '262467975068-u0o6qtjog1o7e1p4jp5kuag34ibhfm1l.apps.googleusercontent.com';

  loginData: AuthRequestModel = {};

  @ViewChild('loginForm', { static: false }) loginForm: NgForm;

  constructor(
    public router: Router,
    private route: ActivatedRoute,
    private authService: AuthService,
    private tokenService: TokenService,
    private userService: UserService,
    private sessionService: SessionService
  ) {
    this.formData.rememberMe = false;
    let rememberedUseremail = this.sessionService.getRememberedUseremail();
    if (rememberedUseremail) {
      this.formData.email = rememberedUseremail;
      this.formData.rememberMe = true;
    }
  }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      if (params['mode'] === 'forgot') {
        this.startForgotPassword();
      }
    });

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
    this.clearResendTimer();
  }

  handleTogglePassword() {
    this.showPassword = !this.showPassword;
  }

  // --- FORGOT PASSWORD WORKFLOW ---

  startForgotPassword() {
    this.step = 'forgot_email';
    this.forgotEmail = this.formData.email || '';
    this.forgotErrorMsg = '';
    this.forgotSuccessMsg = '';
    this.error = '';
  }

  backToLogin() {
    this.step = 'login';
    this.forgotErrorMsg = '';
    this.forgotSuccessMsg = '';
    this.clearResendTimer();
  }

  backToForgotEmail() {
    this.step = 'forgot_email';
    this.forgotErrorMsg = '';
    this.forgotSuccessMsg = '';
    this.forgotOtp = '';
    this.newPassword = '';
    this.confirmPassword = '';
    this.clearResendTimer();
  }

  sendForgotPasswordOtp(form: NgForm) {
    if (!form.valid) {
      return;
    }

    this.isSendingOtp = true;
    this.forgotErrorMsg = '';
    this.forgotSuccessMsg = '';

    this.userService.apiUserForgotPasswordPost({ email: this.forgotEmail.trim() }).subscribe({
      next: (res: any) => {
        this.isSendingOtp = false;
        this.step = 'forgot_otp_password';
        this.forgotSuccessMsg = 'A 6-digit verification code has been sent to your email address.';
        this.startResendTimer();
      },
      error: (err: any) => {
        this.isSendingOtp = false;
        if (err.error && typeof err.error === 'string') {
          this.forgotErrorMsg = err.error;
        } else if (err.error?.message) {
          this.forgotErrorMsg = err.error.message;
        } else if (err.error?.errors?.[0]?.message) {
          this.forgotErrorMsg = err.error.errors[0].message;
        } else {
          this.forgotErrorMsg = 'Failed to send verification code. Please make sure the email address is correct.';
        }
      }
    });
  }

  resendOtp() {
    if (this.resendCountdown > 0 || this.isSendingOtp) {
      return;
    }

    this.isSendingOtp = true;
    this.forgotErrorMsg = '';
    this.forgotSuccessMsg = '';

    this.userService.apiUserResendForgotPasswordOtpPost({ email: this.forgotEmail.trim() }).subscribe({
      next: (res: any) => {
        this.isSendingOtp = false;
        this.forgotSuccessMsg = 'A new 6-digit verification code has been sent.';
        this.startResendTimer();
      },
      error: (err: any) => {
        this.isSendingOtp = false;
        if (err.error && typeof err.error === 'string') {
          this.forgotErrorMsg = err.error;
        } else if (err.error?.message) {
          this.forgotErrorMsg = err.error.message;
        } else {
          this.forgotErrorMsg = 'Failed to resend code. Please try again.';
        }
      }
    });
  }

  resetPasswordWithOtp(form: NgForm) {
    if (!form.valid) {
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.forgotErrorMsg = 'Passwords do not match.';
      return;
    }

    if (this.newPassword.length < 6) {
      this.forgotErrorMsg = 'Password must be at least 6 characters long.';
      return;
    }

    this.isResettingPassword = true;
    this.forgotErrorMsg = '';
    this.forgotSuccessMsg = '';

    this.userService.apiUserResetPasswordWithOtpPost({
      email: this.forgotEmail.trim(),
      otp: this.forgotOtp.trim(),
      newPassword: this.newPassword
    }).subscribe({
      next: (res: any) => {
        // Password successfully reset! Automatically authenticate user
        this.tokenService.apiTokenPost({
          eMail: this.forgotEmail.trim(),
          pwd: this.newPassword
        }).subscribe({
          next: (tokenRes: any) => {
            this.isResettingPassword = false;
            let user = {
              userEmail: this.forgotEmail.trim(),
              userId: tokenRes.value.userId,
              token: tokenRes.value.token,
              userTypeId: tokenRes.value.userTypeId,
              consultancyId: tokenRes.value.consultancyId,
              consultancyUserId: tokenRes.value.consultancyUserId
            };
            this.authService.login(user);
          },
          error: (loginErr: any) => {
            this.isResettingPassword = false;
            // If auto-login fails, redirect to standard login with message
            this.step = 'login';
            this.formData.email = this.forgotEmail.trim();
            this.formData.password = '';
            this.error = '';
            alert('Password reset successfully! Please sign in with your new password.');
          }
        });
      },
      error: (err: any) => {
        this.isResettingPassword = false;
        if (err.error && typeof err.error === 'string') {
          this.forgotErrorMsg = err.error;
        } else if (err.error?.message) {
          this.forgotErrorMsg = err.error.message;
        } else if (err.error?.errors?.[0]?.message) {
          this.forgotErrorMsg = err.error.errors[0].message;
        } else {
          this.forgotErrorMsg = 'Failed to reset password. Please check your verification code.';
        }
      }
    });
  }

  private startResendTimer() {
    this.clearResendTimer();
    this.resendCountdown = 60;
    this.resendInterval = setInterval(() => {
      this.resendCountdown--;
      if (this.resendCountdown <= 0) {
        this.clearResendTimer();
      }
    }, 1000);
  }

  private clearResendTimer() {
    if (this.resendInterval) {
      clearInterval(this.resendInterval);
      this.resendInterval = null;
    }
  }

  // --- GOOGLE AUTH ---

  loginWithGoogle() {
    if (typeof google !== 'undefined' && google.accounts) {
      google.accounts.id.initialize({
        client_id: this.googleClientId,
        callback: (response: any) => this.handleGoogleResponse(response)
      });
      google.accounts.id.prompt((notification: any) => {
        if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
          // Fallback to standard Google prompt if One-Tap is dismissed
        }
      });
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
          console.error('Google Login Error:', err);
          this.isFormSubmitted = false;
          this.error = "Google Sign-In failed. Please try again.";
        }
      });
    }
  }

  // --- STANDARD LOGIN ---

  login() {
    if (this.loginForm.valid) {
      this.isFormSubmitted = true;
      this.error = "";

      this.loginData = {
        'eMail': this.formData.email,
        'pwd': this.formData.password
      };

      this.tokenService.apiTokenPost(this.loginData).subscribe({
        next: (res: any) => {
          this.isFormSubmitted = false;

          if (this.formData.rememberMe) {
            this.sessionService.setRememberedUseremail(this.formData.email);
          } else {
            localStorage.removeItem('ch_remembered_useremail');
          }

          let user = {
            userEmail: this.formData.email,
            userId: res.value.userId,
            token: res.value.token,
            userTypeId: res.value.userTypeId,
            consultancyId: res.value.consultancyId,
            consultancyUserId: res.value.consultancyUserId
          };
          this.authService.login(user);
        },
        error: (error: any) => {
          console.log(error);
          this.isFormSubmitted = false;

          if (error.status == 422) {
            this.error = error.error?.errors[0].message;
          } else if (error.status == 0) {
            this.error = "Network Error";
          } else if (error.error && typeof error.error === 'string') {
            this.error = error.error;
          } else {
            this.error = "Invalid email or password. Please try again.";
          }
        }
      });
    }
  }

}

