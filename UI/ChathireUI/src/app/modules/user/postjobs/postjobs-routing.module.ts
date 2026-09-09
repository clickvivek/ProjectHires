import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PostjobsComponent } from './postjobs.component';

const routes: Routes = [
  { path: '', component: PostjobsComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PostjobsRoutingModule { }
