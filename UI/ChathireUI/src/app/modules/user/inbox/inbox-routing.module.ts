import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { InboxComponent } from './inbox.component';
import { InboxJobDetailsComponent } from './inbox-job-details/inbox-job-details.component';
import { InboxResumeTypesComponent } from './inbox-resume-types/inbox-resume-types.component';
import { inboxResolver } from './inbox.resolver';

const routes: Routes = [
  { path: "", component: InboxComponent, 
    children: [
      { path: "jobdetails/:id", component: InboxJobDetailsComponent,  resolve: { inboxResolver} },
      { path: "resumes/:jobid/:typeid", component: InboxResumeTypesComponent,  resolve: { inboxResolver} }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class InboxRoutingModule { }
