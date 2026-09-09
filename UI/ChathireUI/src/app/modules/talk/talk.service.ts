import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import Talk from 'talkjs';

@Injectable({
  providedIn: 'root'
})
export class TalkService {

  APP_ID = 'tKzUD2dn';

  constructor(
    private http: HttpClient
  ) { }

  fetchTalkUserPresence(id) {
    return this.http.post(`https://api.talkjs.com/v1/tKzUD2dn/presences/`, {userIds : [id]})
  }

}
