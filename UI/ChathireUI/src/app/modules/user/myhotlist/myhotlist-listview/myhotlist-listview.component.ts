import { Component, Input, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import _ from 'underscore';
import { CandidateAppliedJobsModalComponent } from '../../../shared/components/candidate-applied-jobs-modal/candidate-applied-jobs-modal.component';
import { formatRelocation } from 'src/app/data/various';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'myhotlist-listview',
  templateUrl: './myhotlist-listview.component.html',
  styleUrls: ['./myhotlist-listview.component.scss']
})
export class MyhotlistListviewComponent {

  @Input('filteredHotListData') list:Array<any> = [];
  @Input() visaList: any;
  @Input() availabilityList: any;

 @Output() handleDeleteCandidate: EventEmitter<any> = new EventEmitter();
 @Output() handleStatusChange: EventEmitter<any> = new EventEmitter();

  constructor(
    private router: Router,
    private dialog: MatDialog,
    private http: HttpClient,
    private toastr: ToastrService
  ) {

  }

  openAppliedJobs(item: any, event?: Event): void {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    if (!item || !item.id) return;
    this.dialog.open(CandidateAppliedJobsModalComponent, {
      width: '720px',
      maxWidth: '95vw',
      panelClass: 'candidate-applied-jobs-modal-panel',
      data: {
        candidateProfileId: item.id,
        candidateName: item.candidateName,
        candidateRole: item.title
      }
    });
  }

  getVisa(id){
    var name;
    _.some(this.visaList, (item) => {
      if(item.id == id)
        name = item.name;
    });
    return name;
  }

  getAvailability(id) {
    if (id) {
      let newData = this.availabilityList.filter(item => {
        return item.id == id
      })
      if (!_.isEmpty(newData)) {
        return `${newData[0].name}`
      }
      else {
        return ""
      }
    }
    else {
      return 'Not Available'
    }
  }

  getRelocation(data) {
    return formatRelocation(data);
  }

  isSkills(item) {
    return _.isEmpty(item) ? false : true
  }

  onStatusChange(event, item) {
    event.preventDefault();
    const target = event.target as HTMLInputElement;
    const statusId = JSON.parse(target.value) == 2 ? 3 : 2
    this.handleStatusChange.emit({statusId, item});
  }

  editCandidate(id) {
    this.router.navigate(['/editcandidate', id]);
  }

  deleteCandidate(item) {
    this.handleDeleteCandidate.emit(item)
  }

  getResumeDoc(item: any): string {
    if (item?.candidateDocuments && item.candidateDocuments.length > 0) {
      return item.candidateDocuments[0]?.doc || '';
    }
    return item?.resume || '';
  }

  toggleHideResume(item: any, event: Event) {
    const input = event.target as HTMLInputElement;
    const previousState = !!item.hideResume;
    // Checkbox is "Show resume to recruiters":
    // When checked => show resume => hideResume = false
    // When unchecked => hide resume => hideResume = true
    const newHideResume = !input.checked;
    item.hideResume = newHideResume;

    const apiUrl = `${environment.rootUrl}/api/CandidateProfile/ToggleHideResume?candidateProfileId=${item.id}&hideResume=${newHideResume}`;
    this.http.put(apiUrl, {}).subscribe({
      next: (res: any) => {
        if (newHideResume) {
          this.toastr.info('Resume is now hidden from recruiters.', '', {
            timeOut: 4000,
            positionClass: 'toast-top-center'
          });
        } else {
          this.toastr.success('Resume is now visible to recruiters.', '', {
            timeOut: 4000,
            positionClass: 'toast-top-center'
          });
        }
      },
      error: (err: any) => {
        item.hideResume = previousState;
        input.checked = !previousState;
        this.toastr.error('Failed to update resume visibility. Please try again.', '', {
          timeOut: 5000,
          positionClass: 'toast-top-center'
        });
      }
    });
  }

  ngOnInit() {
    
  }

}
