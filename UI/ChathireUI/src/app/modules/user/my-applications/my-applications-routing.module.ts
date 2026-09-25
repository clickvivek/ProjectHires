import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MyApplicationsComponent } from './my-applications.component';

const routes: Routes = [
  { path: '', component: MyApplicationsComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MyApplicationsRoutingModule { }
