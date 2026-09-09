import { Component } from '@angular/core';

import _ from 'underscore';

@Component({
  selector: 'app-postjobs',
  templateUrl: './postjobs.component.html',
  styleUrls: ['./postjobs.component.scss']
})
export class PostjobsComponent  {

  isJobPosted:boolean = false;

  handleJobPost(event) {
    this.isJobPosted = event
  }

}
