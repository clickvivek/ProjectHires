import { Component, ViewChild, Inject, ElementRef, Output, EventEmitter } from '@angular/core';
import { NgForm } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { JobOpeningService } from 'src/app/api';
import { SessionService } from 'src/app/core/session/session.service';
import { CommonService } from 'src/app/api';
import _ from 'underscore';

@Component({
  selector: 'choose-resume-from-desk',
  templateUrl: './choose-resume-from-desk.component.html',
  styleUrls: ['./choose-resume-from-desk.component.scss']
})
export class ChooseResumeFromDeskComponent {

  formData: any = {
    name: "",
    totalExp: null,
    linkedIn: '',
    cityId: null,
    candidatePrefLocations: null
  }

  selectLocationList:any;

  @ViewChild('resumeFileInput', { static: false }) resumeFileInput: ElementRef;
  selectedResume: any | null = null;

  resumeFileName;
  resumeDocumentType; 

  @ViewChild('applyJobsForm', {static: false}) applyJobsForm: NgForm;

  @Output() outParams = new EventEmitter();

  constructor(
    @Inject(MAT_DIALOG_DATA) public job: any,
    private dialogRef: MatDialogRef<ChooseResumeFromDeskComponent>,
    private sessionService: SessionService,
    private commonService: CommonService,
    private jobOpeningService: JobOpeningService,
    private toastr: ToastrService
  ) {

  }

  onReLocationQuery(event:any) {
    this.commonService.apiCommonCityGet(event,undefined,true).subscribe({
      next: (res : any) => {
        this.selectLocationList = res.value
      },
      error: (error:any) => { }
    })
  }

  onSelectedLocations(event:any) {
    let newData:any = []
    if(!_.isEmpty(event)) {
      event.forEach((item, index) => {
        let itemData = {
            id: index,
            candidateId: this.formData.id,
            cityId: item.id || item.cityId
        }
        newData.push(itemData)
      });
      this.formData.candidatePrefLocations = newData;
      this.formData.cityId = event[0].id;
    }
    else {
      this.formData.candidatePrefLocations = []
      this.formData.cityId = null
    }
  }

  getFileType(type) {
    let data = type.split('/');
    return data[1]
  }

  deleteResume() {
    this.resumeFileName = "";
    this.resumeDocumentType = "";
    this.selectedResume = null
    this.resumeFileInput.nativeElement.value = '';
  }

  onFileSelected(event: any) {
    this.selectedResume = event.target.files[0];
    this.resumeFileName = this.selectedResume?.name;
    this.resumeDocumentType = this.selectedResume?.type;
  }

  clearData() {
    this.selectedResume = null
    this.outParams.emit(false)
  }

  applyJob() {

    for (const field in this.applyJobsForm.controls) {
      if (this.applyJobsForm.controls.hasOwnProperty(field)) {
        const control = this.applyJobsForm.controls[field];
        if (control.invalid) {
          console.log(`Invalid field: ${field}`);
        }
      }
    }


    if (this.applyJobsForm.valid && this.selectedResume ) {
      
      this.jobOpeningService.apiJobOpeningApplyWithResumePost(
        this.selectedResume,
        parseInt(this.job.jobOpeningId),
        undefined,
        new Date().toISOString(),
        true,
        1,
        "",
        this.sessionService.consultancyUserId,
        undefined,
        undefined,
        this.formData.name,
        this.formData.totalExp,
        this.formData.LinkedIn,
        this.formData.cityId
      ).subscribe({
        next:(res:any) => {
          setTimeout(() => {
            this.toastr.success('Job applied successfully', '', {
              timeOut: 2000,
              positionClass: 'toast-top-center'
            });
          }, 100);
          this.dialogRef.close()
        },
        error:(error:any) => {
          setTimeout(() => {
            this.toastr.error('Some error occured', '', {
              timeOut: 2000,
              positionClass: 'toast-top-center'
            });
          }, 100);
        }
      })
    }
    else {
      
    }
    
    

  }

}
