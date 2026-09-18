import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, ElementRef, ViewChild, ChangeDetectorRef, OnChanges, SimpleChanges, HostListener } from '@angular/core';
import { ControlContainer, NgForm } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import _ from 'underscore';


@Component({
  selector: 'app-multi-search-field',
  templateUrl: './multi-search-field.component.html',
  styleUrls: ['./multi-search-field.component.scss'],
  viewProviders: [ { provide: ControlContainer, useExisting: NgForm } ]
})
export class MultiSearchFieldComponent implements OnInit, OnChanges, OnDestroy {

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: any) {
    if (!this.element.nativeElement.contains(event.target)) {
      this.isExpanded = false;
    }
  }

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
  @Input() allowCustom:boolean = false;
  @Input() customPromptText:string = 'Add';

  selectedItem:any = [];
  isExpanded:boolean = false;

  leftPadding:any = 10
  topPadding:any = 4

  forceRequired:any = "";

  private searchSubject = new Subject<string>();
  private searchSubscription: Subscription;

  @Output() queryChange = new EventEmitter();
  @Output() inputChange = new EventEmitter();

  @ViewChild('multiInputElem') multiInputElem: ElementRef;
  @ViewChild('badgeListElem') badgeListElem: ElementRef;

  constructor(
    private cdRef : ChangeDetectorRef,
    private element: ElementRef
  ) { }

  isFieldRequired(){
    return this.fieldRequired;
  }

  getItemData(item: any): string {
    if (!item) return '';
    if (!this.fieldType) return '';
    const fields = this.fieldType.split(',');
    const parts: string[] = [];
    fields.forEach((typeItem: string) => {
      const key = typeItem.trim();
      const val = item[key];
      if (val !== undefined && val !== null && String(val).trim() !== '') {
        parts.push(String(val).trim());
      }
    });
    return parts.join(', ');
  }

  handleModelChange() {
    if(this.fieldModel && this.fieldModel.length > 1) {
      this.isExpanded = true;
      this.searchSubject.next(this.fieldModel.trim());
    }
    else if (this.allowCustom && this.fieldModel && this.fieldModel.trim().length > 0) {
      this.isExpanded = true;
    }
    else {
      this.isExpanded = false;
    }
  }

  hasExactMatch(value: string): boolean {
    if (!value || !value.trim()) return false;
    const clean = value.trim().toLowerCase();
    const primaryKey = this.splitFieldType(this.fieldType);

    const inList = (this.fieldList || []).some((item: any) => {
      const v = item && item[primaryKey];
      return v && String(v).trim().toLowerCase() === clean;
    });

    const inSelected = (this.selectedItem || []).some((item: any) => {
      const v = item && item[primaryKey];
      return v && String(v).trim().toLowerCase() === clean;
    });

    return inList || inSelected;
  }

  createCustomItem(event?: Event, value?: string) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    const targetValue = value || this.fieldModel;
    if (!targetValue || !targetValue.trim()) return;
    const clean = targetValue.trim();

    const primaryKey = this.splitFieldType(this.fieldType);
    const inSelected = (this.selectedItem || []).some((item: any) => {
      const v = item && item[primaryKey];
      return v && String(v).trim().toLowerCase() === clean.toLowerCase();
    });
    if (inSelected) {
      this.fieldModel = '';
      this.isExpanded = false;
      return;
    }

    const newObj: any = {
      id: 0,
      skillId: 0,
      isUserDefined: true
    };
    newObj[primaryKey] = clean;

    this.handleSelectedItem(newObj);
  }

  handleEnterKey(event: Event) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    if (this.allowCustom && this.fieldModel && this.fieldModel.trim()) {
      const clean = this.fieldModel.trim().toLowerCase();
      const primaryKey = this.splitFieldType(this.fieldType);
      const matched = (this.fieldList || []).find((item: any) => {
        const v = item && item[primaryKey];
        return v && String(v).trim().toLowerCase() === clean;
      });

      if (matched) {
        this.handleSelectedItem(matched);
      } else {
        this.createCustomItem(event, this.fieldModel);
      }
    }
  }

  compareObj(obj1, obj2) {
    if (!obj1 || !obj2) return false;
    const id1 = obj1.id || obj1.cityId || obj1.skillId;
    const id2 = obj2.id || obj2.cityId || obj2.skillId;
    if (id1 && id2 && id1 !== 0 && id2 !== 0) {
      return id1 === id2;
    }
    const key = this.splitFieldType(this.fieldType);
    const v1 = String(obj1[key] || '').trim().toLowerCase();
    const v2 = String(obj2[key] || '').trim().toLowerCase();
    return v1 === v2;
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

    let inputElement = this.multiInputElem?.nativeElement;
    let badgeElement = this.badgeListElem?.nativeElement;

    if (!inputElement || !badgeElement) return;

    let badgeListHeight = badgeElement.clientHeight;

    inputElement.style.height = `${badgeListHeight+10}px`;

    const childElements = badgeElement.children[0]?.children;
    const lastChild = childElements ? childElements[childElements.length - 1] : null;

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
    return this.getItemData(badge);
  }

  removeBadge(item:any) {
    this.fieldModel = ""
    const newFieldType = this.fieldType.split(',')
    const key = newFieldType[0];
    const targetVal = String(item[key] || '').trim().toLowerCase();

    this.selectedItem = this.selectedItem.filter((listItem: any) => {
      const listVal = String(listItem[key] || '').trim().toLowerCase();
      return listVal !== targetVal;
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
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(250),
      distinctUntilChanged()
    ).subscribe((term: string) => {
      this.queryChange.emit(term);
    });
  }

  ngOnDestroy(): void {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
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
