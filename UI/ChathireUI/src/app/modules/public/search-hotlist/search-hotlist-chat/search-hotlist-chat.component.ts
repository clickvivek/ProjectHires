import { Component, Input } from '@angular/core';
import { picUrl, defaultProfilePic } from 'src/app/data/various';
import { TalkService } from 'src/app/modules/talk/talk.service';
import { SessionService } from 'src/app/core/session/session.service';

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
    private sessionService: SessionService
  ) {

  }

  getProfilePic(url) {
    if(url)
      return `${picUrl}${url}`
    else
      return defaultProfilePic
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

      //console.log(this.item)

      this.talkService.fetchTalkUserPresence(this.chatUser.userId).subscribe((res:any) => {
        let newData = res.data
        //console.log(newData)
        let userData = newData[this.chatUser.userId]
        if(userData?.status === 'online') {
          this.isUserOnline = true
        }
        else {
          this.isUserOnline = false
        }
      })

  }

}
