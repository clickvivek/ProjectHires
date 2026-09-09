import { Component, OnInit, HostListener } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute, Params } from '@angular/router';
import { filter } from 'rxjs/operators';

import { MatDialog } from '@angular/material/dialog';
import { MatDialogRef } from "@angular/material/dialog";

import { JobOpeningService } from 'src/app/api/api/job-opening.service';

import { filterexperienceLevel } from 'src/app/data/filter-data';

import * as moment from 'moment';
import _ from 'underscore';
import { SharedService } from 'src/app/modules/shared/services/shared.service';

@Component({
  selector: 'app-searchjobs',
  templateUrl: './searchjobs.component.html',
  styleUrls: ['./searchjobs.component.scss'],
  providers: [
    { provide: MatDialogRef, useValue: {} },
  ]
})
export class SearchjobsComponent implements OnInit {

  isLoaded:boolean = true;
  isDataAvailable:boolean = false;

  isError:boolean = false;
  error: string = ""

  disabled: boolean = false;
  
  isJobsLoaded:boolean = false;

  skill: string = "";
  location: string = "";
  cityId: any = null;
  visasId: any = [];
  wmIds: any = [];
  expIds: any = [];
  dateString: string = "";

  skillList:any[] = [];
  locationList:any[] = [];

  jobData:any[] = [];
  jobCount:any;

  selectedJobId:number = -1;
  selectedJob:any;

  ItemStartIndex:any = 0;
  ItemEndIndex:any = 24;
  itemLimit:any = 25;
  totalItems:any;

  isMobile:boolean = false;
  isJobName:boolean = true;

  filterParam: any;
  
  previousQueryParams: Params = {}

  isJobSheet:boolean = true

