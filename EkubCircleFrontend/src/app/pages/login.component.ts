import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({ selector: 'app-login', standalone: true, imports: [CommonModule, FormsModule, RouterLink], template: `
<div class="auth-layout"><div class="auth-card">
  <div class="eyebrow">WELCOME BACK</div><h1>Sign in to EkubCircle</h1><p class="muted">Manage your circles, rounds, payments and payouts in one place.</p>
  <form (ngSubmit)="submit(f)" #f="ngForm" novalidate>
    <label>Email or phone
      <input name="emailOrPhone" [(ngModel)]="model.emailOrPhone" #emailCtrl="ngModel" required placeholder="you@example.com or +251…" (input)="error=''" />
      <span class="field-error" *ngIf="emailCtrl.invalid && (emailCtrl.touched || f.submitted)">Please enter your email or phone number.</span>
    </label>
    <label>Password
      <input name="password" type="password" [(ngModel)]="model.password" #passCtrl="ngModel" required placeholder="••••••••" (input)="error=''" />
      <span class="field-error" *ngIf="passCtrl.invalid && (passCtrl.touched || f.submitted)">Password is required.</span>
    </label>
    <button class="btn primary full" [disabled]="loading">{{loading ? 'Signing in…' : 'Sign in'}}</button>
  </form>
  <div class="error" *ngIf="error">{{error}}</div>
  <p class="switch">New to EkubCircle? <a routerLink="/register">Create an account</a></p>
</div><div class="auth-art"><span class="big-logo">E</span><h2>Save together.<br>Grow together.</h2><p>A transparent digital ledger for community Ekub circles.</p></div></div>` })
export class LoginComponent {
  private auth = inject(AuthService); private router = inject(Router);
  model = { emailOrPhone: '', password: '' }; loading = false; error = '';
  submit(form: any) {
    if (form.invalid) {
      form.control.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.error = '';
    this.auth.login(this.model).subscribe({
      next: () => this.router.navigateByUrl('/dashboard'),
      error: e => {
        this.error = e?.error?.message || e?.error?.title || 'Unable to sign in.';
        this.loading = false;
      }
    });
  }
}

