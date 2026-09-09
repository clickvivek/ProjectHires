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

  isSkills(item) {
    return _.isEmpty(item) ? false : true
  }

}
