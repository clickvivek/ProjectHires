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

  getFormattedLocations(locations: any): string {
    if (!locations) return '';
    if (Array.isArray(locations)) {
      if (locations.length === 0) return '';
      return locations
        .map(loc => this.formatLocation(loc))
        .filter(str => str && str.length > 0)
        .join(' ; ');
    }
    return this.formatLocation(locations);
  }

  formatLocation(loc: any): string {
    if (!loc) return '';
    if (typeof loc === 'string') {
      let trimmed = loc.trim().replace(/[,;\s]+$/, '');
      if (trimmed.includes('-')) {
        const parts = trimmed.split('-');
        return parts.map(p => p.trim()).filter(p => p).join(', ');
      }
      if (trimmed.includes(',')) {
        const parts = trimmed.split(',');
        return parts.map(p => p.trim()).filter(p => p).join(', ');
      }
      return trimmed;
    }
    if (typeof loc === 'object') {
      const city = (loc.city1 || loc.city || loc.cityName || '').trim();
      const state = (loc.stateCode || loc.stateName || '').trim();
      return (city && state) ? `${city}, ${state}` : (city || state);
    }
    return String(loc);
  }

  getRelocation(location) {
    if(!_.isEmpty(location)) {
      let newArray:string[] = []
      location.forEach(item => {
        let name = item.city
        let city = name?.city1 ? name.city1.trim() : ''
        let state = name?.stateCode ? name.stateCode.trim() : ''
        let formatted = (city && state) ? `${city}, ${state}` : (city || state)
        if (formatted) newArray.push(formatted)
      });
      return newArray.join('; ')
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
    return (this.selectedJob && this.selectedJob.jobOpeningId == id) ? 'selected' : '';
  }

}
