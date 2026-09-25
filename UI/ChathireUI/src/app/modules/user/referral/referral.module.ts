import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from 'src/app/modules/shared/shared.module';
import { ReferralRoutingModule } from './referral-routing.module';
import { ReferralComponent } from './referral.component';

@NgModule({
  declarations: [
    ReferralComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    SharedModule.forRoot(),
    ReferralRoutingModule
  ]
})
export class ReferralModule { }
