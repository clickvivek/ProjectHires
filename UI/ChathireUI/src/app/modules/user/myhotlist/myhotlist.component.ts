import { Component, OnInit, HostListener, EventEmitter, ViewEncapsulation } from '@angular/core';
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';


import { MatDialog } from '@angular/material/dialog';
import { MatDialogRef } from "@angular/material/dialog";

import { CandidateDetailSheetComponent } from 'src/app/modules/shared/components/candidate-detail-sheet/candidate-detail-sheet.component';
import { HotlistSwitchConfirmationModalComponent } from './hotlist-switch-confirmation-modal/hotlist-switch-confirmation-modal.component';

import { CandidateProfileService } from 'src/app/api/api/candidate-profile.service';
import { SessionService } from 'src/app/core/session/session.service';
import { CommonService, ConsultancyService } from 'src/app/api';
import { ToastrService } from 'ngx-toastr';
import html2canvas from 'html2canvas';

import _ from 'underscore';
import { HotlistDeleteConfirmationModalComponent } from './hotlist-delete-confirmation-modal/hotlist-delete-confirmation-modal.component';
import { HotlistShareModalComponent } from './hotlist-share-modal/hotlist-share-modal.component';

export interface BannerTheme {
  id: string;
  name: string;
  headerGradient: string;
  primaryColor: string;
  lightColor: string;
  accentSubtext: string;
  swatch: string;
}

export const BANNER_THEMES: BannerTheme[] = [
  {
    id: 'plum',
    name: 'ChatHire Plum',
    headerGradient: 'linear-gradient(135deg, #673260 0%, #3D1939 100%)',
    primaryColor: '#673260',
    lightColor: '#F6EFF5',
    accentSubtext: '#E9D5E8',
    swatch: '#673260'
  },
  {
    id: 'navy',
    name: 'Corporate Navy',
    headerGradient: 'linear-gradient(135deg, #1E3A8A 0%, #0F172A 100%)',
    primaryColor: '#1E3A8A',
    lightColor: '#EFF6FF',
    accentSubtext: '#BFDBFE',
    swatch: '#1E3A8A'
  },
  {
    id: 'charcoal',
    name: 'Modern Slate',
    headerGradient: 'linear-gradient(135deg, #374151 0%, #111827 100%)',
    primaryColor: '#1F2937',
    lightColor: '#F3F4F6',
    accentSubtext: '#D1D5DB',
    swatch: '#374151'
  },
  {
    id: 'emerald',
    name: 'Forest Emerald',
    headerGradient: 'linear-gradient(135deg, #065F46 0%, #022C22 100%)',
    primaryColor: '#065F46',
    lightColor: '#ECFDF5',
    accentSubtext: '#A7F3D0',
    swatch: '#065F46'
  },
  {
    id: 'indigo',
    name: 'Royal Indigo',
    headerGradient: 'linear-gradient(135deg, #4338CA 0%, #1E1B4B 100%)',
    primaryColor: '#4338CA',
    lightColor: '#EEF2FF',
    accentSubtext: '#C7D2FE',
    swatch: '#4338CA'
  },
  {
    id: 'ruby',
    name: 'Crimson Ruby',
    headerGradient: 'linear-gradient(135deg, #881337 0%, #4C0519 100%)',
    primaryColor: '#881337',
    lightColor: '#FFF1F2',
    accentSubtext: '#FECDD3',
    swatch: '#881337'
  },
  {
    id: 'teal',
    name: 'Ocean Teal',
    headerGradient: 'linear-gradient(135deg, #0E7490 0%, #083344 100%)',
    primaryColor: '#0E7490',
    lightColor: '#ECFEFF',
    accentSubtext: '#A5F3FC',
    swatch: '#0E7490'
  }
];

@Component({
  selector: 'app-myhotlist',
  templateUrl: './myhotlist.component.html',
  styleUrls: ['./myhotlist.component.scss'],
  providers: [
    { provide: MatDialogRef, useValue: {} },
  ]
})
export class MyhotlistComponent implements OnInit {

  isJobLoaded:boolean = false;
  isJobAvailable:boolean = false;
  isError:boolean = false;
  error:string = ""

  totalItems:any;

  skillSetData:any;
  initialHotListData:any[] = [];
  filteredHotListData:any[] = [];
  isActiveCandidate:any;
  totalExp:any;
  visaList:any;
  cityList:any;

  isMobile:boolean = false;

  selectedCandidateId:number = -1;
  selectedCandidateDetails: any;
  
  selectAvailabilityList: any[] = [];

  searchData: string = ""
  
  isListView: boolean = false;
  isTableView: boolean = false;

  profileId:any;
  user: any;
  company: any = {};
  isGeneratingImage: boolean = false;
  selectedTheme: BannerTheme = BANNER_THEMES[0];

