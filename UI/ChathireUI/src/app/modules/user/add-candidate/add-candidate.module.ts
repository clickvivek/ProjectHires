import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/modules/shared/shared.module';
import { RecaptchaModule } from "ng-recaptcha";
import { ViewResumeModule } from 'src/app/modules/ui/view-resume/view-resume.module';
import { AddCandidateRoutingModule } from './add-candidate-routing.module';
import { AddCandidateComponent } from './add-candidate.component';


@NgModule({
  declarations: [
    AddCandidateComponent
  ],
  imports: [
    CommonModule,
    SharedModule.forRoot(),
    ViewResumeModule,
    RecaptchaModule,
    AddCandidateRoutingModule
  ]
})
export class AddCandidateModule { }
