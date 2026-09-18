import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-hotlist-share-modal',
  templateUrl: './hotlist-share-modal.component.html',
  styleUrls: ['./hotlist-share-modal.component.scss']
})
export class HotlistShareModalComponent implements OnInit {

  imageUrl: string = '';
  imageBlob: Blob | null = null;
  captionText: string = '';
  profileUrl: string = '';
  isCopiedText: boolean = false;
  isCopyingImage: boolean = false;
  themes: any[] = [];
  selectedThemeId: string = 'plum';
  onThemeChange: any;
  isReRendering: boolean = false;

  constructor(
    public dialogRef: MatDialogRef<HotlistShareModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private toastr: ToastrService
  ) {
    this.imageUrl = data?.imageUrl || '';
    this.imageBlob = data?.imageBlob || null;
    this.profileUrl = data?.profileUrl || '';
    this.captionText = data?.captionText || '';
    this.themes = data?.themes || [];
    this.selectedThemeId = data?.selectedThemeId || 'plum';
    this.onThemeChange = data?.onThemeChange || null;
  }

  ngOnInit(): void {}

  async selectTheme(themeId: string) {
    if (this.selectedThemeId === themeId || this.isReRendering) return;
    this.selectedThemeId = themeId;
    if (this.onThemeChange) {
      try {
        this.isReRendering = true;
        const result = await this.onThemeChange(themeId);
        if (result) {
          this.imageUrl = result.imageUrl;
          this.imageBlob = result.blob;
        }
      } catch (err) {
        console.error('Failed to update banner theme:', err);
      } finally {
        this.isReRendering = false;
      }
    }
  }

  copyCaption(showToast: boolean = true) {
    if (!this.captionText) return;
    navigator.clipboard.writeText(this.captionText);
    this.isCopiedText = true;
    if (showToast) {
      this.toastr.success('Caption text copied to clipboard!', '', {
        timeOut: 3000,
        positionClass: 'toast-top-center'
      });
    }
    setTimeout(() => this.isCopiedText = false, 2500);
  }

  shareOnLinkedInFlow() {
    // 1. Download image
    this.downloadImage();

    // 2. Copy caption to clipboard so Ctrl+V immediately pastes post text
    this.copyCaption(false);

    // 3. Open LinkedIn post creator
    window.open('https://www.linkedin.com/feed/?shareActive=true', '_blank');

    this.toastr.success('Image downloaded & caption copied! Paste (Ctrl+V) text and attach image in LinkedIn.', '', {
      timeOut: 6000,
      positionClass: 'toast-top-center'
    });
  }

  downloadImage() {
    if (!this.imageUrl) return;
    const a = document.createElement('a');
    a.href = this.imageUrl;
    a.download = `ChatHire-Hotlist-${new Date().toISOString().slice(0, 10)}.png`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
  }

  close() {
    this.dialogRef.close();
  }

}
