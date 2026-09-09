import { Component } from '@angular/core';
import { SharedService } from 'src/app/modules/shared/services/shared.service';

@Component({
  selector: 'dashboard-highlights',
  templateUrl: './dashboard-highlights.component.html',
  styleUrls: ['./dashboard-highlights.component.scss']
})
export class DashboardHighlightsComponent {

  candidateList: any;

  constructor(
    private sharedService: SharedService
  ) { }

  ngOnInit() {
    this.sharedService.getJsonData().subscribe({
      next: (res : any) => {
        this.candidateList = res.candidateList;
      },
      error: (error:any) => {
        console.log(error);
      }
    })
  }

  

}
