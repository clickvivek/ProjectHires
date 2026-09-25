import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SharedModule } from 'src/app/modules/shared/shared.module';
import { MyApplicationsRoutingModule } from './my-applications-routing.module';
import { MyApplicationsComponent } from './my-applications.component';

@NgModule({
  declarations: [
    MyApplicationsComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    SharedModule.forRoot(),
    MyApplicationsRoutingModule
  ]
})
export class MyApplicationsModule { }
