import { Component, Input, ChangeDetectorRef } from '@angular/core';
import { AuthService } from 'src/app/core/auth/auth.service';
import { MatDialog } from '@angular/material/dialog';
import { DomSanitizer } from '@angular/platform-browser';
import { ToastrService } from 'ngx-toastr';
import { SessionService } from 'src/app/core/session/session.service';
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
    private sessionService: SessionService,
    private sanitizer: DomSanitizer,
    private changeDetection: ChangeDetectorRef,
    private fileDownloadService: FileDownloadService,
    private toastr: ToastrService,
    public dialog: MatDialog
  ) {

  }

  isLoggedIn() {
    return this.authService.isLoggedIn();
  }

  private triggerDownloadFile(anchor?: any) {
    if (!this.doc) return;

    const currentUserId = this.sessionService.userId;

    this.fileDownloadService.checkDownloadResume(this.doc, currentUserId).subscribe({
      next: (res: any) => {
        const checkResult = res?.value;
        if (checkResult && !checkResult.canDownload) {
          this.toastr.warning(
            checkResult.message || 'You have reached your resume download limit for the current cycle.',
            'Download Limit Reached',
            { timeOut: 7000 }
          );
          return;
        }

        // Proceed to download
        this.executeFileDownload();
      },
      error: () => {
        // Fallback to direct attempt if check endpoint has unexpected error
        this.executeFileDownload();
      }
    });
  }

  private executeFileDownload() {
    this.fileDownloadService.downloadFile(this.doc, '1', 'resumes').subscribe({
      next: (res: any) => {
        const blob = res.body;
        if (blob) {
          const objectURL = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = objectURL;
          link.download = this.doc;
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
          setTimeout(() => window.URL.revokeObjectURL(objectURL), 1000);
        }
      },
      error: (err: any) => {
        if (err?.status === 403) {
          this.toastr.warning(
            'Resume download quota exceeded for your subscription plan.',
            'Download Limit Reached',
            { timeOut: 7000 }
          );
          return;
        }
        const finalUrl = `https://hiresblob.blob.core.windows.net/resumes/${this.doc}`;
        const link = document.createElement('a');
        link.href = finalUrl;
        link.download = this.doc;
        link.target = '_blank';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
      }
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
