import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/core/auth/auth.service';
import { generateObfuscatedJobId } from 'src/app/modules/shared/utils/job-url.util';
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
  postedJobId: string | null = null;
  postedJobTitle: string = '';

  constructor(
    private authService: AuthService,
    public router: Router
  ) {}

  ngOnInit(): void {
    this.isLoggedIn = this.authService.isLoggedIn();
  }

  handleJobPost(event: any): void {
    this.isJobPosted = true;
    if (event && typeof event === 'object') {
      const numericId = event.id || event.jobOpeningId;
      if (numericId) {
        this.postedJobId = generateObfuscatedJobId(numericId);
      }
      this.postedJobTitle = event.title || event.jobTitle || '';
    }
  }

  resetJobPost(): void {
    this.isJobPosted = false;
    this.postedJobId = null;
    this.postedJobTitle = '';
  }

  startDirectPosting(): void {
    this.showFormDirectly = true;
  }
}
