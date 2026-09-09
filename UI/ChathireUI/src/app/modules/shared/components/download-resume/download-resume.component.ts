import { Component, Input, ChangeDetectorRef } from '@angular/core';
import { AuthService } from 'src/app/core/auth/auth.service';

import { DomSanitizer } from '@angular/platform-browser';
import { FileDownloadService } from '../../services/file-download.service';

@Component({
  selector: 'download-resume',
  templateUrl: './download-resume.component.html',
  styleUrls: ['./download-resume.component.scss']
})
export class DownloadResumeComponent {

  @Input() doc: any;
  fileUrl: any;

  constructor(
    private authService: AuthService,
    private sanitizer: DomSanitizer,
    private changeDetection: ChangeDetectorRef,
    private fileDownloadService: FileDownloadService,
  ) {

  }

  isLoggedIn() {
    return this.authService.isLoggedIn()
  }

  handleDownload(anchor) {

    if(this.isLoggedIn()) {
      this.fileDownloadService.downloadFile(this.doc, '1', 'resumes').subscribe((res: any) => {
        const blob = res.body;
        const objectURL = URL.createObjectURL(blob); 
        const finalUrl = `https://hiresblob.blob.core.windows.net/resumes/${this.doc}`
        this.fileUrl = this.sanitizer.bypassSecurityTrustUrl(finalUrl);
        this.changeDetection.detectChanges();
        anchor.click();
      })
    }
    else {
      this.authService.reDirectToLogin()
    }

  }

  ngOnInit() {

  }

}
