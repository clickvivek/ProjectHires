import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { SessionService } from 'src/app/core/session/session.service';
import { AuthService } from 'src/app/core/auth/auth.service';
import { ToastrService } from 'ngx-toastr';
import { picUrl, defaultProfilePic } from 'src/app/data/various';
import { PromocodeService, RedeemPromocodeResponseDto } from 'src/app/core/services/promocode.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-my-account',
  templateUrl: './my-account.component.html',
  styleUrls: ['./my-account.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class MyAccountComponent implements OnInit {

  user: any;
  activeTab: 'profile-details' | 'profile-pic' | 'profile-password' | 'redeem-promo' | 'delete-account' = 'profile-details';
  defaultPic = defaultProfilePic;
  picBase = picUrl;

  promoCodeInput = '';
  isRedeeming = false;
  redemptionResult: RedeemPromocodeResponseDto | null = null;

  deleteConfirmInput = '';
  isDeletingAccount = false;

  constructor(
    private sessionService: SessionService,
    private authService: AuthService,
    private promocodeService: PromocodeService,
    private http: HttpClient,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit() {
    this.sessionService.userdetailscast.subscribe((res: any) => {
      this.user = res;
    });
  }

  getProfilePicUrl(): string {
    if (!this.user?.profilePic) {
      return this.defaultPic;
    }
    const pic = this.user.profilePic;
    if (pic.startsWith('http://') || pic.startsWith('https://') || pic.startsWith('data:')) {
      return pic;
    }
    return `${this.picBase}${pic}`;
  }

  onImageError(event: any) {
    event.target.src = this.defaultPic;
  }

  setTab(tab: 'profile-details' | 'profile-pic' | 'profile-password' | 'redeem-promo' | 'delete-account') {
    this.activeTab = tab;
    this.deleteConfirmInput = '';
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

  confirmDeleteAccount() {
    if (this.deleteConfirmInput.trim().toUpperCase() !== 'DELETE') {
      this.toastr.warning('Please type DELETE to confirm.', 'Confirmation Required');
      return;
    }

    this.isDeletingAccount = true;
    const url = `${environment.rootUrl}/api/User/DeactivateAccount`;

    this.http.post<any>(url, {}).subscribe({
      next: () => {
        this.isDeletingAccount = false;
        this.toastr.success('Your account has been deleted and deactivated.', 'Account Closed');
        this.authService.logout().subscribe(() => {
          this.router.navigate(['/login']);
        });
      },
      error: (err: any) => {
        this.isDeletingAccount = false;
        const msg = err?.error?.errors?.[0]?.message || err?.error?.message || err?.message || 'Failed to deactivate account.';
        this.toastr.error(msg, 'Error');
      }
    });
  }
}
