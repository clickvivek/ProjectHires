import { Component, Input } from '@angular/core';
import _ from 'underscore';

@Component({
  selector: 'profile-available-resources-item',
  templateUrl: './profile-available-resources-item.component.html',
  styleUrls: ['./profile-available-resources-item.component.scss']
})
export class ProfileAvailableResourcesItemComponent {

  @Input() job;
  @Input() visaList;
  @Input() availabilityList;

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
      if (!_.isEmpty(newData)) {
        return `${newData[0].name}`
      }
      else {
        return ""
      }
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
    return !_.isEmpty(item);
  }

}
