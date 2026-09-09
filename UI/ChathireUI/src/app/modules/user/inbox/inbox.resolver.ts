import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, RouterStateSnapshot, Router, ResolveFn } from '@angular/router';
import { Observable, of } from 'rxjs';

import { SharedService } from '../../shared/services/shared.service';



@Injectable({
  providedIn: 'root',
})
export class inboxResolver  {

  constructor(
    private router: Router,
    private sharedService: SharedService
  ) {}

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any> | Promise<any> | any
  {

    const data = this.sharedService.getSideNavData()

    if(data == null) {
      this.router.navigateByUrl('/inbox');
    }

    return;

  }

}
