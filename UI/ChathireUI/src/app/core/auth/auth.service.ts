import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Router } from '@angular/router';

import { SessionService } from '../session/session.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {


  // Private
  private _authenticated: boolean;

  constructor(
    private _router: Router,
    private sessionService: SessionService
  ) {
    // Set the defaults
    this._authenticated = false;
  }

  set token(id: any) {
    localStorage.setItem('ch_token', id);
  }

  get token(): any {
    return localStorage.getItem('ch_token');
  }

  isLoggedIn() {
    return this.token != null ? true : false
  }

  login(user:any) {

    // Set the authenticated flag to true
    this._authenticated = true;

    this.token = user.token
    this.sessionService.user(user)

    this.destroyChatPopup()

  }

  reDirectToLogin() {
    this._router.navigate(['login']);
  }

  destroyChatPopup() {

    let talkElementLanucher:any = document.querySelector('#__talkjs_launcher')
    if(talkElementLanucher) {
      const parent = talkElementLanucher.parentNode
      parent.remove()
    }
    

  }

  logout(): Observable<any>
    {

      localStorage.removeItem('ch_token');
      localStorage.removeItem('ch_user_id');
      localStorage.removeItem('ch_usertype_id');
      localStorage.removeItem('ch_useremail');
      localStorage.removeItem('ch_consultancy_id');
      localStorage.removeItem('ch_consultancyuser_id');

      this.destroyChatPopup()

      // Set the authenticated flag to false
      this._authenticated = false;
      
      // Allow the access
      return of(true);

    }

    check(): Observable<boolean>
    {

          // Check the access token availability
          if ( !this.token )
          {
              return of(false);
          }

          // Check if the user is logged in
          if ( this._authenticated )
          {
              return of(true);
          }

          return of(true);

    }

}
