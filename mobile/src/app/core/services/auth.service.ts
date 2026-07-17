import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import { User, LoginRequest, LoginResponse, RegisterRequest, ApiResponse } from '../models';
import { Router } from '@angular/router';
import { Storage } from '@ionic/storage-angular';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  currentUser$: Observable<User | null> = this.currentUserSubject.asObservable();
  private storageReady = false;

  constructor(
    private api: ApiService,
    private router: Router,
    private storage: Storage
  ) {
    this.initStorage();
  }

  private async initStorage() {
    await this.storage.create();
    this.storageReady = true;
    const stored = await this.storage.get('current_user');
    if (stored) {
      this.currentUserSubject.next(JSON.parse(stored));
    }
  }

  get currentUser(): User | null {
    return this.currentUserSubject.value;
  }

  get isLoggedIn(): boolean {
    return !!this.currentUser && !!this.getToken();
  }

  get isAdmin(): boolean {
    return this.currentUser?.roles.includes('Admin') ?? false;
  }

  get isVendor(): boolean {
    return this.currentUser?.roles.includes('Vendor') ?? false;
  }

  get isCustomer(): boolean {
    return this.currentUser?.roles.includes('Customer') ?? false;
  }

  login(credentials: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.api.post<LoginResponse>('/api/v1/identity/auth/login', credentials).pipe(
      tap(async response => {
        if (response.success) {
          await this.setSession(response.data);
          this.currentUserSubject.next(response.data.user);
        }
      })
    );
  }

  register(data: RegisterRequest): Observable<ApiResponse<User>> {
    return this.api.post<User>('/api/v1/identity/auth/register', data);
  }

  async logout(): Promise<void> {
    const refreshToken = await this.getRefreshToken();
    this.api.post('/api/v1/identity/auth/logout', { refreshToken }).subscribe({
      next: () => this.clearSession(),
      error: () => this.clearSession()
    });
  }

  refreshToken(): Observable<ApiResponse<LoginResponse>> {
    return new Observable(observer => {
      this.getRefreshToken().then(refreshToken => {
        this.api.post<LoginResponse>('/api/v1/identity/auth/refresh-token', { refreshToken }).pipe(
          tap(async response => {
            if (response.success) {
              await this.setSession(response.data);
            }
          })
        ).subscribe(observer);
      });
    });
  }

  async getToken(): Promise<string | null> {
    return await this.storage.get('access_token');
  }

  async getRefreshToken(): Promise<string | null> {
    return await this.storage.get('refresh_token');
  }

  private async setSession(data: LoginResponse): Promise<void> {
    await this.storage.set('access_token', data.accessToken);
    await this.storage.set('refresh_token', data.refreshToken);
    await this.storage.set('token_expires', data.expiresAt);
    await this.storage.set('current_user', JSON.stringify(data.user));
  }

  private async clearSession(): Promise<void> {
    await this.storage.remove('access_token');
    await this.storage.remove('refresh_token');
    await this.storage.remove('token_expires');
    await this.storage.remove('current_user');
    this.currentUserSubject.next(null);
    this.router.navigate(['/auth/login']);
  }
}
