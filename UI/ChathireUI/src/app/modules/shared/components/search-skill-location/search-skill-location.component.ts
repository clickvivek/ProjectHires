import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, ViewChild, ElementRef, HostListener, Renderer2, OnChanges, SimpleChanges } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute, Params } from '@angular/router';
import { NgForm } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

import { CommonService } from 'src/app/api/api/common.service'

const STATIC_STATES: any[] = [
  {"id":1,"stateCode":"AL","stateName":"Alabama","countryName":"USA"},
  {"id":2,"stateCode":"AK","stateName":"Alaska","countryName":"USA"},
  {"id":3,"stateCode":"AZ","stateName":"Arizona","countryName":"USA"},
  {"id":4,"stateCode":"AR","stateName":"Arkansas","countryName":"USA"},
  {"id":5,"stateCode":"CA","stateName":"California","countryName":"USA"},
  {"id":6,"stateCode":"CO","stateName":"Colorado","countryName":"USA"},
  {"id":7,"stateCode":"CT","stateName":"Connecticut","countryName":"USA"},
  {"id":8,"stateCode":"DE","stateName":"Delaware","countryName":"USA"},
  {"id":9,"stateCode":"DC","stateName":"District of Columbia","countryName":"USA"},
  {"id":10,"stateCode":"FL","stateName":"Florida","countryName":"USA"},
  {"id":11,"stateCode":"GA","stateName":"Georgia","countryName":"USA"},
  {"id":12,"stateCode":"HI","stateName":"Hawaii","countryName":"USA"},
  {"id":13,"stateCode":"ID","stateName":"Idaho","countryName":"USA"},
  {"id":14,"stateCode":"IL","stateName":"Illinois","countryName":"USA"},
  {"id":15,"stateCode":"IN","stateName":"Indiana","countryName":"USA"},
  {"id":16,"stateCode":"IA","stateName":"Iowa","countryName":"USA"},
  {"id":17,"stateCode":"KS","stateName":"Kansas","countryName":"USA"},
  {"id":18,"stateCode":"KY","stateName":"Kentucky","countryName":"USA"},
  {"id":19,"stateCode":"LA","stateName":"Louisiana","countryName":"USA"},
  {"id":20,"stateCode":"ME","stateName":"Maine","countryName":"USA"},
  {"id":21,"stateCode":"MD","stateName":"Maryland","countryName":"USA"},
  {"id":22,"stateCode":"MA","stateName":"Massachusetts","countryName":"USA"},
  {"id":23,"stateCode":"MI","stateName":"Michigan","countryName":"USA"},
  {"id":24,"stateCode":"MN","stateName":"Minnesota","countryName":"USA"},
  {"id":25,"stateCode":"MS","stateName":"Mississippi","countryName":"USA"},
  {"id":26,"stateCode":"MO","stateName":"Missouri","countryName":"USA"},
  {"id":27,"stateCode":"MT","stateName":"Montana","countryName":"USA"},
  {"id":28,"stateCode":"NE","stateName":"Nebraska","countryName":"USA"},
  {"id":29,"stateCode":"NV","stateName":"Nevada","countryName":"USA"},
  {"id":30,"stateCode":"NH","stateName":"New Hampshire","countryName":"USA"},
  {"id":31,"stateCode":"NJ","stateName":"New Jersey","countryName":"USA"},
  {"id":32,"stateCode":"NM","stateName":"New Mexico","countryName":"USA"},
  {"id":33,"stateCode":"NY","stateName":"New York","countryName":"USA"},
  {"id":34,"stateCode":"NC","stateName":"North Carolina","countryName":"USA"},
  {"id":35,"stateCode":"ND","stateName":"North Dakota","countryName":"USA"},
  {"id":36,"stateCode":"OH","stateName":"Ohio","countryName":"USA"},
  {"id":37,"stateCode":"OK","stateName":"Oklahoma","countryName":"USA"},
  {"id":38,"stateCode":"OR","stateName":"Oregon","countryName":"USA"},
  {"id":39,"stateCode":"PA","stateName":"Pennsylvania","countryName":"USA"},
  {"id":40,"stateCode":"PR","stateName":"Puerto Rico","countryName":"USA"},
  {"id":41,"stateCode":"RI","stateName":"Rhode Island","countryName":"USA"},
  {"id":42,"stateCode":"SC","stateName":"South Carolina","countryName":"USA"},
  {"id":43,"stateCode":"SD","stateName":"South Dakota","countryName":"USA"},
  {"id":44,"stateCode":"TN","stateName":"Tennessee","countryName":"USA"},
  {"id":45,"stateCode":"TX","stateName":"Texas","countryName":"USA"},
  {"id":46,"stateCode":"UT","stateName":"Utah","countryName":"USA"},
  {"id":47,"stateCode":"VT","stateName":"Vermont","countryName":"USA"},
  {"id":48,"stateCode":"VA","stateName":"Virginia","countryName":"USA"},
  {"id":49,"stateCode":"WA","stateName":"Washington","countryName":"USA"},
  {"id":50,"stateCode":"WV","stateName":"West Virginia","countryName":"USA"},
  {"id":51,"stateCode":"WI","stateName":"Wisconsin","countryName":"USA"},
  {"id":52,"stateCode":"WY","stateName":"Wyoming","countryName":"USA"},
  {"id":53,"stateCode":"AB","stateName":"Alberta","countryName":"Canada"},
  {"id":54,"stateCode":"BC","stateName":"British Columbia","countryName":"Canada"},
  {"id":55,"stateCode":"MB","stateName":"Manitoba","countryName":"Canada"},
  {"id":56,"stateCode":"NB","stateName":"New Brunswick","countryName":"Canada"},
  {"id":57,"stateCode":"NL","stateName":"Newfoundland and Labrador","countryName":"Canada"},
  {"id":58,"stateCode":"NS","stateName":"Nova Scotia","countryName":"Canada"},
  {"id":59,"stateCode":"NT","stateName":"Northwest Territories","countryName":"Canada"},
  {"id":60,"stateCode":"NU","stateName":"Nunavut","countryName":"Canada"},
  {"id":61,"stateCode":"ON","stateName":"Ontario","countryName":"Canada"},
  {"id":62,"stateCode":"PE","stateName":"Prince Edward Island","countryName":"Canada"},
  {"id":63,"stateCode":"QC","stateName":"Quebec","countryName":"Canada"},
  {"id":64,"stateCode":"SK","stateName":"Saskatchewan","countryName":"Canada"},
  {"id":65,"stateCode":"YT","stateName":"Yukon","countryName":"Canada"}
];

