import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable, of, switchMap } from 'rxjs';

import { AuthService } from 'src/app/core/auth/auth.service';
import { authRoutes, publicRoutes } from './routes';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(
    private _authService: AuthService,
    private _router: Router
){
}


  private _check(redirectURL): Observable<any>
  {

    return this._authService.check()
    .pipe(
      switchMap((authenticated:any) => {
        
        if (!authenticated && authRoutes.includes(redirectURL) ) {
          this._router.navigate(['login']);

          // Prevent the access
          return of(false);

        }

        if (authenticated && publicRoutes.includes(redirectURL)) {
            this._router.navigate(['/dashboard']);
        }
        
        // Allow the access
        return of(true);

      })
  );

  }

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {

      let redirectUrl = state.url;

      return this._check(redirectUrl);
  }

}
