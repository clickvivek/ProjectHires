import { Component, Input, ViewChild, ElementRef, SimpleChanges  } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { AuthService } from 'src/app/core/auth/auth.service';
import { LoginModalComponent } from 'src/app/modules/shared/components/login-modal/login-modal.component';

@Component({
  selector: 'view-resume',
  templateUrl: './view-resume.component.html',
  styleUrls: ['./view-resume.component.scss']
})
export class ViewResumeComponent {

  @Input() doc: any;
  fileType: string = ""
  fileUrl: string = ""
  isViewer: boolean = false;

  loadDoc:boolean = false;

  defaultFileTypes = ['ppt', 'pptx', 'doc', 'docx', 'xls', 'xlsx', 'pdf']

  @ViewChild('viewResumeImgElem') viewResumeImgElem: ElementRef;
  

  constructor(
    private authService: AuthService,
    public dialog: MatDialog
  ) {

  }

  onSidenavOpened() {
    return true
  }

  private openViewer() {
    this.isViewer = true;
    setTimeout(() => {
      this.loadDoc = true;
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

  isDoc() {
   return this.defaultFileTypes.some(item => item === this.fileType);
  }

  closeViewer() {
    this.isViewer = false
    setTimeout(() => {
      this.loadDoc = false
    })
  }

  ngOnInit() {
    const resume = this.doc.split('.')
    this.fileType = resume[1].toLowerCase()
    this.fileUrl = `https://hiresblob.blob.core.windows.net/resumes/${this.doc}`
  }

  ngOnChanges(changes: SimpleChanges) {
		
  }

}
