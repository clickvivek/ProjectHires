import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/core/auth/auth.service';
import _ from 'underscore';

@Component({
  selector: 'app-postjobs',
  templateUrl: './postjobs.component.html',
  styleUrls: ['./postjobs.component.scss']
})
export class PostjobsComponent implements OnInit {

  isJobPosted: boolean = false;
  isLoggedIn: boolean = false;
  showFormDirectly: boolean = false;

  constructor(
    private authService: AuthService,
    public router: Router
  ) {}

  ngOnInit(): void {
    this.isLoggedIn = this.authService.isLoggedIn();
  }

  handleJobPost(event: any): void {
    this.isJobPosted = event;
  }

  startDirectPosting(): void {
    this.showFormDirectly = true;
  }
}
