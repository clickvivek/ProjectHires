import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';

import Talk from 'talkjs';

import { SessionService } from 'src/app/core/session/session.service';

@Injectable({
  providedIn: 'root'
})
export class TalkService {

  APP_ID = environment.talkJsAppId;

  constructor(
    private http: HttpClient,
    private sessionService: SessionService
  ) { }

  fetchTalkUserPresence(id) {
    return this.http.post(`https://api.talkjs.com/v1/${this.APP_ID}/presences/`, {userIds : [id]})
  }

  initiateChat(chatUserId: number, userId?: number) {
    const userDetails: any = this.sessionService.getUserDetails();
    const currentUserId = userId || this.sessionService.userId || userDetails?.id;
    return this.http.post<any>(`${environment.rootUrl}/api/Subscription/InitiateChat`, {
      userId: currentUserId ? Number(currentUserId) : null,
      chatUserId: Number(chatUserId)
    });
  }

}
