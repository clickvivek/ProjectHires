import { Component, OnInit, EventEmitter } from '@angular/core';
import { MatDialogRef } from "@angular/material/dialog";
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';

import { ConsultancyService, UserService } from 'src/app/api';
import { AuthService } from 'src/app/core/auth/auth.service';
import { SessionService } from 'src/app/core/session/session.service';
import { publicProfileUrlPrefix } from 'src/app/data/various';

import _ from 'underscore';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss'],
  providers: [
    { provide: MatDialogRef, useValue: {} },
  ]
})
export class ProfileComponent  {

  user:any;
  company: any = {};
  myJobs:any;
  myHotlist:any;

  selectedJobId:number = -1;
  selectedJob:any;

  isJobName:boolean = false;

  profileId:any;
  profileUrlPrefix = publicProfileUrlPrefix;
  profileUrl = ""

  consultancyUserId:any;

  isLoaded:boolean = false;
  isProfileAvailable:boolean = false;
  isJobAvailable:boolean = true;

  chatUser:any = {}

  jobDetailsChanged: EventEmitter<any> = new EventEmitter();

  constructor(
    public router: Router,
    private route: ActivatedRoute,
    private authService: AuthService,
    private userService: UserService,
    private consultancyService: ConsultancyService,
    private sessionService: SessionService
  ) { 

    router.events.subscribe((event: any) => {
      
      if (event instanceof NavigationEnd) {
        this.route.params.subscribe((params) => {
          if(params['id']) {
            this.profileId = params['id']
            this.profileUrl = `${this.profileUrlPrefix}${this.profileId}`
            //this.fetchProfile()
            this.fetchUser()
          }
        });
      }

    });

  }

  copyURL(text) {
    navigator.clipboard.writeText(text);
  }

  isLoggedIn() {
    return this.authService.isLoggedIn()
  }

  isPublic() {
    return this.sessionService.userId != this.user.id
  }

  isSelected(id){
    return this.selectedJobId == id ? 'selected' : '';
  }

  handleJobs(event) {
    this.isJobAvailable = event
  }

  showJobDescription(job){
    if(this.selectedJobId != job.id){
      this.selectedJobId = job.id;
    }
    else {
      this.selectedJobId = -1;
    }
    this.selectedJob = job;
    this.jobDetailsChanged.emit(this.selectedJob);
  }

  fetchProfile() {
    this.isLoaded = false;
    this.userService.apiUserGetUserByPublicProfileIdGet(this.profileId).subscribe({
      next:(res:any) => {
        if(!_.isEmpty(res.value)) {
          let publicUser = res.value[0]
          let consultancyId = publicUser.consultancyUsers[0].consultancyId
          this.consultancyUserId = publicUser.consultancyUsers[0].id
          this.user = publicUser.consultancyUsers[0].user
          this.fetchCompanyDetails(consultancyId)
          this.isLoaded = true
          this.isProfileAvailable = true
        }
        else {
          this.user = null
          this.isLoaded = true
          this.isProfileAvailable = false
        }
        
      },
      error:() => {
        
      }
    })
  }

  fetchCompanyDetails(id) {
    this.consultancyService.apiConsultancyConsultancyByIdGet(id).subscribe({
      next: (res:any) => {
        this.company = res.value;
        this.chatUser.companyName = this.company.name
      },
      error: (error:any) => {
        console.log(error);
      }
    })
  }

  fetchUser() {
    this.isLoaded = false;
    this.userService.apiUserGetUserByPublicProfileIdGet(this.profileId).subscribe({
      next: (res : any) => {

        if(!_.isEmpty(res.value)) {
          
          let publicUser = res.value[0]
          let consultancyId = publicUser.consultancyUsers[0].consultancyId
          this.consultancyUserId = publicUser.consultancyUsers[0].id
          this.user = publicUser.consultancyUsers[0].user
          this.fetchCompanyDetails(consultancyId)

          const profileUserName = publicUser?.consultancyUsers[0].publicProfileUserName

          this.chatUser = {
            userId: publicUser.id,
            userFName: publicUser.fname,
            userLName: publicUser.lname,
            profilePic: publicUser.profilePic,
            profileUserName: profileUserName
          }

          this.isLoaded = true
          this.isProfileAvailable = true

        }
        else {
          this.user = null
          this.isLoaded = true
          this.isProfileAvailable = false
        }
        
      },
      error: (error : any) => {},
    })
  }

  ngOnChanges() {

    
    
  }

}
