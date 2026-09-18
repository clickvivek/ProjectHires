import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from 'src/app/modules/shared/shared.module';
import { AdminSetupRoutingModule } from './admin-setup-routing.module';
import { AdminSetupComponent } from './admin-setup.component';

@NgModule({
  declarations: [
    AdminSetupComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    SharedModule.forRoot(),
    AdminSetupRoutingModule
  ]
})
export class AdminSetupModule { }
