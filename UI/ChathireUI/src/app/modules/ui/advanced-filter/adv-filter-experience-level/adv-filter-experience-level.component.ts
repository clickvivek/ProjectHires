import { Component, Output, EventEmitter } from '@angular/core';
import { CommonService } from 'src/app/api';
import { ActivatedRoute } from '@angular/router';
import _ from 'underscore';
import { filterexperienceLevel } from 'src/app/data/filter-data';

@Component({
  selector: 'adv-filter-experience-level',
  templateUrl: './adv-filter-experience-level.component.html',
  styleUrls: ['./adv-filter-experience-level.component.scss']
})
export class AdvFilterExperienceLevelComponent {

  expList = filterexperienceLevel;
  selectedExpArr:Array<number> = [];
  expParamIds: Array<any> = [];

  isFilterExists: boolean = false;
  @Output() inputChange = new EventEmitter();

  constructor(
    private commonService: CommonService,
    private route: ActivatedRoute
  ) {
    
  }

  onInputChange(id, status) {
    if (status) {
      this.selectedExpArr.push(id)
    }
    else {
      this.selectedExpArr = this.selectedExpArr.filter((item) => {
        return item != id
      })
    }
  }

  reset() {
    this.selectedExpArr = [];
    this.expList.map(item => {
      item.checked = false
      return item
    })
    this.isFilterExists = false;
    this.inputChange.emit(this.selectedExpArr)
  }

  applySearch() {
    this.inputChange.emit(this.selectedExpArr)
  }

  getFilterCount() {
    return this.expList.filter(item => item.checked).length; 
  }

  ngOnInit() {

   const params = this.route.snapshot.queryParams;
    
   if (!_.isUndefined(params['exp'])) {
      this.expParamIds = params['exp']?.split(',');
      
      this.expList.map((item, index) => {
        if (this.expParamIds.includes(item.id.toString())) {
          item.checked = true
        }
        return item
      });
           
      this.expList.sort((a, b) => (a.checked === b.checked ? 0 : a.checked ? -1 : 1));

      this.selectedExpArr = []
      this.expList.forEach(item => {
        if (item.checked) {
          this.selectedExpArr.push(item.id)
        }
      });


      this.isFilterExists = true
   }
   else {
      this.expList.map(item => {
        item.checked = false
        return item
      })
     
    }

  }

}
