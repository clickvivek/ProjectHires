import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/modules/shared/shared.module';

import { MyAccountRoutingModule } from './my-account-routing.module';
import { MyAccountComponent } from './my-account.component';
import { PersonalDetailsComponent } from './personal-details/personal-details.component';
import { ChangePasswordComponent } from './change-password/change-password.component';


@NgModule({
  declarations: [
    MyAccountComponent,
    PersonalDetailsComponent,
    ChangePasswordComponent
  ],
  imports: [
    CommonModule,
    SharedModule.forRoot(),
    MyAccountRoutingModule
  ]
})
export class MyAccountModule { }
