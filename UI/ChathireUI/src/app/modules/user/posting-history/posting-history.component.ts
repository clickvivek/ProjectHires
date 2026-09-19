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

  user: any;
  profileId: any;

  isLoaded: boolean = false;
  isError: boolean = false;

  searchData: string = "";
  filterTab: 'all' | 'active' | 'inactive' = 'all';

  ItemStartIndex: number = 0;
  ItemEndIndex: number = 10;
  itemLimit: number = 10;
  totalItems: number = 0;

  initialDataList: any[] = [];
  filteredDataList: any[] = [];

  error: string = "";

  constructor(
    public router: Router,
    private jobOpeningService: JobOpeningService,
    private sessionService: SessionService
  ) { }

  handleSearch(event: any) {
    this.searchData = typeof event === 'string' ? event : (event?.target?.value || '');
    this.filterData();
  }

  clearSearch() {
    this.searchData = '';
    this.filterData();
  }

  setFilterTab(tab: 'all' | 'active' | 'inactive') {
    this.filterTab = tab;
    this.filterData();
  }

  resetFilters() {
    this.searchData = '';
    this.filterTab = 'all';
    this.filterData();
  }

  getActiveCount(): number {
    return this.initialDataList.filter(item => item.active).length;
  }

  getInactiveCount(): number {
    return this.initialDataList.filter(item => !item.active).length;
  }

  filterData() {
    let list = this.initialDataList;

    if (this.filterTab === 'active') {
      list = list.filter(item => item.active);
    } else if (this.filterTab === 'inactive') {
      list = list.filter(item => !item.active);
    }

    if (this.searchData && this.searchData.trim()) {
      list = list.filter((job) => this.isMatch(job, this.searchData));
    }

    this.filteredDataList = list;
    this.totalItems = this.filteredDataList.length;
    this.ItemStartIndex = 0;
    this.ItemEndIndex = Math.min(this.itemLimit, this.totalItems);
  }

  isMatch(job: any, term: string): boolean {
    if (!term) return true;
    term = term.toLowerCase().trim();

    const nameMatch = (job.name || job.jobOpeningName || '').toLowerCase().includes(term);

    // Check locations
    let locationMatch = false;
    if (job.jobOpeningLocations && Array.isArray(job.jobOpeningLocations)) {
      locationMatch = job.jobOpeningLocations.some(loc => {
        const city = loc.city?.city1?.toLowerCase() || '';
        const state = loc.city?.stateCode?.toLowerCase() || loc.city?.idStateNavigation?.stateCode?.toLowerCase() || loc.city?.idStateNavigation?.stateName?.toLowerCase() || '';
        const country = (loc.city?.idStateNavigation?.countryCode || loc.city?.countryCode || loc.country || '').toLowerCase();
        const fullLocation = `${city} ${state} ${country} usa canada united states`.toLowerCase();
        return fullLocation.includes(term);
      });
    }

    return nameMatch || locationMatch;
  }

  getIndexParams(event: any) {
    this.ItemStartIndex = event.ItemStartIndex;
    this.ItemEndIndex = event.ItemEndIndex;
    this.itemLimit = event.itemLimit;
  }

  isDataAvailable(): boolean {
    return this.filteredDataList && this.filteredDataList.length > 0;
  }

  onDeletePost(event: any) {
    this.fetchData();
  }

  fetchData() {
    this.jobOpeningService.apiJobOpeningJobOpeningsByConsultancyUserIDGet(this.sessionService.consultancyUserId, this.profileId).subscribe({
      next: (res: any) => {
        this.isLoaded = true;
        this.isError = false;

        if (!_.isEmpty(res)) {
          this.initialDataList = res.value || [];
          this.filterData();
        } else {
          this.initialDataList = [];
          this.filteredDataList = [];
          this.totalItems = 0;
        }
      },
      error: (error: any) => {
        this.isError = true;
        this.isLoaded = true;
        this.totalItems = 0;
        this.error = 'Failed to load job postings history. Please try again.';
      }
    });
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

