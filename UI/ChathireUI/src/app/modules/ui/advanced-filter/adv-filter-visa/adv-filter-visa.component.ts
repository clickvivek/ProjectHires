import { Component, Output, EventEmitter } from '@angular/core';
import { CommonService } from 'src/app/api';
import { ActivatedRoute } from '@angular/router';
import _ from 'underscore';

@Component({
  selector: 'adv-filter-visa',
  templateUrl: './adv-filter-visa.component.html',
  styleUrls: ['./adv-filter-visa.component.scss']
})
export class AdvFilterVisaComponent {

  visaList: Array<any> = [];
  selectedVisaArr:Array<number> = [];
  visasParamIds: Array<number> = [];
  
  isFilterExists: boolean = false;
  @Output() inputChange = new EventEmitter();

  constructor(
    private commonService: CommonService,
    private route: ActivatedRoute
  ) {

  }

  onInputChange(id, status) {
    if (status) {
      this.selectedVisaArr.push(id)
    }
    else {
      this.selectedVisaArr = this.selectedVisaArr.filter((item) => {
        return item != id
      })
    }
  }

  getFilterCount() {
    return this.visaList.filter(item => item.checked).length; 
  }

  reset() {
    this.selectedVisaArr = [];
    this.visaList.map(item => {
      item.checked = false
      return item
    })
    this.isFilterExists = false;
    this.inputChange.emit(this.selectedVisaArr)
  }

  applySearch() {
    this.inputChange.emit(this.selectedVisaArr)
  }

  ngOnInit() {
    
    this.commonService.apiCommonVisaGet().subscribe({
      next: (res : any) => {
        
        this.visaList = res.value
        
        this.visaList.map(item => {
          item.checked = false
          return item
        })

        const params = this.route.snapshot.queryParams;

        if (!_.isUndefined(params['visas'])) {
          this.visasParamIds = params['visas']?.split(',');
          
          this.visaList.map((item, index) => {
            if (this.visasParamIds.includes(item.id.toString())) {
              item.checked = true
            }
            return item
          });
          
          this.visaList.sort((a, b) => (a.checked === b.checked ? 0 : a.checked ? -1 : 1));

          this.selectedVisaArr = []
          this.visaList.forEach(item => {
            if (item.checked) {
              this.selectedVisaArr.push(item.id)
            }
          });


          this.isFilterExists = true
        }

      },
      error: (error:any) => { }
    })

  }

}
