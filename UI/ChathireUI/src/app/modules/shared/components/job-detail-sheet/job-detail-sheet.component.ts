import { Component, OnInit, Input, Output, Inject, Optional, HostListener, ViewEncapsulation, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatDialogRef } from "@angular/material/dialog";
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { picUrl, defaultProfilePic } from 'src/app/data/various';

import { ConfirmApplyJobComponent } from '../../../public/searchjobs/confirm-apply-job/confirm-apply-job.component';

import * as moment from 'moment';
import _ from 'underscore';

import { UserService } from 'src/app/api/api/user.service';
import { AuthService } from 'src/app/core/auth/auth.service';
import { TalkService } from 'src/app/modules/talk/talk.service';
import { SharedService } from 'src/app/modules/shared/services/shared.service';
import { SessionService } from 'src/app/core/session/session.service';

@Component({
  selector: 'app-job-detail-sheet',
  templateUrl: './job-detail-sheet.component.html',
  styleUrls: ['./job-detail-sheet.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class JobDetailSheetComponent implements OnInit {

  @Input() selectedJob:any;

  chatUser:any

  isMobile: boolean = false;
  profilePicUrl: string = "";

  @Output() outParam = new EventEmitter();

  isUserOnline: boolean = false;


  constructor(
     @Optional() @Inject(MAT_DIALOG_DATA) public modalJobDetails: any,
     public dialog: MatDialog,
     private jobDialogRef: MatDialogRef<JobDetailSheetComponent>,
     private _router: Router,
     private userService: UserService,
     private authService: AuthService,
     private talkService: TalkService,
     private sharedService: SharedService,
     private sessionService: SessionService
  ) { }

  handleJobSheet() {
    this.outParam.emit(false)
  }

  isModal(){
    return this.modalJobDetails != null ? true : false;
  }

  isChatDisabled() {
    return this.selectedJob.userId == this.sessionService.userId
  }

  copyURL() {
    const href = window.location.href
    const replaceUrl = href.replace('/search-jobs', 'jobs')
    const url = `${replaceUrl}&id=${this.generateJobId(this.selectedJob.jobOpeningId)}`
    navigator.clipboard.writeText(url);
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

  dialogClose(){
    if(!_.isEmpty(this.jobDialogRef))
    this.jobDialogRef.close();
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

  getVisaDetails(visas: any) {
    if(!_.isEmpty(visas))
    return visas.join(',');
    else
      return "";
  }

  getProfilePic(url) {
    if(url)
      return `${picUrl}${url}`
    else
      return defaultProfilePic
  }

  generateJobId(userSpecificNumber) {
    
    const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    const numLetters = 6; 

    let result = '';

    for (let i = 0; i < 3; i++) {
        for (let j = 0; j < 4; j++) {
            result += characters.charAt(Math.floor(Math.random() * characters.length));
        }
        result += '-';
    }

    result += characters.charAt(Math.floor(Math.random() * characters.length))+userSpecificNumber+characters.charAt(Math.floor(Math.random() * characters.length)) + '-';

    for (let i = 0; i < numLetters; i++) {
        result += characters.charAt(Math.floor(Math.random() * characters.length));
    }

    for (let i = 0; i < 4; i++) {
        for (let j = 0; j < 4; j++) {
            result += characters.charAt(Math.floor(Math.random() * characters.length));
        }
        if (i !== 3) result += '-';
    }

    return result;

  }

  @HostListener('window:resize', ['$event'])
    onResize(event:any){

      if(event.target.innerWidth <= 991)
      this.isMobile = true;
    else
      this.isMobile = false;

     if(!this.isMobile){
        this.dialogClose();
      }
  }

  ngOnInit() {

  	if(this.modalJobDetails != null){
      this.selectedJob = this.modalJobDetails;
    }

    if(window.innerWidth <= 991)
      this.isMobile = true;
    else
      this.isMobile = false;

  }

  ngOnChanges() {

    console.log(this.selectedJob)
    
    if(this.selectedJob) {


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
    }

    

    
  }

}