  constructor(
  private router: Router,
  private route: ActivatedRoute,
  public dialog: MatDialog,
  private jobOpeningService: JobOpeningService,
  private sharedService: SharedService
  ) {

    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      
      const params = this.route.snapshot.queryParams;

      if (JSON.stringify(params) !== JSON.stringify(this.previousQueryParams) && !_.isEmpty(params)) {
        
        this.previousQueryParams = params;

        this.skill = params['skill'];
        this.location = params['location'];

         if (!_.isUndefined(params['cid'])) {
          this.cityId =params['cid'];
        }
         else {
           this.cityId = null
        }

        if (!_.isUndefined(params['visas'])) {
          this.visasId =params['visas']?.split(',');
        }
        else {
          this.visasId = []
        }

        if (!_.isUndefined(params['wm'])) {
          this.wmIds =params['wm']?.split(',');
        }
        else {
          this.wmIds = []
        }

        if (!_.isUndefined(params['exp'])) {
          this.expIds =params['exp']?.split(',');
        }
        else {
          this.expIds = []
        }

        if (!_.isUndefined(params['date'])) {
          this.dateString =params['date'];
        }
        else {
          this.dateString = ""
        }
        this.handleJobOpenings(this.skill, this.cityId, this.visasId, this.wmIds, this.expIds, this.dateString)

      }
      else {
        this.isDataAvailable = false
        this.skill = ""
        this.location = ""
      }
      
    });

  }

  handleJobSheet(event) {
    this.isJobSheet = event
  }

  getDateInMMDDYYYYFormat(daysToAdd) {
    const currentDate = moment(); 
    const futureDate = currentDate.add(daysToAdd, 'days');
    return futureDate.format('MM/DD/YYYY');
 }

  handleJobOpenings(skill, cityIdArr, visasArr, employmentTypesArr, expArr, dateString) {

    const searchStrings = [skill]
    const cityIds = [cityIdArr]

    let startYearsOfExpInput:any = undefined;
    let endYearsOfExpInput: any = undefined;
    
    let startDate:any = undefined;
    let endDate:any = undefined

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

    if (dateString) {
      startDate = moment(Date.now()).format('MM/DD/YYYY');
      endDate = this.getDateInMMDDYYYYFormat(this.dateString)
    }

    this.isLoaded = false;
    this.jobOpeningService.apiJobOpeningSearchJobOpeningsGet(searchStrings, cityIds, undefined, visasArr, employmentTypesArr, undefined, startYearsOfExpInput, endYearsOfExpInput, undefined, undefined, startDate, endDate).subscribe({
      next: (res: any) => {
                
        this.isLoaded = true
        this.isError = false

        // Handle different response structures
        let jobData: any[] | null = null;
        
        if (Array.isArray(res)) {
          // Direct array response
          jobData = res;
        } else if (res && res.value && Array.isArray(res.value)) {
          // Response wrapped in 'value' property
          jobData = res.value;
        } else if (res && res.data && Array.isArray(res.data)) {
          // Response wrapped in 'data' property
          jobData = res.data;
        } else if (res && typeof res === 'object') {
          // Check if any property contains an array
          for (const key in res) {
            if (Array.isArray(res[key])) {
              jobData = res[key];
              console.log('Found array data in property:', key);
              break;
            }
          }
        }

        console.log('Processed jobData:', jobData);
        console.log('jobData length:', jobData?.length);

        // Check if we have valid job data
        if(jobData && Array.isArray(jobData) && jobData.length > 0) {

          this.jobData = jobData
          this.isDataAvailable = true

          this.totalItems = this.jobData.length;

          if(this.sharedService.getPageToRetain()) {
            let data = this.sharedService.getPageToRetain()
            this.showJobDescription(data.job)
          }
          else {
            this.selectedJob = this.jobData[0];
            this.selectedJobId = this.jobData[0].jobOpeningId;
          }

          if (this.totalItems > this.itemLimit) {
            this.ItemEndIndex = this.itemLimit;
          }
          else {
            this.ItemEndIndex = this.totalItems;
          }
        }
        else {
          this.isDataAvailable = false
        }
  

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
        this.isDataAvailable = false
        this.error = 'Some error occured';
      }
    })
  }

  getIndexParams(event:any){
    this.ItemStartIndex = event.ItemStartIndex;
    this.ItemEndIndex = event.ItemEndIndex;

    if(!this.isMobile){
      this.selectedJobId = this.jobData[this.ItemStartIndex].id;
      this.selectedJob = this.jobData[this.ItemStartIndex];
    }

  }

  getFilterParams(event:any){
    this.filterParam = event;
  }

  isSelected(id:any){
    return this.selectedJobId == id ? 'selected' : '';
  }

  showJobDescription(job:any){
    this.isJobSheet = true
    this.selectedJobId = job.jobOpeningId;
    this.selectedJob = job;
    
  }



  @HostListener('window:resize', ['$event'])
    onResize(event:any){
      if(event.target.innerWidth >= 768)
      this.isJobSheet = true;
  }

  onVisaFilter(event) {
    if (!_.isEmpty(event)) {
      this.router.navigate(['/search-jobs'], { queryParamsHandling: 'merge', queryParams: { visas: event.join() } });
    }
    else {
      const currentParams = { ...this.route.snapshot.queryParams };
      delete currentParams['visas'];
      this.router.navigate(['/search-jobs'], { queryParams: currentParams });
    }
  }

  onWorkModeFilter(event) {
    if (!_.isEmpty(event)) {
      this.router.navigate(['/search-jobs'], { queryParamsHandling: 'merge', queryParams: { wm: event.join() } });
    }
    else {
      const currentParams = { ...this.route.snapshot.queryParams };
      delete currentParams['wm'];
      this.router.navigate(['/search-jobs'], { queryParams: currentParams });
    }
  }

  onExpFilter(event) {
    if (!_.isEmpty(event)) {
      this.router.navigate(['/search-jobs'], { queryParamsHandling: 'merge', queryParams: { exp: event.join() } });
    }
    else {
      const currentParams = { ...this.route.snapshot.queryParams };
      delete currentParams['exp'];
      this.router.navigate(['/search-jobs'], { queryParams: currentParams });
    }
  }

  onDateFilter(event) {
    if (!_.isEmpty(event)) {
      this.router.navigate(['/search-jobs'], { queryParamsHandling: 'merge', queryParams: { date: event } });
    }
    else {
      const currentParams = { ...this.route.snapshot.queryParams };
      delete currentParams['date'];
      this.router.navigate(['/search-jobs'], { queryParams: currentParams });
    }
  }

  ngOnInit() {

    if(window.innerWidth <= 991)
      this.isMobile = true;
    else
      this.isMobile = false;

  }


}
