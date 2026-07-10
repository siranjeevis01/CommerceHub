import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import { User, LoginRequest, LoginResponse, RegisterRequest, ApiResponse } from '../models';
import { Router } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(this.getStoredUser());
  currentUser$: Observable<User | null> = this.currentUserSubject.asObservable();

  constructor(private api: ApiService, private router: Router) {}

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
    return this.api.post<LoginResponse>('/api/v1/auth/login', credentials).pipe(
      tap(response => {
        if (response.success) {
          this.setSession(response.data);
          this.currentUserSubject.next(response.data.user);
        }
      })
    );
  }

  register(data: RegisterRequest): Observable<ApiResponse<User>> {
    return this.api.post<User>('/api/v1/auth/register', data);
  }

  logout(): void {
    this.api.post('/api/v1/auth/logout', { refreshToken: this.getRefreshToken() }).subscribe({
      next: () => this.clearSession(),
      error: () => this.clearSession()
    });
  }

  refreshToken(): Observable<ApiResponse<LoginResponse>> {
    const refreshToken = this.getRefreshToken();
    return this.api.post<LoginResponse>('/api/v1/auth/refresh', { refreshToken }).pipe(
      tap(response => {
        if (response.success) {
          this.setSession(response.data);
        }
      })
    );
  }

  getToken(): string | null {
    return localStorage.getItem('access_token');
  }

  private getRefreshToken(): string | null {
    return localStorage.getItem('refresh_token');
  }

  private setSession(data: LoginResponse): void {
    localStorage.setItem('access_token', data.accessToken);
    localStorage.setItem('refresh_token', data.refreshToken);
    localStorage.setItem('token_expires', data.expiresAt);
    localStorage.setItem('current_user', JSON.stringify(data.user));
  }

  private clearSession(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('token_expires');
    localStorage.removeItem('current_user');
    this.currentUserSubject.next(null);
    this.router.navigate(['/auth/login']);
  }

  private getStoredUser(): User | null {
    const stored = localStorage.getItem('current_user');
    return stored ? JSON.parse(stored) : null;
  }
}
