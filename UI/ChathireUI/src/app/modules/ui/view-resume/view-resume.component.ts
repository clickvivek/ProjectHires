import { Component, Input, ViewChild, SimpleChanges, TemplateRef, ViewEncapsulation } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from 'src/app/core/auth/auth.service';
import { SessionService } from 'src/app/core/session/session.service';
import { LoginModalComponent } from 'src/app/modules/shared/components/login-modal/login-modal.component';
import { FileDownloadService } from 'src/app/modules/shared/services/file-download.service';

@Component({
  selector: 'view-resume',
  templateUrl: './view-resume.component.html',
  styleUrls: ['./view-resume.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class ViewResumeComponent {

  @Input() doc: any;
  fileType: string = "";
  fileUrl: string = "";
  loadDoc: boolean = false;

  defaultFileTypes = ['ppt', 'pptx', 'doc', 'docx', 'xls', 'xlsx', 'pdf'];

  @ViewChild('resumeViewerTemplate') resumeViewerTemplate: TemplateRef<any>;
  private dialogRef: MatDialogRef<any> | null = null;

  constructor(
    private authService: AuthService,
    private sessionService: SessionService,
    private fileDownloadService: FileDownloadService,
    private toastr: ToastrService,
    public dialog: MatDialog
  ) {}

  private resolveFile() {
    if (!this.doc) return;
    const parts = this.doc.split('.');
    this.fileType = parts.length > 1 ? parts[parts.length - 1].toLowerCase() : '';
    if (this.doc.startsWith('http://') || this.doc.startsWith('https://')) {
      this.fileUrl = this.doc;
    } else {
      this.fileUrl = `https://hiresblob.blob.core.windows.net/resumes/${this.doc}`;
    }
  }

  isDoc() {
    return this.defaultFileTypes.some(item => item === this.fileType);
  }

  openViewer() {
    this.resolveFile();
    this.loadDoc = true;

    this.dialogRef = this.dialog.open(this.resumeViewerTemplate, {
      position: { right: '0px', top: '0px' },
      height: '100vh',
      width: '850px',
      maxWidth: '95vw',
      panelClass: 'resume-slide-drawer-panel',
      hasBackdrop: true,
      autoFocus: false,
      restoreFocus: false
    });

    this.dialogRef.afterClosed().subscribe(() => {
      this.dialogRef = null;
      this.loadDoc = false;
    });
  }

  handleResume() {
    if (this.authService.isLoggedIn()) {
      this.openViewer();
    } else {
      const dialogRef = this.dialog.open(LoginModalComponent, {
        width: '440px',
        panelClass: 'login-modal-panel',
        data: { actionText: 'view this resume' }
      });

      dialogRef.afterClosed().subscribe(res => {
        if (res && res.success) {
          this.openViewer();
        }
      });
    }
  }

  downloadResume() {
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

        this.executeFileDownload();
      },
      error: () => {
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

  closeViewer() {
    if (this.dialogRef) {
      this.dialogRef.close();
    }
  }

  ngOnInit() {
    this.resolveFile();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['doc']) {
      this.resolveFile();
    }
  }

}

