import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule  } from 'src/app/modules/shared/shared.module'
import { AdvancedFilterModule } from 'src/app/modules/ui/advanced-filter/advanced-filter.module';

import { SearchjobsRoutingModule } from './searchjobs-routing.module';
import { SearchjobsComponent } from './searchjobs.component';
import { ConfirmApplyJobComponent } from './confirm-apply-job/confirm-apply-job.component';
import { ChooseFromHotlistComponent } from './confirm-apply-job/choose-from-hotlist/choose-from-hotlist.component';
import { ChooseResumeFromDeskComponent } from './confirm-apply-job/choose-resume-from-desk/choose-resume-from-desk.component';


@NgModule({
  declarations: [
    SearchjobsComponent,
    ConfirmApplyJobComponent,
    ChooseFromHotlistComponent,
    ChooseResumeFromDeskComponent
  ],
  imports: [
    CommonModule,
    SharedModule.forRoot(),
    AdvancedFilterModule,
    SearchjobsRoutingModule
  ]
})
export class SearchjobsModule { }
