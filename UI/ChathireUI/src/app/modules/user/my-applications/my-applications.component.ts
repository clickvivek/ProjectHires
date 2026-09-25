import { Component, OnInit } from '@angular/core';
import { CandidateDirectService, CandidateAppliedJob } from 'src/app/core/services/candidate-direct.service';
import { picUrl } from 'src/app/data/various';

@Component({
  selector: 'app-my-applications',
  templateUrl: './my-applications.component.html',
  styleUrls: ['./my-applications.component.scss']
})
export class MyApplicationsComponent implements OnInit {
  isLoading: boolean = true;
  applications: CandidateAppliedJob[] = [];
  filteredApplications: CandidateAppliedJob[] = [];
  selectedFilter: string = 'all';
  searchQuery: string = '';

  constructor(private candidateService: CandidateDirectService) {}

  ngOnInit(): void {
    this.loadApplications();
  }

  loadApplications(): void {
    this.isLoading = true;
    this.candidateService.getMyApplications().subscribe({
      next: (res: any) => {
        this.isLoading = false;
        if (res && res.value) {
          this.applications = res.value;
          this.filterApplications();
        }
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Failed to load applications', err);
      }
    });
  }

  setFilter(filter: string): void {
    this.selectedFilter = filter;
    this.filterApplications();
  }

  onSearchChange(): void {
    this.filterApplications();
  }

  filterApplications(): void {
    let list = this.applications;

    if (this.selectedFilter === 'reviewing') {
      list = list.filter(a => a.statusId === 1 || a.statusId === 2);
    } else if (this.selectedFilter === 'shortlisted') {
      list = list.filter(a => a.statusId === 3 || a.statusId === 4);
    } else if (this.selectedFilter === 'rejected') {
      list = list.filter(a => a.statusId === 5);
    }

    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase().trim();
      list = list.filter(a =>
        (a.jobTitle && a.jobTitle.toLowerCase().includes(q)) ||
        (a.companyName && a.companyName.toLowerCase().includes(q)) ||
        (a.jobLocation && a.jobLocation.toLowerCase().includes(q))
      );
    }

    this.filteredApplications = list;
  }

  getStatusBadgeClass(statusId: number): string {
    switch (statusId) {
      case 1: return 'badge-applied';     // Submitted / Applied
      case 2: return 'badge-review';      // Under Review
      case 3: return 'badge-shortlisted'; // Shortlisted
      case 4: return 'badge-interview';   // Interview
      case 5: return 'badge-rejected';    // Rejected / Not Selected
      default: return 'badge-applied';
    }
  }

  getCompanyLogo(logo: string | null | undefined): string {
    if (!logo) return '';
    if (logo.startsWith('http') || logo.startsWith('data:')) return logo;
    return `${picUrl}${logo}`;
  }

  getResumeUrl(blobUrl: string): string {
    if (!blobUrl) return '';
    if (blobUrl.startsWith('http') || blobUrl.startsWith('https:')) return blobUrl;
    return `${picUrl}${blobUrl}`;
  }

  getCount(filter: string): number {
    if (filter === 'all') return this.applications.length;
    if (filter === 'reviewing') return this.applications.filter(a => a.statusId === 1 || a.statusId === 2).length;
    if (filter === 'shortlisted') return this.applications.filter(a => a.statusId === 3 || a.statusId === 4).length;
    if (filter === 'rejected') return this.applications.filter(a => a.statusId === 5).length;
    return 0;
  }
}
