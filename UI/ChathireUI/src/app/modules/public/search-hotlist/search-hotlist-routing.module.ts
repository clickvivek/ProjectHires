import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SearchHotlistComponent } from './search-hotlist.component';

const routes: Routes = [
  { path: '', component: SearchHotlistComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SearchHotlistRoutingModule { }
