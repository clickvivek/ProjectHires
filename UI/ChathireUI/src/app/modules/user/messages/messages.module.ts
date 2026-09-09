import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TalkModule } from 'src/app/modules/talk/talk.module';
import { SharedModule } from 'src/app/modules/shared/shared.module';

import { MessagesRoutingModule } from './messages-routing.module';
import { MessagesComponent } from './messages.component';


@NgModule({
  declarations: [
    MessagesComponent
  ],
  imports: [
    CommonModule,
    TalkModule,
    SharedModule.forRoot(),
    MessagesRoutingModule
  ]
})
export class MessagesModule { }
