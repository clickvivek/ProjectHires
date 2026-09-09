import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AuthGuard } from './core/auth/guards/auth.guard';

const routes: Routes = [
  { path: 'home', canActivate: [ AuthGuard ], loadChildren: () => import('./modules/public/home/home.module').then(m => m.HomeModule) },
  { path: 'about', loadChildren: () => import('./modules/public/about/about.module').then(m => m.AboutModule) },
  { path: 'contact', loadChildren: () => import('./modules/public/contact/contact.module').then(m => m.ContactModule) },
  { path: 'faq', loadChildren: () => import('./modules/public/faq/faq.module').then(m => m.FaqModule) },
  { path: 'terms', loadChildren: () => import('./modules/public/terms/terms.module').then(m => m.TermsModule) },
  { path: 'privacy', loadChildren: () => import('./modules/public/privacy/privacy.module').then(m => m.PrivacyModule) },
  { path: 'login', canActivate: [ AuthGuard ], loadChildren: () => import('./modules/public/login/login.module').then(m => m.LoginModule) },
  { path: 'signup', canActivate: [ AuthGuard ], loadChildren: () => import('./modules/public/signup/signup.module').then(m => m.SignupModule) },
  { path: 'postjobs', canActivate: [ AuthGuard ], loadChildren: () => import('./modules/user/postjobs/postjobs.module').then(m => m.PostjobsModule) },
  { path: 'dashboard', canActivate: [AuthGuard], loadChildren: () => import('./modules/user/dashboard/dashboard.module').then(m => m.DashboardModule) },
  { path: 'myhotlist', canActivate: [AuthGuard], loadChildren: () => import('./modules/user/myhotlist/myhotlist.module').then(m => m.MyhotlistModule) },
  { path: 'editcandidate/:id', loadChildren: () => import('./modules/user/add-candidate/add-candidate.module').then(m => m.AddCandidateModule)  },
  { path: 'addcandidate', canActivate: [AuthGuard], loadChildren: () => import('./modules/user/add-candidate/add-candidate.module').then(m => m.AddCandidateModule) },
  { path: 'profile/:id', loadChildren: () => import('./modules/public/profile/profile.module').then(m => m.ProfileModule) },
  { path: 'myaccount', canActivate: [AuthGuard], loadChildren: () => import('./modules/user/my-account/my-account.module').then(m => m.MyAccountModule) },
  { path: 'search-jobs', loadChildren: () => import('./modules/public/searchjobs/searchjobs.module').then(m => m.SearchjobsModule) },
  { path: 'search-hotlist', loadChildren: () => import('./modules/public/search-hotlist/search-hotlist.module').then(m => m.SearchHotlistModule) },
  { path: 'posting-history', canActivate: [AuthGuard], loadChildren: () => import('./modules/user/posting-history/posting-history.module').then(m => m.PostingHistoryModule) },
  { path: 'inbox', canActivate: [ AuthGuard ], loadChildren: () => import('./modules/user/inbox/inbox.module').then(m => m.InboxModule) },
  { path: 'chat', canActivate: [ AuthGuard ], loadChildren: () => import('./modules/user/messages/messages.module').then(m => m.MessagesModule) },
  { path: 'jobs', canActivate: [ AuthGuard ], loadChildren: () => import('./modules/public/job-details-page/job-details-page.module').then(m => m.JobDetailsPageModule) },
  { path: '', pathMatch: 'full', redirectTo: 'home' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { useHash: true })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
