import { Component, Input, Output, EventEmitter, HostListener, OnChanges, SimpleChanges  } from '@angular/core';
import { CandidateProfileService } from 'src/app/api/api/candidate-profile.service';
import { SessionService } from 'src/app/core/session/session.service';
import { CommonService } from 'src/app/api';
import _ from 'underscore';

@Component({
  selector: 'profile-available-resources',
  templateUrl: './profile-available-resources.component.html',
  styleUrls: ['./profile-available-resources.component.scss']
})
export class ProfileAvailableResourcesComponent {

  @Input() consultancyUserId;
  @Input() profileId
  @Output() outParams = new EventEmitter();

  isJobLoaded:boolean = false;
  isJobAvailable:boolean = false;
  isError:boolean = false;
  error:string = ""

  initialDataList:any = [];
  filteredDataList: any[] = [];

  visaList:any;
  selectAvailabilityList: any[] = [];

  searchData:string = ""

  isListView:boolean = true;

  constructor(
    private candidateProfileService: CandidateProfileService,
    private commonService: CommonService,
    private sessionService: SessionService
  ) {

  }

  @HostListener('window:resize', ['$event'])
    onResize(event){

    if (event.target.innerWidth <= 991) {
      this.isListView = true
    }
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
      candidate.candidateName.toLowerCase().includes(term) ||
      candidate.title.toLowerCase().includes(term) ||
      candidate.candidateProfileSkills.some((item) =>
        item.name.toLowerCase().includes(term)
      )
    );
  }


  ngOnChanges(changes: SimpleChanges) {
    if(this.consultancyUserId && this.profileId) {
      this.candidateProfileService.apiCandidateProfileGetByConsultancyUserGet(this.consultancyUserId, this.profileId).subscribe({
        next:(res:any) => {
          
          this.initialDataList = res.value
          this.filteredDataList = this.initialDataList
          this.isJobLoaded = true

          if (!_.isEmpty(this.initialDataList)) {
            this.isJobAvailable = true
            this.outParams.emit(true)
          }
          else {
            this.isJobAvailable = false
            this.outParams.emit(false)
          }

        },
        error:(error:any) => {
          this.isJobLoaded = true
          this.isJobAvailable = false
          this.isError = true
          this.error = "Some error occured"
        }
      })
    }

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
