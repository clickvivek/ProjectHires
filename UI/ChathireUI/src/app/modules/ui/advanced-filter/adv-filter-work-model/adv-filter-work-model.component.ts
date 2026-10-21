import { Component, Output, EventEmitter } from '@angular/core';
import { CommonService } from 'src/app/api';
import { ActivatedRoute } from '@angular/router';
import _ from 'underscore';

@Component({
  selector: 'adv-filter-work-model',
  templateUrl: './adv-filter-work-model.component.html',
  styleUrls: ['./adv-filter-work-model.component.scss']
})
export class AdvFilterWorkModelComponent {

  jobTypeList: any[] = [];
  selectedJobTypeArr: Array<number> = [];
  jobTypeParamIds: Array<string> = [];

  isFilterExists: boolean = false;
  @Output() inputChange = new EventEmitter();

  constructor(
    private commonService: CommonService,
    private route: ActivatedRoute
  ) {}

  onInputChange(id: number, status: boolean) {
    if (status) {
      if (!this.selectedJobTypeArr.includes(id)) {
        this.selectedJobTypeArr.push(id);
      }
    } else {
      this.selectedJobTypeArr = this.selectedJobTypeArr.filter((item) => item !== id);
    }
  }

  getFilterCount() {
    return this.jobTypeList ? this.jobTypeList.filter(item => item.checked).length : 0; 
  }

  reset() {
    this.selectedJobTypeArr = [];
    if (this.jobTypeList) {
      this.jobTypeList.forEach(item => {
        item.checked = false;
      });
    }
    this.isFilterExists = false;
    this.inputChange.emit(this.selectedJobTypeArr);
  }

  applySearch() {
    this.isFilterExists = (this.selectedJobTypeArr && this.selectedJobTypeArr.length > 0);
    this.inputChange.emit(this.selectedJobTypeArr);
  }

  ngOnInit() {
    // Fetch JobType list filtered for US by default to match possible values:
    // C2C (Corp to Corp), W2 Contract, W2 Full-Time, 1099 Contract, Part-time W2/C2C/1099, Internship, Contract to Hire
    this.commonService.apiCommonJobTypeGet('US').subscribe({
      next: (res: any) => {
        this.jobTypeList = res.value || [];

        this.jobTypeList.forEach(item => {
          item.checked = false;
        });

        const params = this.route.snapshot.queryParams;

        // Support 'jt' (or legacy 'wm') query parameter
        const paramVal = params['jt'] || params['jobTypes'] || params['wm'];
        if (!_.isUndefined(paramVal) && paramVal) {
          this.jobTypeParamIds = paramVal.toString().split(',');
          
          this.jobTypeList.forEach(item => {
            if (this.jobTypeParamIds.includes(item.id.toString())) {
              item.checked = true;
            }
          });
          
          this.jobTypeList.sort((a, b) => (a.checked === b.checked ? 0 : a.checked ? -1 : 1));

          this.selectedJobTypeArr = [];
          this.jobTypeList.forEach(item => {
            if (item.checked) {
              this.selectedJobTypeArr.push(item.id);
            }
          });

          this.isFilterExists = this.selectedJobTypeArr.length > 0;
        }
      },
      error: (error: any) => {}
    });
  }
}
