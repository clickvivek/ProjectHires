import { Component, OnInit, Input, Output, EventEmitter, ViewChild, ElementRef, HostListener, Renderer2, OnChanges, SimpleChanges } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute, Params } from '@angular/router';
import { NgForm } from '@angular/forms';

import { CommonService } from 'src/app/api/api/common.service'

@Component({
  selector: 'search-skill-location',
  templateUrl: './search-skill-location.component.html',
  styleUrls: ['./search-skill-location.component.scss']
})
export class SearchSkillLocationComponent implements OnInit, OnChanges {

  @Input() skill: any = ""
  @Input() location: any = "";
  @Input() cityId: any = null;
  @Input() stateId: any = null;
  @Input() type: any;
  @Input() page: string = "";

  @Input() isRemoteOnly: boolean = false;
  @Output() isRemoteOnlyChange = new EventEmitter<boolean>();

  formData = {
    selectedSkill: this.skill,
    selectedLocation: this.location,
  }
  
  skillList:any[] = [];
  locationList:any[] = [];

  charLength: number = 2
  
  @ViewChild ('searchSkill', {static: true}) skillInputElement: ElementRef;
  @ViewChild('searchLocation', { static: true }) locationInputElement: ElementRef;
  @ViewChild('skillDropdown', { static: false }) skillDropdownRef: ElementRef;
  @ViewChild('locationDropdown', { static: false }) locationDropdownRef: ElementRef;
  @ViewChild('searchSkillLocationForm', {static: false}) searchSkillLocationForm: NgForm;

  get skillElement(): any {
    return this.skillInputElement?.nativeElement;
  }

  get locationElement(): any {
    return this.locationInputElement?.nativeElement;
  }

  get skillSearchDropdown(): any {
    return this.skillDropdownRef?.nativeElement;
  }

  get locationSearchDropdown(): any {
    return this.locationDropdownRef?.nativeElement;
  }

  constructor(
    private commonService: CommonService,
    public router: Router,
    private route: ActivatedRoute,
    public renderer:Renderer2
    ) {
    
  }

  isSearching: boolean = false;

  onRemoteChange() {
    this.isRemoteOnlyChange.emit(this.isRemoteOnly);
    if (this.isRemoteOnly) {
      this.location = '';
      this.cityId = null;
      this.stateId = null;
      this.locationList = [];
      if (this.locationSearchDropdown) {
        this.renderer.removeClass(this.locationSearchDropdown, 'show');
      }
    }
  }

  submitSkillLocationForm() {
    if (this.searchSkillLocationForm.valid) {
      this.isSearching = true;
      const targetUrl = this.type === 'jobs' ? '/search-jobs' : '/search-hotlist';
      const timestamp = new Date().getTime();

      const queryParams: any = {
        skill: this.skill || '',
        location: this.isRemoteOnly ? '' : (this.location || ''),
        cid: this.isRemoteOnly ? null : (this.cityId || null),
        remote: this.isRemoteOnly ? 'true' : null,
        _t: timestamp
      };

      this.router.navigate([targetUrl], {
        queryParams: queryParams
      }).then(() => {
        setTimeout(() => {
          this.isSearching = false;
        }, 1000);
      }).catch(() => {
        setTimeout(() => {
          this.isSearching = false;
        }, 1000);
      });
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: any) {
    if (this.skillSearchDropdown && this.skillSearchDropdown.classList?.contains('show')) {
      if (this.skillElement && !this.skillElement.contains(event.target)) {
        this.renderer.removeClass(this.skillSearchDropdown, 'show');
      }
    }

    if (this.locationSearchDropdown && this.locationSearchDropdown.classList?.contains('show')) {
      if (this.locationElement && !this.locationElement.contains(event.target)) {
        this.renderer.removeClass(this.locationSearchDropdown, 'show');
      }
    }
  }


  handleSearchSkill() {

    if(this.skill?.length > this.charLength) {
      this.renderer.addClass(this.skillSearchDropdown,'show');
      
      this.commonService.apiCommonSkillsGet(this.skill).subscribe({
        next: (res : any) => {
          this.skillList = res.value
        },
        error: (error:any) => {

        }
      })

      
    }
    else {
      this.renderer.removeClass(this.skillSearchDropdown,'show');
      this.skillList = []
    }

  }

  handleSearchLocation() {
    
    if (this.location?.length > this.charLength) {
      
      // If the user is entering a zipcode (only digits), wait until 5 digits
      const isZipCode = /^\d+$/.test(this.location);
      if (isZipCode && this.location.length < 5) {
        this.renderer.removeClass(this.locationSearchDropdown,'show');
        this.locationList = [];
        return;
      }

      this.renderer.addClass(this.locationSearchDropdown,'show');
      this.commonService.apiCommonCityGet(this.location).subscribe({
        next: (res : any) => {
          this.locationList = res.value
        },
        error: (error:any) => {

        }
      })
    }
    else {
      this.renderer.removeClass(this.locationSearchDropdown,'show');
      this.locationList = []
      this.location = ""
      this.cityId = null
      this.stateId = null
    }

  }

  getSkill(name, id) {
    this.skill = name;
    if (this.skillSearchDropdown) {
      this.renderer.removeClass(this.skillSearchDropdown, 'show');
    }
  }

  formatLocation(item: any): string {
    if (!item) return '';
    const state = item.stateName || item.stateCode || '';
    const parts = [item.city1, state, item.zip, item.countryName].filter(
      p => !!p && String(p).trim().length > 0
    );
    return parts.join(', ');
  }

  getLocation(item: any) {
    this.location = this.formatLocation(item);
    this.cityId = item?.id;
    this.stateId = item?.idState;
    if (this.locationSearchDropdown) {
      this.renderer.removeClass(this.locationSearchDropdown, 'show');
    }
  }

  ngOnInit() {
    // Check if remote is in active queryParams
    const currentParams = this.route.snapshot.queryParams;
    if (currentParams && (currentParams['remote'] === 'true' || currentParams['remote'] === true)) {
      this.isRemoteOnly = true;
      this.location = '';
      this.cityId = null;
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes && changes['skill'] && changes['skill'].currentValue === '' && !changes['skill'].firstChange) {
      this.searchSkillLocationForm?.resetForm();
    }

    if (this.isRemoteOnly) {
      this.location = '';
      this.cityId = null;
    }
  }

}
