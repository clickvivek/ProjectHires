import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PostingHistoryComponent } from './posting-history.component';
import { PostingEditComponent } from './posting-edit/posting-edit.component';
import { postingHistoryResolver } from './posting-history.resolver';

const routes: Routes = [
  { path: "", component: PostingHistoryComponent,
    children: [
      { path: "edit-post/:id", component: PostingEditComponent,  resolve: { postingHistoryResolver} },
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PostingHistoryRoutingModule { }
