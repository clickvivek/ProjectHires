import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

export interface ConfirmActionModalData {
  type: 'repost' | 'delete' | 'deactivate' | 'info';
  title?: string;
  jobTitle?: string;
  message?: string;
  confirmBtnText?: string;
  cancelBtnText?: string;
  details?: string[];
}

@Component({
  selector: 'app-confirm-action-modal',
  templateUrl: './confirm-action-modal.component.html',
  styleUrls: ['./confirm-action-modal.component.scss']
})
export class ConfirmActionModalComponent {

  constructor(
    public dialogRef: MatDialogRef<ConfirmActionModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmActionModalData
  ) {}

  onConfirm(): void {
    this.dialogRef.close(true);
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
