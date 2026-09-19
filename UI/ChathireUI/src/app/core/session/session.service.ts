import { Injectable } from '@angular/core';
import { Observable, of, BehaviorSubject } from 'rxjs';
import { Router } from '@angular/router';

import { UserService } from 'src/app/api';
import { SharedService } from 'src/app/modules/shared/services/shared.service';

@Injectable({
  providedIn: 'root'
})
export class SessionService {

  constructor(
    private _router: Router,
    private _userService: UserService,
    private _sharedService: SharedService
  ) { }

  set userId(id: any)
  {
    localStorage.setItem('ch_user_id', id);
  }

  get userId(): any
  {
      return Number(localStorage.getItem('ch_user_id'));
  }

  set userTypeId(id: any)
  {
    localStorage.setItem('ch_usertype_id', id);
  }

  get userTypeId(): any
  {
      return Number(localStorage.getItem('ch_usertype_id'));
  }

  set consultancyId(id: any)
  {
    localStorage.setItem('ch_consultancy_id', id);
  }

  get consultancyId(): any
  {
      return Number(localStorage.getItem('ch_consultancy_id'));
  }

  set consultancyUserId(id: any)
  {
    localStorage.setItem('ch_consultancyuser_id', id);
  }

  get consultancyUserId(): any
  {
      return Number(localStorage.getItem('ch_consultancyuser_id'));
  }

  set userEmail(email: any) {
    localStorage.setItem('ch_useremail', email);
  }

  get userEmail(): any {
    return localStorage.getItem('ch_useremail');
  }

  setRememberedUseremail(email: any) {
    localStorage.setItem('ch_remembered_useremail', email);
  }

  getRememberedUseremail(): any {
    return localStorage.getItem('ch_remembered_useremail');
  }

  removeRememberedUseremail(key: string): void {
    localStorage.removeItem(key);
  }

  userDetails = new BehaviorSubject(null)
  userdetailscast = this.userDetails.asObservable()

  setUserDetails(value:any) {
    this.userDetails.next(value)
  }

  getUserDetails() {
    return this.userDetails.value
  }

  user(data, skipRedirect: boolean = false): Observable<boolean> {

    this.userId = data.userId
    this.userTypeId = data.userTypeId
    this.consultancyId = data.consultancyId
    this.consultancyUserId = data.consultancyUserId
    this.userEmail = data.userEmail;

    this._userService.apiUserGetUserByUserNameGet(data.userEmail).subscribe((res: any) => {
      
      this.userDetails.next(res.value[0])

      if(!skipRedirect && !this._sharedService.isUserUpdate()) { // when personal details updated after login
    
        if(this._sharedService.getPageToRetain()) {

          let data = this._sharedService.getPageToRetain()
          this._router.navigateByUrl('/'+data.page);
          
        }
        else {
          this._router.navigate(['dashboard']);
        }
        
        this._sharedService.setUserUpdate(false)
      }
      return of(true);
    })

    return of(false);

  }

  refreshUser() {
    this._userService.apiUserGetUserByUserNameGet(this.userEmail).subscribe((res: any) => {
      this.userDetails.next(res.value[0])
      this._sharedService.setProfilePicLoader(false)
    })
  }

}
