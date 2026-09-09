import { Component, OnInit, Input, Output, EventEmitter, OnChanges, SimpleChanges, ElementRef} from '@angular/core';
import { ControlContainer, NgForm } from '@angular/forms';
import _ from 'underscore';

@Component({
  selector: 'app-search-field',
  templateUrl: './search-field.component.html',
  styleUrls: ['./search-field.component.scss'],
  viewProviders: [ { provide: ControlContainer, useExisting: NgForm } ]
})
export class SearchFieldComponent implements OnInit, OnChanges {

  @Input() fieldName:string;
  @Input() fieldText:string;
  @Input() fieldPlaceholder:string;
  @Input() fieldModel:any;
  @Input() fieldRequired:any;
  @Input() fieldList:any;
  @Input() fieldType:any;
  @Input() isEdit:any;
  @Input() editValue:any;

  @Input() fieldDisabled:boolean = false;

  selectedItem:any = null;
  isItemSelected:boolean = false;
  isExpanded:boolean = false;

  @Output() queryChange = new EventEmitter();
  @Output() inputChange = new EventEmitter();

  constructor(
    private element: ElementRef
  ) { }

  isFieldRequired(){
    return this.fieldRequired
  }

  getItemData(item:any) {
    let newData = this.fieldType.split(',')
    let finalData = ""
    newData.forEach((typeItem, index) => {
      finalData = `${finalData + item[typeItem]}${(index !== newData.length-1) ? ', ' : ''}`
    });
    return finalData
  }

  getSelectedItem(item:any) {

    let newData = this.fieldType.split(',')
    let finalData = ""
    newData.forEach((typeItem, index) => {
      finalData = `${finalData + item[typeItem]}${(index !== newData.length-1) ? ', ' : ''}`
    });
    this.fieldModel = finalData;
    this.inputChange.emit(item);
    this.selectedItem = item
    this.isExpanded = false;

  }

  clearList() {
    this.fieldModel = "";
    this.selectedItem = null
  }

  handleModelChange(){
    if(this.fieldModel.length > 1) {
      this.isExpanded = true
      this.queryChange.emit(this.fieldModel);
    }
    else {
      this.isExpanded = false
    }
  }

  ngOnInit() {

  }

  ngOnChanges(changes: SimpleChanges) {
    if (!_.isEmpty(this.editValue) && this.isEdit && !this.fieldModel && _.isEmpty(this.selectedItem)) {
      let newData = this.fieldType.split(',')
      let finalData = ""
      newData.forEach((typeItem, index) => {
        finalData = `${finalData + this.editValue[typeItem]}${(index !== newData.length-1) ? ', ' : ''}`
      });
      
      this.fieldModel = finalData;
      this.inputChange.emit(this.editValue);
    }

  }



}
