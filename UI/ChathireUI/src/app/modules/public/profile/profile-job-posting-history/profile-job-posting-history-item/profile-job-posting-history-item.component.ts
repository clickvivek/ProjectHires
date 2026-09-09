import { Component, Input, Output, EventEmitter } from '@angular/core';

import * as moment from 'moment';
import _ from 'underscore';

@Component({
  selector: 'profile-job-posting-history-item',
  templateUrl: './profile-job-posting-history-item.component.html',
  styleUrls: ['./profile-job-posting-history-item.component.scss']
})
export class ProfileJobPostingHistoryItemComponent {

  @Input() job;
  @Input() selectedJob;
  @Input() last;

  @Output() outParam = new EventEmitter();

  getRelocation(location) {
    if(!_.isEmpty(location)) {
      let newArray:string[] = []
      location.forEach(item => {
        let name = item.city
        newArray.push(name.city1 + '-' + name.stateCode)
      });
      return newArray.join(', ')
    }
    else {
      return ""
    }
  }

  getDate(date) {
    return moment(date).format('MMM D, YYYY')
  }

  showJobDescription(job) {
    this.outParam.emit(job)
  }

  isSelected(id:any){
    return this.selectedJob.jobOpeningId == id ? 'selected' : '';
  }

}