  candidateDetailsChanged: EventEmitter<any> = new EventEmitter();

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    public dialog: MatDialog,
    private commonService: CommonService,
    private consultancyService: ConsultancyService,
    private sessionService: SessionService,
    private candidateProfileService: CandidateProfileService,
    private toastr: ToastrService
  ) { 

    router.events.subscribe((event: any) => {
      
      if (event instanceof NavigationEnd) {
        
        const params = this.route.snapshot.queryParams;

        if (_.isUndefined(params['view'])) {
          this.router.navigate([], { queryParamsHandling: 'merge', queryParams: { view: 'list' } });
          this.isListView = true
          this.isTableView = false
        }
        else {
          const type = params['view'];
          if (type == 'table') {
            this.isListView = false
            this.isTableView = true
          }
          else {
            this.isListView = true
            this.isTableView = false
          }
        }
      }
      
    });

  }

  getParams(event){
    this.isActiveCandidate = event.active;
    this.totalExp = event.experience;
  }

  handleSearch(event:any) {
    this.searchData = event
    this.onSearchData()
  }

  onSearchData() {

    this.filteredHotListData = this.initialHotListData.filter((candidate) =>
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

  showCandidateDetaislModal(){

    const jobDialogRef = this.dialog.open(CandidateDetailSheetComponent, {
        panelClass: 'material',
        disableClose: true,
        data: this.selectedCandidateDetails
    });

  }

  @HostListener('window:resize', ['$event'])
    onResize(event){

    if (event.target.innerWidth <= 991) {
      this.isMobile = true;
      this.router.navigate([], { queryParamsHandling: 'merge', queryParams: { view: 'list' } });
    }
    else {
      this.isMobile = false;
    }

  }

  isSelected(id){
    return this.selectedCandidateId == id ? 'selected' : '';
  }

  showCandidateDescription(candidate){

    this.selectedCandidateId = candidate.id;
    this.selectedCandidateDetails = candidate;
    this.candidateDetailsChanged.emit(this.selectedCandidateDetails);

  }

  onStatusChange({statusId, item}) {
    this.switchConfirmation(statusId, item)
  }

  switchConfirmation(statusId, item) {


    const dialogRef = this.dialog.open(HotlistSwitchConfirmationModalComponent, {
      panelClass: 'material',
      disableClose: true,
      data: statusId
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {

        this.candidateProfileService.apiCandidateProfileActivateOrDeActivatePut(item.id, true, statusId).subscribe({
          next: (res : any) => {

            if(res.value) {

              this.filteredHotListData.map(listItem => {
                if(listItem.id == item.id) {
                  item.statusId = statusId
                  item.status = item.statusId == 2 ? true : false
                  return item
                }
                return item
              })

              setTimeout(() => {
                this.toastr.success(`Marketing ${statusId == 2 ? 'activated' : 'deactivated'} successfully`, '' , {
                  timeOut: 5000,
                  positionClass: 'toast-top-center'
                });
              }, 200)

            }


          },
          error: (error:any) => {
            this.toastr.error('Some error occured', '' , {
              timeOut: 5000,
              positionClass: 'toast-top-center'
            });
          }
        })

      }
    });

  }

handleViewType(type) {
  this.router.navigate([], { queryParamsHandling: 'merge', queryParams: { view: type } });
}

 deleteCandidate(item) {

  const dialogRef = this.dialog.open(HotlistDeleteConfirmationModalComponent, {
    panelClass: 'material',
    disableClose: true
  });

  dialogRef.afterClosed().subscribe((result) => {

    if (result) {

      this.candidateProfileService.apiCandidateProfileActivateOrDeActivatePut(item.id, false, item.statusId).subscribe({
        next: (res : any) => {

          if(res.value) {

            this.filteredHotListData = this.filteredHotListData.filter(listItem => {
              return listItem.id != item.id
            })

            this.toastr.success(`Candidate deleted successfully`, '' , {
              timeOut: 5000,
              positionClass: 'toast-top-center'
            });

          }

        },
        error: (error:any) => {
          this.toastr.error('Some error occured', '' , {
            timeOut: 5000,
            positionClass: 'toast-top-center'
          });
        }

      })

    }

  });


 }

 fetchData() {
  this.candidateProfileService.apiCandidateProfileGetByConsultancyUserGet(this.sessionService.consultancyUserId, this.profileId).subscribe({
    next: (res : any) => {

      this.initialHotListData = (res.value || []).filter(item => {
        return item.active
      });

      this.initialHotListData.map(item => {
        item.status = item.statusId == 2 ? true : false
      });

      this.filteredHotListData = this.initialHotListData;
      this.totalItems = this.initialHotListData.length;

      if(this.filteredHotListData.length > 0){
        this.isJobAvailable = true;
        this.isJobLoaded = true;
      }
      else {
        this.isJobAvailable = false;
        this.isJobLoaded = true;
      }

    },
    error: (error:any) => {

      this.isJobLoaded = true;
      this.isJobLoaded = true;
      this.isError = true
      this.error = "Some error occurred"

     }
  })
 }

  getVisa(id: any) {
    let name = '';
    _.some(this.visaList, (item: any) => {
      if (item.id == id) name = item.name;
    });
    return name || 'N/A';
  }

  getRelocation(data: any) {
    let item = data?.candidatePrefLocations;
    let newData: any = [];
    if (!_.isEmpty(item)) {
      item.forEach((listItem: any) => {
        let rawCity = listItem.cityName || '';
        let parts = rawCity.split('-');
        let city = parts[0] ? parts[0].trim() : '';
        let state = (listItem.stateCode || (parts[1] ? parts[1].trim() : '') || listItem.stateName || '').trim();
        let formatted = (city && state) ? `${city}, ${state}` : (city || state);
        if (formatted) newData.push(formatted);
      });
      return newData.length > 0 ? newData.join('; ') : (data.anyLocation ? 'Any Location' : data.remoteOnly ? 'Remote' : 'Open');
    }
    return data?.anyLocation ? 'Any Location' : data?.remoteOnly ? 'Remote' : 'Open';
  }

  async renderBanner(): Promise<{ imageUrl: string, blob: Blob | null }> {
    const bannerElement = document.getElementById('hotlistBanner');
    if (!bannerElement) throw new Error('Banner element not found');
    const canvas = await html2canvas(bannerElement, {
      scale: 2,
      useCORS: true,
      backgroundColor: '#FFFFFF',
      logging: false
    });
    const imageUrl = canvas.toDataURL('image/png');
    const blob = await new Promise<Blob | null>(resolve => canvas.toBlob(resolve, 'image/png'));
    return { imageUrl, blob };
  }

  async openLinkedInShareModal() {
    const bannerElement = document.getElementById('hotlistBanner');
    if (!bannerElement) {
      this.toastr.error('Could not generate hotlist image.', '', { positionClass: 'toast-top-center' });
      return;
    }

    try {
      this.isGeneratingImage = true;
      const { imageUrl, blob } = await this.renderBanner();
      this.isGeneratingImage = false;

      const profileLink = `https://www.chathire.com/profile/${this.profileId || ''}`;
      const captionText = `🚀 Available Candidates Hotlist Update!\n\nWe have exceptional, interview-ready IT consultants available for immediate C2C / Contract requirements across top technologies.\n\n👉 View live profiles & contact us: ${profileLink}\n\nFeel free to reach out directly or message me for resumes and rates.\n\n#Hotlist #BenchSales #C2C #ITStaffing #Recruitment #ChatHire`;

      this.dialog.open(HotlistShareModalComponent, {
        panelClass: 'material',
        disableClose: false,
        width: '940px',
        maxWidth: '95vw',
        maxHeight: '92vh',
        data: {
          imageUrl: imageUrl,
          imageBlob: blob,
          profileUrl: profileLink,
          captionText: captionText,
          themes: BANNER_THEMES,
          selectedThemeId: this.selectedTheme.id,
          onThemeChange: async (themeId: string) => {
            const found = BANNER_THEMES.find(t => t.id === themeId);
            if (found) {
              this.selectedTheme = found;
              await new Promise(resolve => setTimeout(resolve, 60));
              return await this.renderBanner();
            }
            return null;
          }
        }
      });

    } catch (error) {
      this.isGeneratingImage = false;
      console.error('Error generating hotlist image:', error);
      this.toastr.error('Failed to generate image. Please try again.', '', { positionClass: 'toast-top-center' });
    }
  }

  ngOnInit() {

    if(window.innerWidth <= 991)
      this.isMobile = true;
    else
      this.isMobile = false;

      this.sessionService.userdetailscast.subscribe((res: any) => {
        let user = res;
        this.user = user;
        this.profileId = user?.consultancyUsers[0]?.publicProfileUserName;
        let consultancyId = user?.consultancyUsers[0]?.consultancyId;
        if (consultancyId) {
          this.consultancyService.apiConsultancyConsultancyByIdGet(consultancyId).subscribe({
            next: (res: any) => {
              this.company = res.value;
            }
          });
        }
        this.fetchData();
      });

    this.candidateDetailsChanged.subscribe( jobDetails => {
      if(this.isMobile){
        this.showCandidateDetaislModal();
      }
    });

    this.commonService.apiCommonVisaGet().subscribe({
      next: (res : any) => {
        this.visaList = res.value;
      },
      error: (error:any) => { }
    });

    this.commonService.apiCommonCandidateAvailabilityGet().subscribe({
      next: (res: any) => {
        this.selectAvailabilityList = res.value;
      },
      error: (error:any) => { }
    });

  }

}
