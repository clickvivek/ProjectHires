import { Component, Input, Output, EventEmitter, HostListener, SimpleChanges } from '@angular/core';
import { JobOpeningService } from 'src/app/api';
import _ from 'underscore';

@Component({
  selector: 'profile-job-posting-history',
  templateUrl: './profile-job-posting-history.component.html',
  styleUrls: ['./profile-job-posting-history.component.scss']
})
export class ProfileJobPostingHistoryComponent {

  @Input() consultancyUserId
  @Input() profileId

  @Output() outParams = new EventEmitter();

  isJobLoaded:boolean = false;
  isJobAvailable:boolean = false;
  isError:boolean = false;
  error:string = ""

  initialDataList:any = [];
  filteredDataList: any[] = [];

  ItemStartIndex:any = 0;
  ItemEndIndex:any = 24;
  itemLimit:any = 25;
  totalItems:any;

  searchData:string = ""

  isJobSheet:boolean = true;
  selectedJob:any;
  selectedJobId:number = -1;

  isMobile:boolean = false;
  
  constructor(
    private jobOpeningService: JobOpeningService
  ) {

  }

  getIndexParams(event:any){
    this.ItemStartIndex = event.ItemStartIndex;
    this.ItemEndIndex = event.ItemEndIndex;

    if(!this.isMobile){
      this.selectedJobId = this.filteredDataList[this.ItemStartIndex].id;
      this.selectedJob = this.filteredDataList[this.ItemStartIndex];
    }

  }

  handleJobSheet(event) {
    this.isJobSheet = event
  }

  handleSearch(event:any) {
    this.searchData = event
    this.filterData()
  }

  filterData() {
    
    this.filteredDataList = this.initialDataList.filter((candidate) =>
      this.isMatch(candidate, this.searchData)
    );

  }

  isMatch(candidate, term): boolean {
    term = term.toLowerCase();
    return (
      candidate.jobOpeningName?.toLowerCase().includes(term) ||
      candidate.jobLocation?.toLowerCase().includes(term)
    );
  }

  handleSelectedJob(job) {
    this.selectedJob = job
    this.selectedJobId = this.selectedJob.jobOpeningId;
    this.isJobSheet = true
  }

  @HostListener('window:resize', ['$event'])
    onResize(event:any){
      if(event.target.innerWidth >= 768)
      this.isJobSheet = true;
  }

  ngOnInit() {
    this.fetchJobs();
  }

  ngOnChanges(changes: SimpleChanges) {
    this.fetchJobs();
  }

  fetchJobs() {
    if (this.profileId) {
        this.jobOpeningService.apiJobOpeningSearchJobOpeningsGet(
          undefined, undefined, undefined, undefined, undefined, undefined, undefined, undefined, undefined, undefined, undefined, undefined,
          undefined,
          this.profileId
        ).subscribe({
          next: (res: any) => {
            const rawData = Array.isArray(res) ? res : (res?.value || []);
            this.initialDataList = rawData;
            this.filteredDataList = this.initialDataList;
            this.isJobLoaded = true;

            this.totalItems = this.filteredDataList.length;

            if (this.totalItems === 0) {
              this.isJobAvailable = false;
              this.outParams.emit(false);
            }
            else {
              this.isJobAvailable = true;
              this.outParams.emit(true);
              if (this.totalItems > this.itemLimit) {
                this.ItemEndIndex = this.itemLimit;
              }
              else {
                this.ItemEndIndex = this.totalItems;
              }

              if (this.initialDataList && this.initialDataList.length > 0) {
                this.selectedJob = this.initialDataList[0];
                this.selectedJobId = this.initialDataList[0].jobOpeningId;
              }
            }

          },
          error: (error: any) => {
            this.isJobLoaded = true;
            this.isJobAvailable = false;
            this.isError = true;
            this.error = "Some error occured";
            this.outParams.emit(false);
          }
        });
    }
  }

}
