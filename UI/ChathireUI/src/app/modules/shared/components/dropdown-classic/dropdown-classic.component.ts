import { Component, Input, HostListener, ElementRef, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'dropdown-classic',
  templateUrl: './dropdown-classic.component.html',
  styleUrls: ['./dropdown-classic.component.scss']
})
export class DropdownClassicComponent {

  @Input() fieldName:any;
  @Input() fieldList:any;
  @Input() fieldType:any;
  @Input() fieldRefId?:any;
  @Input() editValue:any;

  isExpanded:boolean = false
  selectedItem = {}

  @Output() changeData = new EventEmitter();

  constructor(
    private element: ElementRef
  ) {

  }

  @HostListener('document:click', ['$event'])
    onDocumentClick(event:any) {

      event.stopPropagation();

      var el = this.element.nativeElement.querySelector('.dropdown-btn');
      var arrowElement = this.element.nativeElement.querySelector('.dropdown-list');

      if (!el?.contains(event.target) && !arrowElement?.contains(event.target)) {
        this.isExpanded = false
      }

   }

  handleSelectedItem(item:any) {
    this.selectedItem = item
    this.changeData.emit({data:item, id: this.fieldRefId})
  }

  isSelectedItem(item:any) {
    return item[this.fieldType] == this.selectedItem[this.fieldType] ? 'selected' : ''
  }

  ngOnInit() {
    
  }

  ngOnChanges() {
    if(this.editValue) {
      let newItem = this.fieldList.filter((item:any) => {
        return item[this.fieldName] == this.editValue
      })
      if(newItem === undefined || newItem.length == 0){
        this.selectedItem = {}
      }
      else {
        this.selectedItem = newItem[0];
      }
    }
    else {
      this.selectedItem = this.fieldList[0];
    }
  }

}
