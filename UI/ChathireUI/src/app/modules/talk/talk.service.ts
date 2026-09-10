import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';

import Talk from 'talkjs';

@Injectable({
  providedIn: 'root'
})
export class TalkService {

  APP_ID = environment.talkJsAppId;

  constructor(
    private http: HttpClient
  ) { }

  fetchTalkUserPresence(id) {
    return this.http.post(`https://api.talkjs.com/v1/${this.APP_ID}/presences/`, {userIds : [id]})
  }

}
