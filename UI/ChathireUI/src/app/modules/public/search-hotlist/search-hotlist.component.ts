import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute, Params } from '@angular/router';
import { filter } from 'rxjs/operators';

import { MatDialog } from '@angular/material/dialog';
import { picUrl, defaultProfilePic } from 'src/app/data/various';

import { CandidateProfileService } from 'src/app/api/api/candidate-profile.service';
import { CommonService } from 'src/app/api';
import { SharedService } from 'src/app/modules/shared/services/shared.service';
import { filterexperienceLevel } from 'src/app/data/filter-data';

import { AuthService } from 'src/app/core/auth/auth.service';

import _ from 'underscore';

@Component({
  selector: 'app-search-hotlist',
  templateUrl: './search-hotlist.component.html',
  styleUrls: ['./search-hotlist.component.scss']
})
export class SearchHotlistComponent implements OnInit {

  isLoaded:boolean = true;
  isDataAvailable:boolean = false;

  isError:boolean = false;
  error: string = ""
  
  ItemStartIndex:any = 0;
  ItemEndIndex:any = 9;
  itemLimit:any = 10;
  totalItems: any;
  
  initialHotListData:any[] = [];
  filteredHotListData: any[] = [];
  
  selectAvailabilityList: any[] = [];
  visaList: any[] = [];

  skill: string = "";
  location: string = "";

  skillId:any;
  cityId: any;
  stateId: any;
  visasId: any = [];
  expIds: any = [];

  previousQueryParams: Params = {}

  isHotListLoaded:boolean = false;
  isHotListAvailable:boolean = false;
  isParamsAvailable:boolean = false;

  isActiveCandidate:any;

  totalExp:any;

  chatUser:any

