import { Component, OnInit, Input, Output, EventEmitter, ViewEncapsulation, SimpleChanges, ElementRef, HostListener, Renderer2 } from '@angular/core';

@Component({
  selector: 'app-pagination-box',
  templateUrl: './pagination-box.component.html',
  styleUrls: ['./pagination-box.component.scss'],
  encapsulation  : ViewEncapsulation.None
})
export class PaginationBoxComponent implements OnInit {

  @Input() ItemStartIndex;
  @Input() ItemEndIndex;
  @Input() itemLimit;
  @Input() totalItems;

  totalPages:number;
  pageItemArray;any;
  itemPageIndex:any = 1;

  visiblePageNum:number = 7;

  pageRowsDataList:any;

  isPageRowSelected:boolean = false;

  @Output() public outputParams = new EventEmitter();

  constructor(
    private element: ElementRef, private renderer: Renderer2
   ) { }

  isPageNumActive(num){
    return this.itemPageIndex == num ? 'active' : '';
  }


  getPageList(totalPages, page, maxLength) {

      var sideWidth = maxLength < 9 ? 1 : 2;
      var leftWidth = (maxLength - sideWidth*2 - 3) >> 1;
      var rightWidth = (maxLength - sideWidth*2 - 2) >> 1;
      if (totalPages <= maxLength) {
          // no breaks in list
          return this.range(1, totalPages);
      }
      if (page <= maxLength - sideWidth - 1 - rightWidth) {
          // no break on left of page
          return this.range(1, maxLength-sideWidth-1)
              .concat([0])
              .concat(this.range(totalPages-sideWidth+1, totalPages));
      }
      if (page >= totalPages - sideWidth - 1 - rightWidth) {
          // no break on right of page
          return this.range(1, sideWidth)
              .concat([0])
              .concat(this.range(totalPages - sideWidth - 1 - rightWidth - leftWidth, totalPages));
      }
      // Breaks on both sides
      return this.range(1, sideWidth)
      .concat([0])
      .concat(this.range(page - leftWidth, page + rightWidth))
      .concat([0])
      .concat(this.range(totalPages-sideWidth+1, totalPages));
  }

  range(start, end) {
    return Array.from(Array(end - start + 1), (_, i) => i + start);
  }

  gotoPageNum(num){

    this.itemPageIndex=num;

    if(num>1)
      this.ItemStartIndex = (this.itemLimit*num)-this.itemLimit;
    else
      this.ItemStartIndex = 0;

    this.ItemEndIndex = (this.itemLimit*num);

    if(this.ItemEndIndex>this.totalItems)
      this.ItemEndIndex = this.totalItems;

    this.outputParams.emit({'ItemStartIndex':this.ItemStartIndex,'ItemEndIndex':this.ItemEndIndex, 'itemLimit': this.itemLimit})

  }


  showPageList(){
    this.isPageRowSelected = true;
  }

  selectPageItem(num){

    this.itemLimit = parseInt(num);
    this.isPageRowSelected = false;

    this.gotoPageNum(1);


  }

  isSelected(num){
    return this.itemLimit == parseInt(num) ? 'selected' : '';
  }

  ngOnInit() {

    this.pageRowsDataList = [
      {id:1, name: '10'},
      {id:2, name: '20'},
      {id:3, name: '30'},
      {id:4, name: '40'},
      {id:4, name: '50'}
    ]

  }

  @HostListener('document:click', ['$event'])
    onDocumentClick(event) {

      var el = this.element.nativeElement;
      var pageSelect = el.querySelector('.page-select-wrapper');

      if (!pageSelect.contains(event.target)) {
        this.isPageRowSelected = false;
      }

   }


  ngOnChanges(changes: SimpleChanges) {


    if(this.itemPageIndex>1)
      this.ItemStartIndex = (this.itemLimit*this.itemPageIndex)-this.itemLimit;
    else
      this.ItemStartIndex = 0;

  	if(this.totalItems<this.itemLimit) {
      this.totalPages = 1;
      this.ItemStartIndex = 0;
      this.ItemEndIndex = this.totalItems;
    }
    else {
  	  this.totalPages = Math.floor(this.totalItems/this.itemLimit);
      if((this.totalItems%this.itemLimit) > 0){
        this.totalPages = this.totalPages+1;
      }
      if(this.totalPages < this.itemPageIndex) {
        this.itemPageIndex = this.totalPages
        this.gotoPageNum(this.itemPageIndex)
      }
    }

    //this.pageItemArray =  Array(this.totalPages).fill(0).map((x,i)=>i+1);
    this.pageItemArray =  this.getPageList(this.totalPages, this.itemPageIndex, this.visiblePageNum);

  }


}
