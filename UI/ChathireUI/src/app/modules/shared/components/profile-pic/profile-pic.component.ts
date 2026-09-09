import { Component, Input } from '@angular/core';
import { picUrl, defaultProfilePic } from 'src/app/data/various';
import { UserService } from 'src/app/api';
import { SessionService } from 'src/app/core/session/session.service';
import { SharedService } from '../../services/shared.service';

@Component({
  selector: 'profile-pic',
  templateUrl: './profile-pic.component.html',
  styleUrls: ['./profile-pic.component.scss']
})
export class ProfilePicComponent {

  @Input() isPublic;
  @Input() user;

  selectedFile: any | null = null;
  profilePicUrl:any = ""

  isLoading:boolean = false;
  
  constructor(
    private _userService: UserService,
    private _sessionService: SessionService,
    private _sharedService: SharedService
  ) { }

  uploadFile(event: any) {
    this.selectedFile = event.target.files[0];
    this._sharedService.setProfilePicLoader(true)
    this._userService.apiUserUploadProfilePicPut(this._sessionService.userEmail, this.selectedFile).subscribe({
      next: (res: any) => { 
        this._sessionService.refreshUser()
      },
      error: (error: any) => { },
    })
  }
  ngOnChanges() {
    if (this.user && this.user?.profilePic) {
      this.profilePicUrl = `${picUrl}${this.user?.profilePic}`
    }
    else {
      this.profilePicUrl = defaultProfilePic
    }
  }

  ngOnInit() {

    this._sharedService.profilepicloadercast.subscribe((res:any) => {
      this.isLoading = res
    })

  }

}
