import { Component, Output, EventEmitter } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import _ from 'underscore';

import { filterDatePosted } from 'src/app/data/filter-data';
import { CommonService } from 'src/app/api';

@Component({
  selector: 'adv-filter-date-posted',
  templateUrl: './adv-filter-date-posted.component.html',
  styleUrls: ['./adv-filter-date-posted.component.scss']
})
export class AdvFilterDatePostedComponent {

  filterDatePostedData = filterDatePosted;
  selectedDateParam = "";

  isFilterExists: boolean = false;
  @Output() inputChange = new EventEmitter();

  constructor(
    private commonService: CommonService,
     private route: ActivatedRoute
  ) {
    
  }

  onDatePostedChange(value) {
    this.selectedDateParam = value
  }

  reset() {
    this.isFilterExists = false;
    this.selectedDateParam = "";
    this.inputChange.emit(this.selectedDateParam)
  }

  applySearch() {
    this.inputChange.emit(this.selectedDateParam)
  }

  ngOnInit() {

    const params = this.route.snapshot.queryParams;

    if (!_.isUndefined(params['date'])) {
      this.selectedDateParam = params['date']
      this.isFilterExists = true
    }
    else {
      this.selectedDateParam = "";
    }

  }

}
