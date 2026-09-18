import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable, of, switchMap } from 'rxjs';
import { AuthService } from 'src/app/core/auth/auth.service';
import { SessionService } from 'src/app/core/session/session.service';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {

  constructor(
    private _authService: AuthService,
    private _sessionService: SessionService,
    private _router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    return this._authService.check().pipe(
      switchMap((authenticated: boolean) => {
        if (!authenticated) {
          this._router.navigate(['login']);
          return of(false);
        }

        const userTypeId = Number(this._sessionService.userTypeId);
        if (userTypeId === 7) {
          return of(true);
        }

        // Non-admin user, redirect to dashboard
        this._router.navigate(['/dashboard']);
        return of(false);
      })
    );
  }
}
