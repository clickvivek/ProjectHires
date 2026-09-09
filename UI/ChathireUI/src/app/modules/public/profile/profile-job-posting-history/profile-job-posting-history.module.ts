import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/modules/shared/shared.module';
import { ProfileJobPostingHistoryComponent } from './profile-job-posting-history.component';
import { ProfileJobPostingHistoryItemComponent } from './profile-job-posting-history-item/profile-job-posting-history-item.component';


@NgModule({
  declarations: [
    ProfileJobPostingHistoryComponent,
    ProfileJobPostingHistoryItemComponent
  ],
  imports: [
    CommonModule,
    SharedModule.forRoot()
  ],
  exports: [
    ProfileJobPostingHistoryComponent
  ]
})
export class ProfileJobPostingHistoryModule { }
