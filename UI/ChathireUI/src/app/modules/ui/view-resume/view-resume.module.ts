import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from 'src/app/material';
import { NgxDocViewerModule } from 'ngx-doc-viewer';
import { SharedModule } from 'src/app/modules/shared/shared.module';

import { ViewResumeComponent } from './view-resume.component';

@NgModule({
  declarations: [
    ViewResumeComponent
  ],
  imports: [
    CommonModule,
    MaterialModule,
    NgxDocViewerModule,
    SharedModule
  ],
  exports: [
    ViewResumeComponent
  ]
})
export class ViewResumeModule { }
