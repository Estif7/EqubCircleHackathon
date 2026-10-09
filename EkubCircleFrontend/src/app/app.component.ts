import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RealtimeService } from './realtime.service';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  template: ` <div class="app-shell">
    <header class="topbar" *ngIf="auth.isLoggedIn()">
      <a routerLink="/dashboard" class="brand"
        ><span class="brand-mark">E</span><span>Ekub<span>Circle</span></span></a
      >
      <nav>
        <a routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
        <a routerLink="/circles/new" routerLinkActive="active">Create Circle</a>
        <a routerLink="/profile" routerLinkActive="active">Profile</a>
        <button class="nav-user" (click)="logout()">{{ auth.user()?.fullName }} · Logout</button>
      </nav>
    </header>
    <div class="toast-stack" *ngIf="notifications.length">
      <div class="toast" *ngFor="let n of notifications; let i = index" (click)="dismiss(i)">
        {{ n }}
      </div>
    </div>
    <main [class.with-nav]="auth.isLoggedIn()"><router-outlet></router-outlet></main>
  </div>`,
})
export class AppComponent {
  auth = inject(AuthService);
  router = inject(Router);
  private realtime = inject(RealtimeService);
  notifications: string[] = [];
  constructor() {
    if (this.auth.isLoggedIn()) this.realtime.connect();
    this.realtime.events$.subscribe(e => this.notify(e.type, e.data));
  }
  private notify(type: string, data: any) {
    const messages: any = {
      circleCreated: 'A new Ekub circle is available.',
      memberJoined: 'A new member joined a circle.',
      circleStarted: 'A circle has started and payout order is locked.',
      paymentRecorded: 'A member payment was recorded.',
      payoutCompleted: 'A circle payout was completed.',
    };
    this.notifications = [messages[type] || 'EkubCircle updated.', ...this.notifications].slice(
      0,
      4
    );
    setTimeout(() => this.notifications.pop(), 5000);
  }
  dismiss(i: number) {
    this.notifications.splice(i, 1);
  }
  logout() {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
