import { Component, Input, ChangeDetectorRef } from '@angular/core';
import { AuthService } from 'src/app/core/auth/auth.service';
import { MatDialog } from '@angular/material/dialog';
import { DomSanitizer } from '@angular/platform-browser';
import { FileDownloadService } from '../../services/file-download.service';
import { LoginModalComponent } from '../login-modal/login-modal.component';

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
    public dialog: MatDialog
  ) {

  }

  isLoggedIn() {
    return this.authService.isLoggedIn();
  }

  private triggerDownloadFile(anchor: any) {
    this.fileDownloadService.downloadFile(this.doc, '1', 'resumes').subscribe((res: any) => {
      const blob = res.body;
      const objectURL = URL.createObjectURL(blob); 
      const finalUrl = `https://hiresblob.blob.core.windows.net/resumes/${this.doc}`;
      this.fileUrl = this.sanitizer.bypassSecurityTrustUrl(finalUrl);
      this.changeDetection.detectChanges();
      anchor.click();
    });
  }

  handleDownload(anchor: any) {
    if (this.isLoggedIn()) {
      this.triggerDownloadFile(anchor);
    } else {
      const dialogRef = this.dialog.open(LoginModalComponent, {
        width: '440px',
        panelClass: 'login-modal-panel',
        data: { actionText: 'download this resume' }
      });

      dialogRef.afterClosed().subscribe(res => {
        if (res && res.success) {
          this.triggerDownloadFile(anchor);
        }
      });
    }
  }

  ngOnInit() {

  }

}
