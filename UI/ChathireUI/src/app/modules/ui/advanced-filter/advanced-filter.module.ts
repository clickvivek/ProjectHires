import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdvFilterDatePostedComponent } from './adv-filter-date-posted/adv-filter-date-posted.component';
import { SharedModule } from 'src/app/modules/shared/shared.module';
import { AdvFilterVisaComponent } from './adv-filter-visa/adv-filter-visa.component';
import { AdvFilterExperienceLevelComponent } from './adv-filter-experience-level/adv-filter-experience-level.component';
import { AdvFilterWorkModelComponent } from './adv-filter-work-model/adv-filter-work-model.component';
import { AdvFilterResetComponent } from './adv-filter-reset/adv-filter-reset.component';
import { AdvFilterCardComponent } from './adv-filter-card/adv-filter-card.component';


@NgModule({
  declarations: [
    AdvFilterDatePostedComponent,
    AdvFilterVisaComponent,
    AdvFilterExperienceLevelComponent,
    AdvFilterWorkModelComponent,
    AdvFilterResetComponent,
    AdvFilterCardComponent,  ],
  imports: [
    CommonModule,
    SharedModule.forRoot()
  ],
  exports: [
    AdvFilterDatePostedComponent,
    AdvFilterVisaComponent,
    AdvFilterExperienceLevelComponent,
    AdvFilterWorkModelComponent,
    AdvFilterResetComponent,
    AdvFilterCardComponent
  ]
})
export class AdvancedFilterModule { }
