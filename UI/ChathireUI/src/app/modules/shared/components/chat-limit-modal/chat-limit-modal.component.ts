import { Component, Inject, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ReferralModalComponent } from '../referral-modal/referral-modal.component';

@Component({
  selector: 'app-chat-limit-modal',
  templateUrl: './chat-limit-modal.component.html',
  styleUrls: ['./chat-limit-modal.component.scss']
})
export class ChatLimitModalComponent implements OnInit, OnDestroy {

  dailyLimit: number = 20;
  usedChatsToday: number = 20;
  remainingChatsToday: number = 0;
  nextSlotAvailableAtUtc: string = '';
  nextSlotWaitSeconds: number = 0;
  nextSlotWaitText: string = '';
  formattedLocalTime: string = '';
  message: string = '';

  private timerInterval: any = null;

  constructor(
    public dialogRef: MatDialogRef<ChatLimitModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private router: Router,
    private dialog: MatDialog
  ) {
    if (data) {
      this.dailyLimit = data.dailyLimit || 20;
      this.usedChatsToday = data.usedChatsToday || this.dailyLimit;
      this.remainingChatsToday = data.remainingChatsToday || 0;
      this.nextSlotAvailableAtUtc = data.nextSlotAvailableAtUtc || '';
      this.nextSlotWaitSeconds = data.nextSlotWaitSeconds || 0;
      this.nextSlotWaitText = data.nextSlotWaitText || '';
      this.message = data.message || '';
    }
  }

  ngOnInit(): void {
    this.calculateLocalTime();
    this.startCountdown();
  }

  private calculateLocalTime(): void {
    if (this.nextSlotAvailableAtUtc) {
      try {
        const d = new Date(this.nextSlotAvailableAtUtc);
        this.formattedLocalTime = d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
      } catch {}
    }
  }

  private startCountdown(): void {
    if (this.nextSlotWaitSeconds > 0) {
      this.timerInterval = setInterval(() => {
        this.nextSlotWaitSeconds--;
        if (this.nextSlotWaitSeconds <= 0) {
          this.nextSlotWaitText = 'a few seconds';
          clearInterval(this.timerInterval);
        } else {
          this.nextSlotWaitText = this.formatSeconds(this.nextSlotWaitSeconds);
        }
      }, 1000);
    }
  }

  private formatSeconds(sec: number): string {
    const hours = Math.floor(sec / 3600);
    const minutes = Math.floor((sec % 3600) / 60);
    const seconds = sec % 60;
    if (hours > 0) {
      return `${hours} hr${hours > 1 ? 's' : ''} ${minutes} min${minutes > 1 ? 's' : ''}`;
    }
    if (minutes > 0) {
      return `${minutes} min${minutes > 1 ? 's' : ''}`;
    }
    return `${seconds} sec${seconds > 1 ? 's' : ''}`;
  }

  ngOnDestroy(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  goToReferral(): void {
    this.dialogRef.close({ action: 'referral' });
    this.dialog.open(ReferralModalComponent, {
      width: '680px',
      maxWidth: '95vw',
      panelClass: 'referral-modal-panel'
    });
  }

  close(): void {
    this.dialogRef.close({ action: 'close' });
  }
}
