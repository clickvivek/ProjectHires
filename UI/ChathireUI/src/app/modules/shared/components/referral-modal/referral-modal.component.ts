import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { ReferralService, SubmitReferralResponseDto } from 'src/app/core/services/referral.service';
import { SessionService } from 'src/app/core/session/session.service';

@Component({
  selector: 'app-referral-modal',
  templateUrl: './referral-modal.component.html',
  styleUrls: ['./referral-modal.component.scss']
})
export class ReferralModalComponent implements OnInit {

  emails: string[] = Array(10).fill('');
  batchPasteInput: string = '';
  showBatchPaste: boolean = false;

  customMessage: string = "Join ChatHire to find verified candidates and post jobs. Use my referral link to get free job postings!";
  isSubmitting: boolean = false;
  successResult: SubmitReferralResponseDto | null = null;

  private emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  private publicDomains = new Set([
    'gmail.com', 'googlemail.com', 'yahoo.com', 'yahoo.co.in', 'yahoo.co.uk', 'yahoo.ca', 'ymail.com',
    'hotmail.com', 'outlook.com', 'live.com', 'msn.com', 'icloud.com', 'me.com', 'mac.com',
    'aol.com', 'mail.com', 'zoho.com', 'protonmail.com', 'proton.me', 'yandex.com', 'gmx.com', 'gmx.net'
  ]);

  constructor(
    public dialogRef: MatDialogRef<ReferralModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private referralService: ReferralService,
    private toastr: ToastrService,
    private sessionService: SessionService
  ) {}

  ngOnInit(): void {}

  trackByIndex(index: number): number {
    return index;
  }

  isEmailValid(email: string): boolean {
    if (!email || !email.trim()) return false;
    return this.emailRegex.test(email.trim().toLowerCase());
  }

  isCompanyEmail(email: string): boolean {
    if (!this.isEmailValid(email)) return false;
    const parts = email.trim().toLowerCase().split('@');
    if (parts.length !== 2) return false;
    const domain = parts[1];
    return !this.publicDomains.has(domain) && domain.includes('.');
  }

  get validCompanyEmails(): string[] {
    const unique = new Set<string>();
    const list: string[] = [];
    for (const raw of this.emails) {
      if (raw && raw.trim()) {
        const clean = raw.trim().toLowerCase();
        if (this.isCompanyEmail(clean) && !unique.has(clean)) {
          unique.add(clean);
          list.push(clean);
        }
      }
    }
    return list;
  }

  get validCount(): number {
    return this.validCompanyEmails.length;
  }

  get canSubmit(): boolean {
    return this.validCount >= 10 && !this.isSubmitting;
  }

  applyBatchPaste(): void {
    if (!this.batchPasteInput) return;
    const tokens = this.batchPasteInput
      .split(/[\n\r,;\t ]+/)
      .map(e => e.trim())
      .filter(e => e.length > 0);

    const validParsed: string[] = [];
    for (const t of tokens) {
      if (this.isCompanyEmail(t) && !validParsed.includes(t.toLowerCase())) {
        validParsed.push(t.toLowerCase());
      }
    }

    for (let i = 0; i < 10; i++) {
      this.emails[i] = validParsed[i] || this.emails[i] || '';
    }

    this.showBatchPaste = false;
    this.batchPasteInput = '';
    this.toastr.info(`Imported ${Math.min(10, validParsed.length)} valid company email(s).`, 'Emails Populated');
  }

  submit(): void {
    if (!this.canSubmit) {
      this.toastr.warning(`Please provide 10 valid company email addresses (${this.validCount}/10 ready).`, '10 Company Emails Required');
      return;
    }

    this.isSubmitting = true;
    const payload = {
      emails: this.validCompanyEmails.slice(0, 10),
      customMessage: this.customMessage
    };

    this.referralService.submitReferrals(payload).subscribe({
      next: (res: any) => {
        this.isSubmitting = false;
        const data: SubmitReferralResponseDto = res?.data || res?.value || res;
        if (data && data.success) {
          this.successResult = data;
          this.toastr.success(data.message || '🎉 +10 Daily Chat Sessions granted for 10 days!', 'Reward Activated!');
          this.sessionService.refreshUser();
        } else {
          this.toastr.warning(data?.message || 'Referral submission had warnings.', 'Notice');
        }
      },
      error: (err: any) => {
        this.isSubmitting = false;
        const msg = err?.error?.message || err?.message || 'Failed to claim reward. Please verify email addresses.';
        this.toastr.error(msg, 'Submission Failed');
      }
    });
  }

  close(): void {
    this.dialogRef.close({ claimed: !!this.successResult });
  }
}
