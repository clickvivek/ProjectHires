import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/modules/shared/shared.module';
import { AdvancedFilterModule } from 'src/app/modules/ui/advanced-filter/advanced-filter.module';
import { ViewResumeModule } from 'src/app/modules/ui/view-resume/view-resume.module';
import { SearchHotlistRoutingModule } from './search-hotlist-routing.module';
import { SearchHotlistComponent } from './search-hotlist.component';
import { SearchHotlistChatComponent } from './search-hotlist-chat/search-hotlist-chat.component';


@NgModule({
  declarations: [
    SearchHotlistComponent,
    SearchHotlistChatComponent
  ],
  imports: [
    CommonModule,
    SharedModule.forRoot(),
    ViewResumeModule,
    AdvancedFilterModule,
    SearchHotlistRoutingModule
  ]
})
export class SearchHotlistModule { }
