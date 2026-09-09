import { Component, Input, Output, EventEmitter } from '@angular/core';
import {  Router, ActivatedRoute, NavigationEnd } from '@angular/router';

import * as moment from 'moment';
import _ from 'underscore';

import { JobOpeningService } from 'src/app/api';
import { ToastrService } from 'ngx-toastr';
import { SharedService } from 'src/app/modules/shared/services/shared.service';


@Component({
  selector: 'posting-history-list',
  templateUrl: './posting-history-list.component.html',
  styleUrls: ['./posting-history-list.component.scss']
})
export class PostingHistoryListComponent {

  @Input() item;

  @Output() deleteParams = new EventEmitter();

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private jobOpeningService: JobOpeningService,
    private toastr: ToastrService,
    private sharedService: SharedService,
  ) {
    
  }

  getRelocation(location) {
    if(!_.isEmpty(location)) {
      let newArray:string[] = []
      location.forEach(item => {
        let name = item.city
        newArray.push(name.city1 + '-' + name.stateCode)
      });
      return newArray.join(', ')
    }
    else {
      return ""
    }
  }

  getDate(date) {
    return moment(date).format('MMMM D, YYYY')
  }

  handleActive(event) {
    
    let newItem = event.item

    this.jobOpeningService.apiJobOpeningActivateDeactivateJobPut(newItem.id, event.status).subscribe({
      next:(res) => {
        this.toastr.success(`Job ${res.value?.active ? "activated" : "deactivated"} successfully`, '' , {
          timeOut: 1000,
          positionClass: 'toast-top-center'
        });
      },
      error:(res) => {
        this.toastr.error('Some error occured', '' , {
          timeOut: 1000,
          positionClass: 'toast-top-center'
        });
      }
    })

  }

  deletePost(item) {

    this.jobOpeningService.apiJobOpeningDeleteJobOpeningDelete(item.id).subscribe({
      next:(res) => {
        this.deleteParams.emit(true)
        this.toastr.success('Job deleted successfully', '' , {
          timeOut: 3000,
          positionClass: 'toast-top-center'
        });
      },
      error:(res) => {
        this.toastr.error('Some error occured', '' , {
          timeOut: 3000,
          positionClass: 'toast-top-center'
        });
      }
    })

  }

  editPost(item) {
    this.sharedService.setSideNavData(item)
    this.router.navigate(['edit-post', item.id], { relativeTo: this.route });
  }

}
