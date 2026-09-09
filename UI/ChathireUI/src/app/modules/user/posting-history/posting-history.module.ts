import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/modules/shared/shared.module';
import { PostJobDetailsModule } from 'src/app/modules/ui/pages/post-job-details/post-job-details.module';

import { PostingHistoryRoutingModule } from './posting-history-routing.module';
import { PostingHistoryComponent } from './posting-history.component';
import { PostingHistoryListComponent } from './posting-history-list/posting-history-list.component';
import { PostingEditComponent } from './posting-edit/posting-edit.component';


@NgModule({
  declarations: [
    PostingHistoryComponent,
    PostingHistoryListComponent,
    PostingEditComponent
  ],
  imports: [
    CommonModule,
    SharedModule.forRoot(),
    PostJobDetailsModule,
    PostingHistoryRoutingModule
  ]
})
export class PostingHistoryModule { }
