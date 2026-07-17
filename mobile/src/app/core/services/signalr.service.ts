import { Injectable } from '@angular/core';
import { HubConnectionBuilder, HubConnection, HubConnectionState } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '@env/environment';
import { Notification } from '../models';
import { Storage } from '@ionic/storage-angular';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hubConnection: HubConnection | null = null;
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  notifications$: Observable<Notification[]> = this.notificationsSubject.asObservable();
  private unreadCountSubject = new BehaviorSubject<number>(0);
  unreadCount$ = this.unreadCountSubject.asObservable();

  constructor(private storage: Storage) {}

  async start(): Promise<void> {
    const token = await this.storage.get('access_token');
    if (!token || this.hubConnection?.state === HubConnectionState.Connected) return;

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.signalrUrl}?access_token=${token}`)
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .build();

    this.hubConnection.on('ReceiveNotification', (notification: Notification) => {
      const current = this.notificationsSubject.value;
      this.notificationsSubject.next([notification, ...current]);
      this.unreadCountSubject.next(this.unreadCountSubject.value + 1);
    });

    this.hubConnection.on('ReceiveOrderUpdate', (data: any) => {
      console.log('Order update:', data);
    });

    try {
      await this.hubConnection.start();
      console.log('SignalR connected');
    } catch (err) {
      console.warn('SignalR connection failed, retrying in 30s:', err);
      setTimeout(() => this.start(), 30000);
    }
  }

  async stop(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = null;
    }
  }

  markAllRead(): void {
    this.unreadCountSubject.next(0);
  }
}
