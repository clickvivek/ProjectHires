import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';



@Component({
  selector: 'app-confirm-apply-job',
  templateUrl: './confirm-apply-job.component.html',
  styleUrls: ['./confirm-apply-job.component.scss']
})
export class ConfirmApplyJobComponent implements OnInit {

  optionSelected: string = "";
  selectedHotlistType: string = "";
  isAppliedSuccess: boolean = false;
  appliedData: any = null;

  constructor(
    @Inject(MAT_DIALOG_DATA) public job: any
  ) { }

  handleOption(type) {
    this.optionSelected = type
  }

  onOptionTypeChange(event) {
    this.optionSelected = ""
  }

  onAppliedSuccess(data: any) {
    this.appliedData = data;
    this.isAppliedSuccess = true;
  }

  getLocationsText(): string {
    if (!this.job?.locations) return '';
    if (Array.isArray(this.job.locations)) {
      return this.job.locations
        .filter(loc => !!loc && String(loc).trim() !== '')
        .join(', ');
    }
    return String(this.job.locations);
  }

  ngOnInit() {


  }

}
