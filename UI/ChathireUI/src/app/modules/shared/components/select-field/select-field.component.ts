import { Component, OnInit, Input, DoCheck, Output, EventEmitter, HostListener, ElementRef, OnChanges, SimpleChanges } from '@angular/core';

import { ControlContainer, NgForm } from '@angular/forms';
import _ from 'underscore';

@Component({
  selector: 'app-select-field',
  templateUrl: './select-field.component.html',
  styleUrls: ['./select-field.component.scss'],
  viewProviders: [ { provide: ControlContainer, useExisting: NgForm } ]
})
export class SelectFieldComponent implements OnInit, DoCheck {

  @Input() fieldName:string;
  @Input() fieldText:string;
  @Input() fieldPlaceholder:string;
  @Input() fieldModel:any;
  @Input() fieldRequired:any;
  @Input() fieldList:any;
  @Input() fieldType:any;
  @Input() isEdit:any;
  @Input() editValue:any;

  @Output() inputChange = new EventEmitter();

  isExpanded:boolean = false;

  constructor(
     private element: ElementRef
  ) { }

  showFieldItems() {
    this.isExpanded = true
  }

  isFieldRequired(){
    return this.fieldRequired;
  }

  getSelectedItem(item:any) {
    this.fieldModel = item[this.fieldType];
    this.inputChange.emit(item);
  }

  isSelectedItem(item:any, modal:any) {
    return item[this.fieldType] == modal ? 'selected': '';
  }

  @HostListener('document:click', ['$event'])
    onDocumentClick(event:any) {

      event.stopPropagation();

      var el = this.element.nativeElement.querySelector('.select');
      var arrowElement = this.element.nativeElement.querySelector('.select-arrow');

      if (!el.contains(event.target) && !arrowElement.contains(event.target)) {
        this.isExpanded = false
      }

   }

  ngOnInit() {

  }

  ngOnChanges(changes: SimpleChanges) {

    if(this.isEdit && this.editValue && !_.isEmpty(this.fieldList)) {
      let newData = this.fieldList?.filter(item => {
        return item.id == this.editValue
      })
      this.fieldModel = newData[0][this.fieldType]
      this.inputChange.emit(newData[0]);
    }

  }

  ngDoCheck() {
    if(this.fieldModel == "null" || this.fieldModel == "undefined" || this.fieldModel == 0){
      this.fieldModel = null;
    }
  }

}
