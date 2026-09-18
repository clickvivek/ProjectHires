import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import _ from 'underscore';

import { JobOpeningService } from 'src/app/api';
import { SessionService } from 'src/app/core/session/session.service';

@Component({
  selector: 'app-posting-history',
  templateUrl: './posting-history.component.html',
  styleUrls: ['./posting-history.component.scss']
})
export class PostingHistoryComponent implements OnInit, OnDestroy {

  private destroy$ = new Subject<void>();

  user:any;
  profileId:any;

  isLoaded:boolean = false;
  isError:boolean = true;

  searchData:string = ""

  ItemStartIndex:any = 0;
  ItemEndIndex:any = 9;
  itemLimit:any = 10;
  totalItems: any;

  initialDataList:any = [];
  filteredDataList: any[] = [];

  error: string = "";

  constructor(
    public router: Router,
    private jobOpeningService: JobOpeningService,
    private sessionService: SessionService
  ) {
  }

  handleSearch(event:any) {
    this.searchData = event
    this.filterData()
  }

  filterData() {
    
    this.filteredDataList = this.initialDataList.filter((candidate) =>
      this.isMatch(candidate, this.searchData)
    );
    this.totalItems = this.filteredDataList.length;

  }

  isMatch(candidate, term): boolean {
    term = term.toLowerCase();
    return (
      candidate.name?.toLowerCase().includes(term) ||
      candidate.jobLocation?.toLowerCase().includes(term)
    );
  }

  getIndexParams(event){
    this.ItemStartIndex = event.ItemStartIndex;
    this.ItemEndIndex = event.ItemEndIndex;
    this.itemLimit = event.itemLimit;
  }

  isDataAvailable() {
    return this.totalItems != 0
  }

  onDeletePost(event) {
    this.fetchData()
  }

  fetchData() {
    this.jobOpeningService.apiJobOpeningJobOpeningsByConsultancyUserIDGet(this.sessionService.consultancyUserId, this.profileId).subscribe({
      next:(res) => {

        this.isLoaded = true
        this.isError = false

        if(!_.isEmpty(res)) {
          
          this.initialDataList = res.value
          this.filteredDataList = this.initialDataList
  
          this.totalItems = this.filteredDataList.length;

          if (this.totalItems > this.itemLimit) {
            this.ItemEndIndex = this.itemLimit;
          }
          else {
            this.ItemEndIndex = this.totalItems;
          }
          this.totalItems = this.filteredDataList.length;
        }
        else {
          this.totalItems = this.filteredDataList.length;
        }


      },
      error:(error) => {
        this.isError = true
        this.isLoaded = true
        this.totalItems = 0;
        this.error = 'Some error occured';
      }
    })
  }

  fetchUser() {
    const cachedUser: any = this.sessionService.getUserDetails();
    if (cachedUser && cachedUser.consultancyUsers?.length) {
      this.user = cachedUser;
      this.profileId = this.user.consultancyUsers[0].publicProfileUserName;
      this.fetchData();
      return;
    }

    this.sessionService.userdetailscast
      .pipe(takeUntil(this.destroy$))
      .subscribe((res: any) => {
        if (res && res.consultancyUsers?.length) {
          this.user = res;
          this.profileId = this.user.consultancyUsers[0].publicProfileUserName;
          this.fetchData();
        }
      });
  }

  ngOnInit() {
    this.fetchUser();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

}
