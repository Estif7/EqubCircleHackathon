import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { RealtimeService } from './realtime.service';
import {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  RegisterResponse,
  VerifyOtpRequest,
} from './models';

const API_BASE = 'http://localhost:5000';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private realtime = inject(RealtimeService);
  private readonly tokenKey = 'ekub_token';
  private readonly userKey = 'ekub_user';

  register(body: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${API_BASE}/api/auth/register`, body);
  }
  verifyOtp(body: VerifyOtpRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${API_BASE}/api/auth/verify-otp`, body).pipe(
      tap(r => {
        this.saveSession(r);
        this.realtime.connect();
      })
    );
  }
  login(body: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${API_BASE}/api/auth/login`, body).pipe(
      tap(r => {
        this.saveSession(r);
        this.realtime.connect();
      })
    );
  }

  saveSession(response: AuthResponse): void {
    localStorage.setItem(this.tokenKey, response.token);
    localStorage.setItem(this.userKey, JSON.stringify(response));
  }
  token(): string | null {
    return localStorage.getItem(this.tokenKey);
  }
  user(): AuthResponse | null {
    const raw = localStorage.getItem(this.userKey);
    return raw ? (JSON.parse(raw) as AuthResponse) : null;
  }
  isLoggedIn(): boolean {
    return !!this.token();
  }
  logout(): void {
    this.realtime.disconnect();
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
  }
}
