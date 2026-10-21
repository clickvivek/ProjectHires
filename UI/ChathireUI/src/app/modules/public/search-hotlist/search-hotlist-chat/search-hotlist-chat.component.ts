import { Component, Input } from '@angular/core';
import { picUrl, defaultProfilePic } from 'src/app/data/various';
import { TalkService } from 'src/app/modules/talk/talk.service';
import { SessionService } from 'src/app/core/session/session.service';
import { UserService } from 'src/app/api';

@Component({
  selector: 'search-hotlist-chat',
  templateUrl: './search-hotlist-chat.component.html',
  styleUrls: ['./search-hotlist-chat.component.scss']
})
export class SearchHotlistChatComponent {

  @Input() item;

  chatUser:any;

  isUserOnline: boolean = false;

  constructor(
    private talkService: TalkService,
    private sessionService: SessionService,
    private userService: UserService
  ) {

  }

  getProfilePic(url) {
    if (url) {
      if (url.startsWith('http://') || url.startsWith('https://')) return url;
      return `${picUrl}${url}`;
    }
    return defaultProfilePic;
  }

  isChatDisabled() {
    return this.item.consultancyUserId == this.sessionService.consultancyUserId
  }


  ngOnChanges() {

      this.chatUser = {}

      this.chatUser.userId = this.item.candidateProfileId
      this.chatUser.userFName = this.item.consultancyUserFName
      this.chatUser.userLName = this.item.consultancyUserLName
      this.chatUser.companyName = this.item.consultancyName
      this.chatUser.profilePic = this.item.profilePic
      this.chatUser.profileUserName = this.item.publicProfileUserName

      // Resolve the actual Recruiter/BenchSales User ID from their public profile if available
      if (this.item.publicProfileUserName) {
        this.userService.apiUserGetUserByPublicProfileIdGet(this.item.publicProfileUserName).subscribe({
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

  private updatePresence() {
    if (!this.chatUser?.userId) return;
    this.talkService.fetchTalkUserPresence(this.chatUser.userId).subscribe({
      next: (res: any) => {
        let newData = res?.data;
        let userData = newData ? newData[this.chatUser.userId] : null;
        this.isUserOnline = (userData?.status === 'online');
      },
      error: () => {
        this.isUserOnline = false;
      }
    });
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

}
