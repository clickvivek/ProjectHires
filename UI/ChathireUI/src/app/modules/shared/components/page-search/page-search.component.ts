import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'page-search',
  templateUrl: './page-search.component.html',
  styleUrls: ['./page-search.component.scss']
})
export class PageSearchComponent {

  @Input() placeholder;
  @Input() maxlength: number;

  searchData:string = ""

  @Output() public outputData = new EventEmitter();

  constructor() {

  }

  onSearchData() {
    this.outputData.emit(this.searchData)
  }

}
