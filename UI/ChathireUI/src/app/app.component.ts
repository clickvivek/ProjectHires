
import { DOCUMENT } from '@angular/common';

import { Component, Inject, ViewChild, ElementRef, Renderer2, AfterViewInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';

import _ from 'underscore';

import { SessionService } from 'src/app/core/session/session.service';
import { AuthService } from './core/auth/auth.service';
import { SharedService } from './modules/shared/services/shared.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent   {

  user:any;
  isNotHomeRoute:boolean = false;

  element:any;

  @ViewChild('talkjsContainer') talkjsContainer!: ElementRef;

  constructor(
    public router: Router,
    @Inject(DOCUMENT) public document: Document,
    r: Renderer2,
    private sessionService: SessionService,
    private authService: AuthService,
    private sharedService: SharedService
    ) {

    const checkIsHome = (url: string) => {
      if (!url) return true;
      const clean = url.split('?')[0].replace(/^#+/, '').replace(/^\/+/, '');
      const rootPath = clean.split('#')[0].split('/')[0];
      return rootPath === '' || rootPath === 'home';
    };

    const initialHref = window.location.hash || window.location.pathname || '';
    this.isNotHomeRoute = !checkIsHome(initialHref);

    router.events.subscribe((event: any) => {
      
      if (event instanceof NavigationEnd) {

        let path = this.router.url;
        var name: any = path.split('/');        
        
        document.body.className = "";

        if (name[1] && name[1].includes("?")) {
          let finalname = name[1].split("?");
          r.addClass(document.body, finalname[0]);
        }
        else if (name[1]) {
            r.addClass(document.body, name[1]);
        }
          
        this.isNotHomeRoute = !checkIsHome(this.router.url);

        if(this.router.url != '/login' && !this.router.url.includes('/search-jobs')) {
          this.sharedService.setPageToRetain(null)
        }

        const resetScroll = () => {
          window.scrollTo({ top: 0, left: 0, behavior: 'auto' });
          if (document.documentElement) document.documentElement.scrollTop = 0;
          if (document.body) document.body.scrollTop = 0;
          const mainEl = document.querySelector('.main-container');
          if (mainEl) (mainEl as HTMLElement).scrollTop = 0;
        };

        resetScroll();
        setTimeout(resetScroll, 20);
        setTimeout(resetScroll, 100);
       }

    });

  }

  showCookieConsent: boolean = false;
  unreadCount: number = 0;
  showNewMessageAlert: boolean = false;
  latestSenderName: string = '';
  private isInitialLoad: boolean = true;
  private alertTimeout: any = null;

  ngOnInit() {
    const consent = localStorage.getItem('chathire_cookie_consent');
    if (!consent) {
      this.showCookieConsent = true;
    }

    this.sessionService.userdetailscast.subscribe((res: any) => {
      this.user = res;
    });

    this.sharedService.inboxunreadcountcast.subscribe((res: any) => {
      let newCount = 0;
      let senderName = '';

      if (Array.isArray(res)) {
        newCount = res.reduce((sum: number, c: any) => sum + (c?.unreadMessageCount || 0), 0);

        // Find the conversation with the latest unread message or recent activity
        if (res.length > 0) {
          // Look for conversation with lastMessage or other participant
          const latestConv = res[0];
          const convObj = latestConv?.conversation || latestConv;
          const lastMsg = convObj?.lastMessage || latestConv?.lastMessage;

          if (lastMsg?.sender?.name) {
            senderName = lastMsg.sender.name;
          } else if (convObj?.otherParticipant?.name) {
            senderName = convObj.otherParticipant.name;
          } else if (convObj?.participants) {
            // Find participant who is not the current user
            const myId = String(this.sessionService.userId || this.user?.id || '');
            const other = Object.values(convObj.participants).find((p: any) => String(p?.id) !== myId) as any;
            if (other?.name) {
              senderName = other.name;
            }
          }
        }
      } else if (typeof res === 'number') {
        newCount = res;
      }

      // If count increases after initial load and not currently on /chat, trigger notification animation
      if (!this.isInitialLoad && newCount > this.unreadCount && !this.isChatRoute()) {
        this.triggerNewMessageAlert(senderName);
      }

      this.unreadCount = newCount;
      this.isInitialLoad = false;
    });
  }

  triggerNewMessageAlert(senderName?: string) {
    this.latestSenderName = senderName ? senderName.trim() : '';
    this.showNewMessageAlert = true;
    if (this.alertTimeout) {
      clearTimeout(this.alertTimeout);
    }
    this.alertTimeout = setTimeout(() => {
      this.showNewMessageAlert = false;
    }, 15000);
  }

  isLoggedIn(): boolean {
    return this.authService.isLoggedIn();
  }

  isChatRoute(): boolean {
    return !!(this.router.url && this.router.url.includes('/chat'));
  }

  acceptCookieConsent() {
    localStorage.setItem('chathire_cookie_consent', 'accepted');
    this.showCookieConsent = false;
  }
}

