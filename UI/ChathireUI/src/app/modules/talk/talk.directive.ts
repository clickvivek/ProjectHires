import { Directive, OnInit, Input, ElementRef, Output, EventEmitter, HostListener} from '@angular/core';
import {  Router } from '@angular/router';
import Talk from 'talkjs';
import { environment } from 'src/environments/environment';

import { picUrl, defaultProfilePic, publicProfileUrlPrefix } from 'src/app/data/various';
import _ from 'underscore';

import { AuthService } from 'src/app/core/auth/auth.service';
import { SessionService } from 'src/app/core/session/session.service';
import { SharedService } from '../shared/services/shared.service';
import { TalkService } from './talk.service';

declare var $:JQueryStatic;

@Directive({
  selector: '.chat-btn'
})

export class ChatButtonDirective  {

  @Input() chatUser;

  @Input() isProfile:boolean = false;

  user:any;

  APP_ID = environment.talkJsAppId;
  popup:any;
  session:any;
  conversation:any;

  talkElementLanucher:any

  userId:any;

  constructor(
    private router: Router,
    private authService: AuthService,
    private sessionService: SessionService,
    private talkService: TalkService
    ) {
  }

  generateRandomId() {
    return Math.floor(Math.random() * (99 - 10 + 1)) + 10;
  }

  @HostListener("click", ["$event"])
   onClick(event:any) {
    if(this.popup) {
      this.popup.destroy()
    }

    if(this.isProfile) {
      if(this.authService.isLoggedIn()) {
        this.initChat(this.chatUser)
      }
      else {
        this.router.navigate(['/login']);
      }
    }
    else {
      this.initChat(this.chatUser)
    }

    this.popup = this.session?.createPopup();
    this.popup?.select(this.conversation);
    this.popup?.mount();
    
    setTimeout(() => {
      
      this.talkElementLanucher = document.querySelector('#__talkjs_launcher')

      this.talkElementLanucher?.addEventListener("click", (e) => {
        const parent = this.talkElementLanucher.parentNode
        parent.remove()
      });

    }, 1000)

  }

  

   initChat(chatUser) {

    let me:any = {};

    if(this.sessionService.userId) {

      this.sessionService.userdetailscast.subscribe((res: any) => {
        
        this.user = res

        const userCompanyName = this.user.consultancyUsers[0].consultancy.name
        const userProfileUrl = `${publicProfileUrlPrefix}${this.user.consultancyUsers[0].publicProfileUserName}` 

        me = new Talk.User({
          id: this.user.id,
          name: `${this.user.fname} ${this.user.lname}`,
          photoUrl: this.user.profilePic ? `${picUrl}${this.user.profilePic}` : defaultProfilePic,
          role: 'PremiumRecruiters',
          custom: {
            companyName: userCompanyName,
            profileUrl: userProfileUrl
          },
        })
      })

    }
    else {

      me =  new Talk.User({
        id: this.generateRandomId(),
        name: "user",
        photoUrl: `${defaultProfilePic}`,
        role: "PremiumRecruiters"
      })

    }

    this.userId = chatUser.userId
    
    const other = new Talk.User({
      id: chatUser.userId,
      name: `${chatUser.userFName} ${chatUser.userLName}`,
      photoUrl: `${picUrl}${chatUser.profilePic}`,
      //welcomeMessage: 'Hey there! How are you? :-)',
      role: 'PremiumRecruiters',
      custom: {
        companyName: chatUser.companyName,
        profileUrl: `${publicProfileUrlPrefix}${chatUser.profileUserName}`
      },
    })

    this.session = new Talk.Session({
      appId: this.APP_ID,
      me: me
    });

    this.conversation = this.session.getOrCreateConversation(
      Talk.oneOnOneId(me, other)
    );

    this.conversation.setParticipant(me);
    this.conversation.setParticipant(other);

    this.talkService.fetchTalkUserPresence(this.userId).subscribe((res:any) => {
      console.log(res)
    })
    

   }
  
   ngOnChanges() {
    
    if(this.chatUser) {
      
      

      
    }
    
   }

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