import { Component, OnInit } from '@angular/core';
import { SharedService } from 'src/app/modules/shared/services/shared.service';
import { SessionService } from 'src/app/core/session/session.service';

@Component({
  selector: 'dashboard-highlights',
  templateUrl: './dashboard-highlights.component.html',
  styleUrls: ['./dashboard-highlights.component.scss']
})
export class DashboardHighlightsComponent implements OnInit {

  candidateList: any[] = [];
  filteredCandidateList: any[] = [];
  searchTerm: string = '';
  user: any;
  isCollapsed: boolean = true; // Collapsed by default

  constructor(
    private sharedService: SharedService,
    private sessionService: SessionService
  ) { }

  ngOnInit() {
    this.sessionService.userdetailscast.subscribe((res: any) => {
      this.user = res;
    });

    this.sharedService.getJsonData().subscribe({
      next: (res: any) => {
        this.candidateList = res.candidateList || [];
        this.filteredCandidateList = [...this.candidateList];
      },
      error: (error: any) => {
        console.log(error);
      }
    });
  }

  toggleCollapse() {
    this.isCollapsed = !this.isCollapsed;
  }

  filterCandidates() {
    if (!this.searchTerm) {
      this.filteredCandidateList = [...this.candidateList];
      return;
    }
    const term = this.searchTerm.toLowerCase().trim();
    this.filteredCandidateList = this.candidateList.filter(c => 
      (c.name && c.name.toLowerCase().includes(term)) ||
      (c.title && c.title.toLowerCase().includes(term))
    );
  }

}
