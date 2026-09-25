import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from 'src/app/modules/shared/shared.module';
import { CandidateProfileRoutingModule } from './candidate-profile-routing.module';
import { CandidateProfileComponent } from './candidate-profile.component';

@NgModule({
  declarations: [
    CandidateProfileComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    SharedModule.forRoot(),
    CandidateProfileRoutingModule
  ]
})
export class CandidateProfileModule { }
