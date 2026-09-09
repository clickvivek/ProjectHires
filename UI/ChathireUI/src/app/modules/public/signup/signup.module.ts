import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule  } from 'src/app/modules/shared/shared.module'

import { SignupRoutingModule } from './signup-routing.module';
import { SignupComponent } from './signup.component';


@NgModule({
  declarations: [
    SignupComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    SharedModule.forRoot(),
    ReactiveFormsModule,
    SignupRoutingModule
  ]
})
export class SignupModule { }
