import { Component, Input, Output, EventEmitter, HostListener, ElementRef, ViewChild, OnChanges, SimpleChanges } from '@angular/core';
import { ControlContainer, NgForm } from '@angular/forms';
import _ from 'underscore';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-multi-select-field',
  templateUrl: './multi-select-field.component.html',
  styleUrls: ['./multi-select-field.component.scss'],
  viewProviders: [ { provide: ControlContainer, useExisting: NgForm } ]
})
export class MultiSelectFieldComponent {

  @Input() fieldName:string;
  @Input() fieldText: string;
  @Input() fieldRequired: any;
  @Input() fieldPlaceholder: string;
  @Input() fieldType:any;
  @Input() fieldModel: any;
  @Input() fieldList: any;
  @Input() fieldItemMinLimit:any = 0;
  @Input() fieldItemMaxLimit:any = 99;

  @Input() isEdit:any;
  @Input() editValue:any;
  
  selectedItem:any = [];
  isExpanded: boolean = false;

  leftPadding:any = 10
  topPadding:any = 4
  
  @Output() inputChange = new EventEmitter();

  @ViewChild('multiInputElem') multiInputElem: ElementRef;
  @ViewChild('badgeListElem') badgeListElem: ElementRef;

  isChecked:boolean = false;

  initialfilterList:any = []

  constructor(
     private element: ElementRef
  ) { }
  
  
  isFieldRequired(){
    return this.fieldRequired;
  }

  showFieldItems(event?: Event) {
    if (event) {
      event.stopPropagation();
    }
    this.isExpanded = !this.isExpanded;
  }

  compareObj(obj1, obj2) {
    return JSON.stringify(obj1) === JSON.stringify(obj2);
  }

  pushObj(arr, newObj) {
    if (!arr.some((obj) => this.compareObj(obj, newObj))) {
      arr.push(newObj);
    }
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

  removeBadge(item:any) {
    
    this.fieldModel = ""
    this.selectedItem = this.selectedItem.filter((listItem, index) => {
      return listItem[this.fieldType] !== item[this.fieldType]
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

  

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: any) {
    if (!this.element.nativeElement.contains(event.target)) {
      this.isExpanded = false;
    }
  }

  getItemData(item:any) {
    let newData = this.fieldType.split(',')
    let finalData = ""
    newData.forEach((typeItem, index) => {
      finalData = `${finalData + item[typeItem]}${(index !== newData.length-1) ? ', ' : ''}`
    });
    return finalData
  }


  ngOnChanges(changes: SimpleChanges) {

    

    //if (!_.isEmpty(this.editValue) && this.isEdit && !this.fieldModel && _.isEmpty(this.selectedItem)) {

    if (!_.isEmpty(this.editValue) && !this.fieldModel && _.isEmpty(this.selectedItem)) {
      
      this.selectedItem = []
      this.editValue.forEach(item => {
        this.handleSelectedItem(item)
      })
    }

    // if item selected, item will be removed from the inital list and vice versa
    if(!_.isEmpty(this.fieldList)) {

      if(_.isEmpty(this.initialfilterList)) {
        this.initialfilterList = [...this.fieldList]

      }
      let initialfilterList = [...this.initialfilterList]
      this.fieldList = initialfilterList.filter(item => {
        return !this.editValue.find(item2 => item2[this.fieldName] === item.id);
      });

    }


  }

}
