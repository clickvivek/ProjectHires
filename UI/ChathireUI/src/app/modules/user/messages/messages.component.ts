import { Component, ViewChild, ElementRef } from '@angular/core';
import { SessionService } from 'src/app/core/session/session.service';
import { SharedService } from '../../shared/services/shared.service';
import { UserService } from 'src/app/api';

import _ from 'underscore';


@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss']
})
export class MessagesComponent {

  user:any;
  chatUser:any
  uneadCount = 0;

  @ViewChild('inboxContainer') inboxContainer: ElementRef;


  constructor(
    private userService: UserService,
    private sharedService: SharedService,
    private sessionService: SessionService
  ) {

  }

  handleUnread(event) {
    if(!_.isEmpty(event)) {
      this.uneadCount = event[0]?.unreadMessageCount
    }
    else {
      this.uneadCount = 0
    }
  }

  ngOnInit() {

    this.userService.apiUserGetUserByUserNameGet('kannantest32@gmail.com').subscribe({
      next:(res:any) => {
        this.chatUser = res.value[0]
      },
      error:() => {

      }
    })

    this.sharedService.inboxunreadcountcast.subscribe((res:any) => {
      if(!_.isEmpty(res)) {
        this.uneadCount = res[0]?.unreadMessageCount
      }
      else {
        this.uneadCount = 0
      }
    })
    
    this.sessionService.userdetailscast.subscribe((res: any) => {
      this.user = res
    })

  }


}
