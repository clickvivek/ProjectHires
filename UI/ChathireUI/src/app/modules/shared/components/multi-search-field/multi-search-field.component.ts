import { Component, OnInit, Input, Output, EventEmitter, ElementRef, ViewChild, ChangeDetectorRef, OnChanges, SimpleChanges } from '@angular/core';
import { ControlContainer, NgForm } from '@angular/forms';

import _ from 'underscore';


@Component({
  selector: 'app-multi-search-field',
  templateUrl: './multi-search-field.component.html',
  styleUrls: ['./multi-search-field.component.scss'],
  viewProviders: [ { provide: ControlContainer, useExisting: NgForm } ]
})
export class MultiSearchFieldComponent implements OnInit, OnChanges {

  @Input() fieldName:string;
  @Input() fieldText:string;
  @Input() fieldPlaceholder:string;
  @Input() fieldModel:any;
  @Input() fieldRequired:any;
  @Input() fieldList:any;
  @Input() fieldType:any;
  @Input() fieldItemMinLimit:any = 0;
  @Input() fieldItemMaxLimit:any = 5;
  @Input() isEdit:any;
  @Input() editValue:any;

  selectedItem:any = [];
  isExpanded:boolean = false;

  leftPadding:any = 10
  topPadding:any = 4

  forceRequired:any = "";

  @Output() queryChange = new EventEmitter();
  @Output() inputChange = new EventEmitter();

  @ViewChild('multiInputElem') multiInputElem: ElementRef;
  @ViewChild('badgeListElem') badgeListElem: ElementRef;

  constructor(
    private cdRef : ChangeDetectorRef
  ) { }

  isFieldRequired(){
    return this.fieldRequired;
  }

  getItemData(item:any) {
    let newData = this.fieldType.split(',')
    let finalData = ""
    newData.forEach((typeItem, index) => {
      finalData = `${finalData + item[typeItem]}${(index !== newData.length-1) ? ', ' : ''}`
    });
    return finalData
  }

  handleModelChange() {
    if(this.fieldModel.length > 1) {
      this.isExpanded = true
      this.queryChange.emit(this.fieldModel);
    }
    else {
      this.isExpanded = false
    }
  }

  compareObj(obj1, obj2) {
    return JSON.stringify(obj1[this.splitFieldType(this.fieldType)]) === JSON.stringify(obj2[this.splitFieldType(this.fieldType)]);
  }

  splitFieldType(fieldType) {
    let newTypes = fieldType.split(',')
    return newTypes[0]
  }

  pushObj(arr, newObj) {
    if (!arr.some((obj) => this.compareObj(obj, newObj))) {
      arr.push(newObj);
      
    }
  }

  handleInputHeight() {

    let inputElement = this.multiInputElem.nativeElement;
    let badgeElement = this.badgeListElem.nativeElement;

    let badgeListHeight = badgeElement.clientHeight;

    inputElement.style.height = `${badgeListHeight+10}px`;

    const childElements = badgeElement.children[0].children;
    const lastChild = childElements[childElements.length - 1];

    if(lastChild) {
      const offsetLeft = lastChild.offsetLeft+lastChild.clientWidth+12;
      const offsetTop = lastChild.offsetTop+2;
      this.leftPadding = offsetLeft
      this.topPadding = offsetTop
    }
    else {
      this.leftPadding = 10
      this.topPadding = 4
    }

    inputElement.focus();

  }

  handleSelectedItem(item:any) {

    if(this.selectedItem.length < this.fieldItemMaxLimit) {
      this.pushObj(this.selectedItem, item)
      this.inputChange.emit(this.selectedItem)
      this.fieldModel = ""
      this.isExpanded = false

      setTimeout(() => {
        this.handleInputHeight()
      }, 100)
    }

    if(this.selectedItem.length == this.fieldItemMaxLimit) {
      setTimeout(() => {
        this.fieldModel = ""
        const inputElement = this.multiInputElem.nativeElement;
        inputElement.blur();
      }, 100)
    }

  }
  
  handleTextIndent() {
    if(this.selectedItem.length == this.fieldItemMaxLimit) {
      return -2000
    }
    else {
      return 0
    }
  }

  getSelectedItemData(badge, fieldType) {
    let newData = fieldType.split(',')
    let finalData = ""
    newData.forEach((item, index) => {
      finalData = `${finalData + badge[item]}${(index !== newData.length-1) ? ', ' : ''}`
    });
    return finalData
  }

  removeBadge(item:any) {
    this.fieldModel = ""
    this.selectedItem = this.selectedItem.filter((listItem, index) => {
      let newFieldType = this.fieldType.split(',')
      return listItem[newFieldType[0]] !== item[newFieldType[0]]
    })
    this.inputChange.emit(this.selectedItem)
    setTimeout(() => {
      this.handleInputHeight()
    }, 100)

  }

  clearModel() {
    this.fieldModel=""
    this.checkRequired()
  }

  checkRequired() {
    if(this.selectedItem.length >= this.fieldItemMinLimit) {
      return false
    }
    else {
      return true
    }
  }

  ngOnInit(): void {

  }

  ngOnChanges(changes: SimpleChanges) {


    if (!_.isEmpty(this.editValue) && this.isEdit && !this.fieldModel && _.isEmpty(this.selectedItem)) {
      this.selectedItem = []
      this.editValue.forEach(item => {
        this.handleSelectedItem(item)
      })
    }

  }


}
