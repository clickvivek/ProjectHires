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

    let item = data.candidatePrefLocations

    let newData:any = []
    if(!_.isEmpty(item)) {
      item.forEach(listItem => {
        let name = listItem.cityName.split('-')
        newData.push(name[0])
      });
      return newData.join(', ')
    }
    else {
      if(data.anyLocation) {
        return "Any Location"
      }
      else if (data.remoteOnly) {
        return "Remote"
      }
      else {
        return "NA"
      }
    }
  }

}