  constructor(
	  private _router: Router,
	  private route: ActivatedRoute,
    public dialog: MatDialog,
    private commonService: CommonService,
    private candidateProfileService: CandidateProfileService,
    private authService: AuthService,
	  private sharedService:SharedService
  ) {

    this._router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      
      // Get the current query parameters
      const params = this.route.snapshot.queryParams;

      // Check if query parameters have changed
      if (JSON.stringify(params) !== JSON.stringify(this.previousQueryParams)) {
        // Update the previous query parameters
        this.previousQueryParams = params;

        this.skill = params['skill'];
        this.location = params['location'];
        
        if (!_.isUndefined(params['cid'])) {
          this.cityId = params['cid'];
        }

        if (!_.isUndefined(params['cid'])) {
          this.stateId = params['stid'];
        }

        if (!_.isUndefined(params['visas'])) {
          this.visasId =params['visas']?.split(',');
        }

        if (!_.isUndefined(params['exp'])) {
          this.expIds =params['exp']?.split(',');
        }
        else {
          this.expIds = []
        }

        this.handleHotlistSearch(this.skill, undefined, this.cityId, this.stateId, this.visasId, this.expIds)

      }
    });


  }

  getIndexParams(event){
    this.ItemStartIndex = event.ItemStartIndex;
    this.ItemEndIndex = event.ItemEndIndex;
    this.itemLimit = event.itemLimit;
  }

  getSkills(skills){
    return skills.split(',');
  }

  getParams(event){
    this.isActiveCandidate = event.active;
    this.totalExp = event.experience;
  }

  getVisa(id){
    var name;
    _.some(this.visaList, (item) => {
      if(item.id == id)
        name = item.name;
    });
    return name;
  }

  getRelocation(location) {
    if (!location || !Array.isArray(location)) return ''
    let newArray = location.map(item => {
      if (typeof item === 'string') {
        let parts = item.split('-')
        let city = parts[0] ? parts[0].trim() : ''
        let state = parts[1] ? parts[1].trim() : ''
        return (city && state) ? `${city}, ${state}` : (city || state)
      } else if (item && typeof item === 'object') {
        let rawCity = item.cityName || item.name || ''
        let parts = rawCity.split('-')
        let city = parts[0] ? parts[0].trim() : ''
        let state = (item.stateCode || (parts[1] ? parts[1].trim() : '') || item.stateName || '').trim()
        return (city && state) ? `${city}, ${state}` : (city || state)
      }
      return item
    })
    let uniqueArray = newArray.filter((value, index, self) => value && self.indexOf(value) === index)
    return uniqueArray.join('; ')
  }

  getProfilePic(url) {
    if(url)
      return `${picUrl}${url}`
    else
      return defaultProfilePic
  }

  isSkills(item) {
    return _.isEmpty(item) ? false : true
  }

  onVisaFilter(event) {
    if (!_.isEmpty(event)) {
      this._router.navigate(['/search-hotlist'], { queryParamsHandling: 'merge', queryParams: { visas: event.join() } });
    }
    else {
      const currentParams = { ...this.route.snapshot.queryParams };
      delete currentParams['visas'];
      this._router.navigate(['/search-hotlist'], { queryParams: currentParams });
    }
  }

  onExpFilter(event) {
    if (!_.isEmpty(event)) {
      this._router.navigate(['/search-hotlist'], { queryParamsHandling: 'merge', queryParams: { exp: event.join() } });
    }
    else {
      const currentParams = { ...this.route.snapshot.queryParams };
      delete currentParams['exp'];
      this._router.navigate(['/search-hotlist'], { queryParams: currentParams });
    }
  }

  handleContact() {

    if(this.authService.isLoggedIn()) {
      
    }
    else {
      this._router.navigate(['/login']);
    }

  }

  handleHotlistSearch(skill, undefined, cityArr, stateArr, visaArr, expArr) {

    const searchStrings = [skill]

    const cityIds = [cityArr];
    const stateIds = [stateArr];

    let startYearsOfExpInput:any = undefined;
    let endYearsOfExpInput: any = undefined;

    let filteredExps: Array<any> = [];

    expArr.forEach(item => {
      let filteredObject = filterexperienceLevel.filter(obj => obj.id === parseInt(item));
      let data = {
        start: filteredObject[0].start,
        end: filteredObject[0].end
      }
      filteredExps.push(data)
    });

    if (!_.isEmpty(filteredExps)) {
      let minStartValue = Number.POSITIVE_INFINITY;
      let maxEndValue = Number.NEGATIVE_INFINITY;
      for (const item of filteredExps) {
        minStartValue = Math.min(minStartValue, item.start);
        maxEndValue = Math.max(maxEndValue, item.end);
      }
      startYearsOfExpInput = minStartValue
      endYearsOfExpInput = maxEndValue
    }

    this.isLoaded = false;

    //passed undefined for city and stateid as of now ( untill api gets fixed)
    this.candidateProfileService.apiCandidateProfileSearchCandidateProfilesGet(searchStrings, undefined, cityIds, stateIds, visaArr, startYearsOfExpInput, endYearsOfExpInput).subscribe({
      next: (res: any) => {
        
        this.isLoaded = true
        this.isDataAvailable = true
        this.isError = false
        
        this.initialHotListData = res
        this.filteredHotListData = this.initialHotListData

        this.totalItems = this.filteredHotListData.length;

        if (this.totalItems > this.itemLimit) {
          this.ItemEndIndex = this.itemLimit;
        }
        else {
          this.ItemEndIndex = this.totalItems;
        }

      },
      error: (error: any) => { 
        this.isError = true
        this.isLoaded = true
        this.isDataAvailable = true
        this.error = 'Some error occured';
      }
    })

  }

  ngOnInit() {

    this.commonService.apiCommonVisaGet().subscribe({
      next: (res : any) => {
        this.visaList = res.value
      },
      error: (error:any) => { }
    })

    this.commonService.apiCommonCandidateAvailabilityGet().subscribe({
      next: (res: any) => {
        this.selectAvailabilityList = res.value
      },
      error: (error:any) => { }
    })

    
  }

}
