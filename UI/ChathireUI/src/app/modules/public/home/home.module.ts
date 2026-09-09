import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HomeComponent } from './home.component';
import { SharedModule  } from 'src/app/modules/shared/shared.module'
import { HomeRoutingModule } from './home-routing.module';
import { HomeSearchTabsComponent } from './home-search-tabs/home-search-tabs.component';



@NgModule({
  declarations: [
    HomeComponent,
    HomeSearchTabsComponent
  ],
  imports: [
    CommonModule,
    HomeRoutingModule,
    SharedModule.forRoot()
  ]
})
export class HomeModule { }
