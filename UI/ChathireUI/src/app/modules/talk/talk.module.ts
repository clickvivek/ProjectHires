import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChatButtonDirective } from './talk.directive';
import { InboxDirective } from './talk.directive';


@NgModule({
  declarations: [
    ChatButtonDirective,
    InboxDirective
  ],
  imports: [
    CommonModule
  ],
  exports: [
    ChatButtonDirective,
    InboxDirective
  ]
})

export class TalkModule {
  
}