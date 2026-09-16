import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import _ from 'underscore';

@Component({
  selector: 'profile-available-resources-table',
  templateUrl: './profile-available-resources-table.component.html',
  styleUrls: ['./profile-available-resources-table.component.scss']
})
export class ProfileAvailableResourcesTableComponent {

  @Input() list;
  @Input() visaList: any;
  @Input() availabilityList: any;

  constructor(
    private router: Router,
    private route: ActivatedRoute
  ) {

  }

  getVisa(id){
    var name;
    _.some(this.visaList, (item) => {
      if(item.id == id)
        name = item.name;
    });
    return name;
  }

  getAvailability(id) {
    if (id) {
      let newData = this.availabilityList.filter(item => {
        return item.id == id
      })
      return `${newData[0].name}`
    }
    else {
      return 'Not Available'
    }
  }

  getRelocation(data) {
    let item = data?.candidatePrefLocations

    let newData: any = []
    if (!_.isEmpty(item)) {
      item.forEach(listItem => {
        let rawCity = listItem.cityName || ''
        let parts = rawCity.split('-')
        let city = parts[0] ? parts[0].trim() : ''
        let state = (listItem.stateCode || (parts[1] ? parts[1].trim() : '') || listItem.stateName || '').trim()
        let formatted = (city && state) ? `${city}, ${state}` : (city || state)
        if (formatted) {
          newData.push(formatted)
        }
      });
      return newData.length > 0 ? newData.join('; ') : (data.anyLocation ? "Any Location" : data.remoteOnly ? "Remote" : "NA")
    }
    else {
      if (data?.anyLocation) {
        return "Any Location"
      }
      else if (data?.remoteOnly) {
        return "Remote"
      }
      else {
        return "NA"
      }
    }
  }

  isSkills(item) {
    return _.isEmpty(item) ? false : true
  }

}
