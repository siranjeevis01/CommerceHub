import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { AuthService } from './auth.service';
import { Notification } from '../models';
import { environment } from '@env/environment';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private hubConnection: signalR.HubConnection | null = null;
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  notifications$ = this.notificationsSubject.asObservable();
  private unreadCountSubject = new BehaviorSubject<number>(0);
  unreadCount$ = this.unreadCountSubject.asObservable();

  constructor(private auth: AuthService) {
    if (auth.isLoggedIn) {
      this.startConnection();
    }
    auth.currentUser$.subscribe(user => {
      if (user && !this.hubConnection) {
        this.startConnection();
      } else if (!user && this.hubConnection) {
        this.stopConnection();
      }
    });
  }

  private startConnection(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/hubs/notification`, {
        accessTokenFactory: () => this.auth.getToken() || ''
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .build();

    this.hubConnection.on('Notification', (notification: Notification) => {
      notification.timestamp = new Date().toISOString();
      const current = this.notificationsSubject.value;
      this.notificationsSubject.next([notification, ...current]);
      this.unreadCountSubject.next(this.unreadCountSubject.value + 1);
    });

    this.hubConnection.start().catch((err: Error) => console.error('SignalR connection error:', err));
  }

  private stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop().catch(() => {});
      this.hubConnection = null;
    }
  }

  markAllAsRead(): void {
    this.unreadCountSubject.next(0);
  }
}
