
import { DOCUMENT } from '@angular/common';

import { Component, Inject, ViewChild, ElementRef, Renderer2, AfterViewInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';

import _ from 'underscore';

import { SessionService } from 'src/app/core/session/session.service';
import { AuthService } from './core/auth/auth.service';
import { SharedService } from './modules/shared/services/shared.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent   {

  user:any;
  isNotHomeRoute:boolean = false;

  element:any;

  @ViewChild('talkjsContainer') talkjsContainer!: ElementRef;

  constructor(
    public router: Router,
    @Inject(DOCUMENT) public document: Document,
    r: Renderer2,
    private sessionService: SessionService,
    private authService: AuthService,
    private sharedService: SharedService
    ) {

    const checkIsHome = (url: string) => {
      const clean = (url || '').split('?')[0].replace('#', '');
      return clean === '' || clean === '/' || clean === '/home';
    };

    const initialHref = window.location.hash || window.location.pathname || '';
    this.isNotHomeRoute = !checkIsHome(initialHref);

    router.events.subscribe((event: any) => {
      
      if (event instanceof NavigationEnd) {

        let path = this.router.url;
        var name: any = path.split('/');        
        
        document.body.className = "";

        if (name[1] && name[1].includes("?")) {
          let finalname = name[1].split("?");
          r.addClass(document.body, finalname[0]);
        }
        else if (name[1]) {
            r.addClass(document.body, name[1]);
        }
          
        this.isNotHomeRoute = !checkIsHome(this.router.url);

        if(this.router.url != '/login' && !this.router.url.includes('/search-jobs')) {
          this.sharedService.setPageToRetain(null)
        }

       }

    });

  }

  ngOnInit() {

    this.sessionService.userdetailscast.subscribe((res: any) => {
      this.user = res
    })

  }

  

}

