import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { SessionService } from 'src/app/core/session/session.service';
import { SharedService } from '../../shared/services/shared.service';
import { UserService } from 'src/app/api';
import { environment } from 'src/environments/environment';
import _ from 'underscore';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss']
})
export class MessagesComponent implements OnInit {

  user: any;
  chatUser: any;
  uneadCount = 0;
  isProduction: boolean = environment.production;
  showTestBanner: boolean = true;
  isReferralPanelOpen: boolean = typeof window !== 'undefined' ? window.innerWidth >= 1200 : false;

  @ViewChild('inboxContainer') inboxContainer: ElementRef;

  constructor(
    private userService: UserService,
    private sharedService: SharedService,
    private sessionService: SessionService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  toggleReferralPanel(): void {
    this.isReferralPanelOpen = !this.isReferralPanelOpen;
  }

  dismissTestBanner(): void {
    this.showTestBanner = false;
  }

  onNewMessageClick(): void {
    this.router.navigate(['/search-hotlist']);
  }

  handleUnread(event: any) {
    if (Array.isArray(event)) {
      this.uneadCount = event.length;
    } else if (typeof event === 'number') {
      this.uneadCount = event;
    } else {
      this.uneadCount = 0;
    }
  }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      const targetEmail = params['email'];
      const targetProfileId = params['profileId'];
      if (targetEmail) {
        this.userService.apiUserGetUserByUserNameGet(targetEmail).subscribe({
          next: (res: any) => {
            if (res.value && res.value.length > 0) {
              this.chatUser = res.value[0];
            }
          },
          error: () => {}
        });
      } else if (targetProfileId) {
        this.userService.apiUserGetUserByPublicProfileIdGet(targetProfileId).subscribe({
          next: (res: any) => {
            if (res.value && res.value.length > 0) {
              this.chatUser = res.value[0];
            }
          },
          error: () => {}
        });
      }
    });

    this.sharedService.inboxunreadcountcast.subscribe((res: any) => {
      if (Array.isArray(res)) {
        this.uneadCount = res.length;
      } else if (typeof res === 'number') {
        this.uneadCount = res;
      } else {
        this.uneadCount = 0;
      }
    });

    this.sessionService.userdetailscast.subscribe((res: any) => {
      if (res) {
        this.user = res;
      }
    });

    const currentDetails = this.sessionService.getUserDetails();
    if (currentDetails) {
      this.user = currentDetails;
    } else if (this.sessionService.userEmail) {
      this.sessionService.refreshUser();
    }
  }
}
