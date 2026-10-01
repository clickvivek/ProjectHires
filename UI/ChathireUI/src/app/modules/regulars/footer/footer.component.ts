import { Component, OnInit } from '@angular/core';
import { AuthService } from 'src/app/core/auth/auth.service';
import { SessionService } from 'src/app/core/session/session.service';

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.scss']
})
export class FooterComponent implements OnInit {

  user: any;

  constructor(
    private authService: AuthService,
    private sessionService: SessionService
  ) { }

  isProfileSetupPending(): boolean {
    if (!this.authService.isLoggedIn()) return false;
    if (Number(this.sessionService.userTypeId) === 7) return false;
    if (Number(this.sessionService.userTypeId) === 5) {
      return !this.user?.fname || !this.user?.cityId;
    }
    return !this.sessionService.consultancyId;
  }

  ngOnInit(): void {
    this.sessionService.userdetailscast.subscribe((res: any) => {
      this.user = res;
    });
  }

}
