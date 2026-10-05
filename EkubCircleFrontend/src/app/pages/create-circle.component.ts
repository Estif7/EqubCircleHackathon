import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../api.service';

@Component({ selector:'app-create-circle', standalone:true, imports:[CommonModule, FormsModule, RouterLink], template:`
<section class="page narrow">
  <div class="page-back"><a routerLink="/dashboard">← Back to dashboard</a></div>
  <div class="eyebrow">ORGANIZER</div>
  <h1>Create a circle</h1>
  <p class="muted">You automatically become the first member and organizer.</p>
  <div class="panel">
    <form (ngSubmit)="submit(f)" #f="ngForm" class="form-grid" novalidate>
      <label class="span-2">Circle name
        <input name="name" [(ngModel)]="model.name" #nameCtrl="ngModel" required minlength="3" placeholder="e.g. Family Ekub" (input)="error=''" />
        <span class="field-error" *ngIf="nameCtrl.invalid && (nameCtrl.touched || f.submitted)">Circle name is required (at least 3 characters).</span>
      </label>
      <label>Contribution (ETB)
        <input name="amount" type="number" min="1" [(ngModel)]="model.contributionAmount" #amountCtrl="ngModel" required (input)="error=''" />
        <span class="field-error" *ngIf="(amountCtrl.invalid || model.contributionAmount <= 0) && (amountCtrl.touched || f.submitted)">Contribution amount must be greater than 0 ETB.</span>
      </label>
      <label>Frequency
        <select name="frequency" [(ngModel)]="model.frequency">
          <option>Monthly</option>
          <option>Weekly</option>
        </select>
      </label>
      <label>Member limit
        <input name="limit" type="number" min="2" [(ngModel)]="model.memberLimit" #limitCtrl="ngModel" required (input)="error=''" />
        <span class="field-error" *ngIf="(limitCtrl.invalid || model.memberLimit < 2) && (limitCtrl.touched || f.submitted)">A circle must have at least 2 members.</span>
      </label>
      <label>Start date
        <input name="date" type="date" [min]="minDate" [(ngModel)]="model.startDate" />
        <span class="field-hint" *ngIf="!model.startDate">Optional. Defaults to start date when locked.</span>
      </label>
      <button type="submit" class="btn primary full span-2" [disabled]="loading">{{loading ? 'Creating…' : 'Create circle'}}</button>
    </form>
    <div class="error" *ngIf="error">{{error}}</div>
  </div>
</section>`})
export class CreateCircleComponent {
  private api = inject(ApiService);
  private router = inject(Router);
  model = { name: '', contributionAmount: 1000, frequency: 'Monthly', memberLimit: 3, startDate: '' };
  minDate = new Date().toISOString().split('T')[0];
  loading = false;
  error = '';

  submit(form: any) {
    const name = this.model.name.trim();
    if (form.invalid || !name || Number(this.model.contributionAmount) <= 0 || Number(this.model.memberLimit) < 2) {
      form.control.markAllAsTouched();
      if (!name) this.error = 'Circle name is required.';
      return;
    }
    this.loading = true;
    this.error = '';
    const startDate = this.model.startDate ? `${this.model.startDate}T00:00:00.000Z` : null;
    const body = {
      name,
      contributionAmount: Number(this.model.contributionAmount),
      frequency: this.model.frequency,
      memberLimit: Number(this.model.memberLimit),
      startDate
    };
    this.api.createCircle(body).subscribe({
      next: r => {
        this.loading = false;
        this.router.navigate(['/circles', r.id]);
      },
      error: e => {
        this.error = e?.error?.message || e?.error?.title || 'Could not create circle.';
        this.loading = false;
      }
    });
  }
}

