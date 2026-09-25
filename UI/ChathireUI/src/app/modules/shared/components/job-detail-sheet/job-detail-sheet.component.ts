import { Component, OnInit, Input, Output, Inject, Optional, HostListener, ViewEncapsulation, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatDialogRef } from "@angular/material/dialog";
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { picUrl, defaultProfilePic } from 'src/app/data/various';

import { ConfirmApplyJobComponent } from '../../../public/searchjobs/confirm-apply-job/confirm-apply-job.component';
import { LoginModalComponent } from '../login-modal/login-modal.component';

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
    if (!date) return '';
    const target = moment(date);
    if (!target.isValid()) return '';
    const current = moment();
    const differenceInDays = current.clone().startOf('day').diff(target.clone().startOf('day'), 'days');
    if (differenceInDays <= 0) {
      return 'Posted today';
    }
    else if (differenceInDays > 30) {
      const differenceInMonths = current.diff(target, 'months');
      return `Posted ${differenceInMonths} months ago`;
    }
    else if (differenceInDays === 1) {
      const differenceInHours = current.diff(target, 'hours');
      if (differenceInHours > 0 && differenceInHours < 24) {
        return `Posted ${differenceInHours} hours ago`;
      }
      return `Posted 1 day ago`;
    }
    else {
      return `Posted ${differenceInDays} days ago`;
    }
  }

  dialogClose(){
    if(!_.isEmpty(this.jobDialogRef))
    this.jobDialogRef.close();
  }

  handleApplyJobConfirmModal(){

    if(this.authService.isLoggedIn()) {
      const applyJobDialogRef = this.dialog.open(ConfirmApplyJobComponent, {
        panelClass: ['material', 'confirm-apply-modal-panel'],
        maxHeight: '90vh',
        disableClose: true,
        data: this.selectedJob
      });
    }
    else {
      const loginDialogRef = this.dialog.open(LoginModalComponent, {
        width: '440px',
        panelClass: 'login-modal-panel',
        data: { actionText: 'apply for this job' }
      });

      loginDialogRef.afterClosed().subscribe(res => {
        if (res && res.success) {
          const applyJobDialogRef = this.dialog.open(ConfirmApplyJobComponent, {
            panelClass: ['material', 'confirm-apply-modal-panel'],
            maxHeight: '90vh',
            disableClose: true,
            data: this.selectedJob
          });
        }
      });
    }
    
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

  getCompanyLogoUrl(logo: string | null | undefined): string {
    if (!logo) return '';
    if (logo.startsWith('http://') || logo.startsWith('https://') || logo.startsWith('data:image')) return logo;
    return `${picUrl}${logo}`;
  }

  getCompanyInitial(name: string | null | undefined): string {
    if (!name) return 'C';
    return name.trim().charAt(0).toUpperCase();
  }

  getUserInitial(fName?: string, lName?: string): string {
    if (fName && fName.trim().length > 0) {
      return fName.trim().charAt(0).toUpperCase();
    }
    if (lName && lName.trim().length > 0) {
      return lName.trim().charAt(0).toUpperCase();
    }
    return 'R';
  }

  getDisplayVisas(visas: any): string[] {
    if (!visas || !Array.isArray(visas) || visas.length === 0) {
      return ['Any Visa'];
    }
    const cleanVisas = visas.filter(v => typeof v === 'string' && v.trim().length > 0);
    if (cleanVisas.length === 0) {
      return ['Any Visa'];
    }
    const hasAny = cleanVisas.some(v => {
      const lower = v.trim().toLowerCase();
      return lower === 'any' || lower === 'any visa' || lower === 'all' || lower === 'all visas' || lower === 'any/all';
    });
    if (hasAny || cleanVisas.length >= 6) {
      return ['Any Visa'];
    }
    return cleanVisas;
  }

}
