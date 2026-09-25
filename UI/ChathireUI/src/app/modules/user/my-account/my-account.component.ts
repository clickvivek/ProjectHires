import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { SessionService } from 'src/app/core/session/session.service';
import { ToastrService } from 'ngx-toastr';
import { picUrl, defaultProfilePic } from 'src/app/data/various';
import { PromocodeService, RedeemPromocodeResponseDto } from 'src/app/core/services/promocode.service';

@Component({
  selector: 'app-my-account',
  templateUrl: './my-account.component.html',
  styleUrls: ['./my-account.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class MyAccountComponent implements OnInit {

  user: any;
  activeTab: 'profile-details' | 'profile-pic' | 'profile-password' | 'redeem-promo' = 'profile-details';
  defaultPic = defaultProfilePic;
  picBase = picUrl;

  promoCodeInput = '';
  isRedeeming = false;
  redemptionResult: RedeemPromocodeResponseDto | null = null;

  constructor(
    private sessionService: SessionService,
    private promocodeService: PromocodeService,
    private toastr: ToastrService
  ) {}

  ngOnInit() {
    this.sessionService.userdetailscast.subscribe((res: any) => {
      this.user = res;
    });
  }

  setTab(tab: 'profile-details' | 'profile-pic' | 'profile-password' | 'redeem-promo') {
    this.activeTab = tab;
  }

  redeemPromoCode() {
    if (!this.promoCodeInput || !this.promoCodeInput.trim()) {
      this.toastr.warning('Please enter a promo code.', 'Input Required');
      return;
    }

    this.isRedeeming = true;
    this.redemptionResult = null;

    const payload = {
      promocode: this.promoCodeInput.trim().toUpperCase(),
      userId: this.user?.id
    };

    this.promocodeService.redeem(payload).subscribe({
      next: (res: any) => {
        this.isRedeeming = false;
        const data: RedeemPromocodeResponseDto = res?.value || res?.data || res;
        if (data && data.success) {
          this.redemptionResult = data;
          this.toastr.success(data.message || 'Promo code redeemed successfully!', 'Success!');
          this.promoCodeInput = '';
        } else {
          const msg = data?.message || 'Failed to redeem promo code.';
          this.toastr.error(msg, 'Redemption Failed');
        }
      },
      error: (err: any) => {
        this.isRedeeming = false;
        const msg = err?.error?.message || err?.message || 'Error occurred while redeeming promo code.';
        this.toastr.error(msg, 'Error');
      }
    });
  }
}
