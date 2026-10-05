import { Directive, OnInit, Input, ElementRef, Output, EventEmitter, HostListener, OnChanges, OnDestroy } from '@angular/core';
import {  Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import Talk from 'talkjs';
import { environment } from 'src/environments/environment';

import { picUrl, defaultProfilePic, publicProfileUrlPrefix, getFullPublicProfileUrl } from 'src/app/data/various';
import _ from 'underscore';

import { AuthService } from 'src/app/core/auth/auth.service';
import { SessionService } from 'src/app/core/session/session.service';
import { SharedService } from '../shared/services/shared.service';
import { TalkService } from './talk.service';
import { LoginModalComponent } from '../shared/components/login-modal/login-modal.component';
import { ChatLimitModalComponent } from '../shared/components/chat-limit-modal/chat-limit-modal.component';

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
        userProfileUrl = getFullPublicProfileUrl(cachedDetails.consultancyUsers[0].publicProfileUserName);
      }
    } else if (cachedDetails?.directCandidateDetail?.publicProfileSlug) {
      userProfileUrl = getFullPublicProfileUrl(cachedDetails.directCandidateDetail.publicProfileSlug);
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
    let profileUserName = chatUser?.profileUserName || '';
    if (!profileUserName && chatUser?.consultancyUsers && chatUser.consultancyUsers.length > 0) {
      profileUserName = chatUser.consultancyUsers[0]?.publicProfileUserName || '';
    }
    const profileUrl = getFullPublicProfileUrl(profileUserName);

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

  private handleChatClick(): void {
    if (!this.chatUser) return;
    const targetUserId = this.chatUser?.userId || this.chatUser?.id;

    if (!targetUserId) {
      this.openChatPopup();
      return;
    }

    this.talkService.initiateChat(targetUserId).subscribe({
      next: (res: any) => {
        const result = res?.value;
        if (result && result.canChat === false) {
          this.dialog.open(ChatLimitModalComponent, {
            width: '460px',
            panelClass: 'chat-limit-modal-panel',
            data: {
              dailyLimit: result.dailyChatLimit,
              usedChatsToday: result.usedChatsToday,
              remainingChatsToday: result.remainingChatsToday,
              nextSlotAvailableAtUtc: result.nextSlotAvailableAtUtc,
              nextSlotWaitSeconds: result.nextSlotWaitSeconds,
              nextSlotWaitText: result.nextSlotWaitText,
              message: result.message
            }
          });
        } else {
          this.openChatPopup();
        }
      },
      error: (err: any) => {
        console.error('Chat limit check error:', err);
        this.openChatPopup();
      }
    });
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
            this.handleChatClick();
          }, 300);
        }
      });
      return;
    }

    this.handleChatClick();
  }

  async initChat(chatUser: any): Promise<void> {
    const me = this.buildMeUser();
    const other = this.buildOtherUser(chatUser);
    this.userId = chatUser?.userId || chatUser?.id;

    this.session = await this.talkService.getOrCreateSession(this.user);
    if (!this.session) {
      const auth = await this.talkService.getAuthToken(me.id);
      const sessionOptions: any = {
        appId: this.APP_ID,
        me: me
      };
      if (auth?.signature) sessionOptions.signature = auth.signature;
      if (auth?.token) {
        sessionOptions.token = auth.token;
        sessionOptions.tokenFetcher = async () => {
          const fresh = await this.talkService.getAuthToken(me.id, true);
          return fresh?.token || auth.token;
        };
      }
      this.session = new Talk.Session(sessionOptions);
    }

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

export class InboxDirective implements OnInit, OnChanges, OnDestroy {

  @Input() user: any;
  @Input() chatUser: any;

  @Output() onUnreadEvent = new EventEmitter();

  APP_ID = environment.talkJsAppId;
  session: any;
  conversation: any;
  inbox: any;
  private isMounted = false;

  constructor(
    private element: ElementRef,
    private sharedService: SharedService,
    private sessionService: SessionService,
    private talkService: TalkService,
    public dialog: MatDialog
  ) {}

  generateRandomId(): number {
    return Math.floor(Math.random() * (99 - 10 + 1)) + 10;
  }

  private buildMeUser(): any {
    const cachedDetails = this.user || this.sessionService.getUserDetails();
    const uid = cachedDetails?.id || this.sessionService.userId || this.generateRandomId();
    const fname = cachedDetails?.fname || '';
    const lname = cachedDetails?.lname || '';
    const email = cachedDetails?.email || this.sessionService.userEmail || '';
    const name = (fname || lname) ? `${fname} ${lname}`.trim() : (email ? email.split('@')[0] : 'User');
    
    let photoUrl = defaultProfilePic;
    if (cachedDetails?.profilePic) {
      photoUrl = (cachedDetails.profilePic.startsWith('http://') || cachedDetails.profilePic.startsWith('https://'))
        ? cachedDetails.profilePic
        : `${picUrl}${cachedDetails.profilePic}`;
    }

    let userCompanyName = '';
    let userProfileUrl = '';
    if (cachedDetails?.consultancyUsers && cachedDetails.consultancyUsers.length > 0) {
      userCompanyName = cachedDetails.consultancyUsers[0]?.consultancy?.name || '';
      if (cachedDetails.consultancyUsers[0]?.publicProfileUserName) {
        userProfileUrl = getFullPublicProfileUrl(cachedDetails.consultancyUsers[0].publicProfileUserName);
      }
    } else if (cachedDetails?.directCandidateDetail?.publicProfileSlug) {
      userProfileUrl = getFullPublicProfileUrl(cachedDetails.directCandidateDetail.publicProfileSlug);
    }

    return new Talk.User({
      id: String(uid),
      name: name,
      photoUrl: photoUrl,
      role: 'PremiumRecruiters',
      custom: {
        companyName: userCompanyName,
        profileUrl: userProfileUrl
      }
    });
  }

  private buildOtherUser(chatUser: any): any {
    if (!chatUser) return null;
    const uid = chatUser?.userId || chatUser?.id;
    if (!uid) return null;
    const fname = chatUser?.userFName || chatUser?.fname || '';
    const lname = chatUser?.userLName || chatUser?.lname || '';
    const name = (fname || lname) ? `${fname} ${lname}`.trim() : (chatUser?.userName || 'Recruiter');
    const photo = chatUser?.profilePic;
    const photoUrl = photo ? (photo.startsWith('http') ? photo : `${picUrl}${photo}`) : defaultProfilePic;
    
    let companyName = chatUser?.companyName || '';
    let profileUrl = '';
    if (chatUser?.consultancyUsers && chatUser.consultancyUsers.length > 0) {
      companyName = companyName || chatUser.consultancyUsers[0]?.consultancy?.name || '';
      if (chatUser.consultancyUsers[0]?.publicProfileUserName) {
        profileUrl = getFullPublicProfileUrl(chatUser.consultancyUsers[0].publicProfileUserName);
      }
    } else if (chatUser?.profileUserName) {
      profileUrl = getFullPublicProfileUrl(chatUser.profileUserName);
    }

    return new Talk.User({
      id: String(uid),
      name: name,
      photoUrl: photoUrl,
      role: 'PremiumRecruiters',
      custom: {
        companyName: companyName,
        profileUrl: profileUrl
      }
    });
  }

  async initInbox(): Promise<void> {
    if (this.isMounted) return;

    try {
      await Talk.ready;
      const me = this.buildMeUser();

      this.session = await this.talkService.getOrCreateSession(this.user);
      if (!this.session) {
        const auth = await this.talkService.getAuthToken(me.id);
        const sessionOptions: any = {
          appId: this.APP_ID,
          me: me
        };
        if (auth?.signature) sessionOptions.signature = auth.signature;
        if (auth?.token) {
          sessionOptions.token = auth.token;
          sessionOptions.tokenFetcher = async () => {
            const fresh = await this.talkService.getAuthToken(me.id, true);
            return fresh?.token || auth.token;
          };
        }
        this.session = new Talk.Session(sessionOptions);
      }

      this.inbox = this.session.createInbox({
        showFeedHeader: false,
        showMobileBackButton: true,
        messageField: {
          placeholder: 'Type a message...',
          enterSendsMessage: true
        }
      });

      if (this.chatUser) {
        const targetUserId = this.chatUser?.userId || this.chatUser?.id;
        if (targetUserId) {
          this.talkService.initiateChat(targetUserId).subscribe({
            next: (res: any) => {
              const result = res?.value;
              if (result && result.canChat === false) {
                this.dialog.open(ChatLimitModalComponent, {
                  width: '460px',
                  panelClass: 'chat-limit-modal-panel',
                  data: {
                    dailyLimit: result.dailyChatLimit,
                    usedChatsToday: result.usedChatsToday,
                    remainingChatsToday: result.remainingChatsToday,
                    nextSlotAvailableAtUtc: result.nextSlotAvailableAtUtc,
                    nextSlotWaitSeconds: result.nextSlotWaitSeconds,
                    nextSlotWaitText: result.nextSlotWaitText,
                    message: result.message
                  }
                });
              }
            },
            error: () => {}
          });
        }

        const other = this.buildOtherUser(this.chatUser);
        if (other) {
          this.conversation = this.session.getOrCreateConversation(
            Talk.oneOnOneId(me, other)
          );
          this.conversation.setParticipant(me);
          this.conversation.setParticipant(other);
          this.inbox.select(this.conversation);
        }
      }

      const msgArea = this.element.nativeElement.querySelector('.message-area');
      if (msgArea) {
        msgArea.innerHTML = '';
        await this.inbox.mount(msgArea);
        this.isMounted = true;
      }

      this.session.unreads.on('change', (unreadConversations: any) => {
        this.sharedService.setInboxUnReadCount(unreadConversations);
      });
    } catch (err) {
      console.error('TalkJS Inbox initialization error:', err);
    }
  }

  ngOnInit(): void {
    this.sessionService.userdetailscast.subscribe((res: any) => {
      if (res) {
        this.user = res;
        this.initInbox();
      }
    });
    if (this.sessionService.getUserDetails() || this.sessionService.userId) {
      this.initInbox();
    }
  }

  ngOnChanges(): void {
    if (this.user || this.sessionService.getUserDetails() || this.sessionService.userId) {
      this.initInbox();
    }
  }

  ngOnDestroy(): void {
    if (this.inbox) {
      this.inbox.destroy();
    }
    // Note: Do not destroy this.session here so global unread tracking continues across all pages.
    // The session is cleaned up globally on user logout via TalkService.destroySession().
  }
}