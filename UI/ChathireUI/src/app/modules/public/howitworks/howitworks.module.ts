import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { HowitworksRoutingModule } from './howitworks-routing.module';
import { HowitworksComponent } from './howitworks.component';
import { SharedModule } from 'src/app/modules/shared/shared.module';

@NgModule({
  declarations: [
    HowitworksComponent
  ],
  imports: [
    CommonModule,
    HowitworksRoutingModule,
    SharedModule.forRoot()
  ]
})
export class HowitworksModule { }
