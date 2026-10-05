import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({ selector: 'app-register', standalone: true, imports: [CommonModule, FormsModule, RouterLink], template: `
<div class="auth-layout"><div class="auth-card wide">
  <div class="eyebrow">GET STARTED</div><h1>Create your account</h1><p class="muted">Fayda verification is simulated for the hackathon demo.</p>
  <form (ngSubmit)="submit()" #f="ngForm" class="form-grid">
    <label>Full name<input name="fullName" [(ngModel)]="model.fullName" required /></label>
    <label>Phone number<input name="phoneNumber" [(ngModel)]="model.phoneNumber" required placeholder="+251…" /></label>
    <label>Email<input name="email" type="email" [(ngModel)]="model.email" required /></label>
    <label>Fayda FAN number<input name="FaydaFanNumber" [(ngModel)]="model.FaydaFanNumber" required /></label>
    <label>Password<input name="password" type="password" [(ngModel)]="model.password" required /></label>
    <label>Confirm password<input name="confirmPassword" type="password" [(ngModel)]="model.confirmPassword" required /></label>
    <button class="btn primary full span-2" [disabled]="loading">{{loading ? 'Creating…' : 'Create account'}}</button>
  </form>
  <div class="error" *ngIf="error">{{error}}</div>
  <p class="switch">Already registered? <a routerLink="/login">Sign in</a></p>
</div></div>` })
export class RegisterComponent {
  private auth = inject(AuthService); private router = inject(Router);
  model = { fullName: '', phoneNumber: '', email: '', password: '', confirmPassword: '', FaydaFanNumber: '' }; loading = false; error = '';
  submit() { this.loading = true; this.error = ''; this.auth.register(this.model).subscribe({ next: r => { sessionStorage.setItem('ekub_pending_user', String(r.userId)); sessionStorage.setItem('ekub_demo_otp', r.demoOtp || ''); this.router.navigate(['/verify-otp']); }, error: e => { this.error = e?.error?.message || e?.error?.title || this.flattenErrors(e?.error?.errors) || 'Registration failed.'; this.loading = false; } }); }
  private flattenErrors(errors: Record<string,string[]> | undefined) { return errors ? Object.values(errors).flat().join(' ') : ''; }
}