@Component({
  selector: 'search-skill-location',
  templateUrl: './search-skill-location.component.html',
  styleUrls: ['./search-skill-location.component.scss']
})
export class SearchSkillLocationComponent implements OnInit, OnChanges, OnDestroy {

  @Input() skill: any = ""
  @Input() location: any = "";
  @Input() cityId: any = null;
  @Input() stateId: any = null;
  @Input() type: any;
  @Input() page: string = "";

  @Input() isRemoteOnly: boolean = false;
  @Output() isRemoteOnlyChange = new EventEmitter<boolean>();

  @Input() radiusMiles: number = 50;
  @Output() radiusMilesChange = new EventEmitter<number>();

  formData = {
    selectedSkill: this.skill,
    selectedLocation: this.location,
  }
  
  skillList:any[] = [];
  locationList:any[] = [];

  // Static cached states for instant (0ms) state lookup
  private cachedStates: any[] = STATIC_STATES;
  private locationSearchSubject: Subject<string> = new Subject<string>();
  private locationSearchSub: Subscription | null = null;

  isSkillExpanded: boolean = false;
  isLocationExpanded: boolean = false;

  charLength: number = 2
  
  @ViewChild ('searchSkill', {static: true}) skillInputElement: ElementRef;
  @ViewChild('searchLocation', { static: true }) locationInputElement: ElementRef;
  @ViewChild('searchSkillLocationForm', {static: false}) searchSkillLocationForm: NgForm;

  get skillElement(): any {
    return this.skillInputElement?.nativeElement;
  }

