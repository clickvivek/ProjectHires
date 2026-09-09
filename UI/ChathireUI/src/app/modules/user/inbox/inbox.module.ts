import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/modules/shared/shared.module';

import { InboxRoutingModule } from './inbox-routing.module';
import { InboxComponent } from './inbox.component';
import { InboxJobDetailsComponent } from './inbox-job-details/inbox-job-details.component';
import { InboxResumeTypesComponent } from './inbox-resume-types/inbox-resume-types.component';
import { InboxResumeCommentsComponent } from './inbox-resume-comments/inbox-resume-comments.component';
import { InboxResumeContactComponent } from './inbox-resume-contact/inbox-resume-contact.component';


@NgModule({
  declarations: [
    InboxComponent,
    InboxJobDetailsComponent,
    InboxResumeTypesComponent,
    InboxResumeCommentsComponent,
    InboxResumeContactComponent
  ],
  imports: [
    CommonModule,
    SharedModule.forRoot(),
    InboxRoutingModule
  ]
})
export class InboxModule { }
