import { Component } from '@angular/core';
import { Subscription } from 'rxjs';

import { SharedService } from 'src/app/modules/shared/services/shared.service';

@Component({
  selector: 'view-comments',
  templateUrl: './view-comments.component.html',
  styleUrls: ['./view-comments.component.scss']
})
export class ViewCommentsComponent {

  isInitiate:boolean = false;
  isOpened:boolean = false;
  subscription: Subscription;

  constructor(
    private sharedService: SharedService
  ) {
    
  }

  handleComments() {
    this.isInitiate = true
    this.sharedService.setSideNavData(null)
    setTimeout(() => {
      this.sharedService.setSideNavData("hello")
    }, 2500)    
  }

  ngOnInit() {

    this.subscription = this.sharedService.sidenavdatacast.subscribe((res:any) => {
      if(res) {
        setTimeout(() => {
          this.isOpened = true
        });
      }
      else {
        this.isOpened = false
      }

    })

  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

}
