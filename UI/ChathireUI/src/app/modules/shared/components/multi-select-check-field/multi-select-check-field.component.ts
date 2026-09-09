import { Component, AfterViewInit, Input, Output, EventEmitter, HostListener, ElementRef, ViewChild } from '@angular/core';
import { ControlContainer, NgForm } from '@angular/forms';

@Component({
  selector: 'app-multi-select-check-field',
  templateUrl: './multi-select-check-field.component.html',
  styleUrls: ['./multi-select-check-field.component.scss'],
  viewProviders: [ { provide: ControlContainer, useExisting: NgForm } ]
})
export class MultiSelectCheckFieldComponent {


  @Input() fieldName:string;
  @Input() fieldText: string;
  @Input() fieldRequired: any;
  @Input() fieldPlaceholder: string;
  @Input() fieldType:any;
  @Input() fieldModel: any;
  @Input() fieldList: any;
  @Input() fieldItemMinLimit:any = 0;
  @Input() fieldItemMaxLimit:any = 99;
  
  selectedItem:any = [];
  isExpanded: boolean = false;

  leftPadding:any = 10
  topPadding:any = 4
  
  @Output() inputChange = new EventEmitter();

  @ViewChild('multiInputElem') multiInputElem: ElementRef;
  @ViewChild('badgeListElem') badgeListElem: ElementRef;

  isChecked:boolean = false;

  constructor(
     private element: ElementRef
  ) { }
  
  
  isFieldRequired(){
    return this.fieldRequired;
  }

  showFieldItems() {
    this.isExpanded = true
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
    onDocumentClick(event:any) {

      event.stopPropagation();

      var el = this.element.nativeElement.querySelector('.select');
      var arrowElement = this.element.nativeElement.querySelector('.select-arrow');

      if (!el.contains(event.target) && !arrowElement.contains(event.target)) {
        this.isExpanded = false
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


}
