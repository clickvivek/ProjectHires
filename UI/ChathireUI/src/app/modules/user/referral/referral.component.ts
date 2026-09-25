import { Component, OnInit } from '@angular/core';
import { ReferralService, ReferralStatsDto, SubmitReferralResponseDto, UserReferralDto } from 'src/app/core/services/referral.service';
import { ToastrService } from 'ngx-toastr';
import { SessionService } from 'src/app/core/session/session.service';

@Component({
  selector: 'app-referral',
  templateUrl: './referral.component.html',
  styleUrls: ['./referral.component.scss']
})
export class ReferralComponent implements OnInit {
  emailInputText: string = '';
  customMessage: string = "Join ChatHire to find high-quality candidates and jobs. Use my referral link to get 3 months of free job postings or unlimited hotlist postings!";
  
  isSubmitting: boolean = false;
  isLoadingStats: boolean = false;
  isCopiedLink: boolean = false;
  isCopiedCode: boolean = false;

  submissionResult: SubmitReferralResponseDto | null = null;
  stats: ReferralStatsDto = {
    totalInvited: 0,
    totalRegistered: 0,
    freePostingsEarned: 0,
    freeMonthsEarned: 0,
    referralCode: '',
    referralLink: '',
    referrals: []
  };

  currentUser: any = null;
  userId: number = 0;
  private emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  constructor(
    private referralService: ReferralService,
    private toastr: ToastrService,
    private sessionService: SessionService
  ) {}

  ngOnInit(): void {
    this.userId = this.sessionService.userId;
    this.currentUser = this.sessionService.getUserDetails();
    this.sessionService.userdetailscast.subscribe((u: any) => {
      if (u) {
        this.currentUser = u;
      }
    });
    this.loadStats();
  }

  loadStats(): void {
    this.isLoadingStats = true;
    this.referralService.getReferralStats().subscribe({
      next: (res: any) => {
        this.isLoadingStats = false;
        if (res && res.data) {
          this.stats = res.data;
        }
      },
      error: (err: any) => {
        this.isLoadingStats = false;
        console.error('Error fetching referral stats', err);
      }
    });
  }

  // Parse raw text into discrete email tokens
  get rawTokens(): string[] {
    if (!this.emailInputText) return [];
    return this.emailInputText
      .split(/[\n\r,;\t ]+/)
      .map(e => e.trim())
      .filter(e => e.length > 0);
  }

  // Valid formatted unique emails
  get parsedEmails(): string[] {
    const tokens = this.rawTokens;
    const unique = new Set<string>();
    const valid: string[] = [];

    for (const token of tokens) {
      const lower = token.toLowerCase();
      if (this.emailRegex.test(lower)) {
        if (!unique.has(lower)) {
          unique.add(lower);
          valid.push(lower);
        }
      }
    }
    return valid;
  }

  get invalidTokens(): string[] {
    return this.rawTokens.filter(t => !this.emailRegex.test(t.toLowerCase()));
  }

  get duplicateCount(): number {
    const validTokens = this.rawTokens.filter(t => this.emailRegex.test(t.toLowerCase()));
    return validTokens.length - this.parsedEmails.length;
  }

  get emailCount(): number {
    return this.parsedEmails.length;
  }

  get progressPercentage(): number {
    const count = this.emailCount;
    return Math.min(100, Math.round((count / 10) * 100));
  }

  get canSubmit(): boolean {
    return this.parsedEmails.length >= 10 && !this.isSubmitting;
  }

  onKeyDown(event: KeyboardEvent): void {
    // Submit on Ctrl+Enter or Cmd+Enter
    if ((event.ctrlKey || event.metaKey) && event.key === 'Enter') {
      event.preventDefault();
      if (this.canSubmit) {
        this.submitReferrals();
      } else {
        this.toastr.warning(`Please enter at least 10 valid email addresses (${this.emailCount}/10 entered).`, 'Minimum 10 Emails Required');
      }
    }
  }

