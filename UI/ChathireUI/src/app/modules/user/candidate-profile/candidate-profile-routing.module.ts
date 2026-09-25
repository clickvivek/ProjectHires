import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CandidateProfileComponent } from './candidate-profile.component';

const routes: Routes = [
  { path: '', component: CandidateProfileComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CandidateProfileRoutingModule { }
