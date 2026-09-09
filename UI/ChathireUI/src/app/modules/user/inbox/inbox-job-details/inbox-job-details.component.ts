import { Component, Input } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import {  Router } from '@angular/router';
import * as moment from 'moment';

import { SharedService } from 'src/app/modules/shared/services/shared.service';

@Component({
  selector: 'inbox-job-details',
  templateUrl: './inbox-job-details.component.html',
  styleUrls: ['./inbox-job-details.component.scss']
})
export class InboxJobDetailsComponent {

  selectedJob;

  isOpened:boolean = false;

  constructor(
    private router: Router,
    private sanitizer: DomSanitizer,
    private sharedService: SharedService
  ){

  }

  onSidenavClose() {
    this.sharedService.setSideNavData(null)
    this.router.navigate(['/inbox']);
  }

  getDate(date) {
    return moment(date).format('MMMM D, YYYY')
  }
  
  getDescription(content) {
    return this.sanitizer.bypassSecurityTrustHtml(content);
  }

  ngOnInit() {

   

    this.sharedService.sidenavdatacast.subscribe((res:any) => {
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


}
