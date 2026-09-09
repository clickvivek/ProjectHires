import { Component } from '@angular/core';
import { Subscription } from 'rxjs';

import {  Router } from '@angular/router';
import { SharedService } from 'src/app/modules/shared/services/shared.service';

@Component({
  selector: 'app-posting-edit',
  templateUrl: './posting-edit.component.html',
  styleUrls: ['./posting-edit.component.scss']
})
export class PostingEditComponent {

  isOpened:boolean = false;
  subscription: Subscription;
  
  selectedJob;

  constructor(
    private router: Router,
    private sharedService: SharedService
  ){

  }

  onSidenavClose() {
    this.sharedService.setSideNavData(null)
    this.router.navigate(['/posting-history']);
  }

  handleClose(event) {
    this.isOpened = !event
  }

  ngOnInit() {

    this.subscription = this.sharedService.sidenavdatacast.subscribe((res:any) => {
      if(res) {
        setTimeout(() => {
          this.isOpened = true
          this.selectedJob = res
        })
      }
      else {
        setTimeout(() => {
          this.isOpened = false
          this.selectedJob = null
        })
      }
    })

  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

}
