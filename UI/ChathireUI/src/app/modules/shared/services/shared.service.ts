import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';

const httpOptions = {
    headers: new HttpHeaders({
      'Content-Type' : 'application/x-www-form-urlencoded; charset=UTF-8'
    })
};

@Injectable({
  providedIn: 'root'
})


export class SharedService {

  private passwordreset = new BehaviorSubject<boolean>(false);
  passwordresetcast = this.passwordreset.asObservable();

  private skillSetData = new BehaviorSubject<any>(null);
  skillsetdatacast = this.skillSetData.asObservable();

  private locationSetData = new BehaviorSubject<any>(null);
  locationsetdatacast = this.locationSetData.asObservable();

  private sideNavData = new BehaviorSubject<any>(null);
  sidenavdatacast = this.sideNavData.asObservable();

  private userupdate = new BehaviorSubject<boolean>(false);
  userupdatecast = this.userupdate.asObservable();

  private inboxUnReadCount = new BehaviorSubject<boolean>(false);
  inboxunreadcountcast = this.inboxUnReadCount.asObservable();

  private pageToRetain = new BehaviorSubject<any>(null);
  pagetoretaincast = this.pageToRetain.asObservable();

  private profilePicLoader = new BehaviorSubject<boolean>(false);
  profilepicloadercast = this.profilePicLoader.asObservable();

  constructor(
    private http: HttpClient
  ) { }

  isResetPassword(value:any) {
    this.passwordreset.next(value);
  }

  getJsonData(): Observable<any>{
    return this.http.get('assets/json/data.json');
  }

  setSkillData(value:any){
    this.skillSetData.next(value);
  }

  setLocationData(value:any){
    this.locationSetData.next(value);
  }

  setSideNavData(value:any){
    this.sideNavData.next(value);
  }

  getInboxUnReadCount() {
    return this.inboxUnReadCount.value
  }

  setInboxUnReadCount(value:any){
    this.inboxUnReadCount.next(value);
  }

  getSideNavData() {
    return this.sideNavData.value
  }

  setUserUpdate(value:any){
    this.userupdate.next(value);
  }

  getPageToRetain() {
    return this.pageToRetain.value
  }

  setPageToRetain(value:any){
    this.pageToRetain.next(value);
  }

  getProfilePicLoader() {
    return this.profilePicLoader.value
  }

  setProfilePicLoader(value:any){
    this.profilePicLoader.next(value);
  }

  isUserUpdate() {
    return this.userupdate.value
  }

}
