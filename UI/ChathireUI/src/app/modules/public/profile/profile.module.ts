import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule  } from 'src/app/modules/shared/shared.module'
import { ProfileJobPostingHistoryModule } from './profile-job-posting-history/profile-job-posting-history.module';
import { ProfileAvailableResourcesModule } from './profile-available-resources/profile-available-resources.module';

import { ProfileRoutingModule } from './profile-routing.module';
import { ProfileComponent } from './profile.component';


@NgModule({
  declarations: [
    ProfileComponent
  ],
  imports: [
    CommonModule,
    SharedModule.forRoot(),
    ProfileJobPostingHistoryModule,
    ProfileAvailableResourcesModule,
    ProfileRoutingModule
  ]
})
export class ProfileModule { }
