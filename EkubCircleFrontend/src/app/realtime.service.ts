import { Injectable, inject } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Subject } from 'rxjs';

const API_BASE = 'http://localhost:5000';

export interface RealtimeEvent { type: string; data?: any; }

@Injectable({ providedIn: 'root' })
export class RealtimeService {
  private connection: HubConnection | null = null;
  private eventsSubject = new Subject<RealtimeEvent>();
  readonly events$ = this.eventsSubject.asObservable();
  private startedForToken = '';

  connect(): void {
    const token = localStorage.getItem('ekub_token');
    if (!token || this.startedForToken === token) return;
    this.startedForToken = token;
    this.connection = new HubConnectionBuilder()
      .withUrl(`${API_BASE}/hubs/notifications`, { accessTokenFactory: () => localStorage.getItem('ekub_token') || '' })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    ['circleCreated','memberJoined','circleStarted','paymentRecorded','payoutCompleted'].forEach(type => {
      this.connection!.on(type, (data: any) => this.eventsSubject.next({ type, data }));
    });
    this.connection.onclose(() => { this.startedForToken = ''; });
    this.connection.start().catch(() => { this.startedForToken = ''; });
  }

  disconnect(): void {
    const connection = this.connection;
    this.connection = null;
    this.startedForToken = '';
    if (connection) connection.stop().catch(() => {});
  }
}
