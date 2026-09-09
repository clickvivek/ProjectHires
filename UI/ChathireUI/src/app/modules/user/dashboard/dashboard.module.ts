import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/modules/shared/shared.module';
import { DashboardRoutingModule } from './dashboard-routing.module';
import { DashboardComponent } from './dashboard.component';
import { DashboardHighlightsComponent } from './dashboard-highlights/dashboard-highlights.component';

@NgModule({
  declarations: [
    DashboardComponent,
    DashboardHighlightsComponent
  ],
  imports: [
    CommonModule,
    SharedModule.forRoot(),
    DashboardRoutingModule
  ]
})
export class DashboardModule { }
