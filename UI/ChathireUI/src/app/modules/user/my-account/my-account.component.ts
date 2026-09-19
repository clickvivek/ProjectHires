import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { SessionService } from 'src/app/core/session/session.service';
import { picUrl, defaultProfilePic } from 'src/app/data/various';

@Component({
  selector: 'app-my-account',
  templateUrl: './my-account.component.html',
  styleUrls: ['./my-account.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class MyAccountComponent implements OnInit {

  user: any;
  activeTab: 'profile-details' | 'profile-pic' | 'profile-password' = 'profile-details';
  defaultPic = defaultProfilePic;
  picBase = picUrl;

  constructor(
    private sessionService: SessionService
  ) {}

  ngOnInit() {
    this.sessionService.userdetailscast.subscribe((res: any) => {
      this.user = res;
    });
  }

  setTab(tab: 'profile-details' | 'profile-pic' | 'profile-password') {
    this.activeTab = tab;
  }
}