  get locationElement(): any {
    return this.locationInputElement?.nativeElement;
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
      this.isLocationExpanded = false;
    }
  }

  submitSkillLocationForm() {
    if (this.searchSkillLocationForm.valid) {
      this.isSearching = true;
      this.isSkillExpanded = false;
      this.isLocationExpanded = false;
      const targetUrl = this.type === 'jobs' ? '/search-jobs' : '/search-hotlist';
      const timestamp = new Date().getTime();

      const queryParams: any = {
        skill: this.skill || '',
        location: this.isRemoteOnly ? '' : (this.location || ''),
        cid: this.isRemoteOnly ? null : (this.cityId || null),
        sid: this.isRemoteOnly ? null : (this.stateId || null),
        radius: (this.type === 'jobs' && !this.isRemoteOnly && this.radiusMiles) ? this.radiusMiles : null,
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
    if (this.isSkillExpanded && this.skillElement && !this.skillElement.contains(event.target)) {
      this.isSkillExpanded = false;
    }

    if (this.isLocationExpanded && this.locationElement && !this.locationElement.contains(event.target)) {
      this.isLocationExpanded = false;
    }
  }

  handleSearchSkill() {
    if(this.skill?.length > this.charLength) {
      this.isSkillExpanded = true;
      
      this.commonService.apiCommonSkillsGet(this.skill).subscribe({
        next: (res : any) => {
          this.skillList = res.value
          if (this.skillList && this.skillList.length > 0) {
            this.isSkillExpanded = true;
          }
        },
        error: () => {}
      })
    }
    else {
      this.isSkillExpanded = false;
      this.skillList = []
    }
  }

  private filterMatchingStates(term: string): any[] {
    if (!term || !this.cachedStates?.length) return [];
    const lower = term.trim().toLowerCase();
    if (lower.length === 0) return [];

    return this.cachedStates
      .filter((s: any) => {
        const name = (s.stateName || '').toLowerCase();
        const code = (s.stateCode || '').toLowerCase();
        return name.startsWith(lower) || (lower.length >= 2 && code.startsWith(lower));
      })
      .slice(0, 5)
      .map((s: any) => ({
        id: s.id,
        idState: s.id,
        city1: s.stateName,
        stateName: s.stateName,
        stateCode: s.stateCode,
        countryName: s.countryName || 'USA',
        isState: true
      }));
  }

  handleSearchLocation() {
    const rawVal = this.location ? this.location.trim() : '';

    if (rawVal.length > this.charLength) {
      // If the user is entering a zipcode (only digits), wait until 5 digits
      const isZipCode = /^\d+$/.test(rawVal);
      if (isZipCode && rawVal.length < 5) {
        this.isLocationExpanded = false;
        this.locationList = [];
        return;
      }

      // INSTANT (0ms): Look up matching state(s) from cached list immediately
      const instantStates = this.filterMatchingStates(rawVal);
      if (instantStates.length > 0) {
        const existingCities = this.locationList.filter(item => !item.isState);
        this.locationList = [...instantStates, ...existingCities];
        this.isLocationExpanded = true;
      }

      // Queue the debounced remote query for cities
      this.locationSearchSubject.next(rawVal);
    }
    else {
      this.isLocationExpanded = false;
      this.locationList = [];
      this.cityId = null;
      this.stateId = null;
    }
  }

  private fetchRemoteCities(term: string) {
    this.commonService.apiCommonCityGet(term).subscribe({
      next: (res: any) => {
        const returned = res?.value || [];
        const instantStates = this.filterMatchingStates(term);

        // Merge: state matches first, then regular cities (avoiding duplicates)
        const returnedStates = returned.filter((x: any) => x.isState);
        const returnedCities = returned.filter((x: any) => !x.isState);

        const statesMap = new Map<number, any>();
        instantStates.forEach((s: any) => statesMap.set(s.idState, s));
        returnedStates.forEach((s: any) => statesMap.set(s.idState, s));

        this.locationList = [...Array.from(statesMap.values()), ...returnedCities];

        if (this.locationList.length > 0 && this.location?.length > this.charLength) {
          this.isLocationExpanded = true;
        }
      },
      error: () => {}
    });
  }

  getSkill(name, id) {
    this.skill = name;
    this.isSkillExpanded = false;
  }

  formatLocation(item: any): string {
    if (!item) return '';
    if (item.isState) {
      const stateName = item.stateName || item.city1 || '';
      return `${stateName} (State, USA)`;
    }
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
    this.isLocationExpanded = false;
  }

  ngOnInit() {
    // Check if remote is in active queryParams
    const currentParams = this.route.snapshot.queryParams;
    if (currentParams && (currentParams['remote'] === 'true' || currentParams['remote'] === true)) {
      this.isRemoteOnly = true;
      this.location = '';
      this.cityId = null;
      this.stateId = null;
    } else if (currentParams) {
      if (currentParams['sid']) {
        this.stateId = currentParams['sid'];
      }
      if (currentParams['radius']) {
        const parsed = parseInt(currentParams['radius'], 10);
        if (!isNaN(parsed) && [50, 100, 150, 200].includes(parsed)) {
          this.radiusMiles = parsed;
        }
      }
    }

    // Refresh states in background
    this.commonService.apiCommonStateGet().subscribe({
      next: (res: any) => {
        if (res && res.value && res.value.length > 0) {
          this.cachedStates = res.value;
        }
      },
      error: () => {}
    });

    // Set up debounced remote city search stream
    this.locationSearchSub = this.locationSearchSubject.pipe(
      debounceTime(250),
      distinctUntilChanged()
    ).subscribe((term: string) => {
      this.fetchRemoteCities(term);
    });
  }

  ngOnDestroy() {
    if (this.locationSearchSub) {
      this.locationSearchSub.unsubscribe();
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes && changes['skill'] && changes['skill'].currentValue === '' && !changes['skill'].firstChange) {
      this.searchSkillLocationForm?.resetForm();
    }

    if (this.isRemoteOnly) {
      this.location = '';
      this.cityId = null;
      this.stateId = null;
    }
  }

}
