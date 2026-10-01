import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { SessionService } from 'src/app/core/session/session.service';
import { SharedService } from '../../shared/services/shared.service';
import { UserService } from 'src/app/api';
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

  @ViewChild('inboxContainer') inboxContainer: ElementRef;

  constructor(
    private userService: UserService,
    private sharedService: SharedService,
    private sessionService: SessionService,
    private route: ActivatedRoute
  ) {}

  handleUnread(event: any) {
    if (!_.isEmpty(event)) {
      this.uneadCount = event[0]?.unreadMessageCount || 0;
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
      if (!_.isEmpty(res)) {
        this.uneadCount = res[0]?.unreadMessageCount || 0;
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
