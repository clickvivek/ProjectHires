import { Component, OnInit, Input, Inject, Optional, HostListener, ViewEncapsulation } from '@angular/core';
import { Router, Event, NavigationEnd, ActivatedRoute } from '@angular/router';

import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

import _ from 'underscore';

@Component({
  selector: 'app-candidate-detail-sheet',
  templateUrl: './candidate-detail-sheet.component.html',
  styleUrls: ['./candidate-detail-sheet.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class CandidateDetailSheetComponent implements OnInit {

  @Input() selectedCandidateDetails:any;

  isMobile:boolean = false;

  constructor(
  	@Optional() @Inject(MAT_DIALOG_DATA) public modalCandidateDetails: any,
     public dialog: MatDialog,
     private candidateDialogRef: MatDialogRef<CandidateDetailSheetComponent>,
  	 private route: ActivatedRoute
  ) { }

  isModal(){
    return this.modalCandidateDetails != null ? true : false;
  }

  dialogClose(){
    if(!_.isEmpty(this.candidateDialogRef))
    this.candidateDialogRef.close();
  }



  getVisaDetails(visa){
    if(visa != null)
    return visa.split(',');
    else
      return [];
  }

  ngOnInit() {

  	if(this.modalCandidateDetails != null){
      this.selectedCandidateDetails = this.modalCandidateDetails;
    }

    if(window.innerWidth <= 991)
      this.isMobile = true;
    else
      this.isMobile = false;

  }

}
