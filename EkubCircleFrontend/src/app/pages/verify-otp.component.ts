import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-verify-otp',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: ` <div class="auth-layout">
    <div class="auth-card otp-card">
      <div class="eyebrow">VERIFICATION</div>
      <h1>Verify your account</h1>
      <p class="muted">Enter the six-digit simulated Fayda OTP from registration.</p>
      <form (ngSubmit)="submit(f)" #f="ngForm" novalidate>
        <label
          >OTP code
          <input
            name="otp"
            [(ngModel)]="otp"
            #otpCtrl="ngModel"
            inputmode="numeric"
            maxlength="6"
            minlength="6"
            pattern="^[0-9]{6}$"
            placeholder="000000"
            required
            (input)="error = ''"
          />
          <span class="field-error" *ngIf="otpCtrl.invalid && (otpCtrl.touched || f.submitted)"
            >Please enter the complete 6-digit numeric OTP code.</span
          >
        </label>
        <button class="btn primary full" [disabled]="loading">
          {{ loading ? 'Verifying…' : 'Verify account' }}
        </button>
      </form>
      <div
        class="demo-otp"
        *ngIf="demoOtp"
        (click)="fillDemoOtp()"
        style="cursor: pointer;"
        title="Click to auto-fill"
      >
        Demo OTP: <strong>{{ demoOtp }}</strong>
        <span style="display:block; font-size:11px; margin-top:3px; opacity:0.85;"
          >(Click to auto-fill)</span
        >
      </div>
      <div class="error" *ngIf="error">{{ error }}</div>
      <p class="switch"><a routerLink="/login">Back to sign in</a></p>
    </div>
  </div>`,
})
export class VerifyOtpComponent {
  private auth = inject(AuthService);
  private router = inject(Router);
  otp = '';
  loading = false;
  error = '';
  demoOtp = sessionStorage.getItem('ekub_demo_otp') || '';

  fillDemoOtp() {
    if (this.demoOtp) {
      this.otp = this.demoOtp;
      this.error = '';
    }
  }

  submit(form: any) {
    if (form.invalid) {
      form.control.markAllAsTouched();
      return;
    }
    const id = Number(sessionStorage.getItem('ekub_pending_user'));
    if (!id) {
      this.error = 'No pending registration was found. Please register first.';
      return;
    }
    this.loading = true;
    this.error = '';
    this.auth.verifyOtp({ userId: id, otp: this.otp }).subscribe({
      next: () => {
        sessionStorage.removeItem('ekub_pending_user');
        sessionStorage.removeItem('ekub_demo_otp');
        this.router.navigateByUrl('/dashboard');
      },
      error: e => {
        this.error = e?.error?.message || e?.error?.title || 'Verification failed.';
        this.loading = false;
      },
    });
  }
}
