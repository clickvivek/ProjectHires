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


  constructor(
    @Inject(MAT_DIALOG_DATA) public job: any
  ) { }

  handleOption(type) {
    this.optionSelected = type
  }

  onOptionTypeChange(event) {
    this.optionSelected = ""
  }

  ngOnInit() {


  }

}
