import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule  } from 'src/app/modules/shared/shared.module'

import { JobDetailsPageRoutingModule } from './job-details-page-routing.module';
import { JobDetailsPageComponent } from './job-details-page.component';


@NgModule({
  declarations: [
    JobDetailsPageComponent
  ],
  imports: [
    CommonModule,
    SharedModule.forRoot(),
    JobDetailsPageRoutingModule
  ]
})
export class JobDetailsPageModule { }
