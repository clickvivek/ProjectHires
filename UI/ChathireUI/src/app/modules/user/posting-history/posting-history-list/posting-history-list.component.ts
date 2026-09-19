import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

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

  @Input() item: any;
  @Output() deleteParams = new EventEmitter();

  // List of Canadian Province Codes & Names
  private canadianProvinces = new Set([
    'AB', 'BC', 'MB', 'NB', 'NL', 'NS', 'NT', 'NU', 'ON', 'PE', 'QC', 'SK', 'YT',
    'ALBERTA', 'BRITISH COLUMBIA', 'MANITOBA', 'NEW BRUNSWICK', 'NEWFOUNDLAND',
    'NEWFOUNDLAND AND LABRADOR', 'NOVA SCOTIA', 'NORTHWEST TERRITORIES', 'NUNAVUT',
    'ONTARIO', 'PRINCE EDWARD ISLAND', 'QUEBEC', 'SASKATCHEWAN', 'YUKON'
  ]);

  // List of US State Codes & Names
  private usStates = new Set([
    'AL', 'AK', 'AZ', 'AR', 'CA', 'CO', 'CT', 'DE', 'FL', 'GA',
    'HI', 'ID', 'IL', 'IN', 'IA', 'KS', 'KY', 'LA', 'ME', 'MD',
    'MA', 'MI', 'MN', 'MS', 'MO', 'MT', 'NE', 'NV', 'NH', 'NJ',
    'NM', 'NY', 'NC', 'ND', 'OH', 'OK', 'OR', 'PA', 'RI', 'SC',
    'SD', 'TN', 'TX', 'UT', 'VT', 'VA', 'WA', 'WV', 'WI', 'WY',
    'DC', 'PR', 'VI', 'GU', 'MP', 'AS'
  ]);

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private jobOpeningService: JobOpeningService,
    private toastr: ToastrService,
    private sharedService: SharedService,
  ) { }

  getFormattedLocation(locations: any): string {
    if (!locations || !Array.isArray(locations) || locations.length === 0) {
      return '';
    }

    const formattedLocations: string[] = [];

    locations.forEach(loc => {
      let city = '';
      let state = '';
      let country = '';

      if (loc.city) {
        city = loc.city.city1 ? loc.city.city1.trim() : '';
        state = loc.city.stateCode ? loc.city.stateCode.trim() : (loc.city.idStateNavigation?.stateCode ? loc.city.idStateNavigation.stateCode.trim() : '');

        const countryCode = loc.city.idStateNavigation?.countryCode || loc.city.countryCode || loc.countryCode || loc.country;
        if (countryCode) {
          const codeUpper = String(countryCode).trim().toUpperCase();
          if (codeUpper === 'US' || codeUpper === 'USA' || codeUpper === 'UNITED STATES') {
            country = 'USA';
          } else if (codeUpper === 'CA' || codeUpper === 'CAN' || codeUpper === 'CANADA') {
            country = 'Canada';
          }
        }
      } else if (typeof loc === 'string') {
        const parts = loc.split(',').map(p => p.trim());
        city = parts[0] || '';
        state = parts[1] || '';
        if (parts[2]) country = parts[2];
      }

      // If country is not explicitly specified via navigation, infer from state code
      if (!country && state) {
        const stateUpper = state.toUpperCase();
        if (this.canadianProvinces.has(stateUpper)) {
          country = 'Canada';
        } else if (this.usStates.has(stateUpper)) {
          country = 'USA';
        } else {
          country = 'USA';
        }
      } else if (!country) {
        country = 'USA';
      }

      // Format: "Dallas, TX, USA" or "Toronto, ON, Canada"
      let formatted = '';
      if (city && state) {
        formatted = `${city}, ${state}, ${country}`;
      } else if (city) {
        formatted = `${city}, ${country}`;
      } else if (state) {
        formatted = `${state}, ${country}`;
      } else if (country) {
        formatted = country;
      }

      if (formatted && !formattedLocations.includes(formatted)) {
        formattedLocations.push(formatted);
      }
    });

    return formattedLocations.join(' ; ');
  }

  getDate(date: any) {
    if (!date) return '';
    return moment(date).format('MMMM D, YYYY');
  }

  generateJobId(id: any): string {
    if (!id) return '';
    try {
      return btoa(`chathire_${id}`);
    } catch {
      return String(id);
    }
  }

  handleActiveToggle(newStatus: boolean) {
    this.jobOpeningService.apiJobOpeningActivateDeactivateJobPut(this.item.id, newStatus).subscribe({
      next: (res: any) => {
        this.toastr.success(`Job ${res.value?.active ? "activated" : "deactivated"} successfully`, '', {
          timeOut: 1500,
          positionClass: 'toast-top-center'
        });
      },
      error: (err: any) => {
        this.item.active = !newStatus;
        this.toastr.error('Failed to update status. Please try again.', '', {
          timeOut: 2000,
          positionClass: 'toast-top-center'
        });
      }
    });
  }

  deletePost(item: any) {
    if (confirm(`Are you sure you want to delete "${item.name}"?`)) {
      this.jobOpeningService.apiJobOpeningDeleteJobOpeningDelete(item.id).subscribe({
        next: (res: any) => {
          this.deleteParams.emit(true);
          this.toastr.success('Job deleted successfully', '', {
            timeOut: 2000,
            positionClass: 'toast-top-center'
          });
        },
        error: (err: any) => {
          this.toastr.error('Failed to delete job. Please try again.', '', {
            timeOut: 2000,
            positionClass: 'toast-top-center'
          });
        }
      });
    }
  }

  editPost(item: any) {
    this.sharedService.setSideNavData(item);
    this.router.navigate(['edit-post', item.id], { relativeTo: this.route });
  }

}

