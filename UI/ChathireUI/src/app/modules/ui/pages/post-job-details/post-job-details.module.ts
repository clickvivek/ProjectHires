import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/modules/shared/shared.module';
import { QuillModule } from 'ngx-quill'

import { PostJobDetailsComponent } from './post-job-details.component';

@NgModule({
  declarations: [
    PostJobDetailsComponent
  ],
  imports: [
    CommonModule,
    RouterModule,
    SharedModule.forRoot(),
    QuillModule.forRoot(),
  ],
  exports: [
    PostJobDetailsComponent,
    RouterModule
  ]
})
export class PostJobDetailsModule { }
