import { Component, OnInit, HostListener } from '@angular/core';
import { Router, Event, NavigationEnd } from '@angular/router';
import { picUrl, defaultProfilePic } from 'src/app/data/various';
import { AuthService } from 'src/app/core/auth/auth.service';
import { SessionService } from 'src/app/core/session/session.service';
import { SharedService } from '../../shared/services/shared.service';

import _ from 'underscore';


@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit {

  isMenuExpanded:boolean = false;
  isJobMiniFormSubmitted: boolean = false;
  isMobile: boolean = false;
  
  isScrolled: boolean = false;

  user:any = {};

  skillName:string = ""
  locationName: string = ""
  
  profilePicUrl:any = ""
  profileId:any;

  uneadCount = 0;

  constructor(
    public _router: Router,
    private authService: AuthService,
    private sessionService: SessionService,
    private sharedService: SharedService
    ) {

      _router.events.subscribe( (event: Event) => {

      if (event instanceof NavigationEnd) {
           this.isMenuExpanded = false;
       }
       
    });

  }

  @HostListener('window:resize', ['$event'])
  onResize(event: any) {
      this.mobileScreen()
  }
  
  showMenuMini() {
  	this.isMenuExpanded = !this.isMenuExpanded;
  }

  isLoggedIn() {
    return this.authService.isLoggedIn()
  }

  isAdmin() {
    return Number(this.sessionService.userTypeId) === 7;
  }

  isCandidate() {
    return Number(this.sessionService.userTypeId) === 5 || (this.user && Number(this.user.userTypeId) === 5);
  }

  isConsultancyId() {
    return this.sessionService.consultancyId
  }

  isNotProfile() {
    return !this._router.url.includes('profile');
  }

  isCopied: boolean = false;

  getProfileUrl(): string {
    const url = this._router.url;
    const match = url.match(/\/profile\/([^\/\?#]+)/);
    if (match && match[1]) {
      return `www.chathire.com/profile/${match[1]}`;
    }
    if (this.profileId) {
      return `www.chathire.com/profile/${this.profileId}`;
    }
    return 'www.chathire.com/profile';
  }

  copyURL(text: string) {
    if (!text) return;
    navigator.clipboard.writeText(text);
    this.isCopied = true;
    setTimeout(() => {
      this.isCopied = false;
    }, 2000);
  }

  logout() {
    this.authService.logout().subscribe({
      next: (res:any) => {
        this._router.navigate(['/login']);
      },
      error: (error) => {
        console.log(error);
      }
    })
  }

  mobileScreen() {
    if(window.innerWidth <= 991)
      this.isMobile = true;
    else
      this.isMobile = false;
  }

  getUserName(email) {
    let newData = email.split('@')
    return newData[0]
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

  onImageError(event: any) {
    event.target.style.display = 'none';
  }

  ngOnInit() {

    this.mobileScreen();

    this.sessionService.userdetailscast.subscribe((res: any) => {
      this.user = res;
      if (this.user && this.user?.profilePic) {
        if (this.user.profilePic.startsWith('http://') || this.user.profilePic.startsWith('https://')) {
          this.profilePicUrl = this.user.profilePic;
        } else {
          this.profilePicUrl = `${picUrl}${this.user?.profilePic}`;
        }
      } else {
        this.profilePicUrl = defaultProfilePic;
      }
      this.profileId = this.user?.consultancyUsers && this.user?.consultancyUsers[0] 
        ? this.user?.consultancyUsers[0].publicProfileUserName 
        : (this.user?.directCandidateDetail?.publicProfileSlug || '');
    });

    this.sharedService.inboxunreadcountcast.subscribe((res: any) => {
      if (!_.isEmpty(res)) {
        this.uneadCount = res[0]?.unreadMessageCount;
      } else {
        this.uneadCount = 0;
      }
    });

    if (this.isLoggedIn()) {
      this.sessionService.refreshUser();
    }

  }

}