  submitReferrals(): void {
    if (this.parsedEmails.length < 10) {
      this.toastr.error(`You have entered ${this.parsedEmails.length} valid email(s). A minimum of 10 emails is required to submit referrals.`, 'Minimum 10 Emails Required');
      return;
    }

    this.isSubmitting = true;
    this.submissionResult = null;

    const payload = {
      emails: this.parsedEmails,
      customMessage: this.customMessage
    };

    this.referralService.submitReferrals(payload).subscribe({
      next: (res: any) => {
        this.isSubmitting = false;
        if (res && res.data) {
          this.submissionResult = res.data;
          if (this.submissionResult?.success) {
            this.toastr.success(this.submissionResult.message, 'Referrals Submitted!');
            this.emailInputText = '';
            this.loadStats();
          } else {
            this.toastr.warning(this.submissionResult?.message || 'Referral submission had warnings.', 'Notice');
          }
        } else {
          this.toastr.error('Unexpected response from referral service.', 'Error');
        }
      },
      error: (err: any) => {
        this.isSubmitting = false;
        const msg = err?.error?.message || err?.message || 'Failed to submit referrals. Please try again.';
        this.toastr.error(msg, 'Submission Failed');
      }
    });
  }

  copyLink(): void {
    const link = this.stats.referralLink || `https://chathire.com/#/signup?ref=REF${this.currentUser?.id || ''}`;
    this.copyToClipboard(link);
    this.isCopiedLink = true;
    this.toastr.success('Referral link copied to clipboard!', 'Copied');
    setTimeout(() => {
      this.isCopiedLink = false;
    }, 3000);
  }

  copyCode(): void {
    const code = this.stats.referralCode || `REF${this.currentUser?.id || ''}`;
    this.copyToClipboard(code);
    this.isCopiedCode = true;
    this.toastr.success('Referral code copied to clipboard!', 'Copied');
    setTimeout(() => {
      this.isCopiedCode = false;
    }, 3000);
  }

  private copyToClipboard(text: string): void {
    if (navigator && navigator.clipboard && navigator.clipboard.writeText) {
      navigator.clipboard.writeText(text);
    } else {
      const el = document.createElement('textarea');
      el.value = text;
      document.body.appendChild(el);
      el.select();
      document.execCommand('copy');
      document.body.removeChild(el);
    }
  }

  shareWhatsApp(): void {
    const link = encodeURIComponent(this.stats.referralLink || 'https://chathire.com');
    const msg = encodeURIComponent(`Hey! Sign up on ChatHire with my invite link to get 3 Months Free Job Postings and unlimited candidate connections: `) + link;
    window.open(`https://api.whatsapp.com/send?text=${msg}`, '_blank');
  }

  shareLinkedIn(): void {
    const link = encodeURIComponent(this.stats.referralLink || 'https://chathire.com');
    window.open(`https://www.linkedin.com/sharing/share-offsite/?url=${link}`, '_blank');
  }

  shareTwitter(): void {
    const link = encodeURIComponent(this.stats.referralLink || 'https://chathire.com');
    const text = encodeURIComponent('Get 3 months free job postings and hire top talent on ChatHire! Join now:');
    window.open(`https://twitter.com/intent/tweet?text=${text}&url=${link}`, '_blank');
  }

  shareEmail(): void {
    const link = this.stats.referralLink || 'https://chathire.com';
    const subject = encodeURIComponent('Exclusive Invitation: Join ChatHire & Get 3 Months Free Job Postings!');
    const body = encodeURIComponent(`Hi,\n\nI'm inviting you to try ChatHire for hiring and recruiting top talent.\n\nSign up using my referral link to get 3 Months of Free Job Postings or Unlimited Hotlist Profile Postings:\n${link}\n\nBest regards!`);
    window.open(`mailto:?subject=${subject}&body=${body}`, '_blank');
  }
}
