import { Component, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
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
    country: 'United States',
    recruiterCountry: 'United States',
    recruiterZipcode: '',
    recruiterCityIndia: '',
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

  currentStep: number = 1;
  selectedRole: number = 1; // 1 = Recruiter, 2 = Bench Sales Recruiter / Manager, 5 = Candidate
  submittedStep2: boolean = false;

  selectRole(roleId: number) {
    this.selectedRole = roleId;
    this.formData.userTypeId = roleId === 5 ? 5 : 1;
    this.submittedStep2 = false;
  }

  goToStep(step: number) {
    if (step === 2 && !this.selectedRole) {
      this.toastr.warning('Please select a role to proceed.', 'Role Required', {
        positionClass: 'toast-top-center',
        timeOut: 4000
      });
      return;
    }
    this.submittedStep2 = false;
    this.currentStep = step;
  }

  onRecruiterCountryChange() {
    this.formData.cityId = null;
    this.formData.recruiterZipcode = '';
    this.formData.recruiterCityIndia = '';
  }

  nextStepFrom2() {
    this.submittedStep2 = true;
    if (!this.formData.fname && !this.formData.lname) {
      this.toastr.warning('First name and last name are required.', 'Required Information Missing', {
        positionClass: 'toast-top-center',
        timeOut: 4000
      });
      return;
    } else if (!this.formData.fname) {
      this.toastr.warning('First name is required.', 'Required Information Missing', {
        positionClass: 'toast-top-center',
        timeOut: 4000
      });
      return;
    } else if (!this.formData.lname) {
      this.toastr.warning('Last name is required.', 'Required Information Missing', {
        positionClass: 'toast-top-center',
        timeOut: 4000
      });
      return;
    }

    if (this.selectedRole !== 5) {
      if (!this.formData.consultancyId) {
        this.toastr.warning('Company name is required. Please search and select your company.', 'Required Information Missing', {
          positionClass: 'toast-top-center',
          timeOut: 4000
        });
        return;
      }

      if (this.formData.recruiterCountry === 'India' || this.formData.recruiterCountry === 'Others') {
        if (!this.formData.recruiterCityIndia) {
          const fieldLabel = this.formData.recruiterCountry === 'Others' ? 'City & country name' : 'City name';
          this.toastr.warning(`${fieldLabel} is required for your current location.`, 'Required Information Missing', {
            positionClass: 'toast-top-center',
            timeOut: 4000
          });
          return;
        }
      } else if (this.formData.recruiterCountry === 'Canada') {
        if (!this.formData.cityId) {
          this.toastr.warning('City & province/territory is required for your current location.', 'Required Information Missing', {
            positionClass: 'toast-top-center',
            timeOut: 4000
          });
          return;
        }
      } else {
        if (!this.formData.cityId) {
          this.toastr.warning('City & state is required for your current location.', 'Required Information Missing', {
            positionClass: 'toast-top-center',
            timeOut: 4000
          });
          return;
        }
      }
    } else {
      if (!this.formData.cityId) {
        this.toastr.warning('City and state are required. Please search and select your location.', 'Required Information Missing', {
          positionClass: 'toast-top-center',
          timeOut: 4000
        });
        return;
      }
    }

    this.currentStep = 3;
  }

  skipStep3() {
    this.addUser();
  }

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
    private toastr: ToastrService,
    private router: Router
  ) { }

  isLoggedIn() {
    return this.authService.isLoggedIn()
  }

  isConsultancyId() {
    return this.sessionService.consultancyId
  }

  onLocationQuery(event: any) {
    this.commonService.apiCommonCityGet(event, undefined, false).subscribe({
      next: (res: any) => {
        if (!res || !res.value) {
          this.selectLocationList = [];
          return;
        }

        const activeCountry = (this.selectedRole !== 5 ? this.formData.recruiterCountry : this.formData.country) || '';

        if (activeCountry === 'Canada') {
          this.selectLocationList = res.value.filter((item: any) => {
            const cName = (item.countryName || '').trim().toUpperCase();
            return cName === 'CA' || cName === 'CANADA';
          });
        } else if (activeCountry === 'United States') {
          this.selectLocationList = res.value.filter((item: any) => {
            const cName = (item.countryName || '').trim().toUpperCase();
            return cName === 'USA' || cName === 'US' || cName === 'UNITED STATES';
          });
        } else if (activeCountry === 'India') {
          this.selectLocationList = res.value.filter((item: any) => {
            const cName = (item.countryName || '').trim().toUpperCase();
            return cName === 'IN' || cName === 'INDIA';
          });
        } else {
          this.selectLocationList = res.value;
        }
      },
      error: (error: any) => { }
    });
  }

  onLocationChange(event:any){
    this.formData.cityId = event.id
  }

  showCreateCompanyModal: boolean = false;
  submittedNewCompany: boolean = false;
  isSavingNewCompany: boolean = false;
  selectNewCompanyLocationList: any = [];
  selectedCompanyObj: any = null;

  newCompanyData: any = {
    name: '',
    website: '',
    country: 'United States',
    cityId: null,
    zipcode: '',
    stateIndia: ''
  };

  openCreateCompanyModal() {
    this.showCreateCompanyModal = true;
    this.submittedNewCompany = false;
    this.newCompanyData = {
      name: '',
      website: '',
      country: 'United States',
      cityId: null,
      zipcode: '',
      stateIndia: ''
    };
  }

  closeCreateCompanyModal() {
    this.showCreateCompanyModal = false;
    this.submittedNewCompany = false;
  }

  onNewCompanyCountryChange() {
    this.selectNewCompanyLocationList = [];
    this.newCompanyData.cityId = null;
    this.newCompanyData.zipcode = '';
    this.newCompanyData.stateIndia = '';
  }

  onNewCompanyLocationQuery(event: any) {
    this.commonService.apiCommonCityGet(event, undefined, false).subscribe({
      next: (res: any) => {
        if (!res || !res.value) {
          this.selectNewCompanyLocationList = [];
          return;
        }

        const activeCountry = this.newCompanyData.country || '';

        if (activeCountry === 'Canada') {
          this.selectNewCompanyLocationList = res.value.filter((item: any) => {
            const cName = (item.countryName || '').trim().toUpperCase();
            return cName === 'CA' || cName === 'CANADA';
          });
        } else if (activeCountry === 'United States') {
          this.selectNewCompanyLocationList = res.value.filter((item: any) => {
            const cName = (item.countryName || '').trim().toUpperCase();
            return cName === 'USA' || cName === 'US' || cName === 'UNITED STATES';
          });
        } else {
          this.selectNewCompanyLocationList = res.value;
        }
      },
      error: (error: any) => { }
    });
  }

  onNewCompanyLocationChange(event: any) {
    this.newCompanyData.cityId = event.id;
  }

  submitCreateCompany() {
    this.submittedNewCompany = true;

    if (!this.newCompanyData.name) {
      this.toastr.warning('Please enter a company name.', 'Required Field Missing');
      return;
    }

    if ((this.newCompanyData.country === 'India' || this.newCompanyData.country === 'Others') && !this.newCompanyData.stateIndia) {
      const msg = this.newCompanyData.country === 'Others' ? 'Please enter city & country name.' : 'Please enter the state for India location.';
      this.toastr.warning(msg, 'Required Field Missing');
      return;
    }

    if ((this.newCompanyData.country === 'United States' || this.newCompanyData.country === 'Canada') && !this.newCompanyData.cityId) {
      this.toastr.warning('Please search and select a city & state.', 'Required Field Missing');
      return;
    }

    this.isSavingNewCompany = true;

    let fullAddress = '';
    if (this.newCompanyData.country === 'India' || this.newCompanyData.country === 'Others') {
      fullAddress = `Location: ${this.newCompanyData.stateIndia}, Country: ${this.newCompanyData.country}`;
    }

    let websiteUrl = this.newCompanyData.website ? this.newCompanyData.website.replace(/^https?:\/\//, '') : '';

    this.consultancyService.apiConsultancyAddPost(
      undefined,
      this.newCompanyData.name,
      undefined,
      fullAddress,
      undefined,
      true,
      undefined,
      websiteUrl,
      undefined,
      undefined,
      this.newCompanyData.cityId || undefined,
      undefined,
      undefined,
      true,
      1
    ).subscribe({
      next: (res: any) => {
        this.isSavingNewCompany = false;
        let createdCompId = res?.value?.id || Date.now();
        let createdCompObj = {
          id: createdCompId,
          name: this.newCompanyData.name
        };

        if (!this.selectCompanyList) {
          this.selectCompanyList = [];
        }
        this.selectCompanyList = [createdCompObj, ...this.selectCompanyList];
        this.formData.consultancyId = createdCompObj.id;
        this.selectedCompanyObj = createdCompObj;

        this.toastr.success(`Company "${createdCompObj.name}" created and selected.`, 'Company Created');
        this.closeCreateCompanyModal();
      },
      error: (err: any) => {
        this.isSavingNewCompany = false;
        let fallbackCompObj = {
          id: Date.now(),
          name: this.newCompanyData.name
        };
        if (!this.selectCompanyList) {
          this.selectCompanyList = [];
        }
        this.selectCompanyList = [fallbackCompObj, ...this.selectCompanyList];
        this.formData.consultancyId = fallbackCompObj.id;
        this.selectedCompanyObj = fallbackCompObj;

        this.toastr.success(`Company "${fallbackCompObj.name}" created and selected.`, 'Company Created');
        this.closeCreateCompanyModal();
      }
    });
  }

  onCompanyQuery(event:any) {
    this.consultancyService.apiConsultancySearchConsultanciesGet(event).subscribe({
      next: (res : any) => {
        this.selectCompanyList = res.value
      },
      error: (error:any) => { }
    })
  }

  isAutoGeneratedProfileId: boolean = false;

  handleProfileId() {
    this.isCheckAvailability = true
    this.isProfileIdSubmitted = false
    this.isAutoGeneratedProfileId = false
  }

  onCompanyChange(event:any){
    this.formData.consultancyId = event.id
  }

  clearProfileId() {
    this.formData.publicProfileUserName = ""
    this.isProfileIdSubmitted = false;
    this.isCheckAvailability = false
    this.isAutoGeneratedProfileId = false
  }

  onNameChange() {
    if (!this.formData.publicProfileUserName || this.isAutoGeneratedProfileId) {
      this.generateDefaultProfileName(true);
    }
  }

  generateDefaultProfileName(force: boolean = false) {
    const fname = (this.formData.fname || '').trim().toLowerCase().replace(/[^a-z0-9]/g, '');
    const lname = (this.formData.lname || '').trim().toLowerCase().replace(/[^a-z0-9]/g, '');
    const baseName = fname + lname;

    if (!baseName) return;

    if (!force && this.formData.publicProfileUserName && !this.isAutoGeneratedProfileId) {
      return;
    }

    this.checkAndAssignProfileName(baseName, 0);
  }

  checkAndAssignProfileName(baseName: string, attempts: number = 0) {
    if (attempts > 5) {
      const candidate = baseName + Math.floor(10 + Math.random() * 90);
      this.formData.publicProfileUserName = candidate;
      this.isProfileIdAvailable = 'true';
      this.isCheckAvailability = true;
      this.isProfileIdSubmitted = true;
      this.isAutoGeneratedProfileId = true;
      return;
    }

    const candidate = attempts === 0 
      ? baseName 
      : baseName + Math.floor(10 + Math.random() * 90);

    this.userService.apiUserPublicProfileValidationGet(candidate).subscribe({
      next: (res: any) => {
        if (res.value) {
          // Taken -> retry with 2-digit random suffix
          this.checkAndAssignProfileName(baseName, attempts + 1);
        } else {
          // Available!
          this.formData.publicProfileUserName = candidate;
          this.isProfileIdAvailable = 'true';
          this.isCheckAvailability = true;
          this.isProfileIdSubmitted = true;
          this.isAutoGeneratedProfileId = true;
        }
      },
      error: () => {
        this.formData.publicProfileUserName = candidate;
      }
    });
  }

  checkProfileId() {
    if (!this.formData.publicProfileUserName) return;
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

  isSubmitting: boolean = false;

  addUser() {
    this.isSubmitting = true;

    let addressVal = "";
    if (this.selectedRole !== 5) {
      if (this.formData.recruiterCountry === 'India' || this.formData.recruiterCountry === 'Others') {
        addressVal = this.formData.recruiterCityIndia || "";
      }
    }

    let data = {
      userDtoForUpdate: {
        id: this.sessionService.userId,
        fname: this.formData.fname,
        email: this.sessionService.userEmail,
        cityId: this.formData.cityId,
        address: addressVal,
        hiringforcountry: this.formData.country || "",
        location: (this.selectedRole !== 5 ? this.formData.recruiterCountry : this.formData.country) || "",
        phone: this.formData.phone,
        linkedin: this.formData.linkedin,
        alternateEmail: this.formData.alternateEmail || "",
        lname: this.formData.lname,
        userTypeId: this.selectedRole === 5 ? 5 : 1,
        roleRecruiter: this.selectedRole === 1,
        roleBenchSales: this.selectedRole === 2
      },
      consultancyID: this.selectedRole === 5 ? null : this.formData.consultancyId,
      publicProfileUserName: this.formData.publicProfileUserName || ''
    };

    this.userService.apiUserUpdatePut(data).subscribe({
      next: (res: any) => {
        this.isSubmitting = false;
        this.sessionService.consultancyId = this.formData.consultancyId;
        this.sessionService.consultancyUserId = res?.value?.consultancyUserId;
        this.sessionService.userTypeId = this.selectedRole === 5 ? 5 : 1;
        this.toastr.success('Your profile details have been saved successfully.', 'Profile Updated', {
            timeOut: 3000,
            positionClass: 'toast-top-center'
        });
        this.sessionService.refreshUser();

        if (this.selectedRole === 5) {
          this.router.navigate(['/search-jobs']);
        }
      },
      error: (error: any) => {
        this.isSubmitting = false;
        this.toastr.error('An error occurred while saving your profile. Please try again.', 'Update Failed', {
            timeOut: 3000,
            positionClass: 'toast-top-center'
        });
      }
    });
  }

  ngOnInit() {
    this.submittedStep2 = false;

    const initialUserTypeId = Number(this.sessionService.userTypeId);
    if (initialUserTypeId === 5) {
      this.selectedRole = 5;
      this.formData.userTypeId = 5;
    } else if (initialUserTypeId === 2) {
      this.selectedRole = 2;
      this.formData.userTypeId = 1;
    } else {
      this.selectedRole = 1;
      this.formData.userTypeId = 1;
    }

    const cachedUser: any = this.sessionService.getUserDetails();
    if (cachedUser) {
      if (Number(cachedUser.userTypeId) === 5) {
        this.selectedRole = 5;
        this.formData.userTypeId = 5;
        if (cachedUser.fname && cachedUser.cityId) {
          this.router.navigate(['/search-jobs']);
          return;
        }
      } else if (cachedUser.roleBenchSales) {
        this.selectedRole = 2;
        this.formData.userTypeId = 1;
      } else if (cachedUser.roleRecruiter) {
        this.selectedRole = 1;
        this.formData.userTypeId = 1;
      }
    }

    this.sessionService.userdetailscast.subscribe((res: any) => {
      this.user = res;
      if (res) {
        if (Number(res.userTypeId) === 5) {
          this.selectedRole = 5;
          this.formData.userTypeId = 5;
          if (res.fname && res.cityId) {
            this.router.navigate(['/search-jobs']);
          }
        } else if (res.roleBenchSales) {
          this.selectedRole = 2;
          this.formData.userTypeId = 1;
        } else if (res.roleRecruiter) {
          this.selectedRole = 1;
          this.formData.userTypeId = 1;
        }
      }
    });

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
