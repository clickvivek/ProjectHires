import { Component, Input, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ReferralService, ReferralStatsDto } from 'src/app/core/services/referral.service';
import { ToastrService } from 'ngx-toastr';
import { ReferralModalComponent } from '../referral-modal/referral-modal.component';

@Component({
  selector: 'app-referral-banner',
  templateUrl: './referral-banner.component.html',
  styleUrls: ['./referral-banner.component.scss']
})
export class ReferralBannerComponent implements OnInit {

  @Input() layout: 'sidebar' | 'horizontal' = 'sidebar';
  @Input() title: string = 'Refer & Increase Chat Limit';
  @Input() compact: boolean = false;

  referralLink: string = '';
  referralCode: string = '';
  isCopied: boolean = false;
  isLoading: boolean = false;

  constructor(
    private referralService: ReferralService,
    private toastr: ToastrService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadReferralData();
  }

  loadReferralData(): void {
    this.isLoading = true;
    this.referralService.getReferralStats().subscribe({
      next: (res: any) => {
        const stats: ReferralStatsDto = res?.value || res;
        if (stats?.referralLink) {
          this.referralLink = stats.referralLink;
          this.referralCode = stats.referralCode;
        } else {
          this.generateFallbackLink();
        }
        this.isLoading = false;
      },
      error: () => {
        this.generateFallbackLink();
        this.isLoading = false;
      }
    });
  }

  private generateFallbackLink(): void {
    const origin = window.location.origin;
    this.referralLink = `${origin}/#/refer-for-rewards`;
  }

  openReferralModal(event?: Event): void {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    this.dialog.open(ReferralModalComponent, {
      width: '680px',
      maxWidth: '95vw',
      panelClass: 'referral-modal-panel',
      data: {
        referralCode: this.referralCode,
        referralLink: this.referralLink
      }
    });
  }

  copyLink(event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    if (!this.referralLink) {
      this.generateFallbackLink();
    }

    navigator.clipboard.writeText(this.referralLink).then(() => {
      this.isCopied = true;
      this.toastr.success('Referral link copied to clipboard! Share it with colleagues to increase your chat limit.', 'Link Copied', {
        timeOut: 3000,
        positionClass: 'toast-top-center'
      });
      setTimeout(() => {
        this.isCopied = false;
      }, 3000);
    }).catch(() => {
      this.toastr.info('Please copy this link: ' + this.referralLink, 'Referral Link', {
        timeOut: 4000,
        positionClass: 'toast-top-center'
      });
    });
  }
}
