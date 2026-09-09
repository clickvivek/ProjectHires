
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { JobOpeningService } from 'src/app/api';
import { SessionService } from 'src/app/core/session/session.service';
import { ToastrService } from 'ngx-toastr';
import * as moment from 'moment';

@Component({
  selector: 'inbox-resume-comments',
  templateUrl: './inbox-resume-comments.component.html',
  styleUrls: ['./inbox-resume-comments.component.scss']
})
export class InboxResumeCommentsComponent {

  @Input() resume;
  @Output() hideComment = new EventEmitter();

  isLoaded:boolean = false;
  commentList:any
  comment:string = ""

  constructor(
    private jobOpeningService: JobOpeningService,
    private sessionService: SessionService,
    private toastr: ToastrService
  ) { }

  hideComments() {
    this.hideComment.emit(true)
  }

  deleteComment(item) {
    
    this.jobOpeningService.apiJobOpeningDeleteJobProfileCommentDelete(item.id).subscribe({
      next:(res:any) => {
        this.commentList = this.commentList.filter(comment => {
          return comment.id != item.id
        })
        this.toastr.success('Comment deleted successfully', '' , {
          timeOut: 3000,
          positionClass: 'toast-top-center'
        });
      },
      error:(error:any) => {
        this.toastr.error('Some error occured', '' , {
          timeOut: 3000,
          positionClass: 'toast-top-center'
        });
      }
    })

  }

  addComment() {

    let data = {
      jobopeningCandidateProfileMapId: this.resume.id,
      consultancyUserId: this.sessionService.consultancyUserId,
      comment: this.comment,
      active: true
    }

    this.jobOpeningService.apiJobOpeningAddJobProfileCommentPost(data).subscribe({
      next:(res:any) => {
        this.fetchComments()
        this.comment = "";
        this.toastr.success('Comment added successfully', '' , {
          timeOut: 3000,
          positionClass: 'toast-top-center'
        });
      },
      error:(error:any) => {
        this.toastr.error('Some error occured', '' , {
          timeOut: 3000,
          positionClass: 'toast-top-center'
        });
      }
    })

  }

  fetchComments() {
    this.jobOpeningService.apiJobOpeningGetJobOpeningProfileConsultancyCommentGet(this.resume.id).subscribe({
      next:(res:any) => {
        this.isLoaded = true;
        this.commentList = res.value
        this.commentList.sort((a, b) => new Date(b.updated).getTime() - new Date(a.updated).getTime());
      },
      error:(error:any) => {

      }
    })
  }

  adjustTimestampToSystemTimezone(timestamp) {
    const postDate = new Date(timestamp);
    const timezoneOffset = postDate.getTimezoneOffset();
    postDate.setMinutes(postDate.getMinutes() - timezoneOffset);
  
    return postDate;
  }

  getPostedDays(timestamp) {
    
    let postDate:any = this.adjustTimestampToSystemTimezone(timestamp);;
    let currentDate:any = new Date();

    const secondsAgo = Math.floor((currentDate - postDate) / 1000);
    const minutesAgo = Math.floor(secondsAgo / 60);
    const hoursAgo = Math.floor(minutesAgo / 60);
    const daysAgo = Math.floor(hoursAgo / 24);
    const monthsAgo = Math.floor(daysAgo / 30);

    let timeAgo;

    if (secondsAgo < 60) {
      timeAgo = `${secondsAgo} ${secondsAgo === 1 ? 'second' : 'seconds'}`;
    } else if (minutesAgo < 60) {
      timeAgo = `${minutesAgo} ${minutesAgo === 1 ? 'minute' : 'minutes'}`;
    } else if (hoursAgo < 24) {
      timeAgo = `${hoursAgo} ${hoursAgo === 1 ? 'hour' : 'hours'}`;
    } else if (daysAgo < 30) {
      timeAgo = `${daysAgo} ${daysAgo === 1 ? 'day' : 'days'}`;
    } else {
      timeAgo = `${monthsAgo} ${monthsAgo === 1 ? 'month' : 'months'}`;
    }

    return timeAgo

  }

  ngOnInit() {
    this.fetchComments()
  }

}
