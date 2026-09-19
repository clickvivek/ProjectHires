import { Directive, OnInit, Input, ElementRef, Output, EventEmitter, HostListener} from '@angular/core';
import {  Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import Talk from 'talkjs';
import { environment } from 'src/environments/environment';

import { picUrl, defaultProfilePic, publicProfileUrlPrefix } from 'src/app/data/various';
import _ from 'underscore';

import { AuthService } from 'src/app/core/auth/auth.service';
import { SessionService } from 'src/app/core/session/session.service';
import { SharedService } from '../shared/services/shared.service';
import { TalkService } from './talk.service';
import { LoginModalComponent } from '../shared/components/login-modal/login-modal.component';

declare var $:JQueryStatic;

@Directive({
  selector: '.chat-btn'
})

export class ChatButtonDirective implements OnInit {

  @Input() chatUser: any;

  @Input() isProfile: boolean = false;

  user: any;

  APP_ID = environment.talkJsAppId;
  popup: any;
  session: any;
  conversation: any;

  talkElementLanucher: any;

  userId: any;

  constructor(
    private router: Router,
    private authService: AuthService,
    private sessionService: SessionService,
    private talkService: TalkService,
    public dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.sessionService.userdetailscast.subscribe((res: any) => {
      if (res) {
        this.user = res;
      }
    });
  }

  generateRandomId(): number {
    return Math.floor(Math.random() * (99 - 10 + 1)) + 10;
  }

  private buildMeUser(): any {
    const cachedDetails = this.sessionService.getUserDetails() || this.user;
    const uid = cachedDetails?.id || this.sessionService.userId || this.generateRandomId();
    const fname = cachedDetails?.fname || '';
    const lname = cachedDetails?.lname || '';
    const email = cachedDetails?.email || this.sessionService.userEmail || '';
    const name = (fname || lname) ? `${fname} ${lname}`.trim() : (email ? email.split('@')[0] : 'User');
    const photoUrl = cachedDetails?.profilePic ? `${picUrl}${cachedDetails.profilePic}` : defaultProfilePic;

    let userCompanyName = '';
    let userProfileUrl = '';
    if (cachedDetails?.consultancyUsers && cachedDetails.consultancyUsers.length > 0) {
      userCompanyName = cachedDetails.consultancyUsers[0]?.consultancy?.name || '';
      if (cachedDetails.consultancyUsers[0]?.publicProfileUserName) {
        userProfileUrl = `${publicProfileUrlPrefix}${cachedDetails.consultancyUsers[0].publicProfileUserName}`;
      }
    }

    return new Talk.User({
      id: String(uid),
      name: name,
      photoUrl: photoUrl,
      role: 'PremiumRecruiters',
      custom: {
        companyName: userCompanyName,
        profileUrl: userProfileUrl
      },
    });
  }

  private buildOtherUser(chatUser: any): any {
    const uid = chatUser?.userId || chatUser?.id || this.generateRandomId();
    const fname = chatUser?.userFName || chatUser?.fname || '';
    const lname = chatUser?.userLName || chatUser?.lname || '';
    const name = (fname || lname) ? `${fname} ${lname}`.trim() : (chatUser?.userName || 'Recruiter');
    const photo = chatUser?.profilePic;
    const photoUrl = photo ? (photo.startsWith('http') ? photo : `${picUrl}${photo}`) : defaultProfilePic;
    const companyName = chatUser?.companyName || '';
    const profileUserName = chatUser?.profileUserName || '';
    const profileUrl = profileUserName ? `${publicProfileUrlPrefix}${profileUserName}` : '';

    return new Talk.User({
      id: String(uid),
      name: name,
      photoUrl: photoUrl,
      role: 'PremiumRecruiters',
      custom: {
        companyName: companyName,
        profileUrl: profileUrl
      },
    });
  }

  private openChatPopup(): void {
    if (!this.chatUser) return;

    this.initChat(this.chatUser);

    if (this.session && this.conversation) {
      this.popup = this.session.createPopup();
      this.popup.select(this.conversation);
      this.popup.mount();

      setTimeout(() => {
        this.talkElementLanucher = document.querySelector('#__talkjs_launcher');
        this.talkElementLanucher?.addEventListener("click", () => {
          const parent = this.talkElementLanucher?.parentNode;
          parent?.remove();
        });
      }, 1000);
    }
  }

  @HostListener("click", ["$event"])
  onClick(event: any): void {
    if (this.popup) {
      this.popup.destroy();
    }

    if (!this.authService.isLoggedIn()) {
      const dialogRef = this.dialog.open(LoginModalComponent, {
        width: '440px',
        panelClass: 'login-modal-panel',
        data: { actionText: 'chat with this recruiter' }
      });

      dialogRef.afterClosed().subscribe(res => {
        if (res && res.success) {
          // Open chat immediately on the current page without changing the URL
          setTimeout(() => {
            this.openChatPopup();
          }, 300);
        }
      });
      return;
    }

    this.openChatPopup();
  }

  initChat(chatUser: any): void {
    const me = this.buildMeUser();
    const other = this.buildOtherUser(chatUser);
    this.userId = chatUser?.userId || chatUser?.id;

    this.session = new Talk.Session({
      appId: this.APP_ID,
      me: me
    });

    this.conversation = this.session.getOrCreateConversation(
      Talk.oneOnOneId(me, other)
    );

    this.conversation.setParticipant(me);
    this.conversation.setParticipant(other);

    if (this.userId) {
      this.talkService.fetchTalkUserPresence(this.userId).subscribe({
        next: () => {},
        error: () => {}
      });
    }
  }

  ngOnChanges(): void {}
}

@Directive({
  selector: '#inboxContainer'
})

export class InboxDirective  {

  @Input() user;
  @Input() chatUser;

  @Output() onUnreadEvent = new EventEmitter()

  APP_ID = environment.talkJsAppId;
  session:any;
  conversation:any;

  constructor(
    private element: ElementRef,
    private sharedService: SharedService
  ) {
    
  }

  generateRandomId() {
    return Math.floor(Math.random() * (99 - 10 + 1)) + 10;
  }

  initInbox(user, chatUser) {

    const userCompanyName = user.consultancyUsers[0].consultancy.name
    const userProfileUrl = `${publicProfileUrlPrefix}${user.consultancyUsers[0].publicProfileUserName}` 

    const chatUserCompanyName = chatUser.consultancyUsers[0].consultancy.name
    const chatUserProfileUrl = `${publicProfileUrlPrefix}${chatUser.consultancyUsers[0].publicProfileUserName}` 
    

    const other =  new Talk.User({
      id: chatUser.id,
      name: `${chatUser.fname} ${chatUser.lname}`,
      photoUrl: `${picUrl}${chatUser.profilePic}`,
      //welcomeMessage: 'Hey there! How are you? :-)',
      role: 'PremiumRecruiters',
      custom: {
        companyName: chatUserCompanyName,
        profileUrl: chatUserProfileUrl
      },
    })

    const me = new Talk.User({
      id: user.id,
      name: `${user.fname} ${user.lname}`,
      photoUrl: user.profilePic ? `${picUrl}${user.profilePic}` : defaultProfilePic,
      role: 'PremiumRecruiters',
      custom: {
        companyName: userCompanyName,
        profileUrl: userProfileUrl
      },
    })

    this.session = new Talk.Session({
      appId: this.APP_ID,
      me: me
    });

   

    this.conversation = this.session.getOrCreateConversation(
      Talk.oneOnOneId(me, other)
    );

    this.conversation.setParticipant(other);
    this.conversation.setParticipant(me);
    
    const inbox = this.session.createInbox()
    inbox.select(this.conversation);

    const msgArea = this.element.nativeElement.querySelector('.message-area')
    inbox.mount(msgArea);

    this.session.unreads.on("change", (unreadConversations) => {
      this.sharedService.setInboxUnReadCount(unreadConversations)
    })

  }

  ngOnChanges() {
      if(this.user && this.chatUser)
      this.initInbox(this.user, this.chatUser)

  }

}