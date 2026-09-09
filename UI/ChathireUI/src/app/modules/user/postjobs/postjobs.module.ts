import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule  } from 'src/app/modules/shared/shared.module'
import { PostJobDetailsModule } from 'src/app/modules/ui/pages/post-job-details/post-job-details.module';

import { PostjobsRoutingModule } from './postjobs-routing.module';
import { PostjobsComponent } from './postjobs.component';


@NgModule({
  declarations: [
    PostjobsComponent
  ],
  imports: [
    CommonModule,
    SharedModule.forRoot(),
    PostJobDetailsModule,
    PostjobsRoutingModule
  ],
  exports: [
    PostjobsComponent
  ]
})
export class PostjobsModule { }
