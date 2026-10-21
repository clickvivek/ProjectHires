import { Component, Input, Output, EventEmitter, OnInit, OnChanges } from '@angular/core';
import { picUrl, defaultProfilePic } from 'src/app/data/various';
import { TalkService } from 'src/app/modules/talk/talk.service';
import { SessionService } from 'src/app/core/session/session.service';
import { UserService } from 'src/app/api';

@Component({
  selector: 'inbox-resume-contact',
  templateUrl: './inbox-resume-contact.component.html',
  styleUrls: ['./inbox-resume-contact.component.scss']
})
export class InboxResumeContactComponent implements OnInit, OnChanges {

  @Input() resume: any;
  @Output() hideContact = new EventEmitter();

  isLoaded: boolean = false;
  contact: any = null;

  chatUser: any = null;
  isUserOnline: boolean = false;

  constructor(
    private talkService: TalkService,
    private sessionService: SessionService,
    private userService: UserService
  ) {}

  closeContact(): void {
    this.hideContact.emit(true);
  }

  getProfilePic(url: string): string {
    if (url) {
      if (url.startsWith('http://') || url.startsWith('https://')) return url;
      return `${picUrl}${url}`;
    }
    return defaultProfilePic;
  }

  getUserInitial(fName?: string, lName?: string): string {
    if (fName && fName.trim().length > 0) {
      return fName.trim().charAt(0).toUpperCase();
    }
    if (lName && lName.trim().length > 0) {
      return lName.trim().charAt(0).toUpperCase();
    }
    return 'U';
  }

  isChatDisabled(): boolean {
    return this.contact?.id == this.sessionService.consultancyUserId;
  }

  ngOnInit(): void {
    this.setupContactAndChat();
  }

  ngOnChanges(): void {
    this.setupContactAndChat();
  }

  private setupContactAndChat(): void {
    if (!this.resume) return;

    this.contact = this.resume.consultancyUser;
    this.isLoaded = true;

    if (!this.contact) return;

    const userObj = this.contact.user || {};

    this.chatUser = {
      userId: userObj.id || this.contact.userId || this.resume.candidateProfileId,
      userFName: userObj.fname || '',
      userLName: userObj.lname || '',
      companyName: this.contact.consultancy?.name || '',
      profilePic: userObj.profilePic || '',
      profileUserName: this.contact.publicProfileUserName || ''
    };

    if (this.contact.publicProfileUserName) {
      this.userService.apiUserGetUserByPublicProfileIdGet(this.contact.publicProfileUserName).subscribe({
        next: (res: any) => {
          if (res?.value && res.value.length > 0) {
            const recruiterUser = res.value[0];
            if (recruiterUser?.id) {
              this.chatUser.userId = recruiterUser.id;
              this.updatePresence();
            }
          }
        },
        error: () => {}
      });
    }

    this.updatePresence();
  }

  private updatePresence(): void {
    if (!this.chatUser?.userId) return;
    this.talkService.fetchTalkUserPresence(this.chatUser.userId).subscribe({
      next: (res: any) => {
        const newData = res?.data;
        const userData = newData ? newData[this.chatUser.userId] : null;
        this.isUserOnline = (userData?.status === 'online');
      },
      error: () => {
        this.isUserOnline = false;
      }
    });
  }

}
