import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';

import { CoreModule } from 'src/app/core/core.module';
import { RegularsModule } from './modules/regulars/regulars.module';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { BASE_PATH } from 'src/app/api';
import { environment } from 'src/environments/environment';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    CoreModule.forRoot(),
    RegularsModule,
    BrowserAnimationsModule,
    HttpClientModule
  ],
  providers: [
    { provide: BASE_PATH, useValue: environment.rootUrl }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
