import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/modules/shared/shared.module';
import { AdvancedFilterModule } from 'src/app/modules/ui/advanced-filter/advanced-filter.module';
import { ViewResumeModule } from 'src/app/modules/ui/view-resume/view-resume.module';

import { MyhotlistRoutingModule } from './myhotlist-routing.module';
import { MyhotlistComponent } from './myhotlist.component';
import { HotlistSwitchConfirmationModalComponent } from './hotlist-switch-confirmation-modal/hotlist-switch-confirmation-modal.component';
import { HotlistDeleteConfirmationModalComponent } from './hotlist-delete-confirmation-modal/hotlist-delete-confirmation-modal.component';
import { MyhotlistListviewComponent } from './myhotlist-listview/myhotlist-listview.component';
import { MyhotlistTableviewComponent } from './myhotlist-tableview/myhotlist-tableview.component';


@NgModule({
  declarations: [
    MyhotlistComponent,
    HotlistSwitchConfirmationModalComponent,
    HotlistDeleteConfirmationModalComponent,
    MyhotlistListviewComponent,
    MyhotlistTableviewComponent
  ],
  imports: [
    CommonModule,
    SharedModule.forRoot(),
    ViewResumeModule,
    AdvancedFilterModule,
    MyhotlistRoutingModule
  ]
})
export class MyhotlistModule { }
