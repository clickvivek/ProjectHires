import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import _ from 'underscore';
import { CandidateAppliedJobsModalComponent } from '../../../shared/components/candidate-applied-jobs-modal/candidate-applied-jobs-modal.component';
import { formatRelocation } from 'src/app/data/various';

@Component({
  selector: 'myhotlist-tableview',
  templateUrl: './myhotlist-tableview.component.html',
  styleUrls: ['./myhotlist-tableview.component.scss']
})
export class MyhotlistTableviewComponent {

  @Input('filteredHotListData') list:Array<any> = [];
  @Input() visaList: any;
  @Input() availabilityList: any;

  @Output() handleDeleteCandidate: EventEmitter<any> = new EventEmitter();
  @Output() handleStatusChange: EventEmitter<any> = new EventEmitter();

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private dialog: MatDialog
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
      return `${newData[0].name}`
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


}
