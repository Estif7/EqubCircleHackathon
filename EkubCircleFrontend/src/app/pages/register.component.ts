import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: ` <div class="auth-layout">
    <div class="auth-card wide">
      <div class="eyebrow">GET STARTED</div>
      <h1>Create your account</h1>
      <p class="muted">Fayda verification is simulated for the hackathon demo.</p>
      <form (ngSubmit)="submit(f)" #f="ngForm" class="form-grid" novalidate>
        <label
          >Full name
          <input
            name="fullName"
            [(ngModel)]="model.fullName"
            #nameCtrl="ngModel"
            required
            minlength="2"
            placeholder="Abebe Bikila"
            (input)="error = ''"
          />
          <span class="field-error" *ngIf="nameCtrl.invalid && (nameCtrl.touched || f.submitted)"
            >Full name is required (at least 2 characters).</span
          >
        </label>
        <label
          >Phone number
          <input
            name="phoneNumber"
            [(ngModel)]="model.phoneNumber"
            #phoneCtrl="ngModel"
            required
            pattern="^(\\+?251|0)[79]\\d{8}$"
            placeholder="+251911223344"
            (input)="error = ''"
          />
          <span class="field-error" *ngIf="phoneCtrl.invalid && (phoneCtrl.touched || f.submitted)"
            >Enter a valid Ethiopian phone number (e.g. +2519... or 09...).</span
          >
        </label>
        <label
          >Email
          <input
            name="email"
            type="email"
            [(ngModel)]="model.email"
            #emailCtrl="ngModel"
            required
            email
            placeholder="abebe@example.com"
            (input)="error = ''"
          />
          <span class="field-error" *ngIf="emailCtrl.invalid && (emailCtrl.touched || f.submitted)"
            >Please enter a valid email address.</span
          >
        </label>
        <label
          >Fayda FAN number
          <input
            name="FaydaFanNumber"
            [(ngModel)]="model.FaydaFanNumber"
            #fanCtrl="ngModel"
            required
            minlength="4"
            placeholder="FAN-123456"
            (input)="error = ''"
          />
          <span class="field-error" *ngIf="fanCtrl.invalid && (fanCtrl.touched || f.submitted)"
            >Fayda FAN is required (min 4 characters).</span
          >
        </label>
        <label
          >Password
          <input
            name="password"
            type="password"
            [(ngModel)]="model.password"
            #passCtrl="ngModel"
            required
            minlength="6"
            placeholder="At least 6 characters"
            (input)="error = ''"
          />
          <span class="field-error" *ngIf="passCtrl.invalid && (passCtrl.touched || f.submitted)"
            >Password must be at least 6 characters.</span
          >
        </label>
        <label
          >Confirm password
          <input
            name="confirmPassword"
            type="password"
            [(ngModel)]="model.confirmPassword"
            #confirmCtrl="ngModel"
            required
            placeholder="Repeat password"
            (input)="error = ''"
          />
          <span
            class="field-error"
            *ngIf="confirmCtrl.invalid && (confirmCtrl.touched || f.submitted)"
            >Please repeat your password.</span
          >
          <span
            class="field-error"
            *ngIf="
              model.confirmPassword &&
              model.password !== model.confirmPassword &&
              (confirmCtrl.touched || f.submitted)
            "
            >Passwords do not match.</span
          >
        </label>
        <button class="btn primary full span-2" [disabled]="loading">
          {{ loading ? 'Creating…' : 'Create account' }}
        </button>
      </form>
      <div class="error" *ngIf="error">{{ error }}</div>
      <p class="switch">Already registered? <a routerLink="/login">Sign in</a></p>
    </div>
  </div>`,
})
export class RegisterComponent {
  private auth = inject(AuthService);
  private router = inject(Router);
  model = {
    fullName: '',
    phoneNumber: '',
    email: '',
    password: '',
    confirmPassword: '',
    FaydaFanNumber: '',
  };
  loading = false;
  error = '';

  submit(form: any) {
    if (form.invalid) {
      form.control.markAllAsTouched();
      return;
    }
    if (this.model.password !== this.model.confirmPassword) {
      this.error = 'Passwords do not match.';
      return;
    }
    this.loading = true;
    this.error = '';
    this.auth.register(this.model).subscribe({
      next: r => {
        sessionStorage.setItem('ekub_pending_user', String(r.userId));
        sessionStorage.setItem('ekub_demo_otp', r.demoOtp || '');
        this.router.navigate(['/verify-otp']);
      },
      error: e => {
        this.error =
          e?.error?.message ||
          e?.error?.title ||
          this.flattenErrors(e?.error?.errors) ||
          'Registration failed.';
        this.loading = false;
      },
    });
  }

  private flattenErrors(errors: Record<string, string[]> | undefined) {
    return errors ? Object.values(errors).flat().join(' ') : '';
  }
}
