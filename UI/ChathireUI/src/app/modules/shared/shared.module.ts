
import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from 'src/app/material';
import { ToastrModule } from 'ngx-toastr';
import { NgScrollbarModule, NG_SCROLLBAR_OPTIONS } from 'ngx-scrollbar';
import { TalkModule } from 'src/app/modules/talk/talk.module';

import { SearchFieldComponent } from './components/search-field/search-field.component'
import { SelectFieldComponent } from './components/select-field/select-field.component'
import { MultiSearchFieldComponent } from './components/multi-search-field/multi-search-field.component'
import { MultiSelectFieldComponent } from './components/multi-select-field/multi-select-field.component';
import { PaginationBoxComponent } from './components/pagination-box/pagination-box.component'
import { CandidateDetailSheetComponent } from './components/candidate-detail-sheet/candidate-detail-sheet.component'
import { JobDetailSheetComponent } from './components/job-detail-sheet/job-detail-sheet.component';
import { ProfilePicComponent } from './components/profile-pic/profile-pic.component';

import { SimpleSearchPipe } from './pipe/shared.pipe';
import { EscapeHtmlPipe } from './pipe/shared.pipe';
import { subStringPipe } from './pipe/shared.pipe';
import { jobLocationPipe } from './pipe/shared.pipe';

import { AwakeClassDirective } from './directives/shared.directive';
import { SearchDirective } from './directives/shared.directive';
import { FilterBoxDirective } from './directives/shared.directive';
import { filterCancelDirective } from './directives/shared.directive';
import { showAdvFilterDirective } from './directives/shared.directive';
import { hideAdvFilterDirective  } from './directives/shared.directive';
import { SideNavDirective  } from './directives/shared.directive';
import { ProfileMapColorDirective } from './directives/shared.directive';
import { CustomTooltipDirective } from './directives/shared.directive';
import { ExternalLinkDirective } from './directives/shared.directive';

import { IntlTelInputComponent } from './components/intl-tel-input/intl-tel-input.component';
import { SearchSkillLocationComponent } from './components/search-skill-location/search-skill-location.component';
import { DownloadResumeComponent } from './components/download-resume/download-resume.component';

import { FileDownloadService } from './services/file-download.service';
import { DropdownClassicComponent } from './components/dropdown-classic/dropdown-classic.component';
import { ViewCommentsComponent } from './components/view-comments/view-comments.component';
import { LoaderComponent } from './components/loader/loader.component';
import { PageSearchComponent } from './components/page-search/page-search.component';
import { SliderClassicComponent } from './components/slider-classic/slider-classic.component';
import { SliderLargeComponent } from './components/slider-large/slider-large.component';
import { MultiSelectCheckFieldComponent } from './components/multi-select-check-field/multi-select-check-field.component';
import { SubmitBtnComponent } from './components/submit-btn/submit-btn.component';
import { FancyPageSearchComponent } from './components/fancy-page-search/fancy-page-search.component';

@NgModule({
  declarations: [
    SearchFieldComponent,
    SelectFieldComponent,
    MultiSearchFieldComponent,
    MultiSelectFieldComponent,
    PaginationBoxComponent,
    CandidateDetailSheetComponent,
    JobDetailSheetComponent,
    ProfilePicComponent,
    SimpleSearchPipe,
    EscapeHtmlPipe,
    subStringPipe,
    jobLocationPipe,
    AwakeClassDirective,
    SearchDirective,
    FilterBoxDirective,
    filterCancelDirective,
    showAdvFilterDirective,
    hideAdvFilterDirective,
    SideNavDirective,
    ProfileMapColorDirective,
    CustomTooltipDirective,
    ExternalLinkDirective,
    IntlTelInputComponent,
    SearchSkillLocationComponent,
    DownloadResumeComponent,
    DropdownClassicComponent,
    ViewCommentsComponent,
    LoaderComponent,
    PageSearchComponent,
    SliderClassicComponent,
    SliderLargeComponent,
    MultiSelectCheckFieldComponent,
    SubmitBtnComponent,
    FancyPageSearchComponent
],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    ReactiveFormsModule,
    MaterialModule,
    NgScrollbarModule,
    ToastrModule.forRoot(),
    TalkModule
  ],
  exports: [
    FormsModule,
    ReactiveFormsModule,
    MaterialModule,
    NgScrollbarModule,
    TalkModule,
    SearchFieldComponent,
    SelectFieldComponent,
    MultiSelectFieldComponent,
    MultiSearchFieldComponent,
    PaginationBoxComponent,
    CandidateDetailSheetComponent,
    IntlTelInputComponent,
    SearchSkillLocationComponent,
    DownloadResumeComponent,
    DropdownClassicComponent,
    ViewCommentsComponent,
    LoaderComponent,
    PageSearchComponent,
    SliderClassicComponent,
    SliderLargeComponent,
    MultiSelectCheckFieldComponent,
    JobDetailSheetComponent,
    ProfilePicComponent,
    SubmitBtnComponent,
    FancyPageSearchComponent,
    SimpleSearchPipe,
    EscapeHtmlPipe,
    subStringPipe,
    jobLocationPipe,
    AwakeClassDirective,
    SearchDirective,
    FilterBoxDirective,
    filterCancelDirective,
    hideAdvFilterDirective,
    SideNavDirective,
    showAdvFilterDirective,
    ProfileMapColorDirective,
    CustomTooltipDirective,
    ExternalLinkDirective
  ]
})
  
export class SharedModule {
  static forRoot(): ModuleWithProviders<SharedModule> {
    return {
      ngModule: SharedModule,
      providers: [FileDownloadService]
    };
  }
}
