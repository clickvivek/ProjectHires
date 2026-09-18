import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, OnChanges, SimpleChanges, ElementRef, HostListener } from '@angular/core';
import { ControlContainer, NgForm } from '@angular/forms';
import { picUrl, defaultProfilePic } from 'src/app/data/various';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import _ from 'underscore';

@Component({
  selector: 'app-search-field',
  templateUrl: './search-field.component.html',
  styleUrls: ['./search-field.component.scss'],
  viewProviders: [ { provide: ControlContainer, useExisting: NgForm } ]
})
export class SearchFieldComponent implements OnInit, OnChanges, OnDestroy {

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
  @Input() isEdit:any;
  @Input() editValue:any;

  @Input() fieldDisabled:boolean = false;

  @Input() showCreateOption: boolean = false;

  picBaseUrl: string = picUrl;
  imgErrorMap: { [key: string]: boolean } = {};

  selectedItem:any = null;
  isItemSelected:boolean = false;
  isExpanded:boolean = false;

  private searchSubject = new Subject<string>();
  private searchSubscription: Subscription;

  @Output() queryChange = new EventEmitter();
  @Output() inputChange = new EventEmitter();
  @Output() createNewClick = new EventEmitter();

  isCompanyItem(item: any): boolean {
    return !!(item && (item.logo !== undefined || item.domainname !== undefined || item.website !== undefined || this.fieldName === 'consultancyId' || this.fieldName === 'companyId'));
  }

  getLogoUrl(logo: string | null | undefined): string {
    if (!logo) {
      return '';
    }
    if (logo.startsWith('http://') || logo.startsWith('https://') || logo.startsWith('data:image')) {
      return logo;
    }
    return `${this.picBaseUrl}${logo}`;
  }

  getInitials(name: string): string {
    if (!name) return 'C';
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0].charAt(0) + parts[1].charAt(0)).toUpperCase();
    }
    return name.slice(0, 2).toUpperCase();
  }

  onLogoError(key: any) {
    this.imgErrorMap[key] = true;
  }

  hasLogoError(key: any): boolean {
    return !!this.imgErrorMap[key];
  }

  triggerCreateNew(event: Event) {
    if (event) {
      event.stopPropagation();
    }
    this.isExpanded = false;
    this.createNewClick.emit();
  }

  constructor(
    private element: ElementRef
  ) { }

  isFieldRequired(){
    return this.fieldRequired
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

  getSelectedItem(item: any) {
    const finalData = this.getItemData(item);
    this.fieldModel = finalData;
    this.inputChange.emit(item);
    this.selectedItem = item;
    this.isExpanded = false;
  }

  clearList() {
    this.fieldModel = "";
    this.selectedItem = null;
  }

  handleModelChange(){
    if(this.fieldModel && this.fieldModel.length > 1) {
      this.isExpanded = true;
      this.searchSubject.next(this.fieldModel.trim());
    }
    else {
      this.isExpanded = false;
    }
  }

  ngOnInit() {
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(250),
      distinctUntilChanged()
    ).subscribe((term: string) => {
      this.queryChange.emit(term);
    });
  }

  ngOnDestroy() {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    if (!_.isEmpty(this.editValue) && this.isEdit && (changes['editValue'] || !this.fieldModel)) {
      this.fieldModel = this.getItemData(this.editValue);
      this.selectedItem = this.editValue;
      this.inputChange.emit(this.editValue);
    }
  }



}
