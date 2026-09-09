import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/modules/shared/shared.module';
import { ProfileAvailableResourcesComponent } from './profile-available-resources.component';
import { ProfileAvailableResourcesItemComponent } from './profile-available-resources-item/profile-available-resources-item.component';
import { ProfileAvailableResourcesTableComponent } from './profile-available-resources-table/profile-available-resources-table.component';



@NgModule({
  declarations: [
    ProfileAvailableResourcesComponent,
    ProfileAvailableResourcesItemComponent,
    ProfileAvailableResourcesTableComponent
  ],
  imports: [
    CommonModule,
    SharedModule.forRoot()
  ],
  exports: [
    ProfileAvailableResourcesComponent,
    ProfileAvailableResourcesItemComponent
  ]
})
export class ProfileAvailableResourcesModule { }
