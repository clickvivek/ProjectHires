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

  workModelList: any;
  selectedWorkModeArr:Array<number> = [];
  workModeParamIds: Array<number> = [];

  isFilterExists: boolean = false;
  @Output() inputChange = new EventEmitter();

  constructor(
    private commonService: CommonService,
    private route: ActivatedRoute
  ) {
    
  }

  onInputChange(id, status) {
    if (status) {
      this.selectedWorkModeArr.push(id)
    }
    else {
      this.selectedWorkModeArr = this.selectedWorkModeArr.filter((item) => {
        return item != id
      })
    }
  }

  getFilterCount() {
    return this.workModelList.filter(item => item.checked).length; 
  }

  reset() {
    this.selectedWorkModeArr = [];
    this.workModelList.map(item => {
      item.checked = false
      return item
    })
    this.isFilterExists = false;
    this.inputChange.emit(this.selectedWorkModeArr)
  }

  applySearch() {
    this.inputChange.emit(this.selectedWorkModeArr)
  }

  ngOnInit() {
    
    this.commonService.apiCommonEmploymentTypeGet().subscribe({
      next: (res : any) => {
        
        this.workModelList = res.value

        this.workModelList.map(item => {
          item.checked = false
          return item
        })

        const params = this.route.snapshot.queryParams;

        if (!_.isUndefined(params['wm'])) {
          this.workModeParamIds = params['wm']?.split(',');
          
          this.workModelList.map((item, index) => {
            if (this.workModeParamIds.includes(item.id.toString())) {
              item.checked = true
            }
            return item
          });
          
          this.workModelList.sort((a, b) => (a.checked === b.checked ? 0 : a.checked ? -1 : 1));

          this.selectedWorkModeArr = []
          this.workModelList.forEach(item => {
            if (item.checked) {
              this.selectedWorkModeArr.push(item.id)
            }
          });


          this.isFilterExists = true
        }

      },
      error: (error:any) => { }
    })

  }

}
