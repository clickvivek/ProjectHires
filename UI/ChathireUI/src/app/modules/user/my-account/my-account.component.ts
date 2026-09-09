import { Component, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { SessionService } from 'src/app/core/session/session.service';

@Component({
  selector: 'app-my-account',
  templateUrl: './my-account.component.html',
  styleUrls: ['./my-account.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class MyAccountComponent {

  user:any;

  constructor(
    private sessionService: SessionService
  ) { 

  }

  ngOnInit() {
    
    this.sessionService.userdetailscast.subscribe((res: any) => {
      this.user = res
    })

  }

}
