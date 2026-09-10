import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

import { AuthService } from 'src/app/core/auth/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor
{
    /**
     * Constructor
     *
     * @param {AuthService} _authService
     */

    request:any;
    isRefreshing:boolean = false;

    //talkjs secret key
    secretKey = environment.talkJsSecretKey;

    constructor(
        private _authService: AuthService
    )
    {}


    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>>
    {


        const accessToken = this._authService.token;

        var requestUrl;
        var isApiUrl = req.url.includes('/api');

        if(isApiUrl)
        requestUrl = req.url
        else
        requestUrl = ''

        if ( accessToken )
        {   
            //talkjs
            if(req.url.includes('talkjs.com')) {

                this.request = req.clone({
                    url: req.url,
                    setHeaders: {
                        Authorization: 'Bearer ' + this.secretKey,
                        'Content-Type': 'application/json'
                    }
                });

            }
            else {

                this.request = req.clone({
                    url: requestUrl,
                    setHeaders: {
                      Authorization: 'Bearer ' + accessToken,
                      "Set-Cookie": "jsessionid=oIZEL75SLnw;HttpOnly;Secure;SameSite=Strict"
                    }
                });

            }

        }
        else {
            this.request = req.clone({
                url: requestUrl
            });
        }

        // Handle response
        return next.handle(this.request)

    }


}


