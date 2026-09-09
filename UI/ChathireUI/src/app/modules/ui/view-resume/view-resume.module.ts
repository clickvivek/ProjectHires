import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from 'src/app/material';
import { NgxDocViewerModule } from 'ngx-doc-viewer';

import { ViewResumeComponent } from './view-resume.component';



@NgModule({
  declarations: [
    ViewResumeComponent
  ],
  imports: [
    CommonModule,
    MaterialModule,
    NgxDocViewerModule
  ],
  exports: [
    ViewResumeComponent
  ]
})
export class ViewResumeModule { }
