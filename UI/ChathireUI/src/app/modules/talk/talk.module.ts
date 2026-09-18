import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from 'src/app/material';
import { ChatButtonDirective } from './talk.directive';
import { InboxDirective } from './talk.directive';

@NgModule({
  declarations: [
    ChatButtonDirective,
    InboxDirective
  ],
  imports: [
    CommonModule,
    MaterialModule
  ],
  exports: [
    ChatButtonDirective,
    InboxDirective
  ]
})

export class TalkModule {
  
}