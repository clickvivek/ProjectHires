import { Component } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute, Params } from '@angular/router';
import { filter } from 'rxjs/operators';
import { MatDialog } from '@angular/material/dialog';
import { picUrl, defaultProfilePic } from 'src/app/data/various';

import * as moment from 'moment';
import _ from 'underscore';

import { ConfirmApplyJobComponent } from '../../public/searchjobs/confirm-apply-job/confirm-apply-job.component';

import { UserService } from 'src/app/api/api/user.service';
import { JobOpeningService } from 'src/app/api/api/job-opening.service';
import { AuthService } from 'src/app/core/auth/auth.service';
import { SharedService } from 'src/app/modules/shared/services/shared.service';
import { SessionService } from 'src/app/core/session/session.service';
import { TalkService } from 'src/app/modules/talk/talk.service';


@Component({
  selector: 'app-job-details-page',
  templateUrl: './job-details-page.component.html',
  styleUrls: ['./job-details-page.component.scss']
})
export class JobDetailsPageComponent {

  previousQueryParams: Params = {}

  skill: string = "";
  location: string = "";
  cityId: any = null;
  visasId: any = [];
  wmIds: any = [];
  expIds: any = [];
  dateString: string = "";

  jobId:any;
  selectedJob:any;

  chatUser:any

  isUserOnline: boolean = false;

  profilePicUrl: string = "";

  isLoaded:boolean = false;

  constructor(
    public dialog: MatDialog,
    public _router: Router,
    private route: ActivatedRoute,
    private userService: UserService,
    private talkService: TalkService,
    private authService: AuthService,
    private sharedService: SharedService,
    private jobOpeningService: JobOpeningService,
    private sessionService: SessionService
  ) {

    this._router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {

      const params = this.route.snapshot.queryParams;

      if (JSON.stringify(params) !== JSON.stringify(this.previousQueryParams) && !_.isEmpty(params)) {
        
        this.previousQueryParams = params;

        if(params['id']) {
          const randomArray = params['id'].split('-')
          const randomId = randomArray[3]
          this.jobId = randomId.substring(1, randomId.length - 1);
        }

        this.skill = params['skill'];
        this.location = params['location'];

         if (!_.isUndefined(params['cid'])) {
          this.cityId =params['cid'];
        }
         else {
           this.cityId = null
        }

        if (!_.isUndefined(params['visas'])) {
          this.visasId =params['visas']?.split(',');
        }
        else {
          this.visasId = []
        }

        if (!_.isUndefined(params['wm'])) {
          this.wmIds =params['wm']?.split(',');
        }
        else {
          this.wmIds = []
        }

        if (!_.isUndefined(params['exp'])) {
          this.expIds =params['exp']?.split(',');
        }
        else {
          this.expIds = []
        }

        if (!_.isUndefined(params['date'])) {
          this.dateString =params['date'];
        }
        else {
          this.dateString = ""
        }
        
        this.handleJobOpenings(this.skill, this.cityId, this.visasId, this.wmIds, this.expIds, this.dateString)


      }

    });

  }

  isChatDisabled() {
    return this.selectedJob.userId == this.sessionService.userId
  }

  getProfilePic(url) {
    if(url)
      return `${picUrl}${url}`
    else
      return defaultProfilePic
  }

  getFormattedLocations(locations: any): string {
    if (!locations || !Array.isArray(locations) || locations.length === 0) {
      return '';
    }
    const formatted = locations
      .filter(loc => loc && typeof loc === 'string' && loc.trim().length > 0)
      .map(loc => {
        let trimmed = loc.trim().replace(/[,;\s]+$/, '');
        if (trimmed.includes('-')) {
          const parts = trimmed.split('-');
          return parts.map(p => p.trim()).filter(p => p).join(', ');
        }
        if (trimmed.includes(',')) {
          const parts = trimmed.split(',');
          return parts.map(p => p.trim()).filter(p => p).join(', ');
        }
        return trimmed;
      })
      .filter(str => str.length > 0);

    return formatted.join(' ; ');
  }

  copyURL() {
    navigator.clipboard.writeText("");
  }

  handleApplyJobConfirmModal(){

    if(this.authService.isLoggedIn()) {
      const applyJobDialogRef = this.dialog.open(ConfirmApplyJobComponent, {
        panelClass: 'material',
        disableClose: true,
        data: this.selectedJob
      });
    }
    else {
      this.sharedService.setPageToRetain({page: this._router.url, job: this.selectedJob})
      this._router.navigate(['/login']);


    }
    
  }

  handleJobOpenings(skill, cityIdArr, visasArr, employmentTypesArr, expArr, dateString) {

    const searchStrings = [skill]
    const cityIds = [cityIdArr]

    let startYearsOfExpInput:any = undefined;
    let endYearsOfExpInput: any = undefined;
    
    let startDate:any = undefined;
    let endDate:any = undefined

    this.jobOpeningService.apiJobOpeningSearchJobOpeningsGet(searchStrings, cityIds, undefined, visasArr, employmentTypesArr, undefined, startYearsOfExpInput, endYearsOfExpInput, undefined, undefined, startDate, endDate).subscribe({
      next: (res: any) => {

        const jobItem = res.filter(item => item.jobOpeningId == this.jobId )
        this.selectedJob = jobItem[0]
        console.log(this.selectedJob)

        this.isLoaded = true

        this.userService.apiUserGetUserByUserNameGet(this.selectedJob.userName).subscribe({
          next: (res : any) => {
  
            const user = res.value[0]
            const profileUserName = user?.consultancyUsers[0].publicProfileUserName
            this.chatUser = this.selectedJob
            this.chatUser.profileUserName = profileUserName
  
            this.talkService.fetchTalkUserPresence(this.chatUser.userId).subscribe((res:any) => {
              let newData = res.data
              let userData = newData[this.chatUser.userId]
              if(userData?.status === 'online') {
                this.isUserOnline = true
              }
              else {
                this.isUserOnline = false
              }
            })
  
          },
          error: (error : any) => {},
        })


      }, 
      error: (error: any) => {
        
      }
    })

  }

  getPostedDays(date) {
    const targetDate = moment(date).toDate();
    const currentDate = new Date();
    const differenceInDays = moment(currentDate).diff(targetDate, 'days');
    if (differenceInDays > 30) {
      const differenceInMonths = moment(currentDate).diff(targetDate, 'months');
      return `${differenceInMonths} months `;
    }
    else if (differenceInDays === 1) {
      const differenceInHours = moment(currentDate).diff(targetDate, 'hours');
      return `${differenceInHours} hours `;
    }
    else {
      return `${differenceInDays} days`;
    }
    
  }

  ngOnInit() {

  }

}
