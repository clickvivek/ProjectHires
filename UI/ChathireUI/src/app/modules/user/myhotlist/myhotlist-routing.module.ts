import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MyhotlistComponent } from './myhotlist.component';

const routes: Routes = [
  { path: '', component: MyhotlistComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MyhotlistRoutingModule { }
