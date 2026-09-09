import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { JobDetailsPageComponent } from './job-details-page.component';

const routes: Routes = [
  { path: '', component: JobDetailsPageComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class JobDetailsPageRoutingModule { }
