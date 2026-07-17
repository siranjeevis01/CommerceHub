import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { Storage } from '@ionic/storage-angular';
import { environment } from '@env/environment';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject = new BehaviorSubject<string | null>(null);

  constructor(private storage: Storage) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (req.url.includes('/auth/login') || req.url.includes('/auth/register') || req.url.includes('/auth/refresh-token')) {
      return next.handle(req);
    }

    return new Observable(observer => {
      this.storage.get('access_token').then(token => {
        const authReq = token
          ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
          : req;

        next.handle(authReq).pipe(
          catchError((error: HttpErrorResponse) => {
            if (error.status === 401 && !req.url.includes('/auth/')) {
              return this.handle401Error(req, next);
            }
            return throwError(() => error);
          })
        ).subscribe(observer);
      });
    });
  }

  private handle401Error(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      return new Observable(observer => {
        this.storage.get('refresh_token').then(refreshToken => {
          if (!refreshToken) {
            this.isRefreshing = false;
            this.storage.remove('access_token');
            this.storage.remove('refresh_token');
            observer.error(new Error('No refresh token'));
            return;
          }

          // Use HttpClient directly to avoid interceptor loop
          fetch(`${environment.apiUrl}/api/v1/identity/auth/refresh-token`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ refreshToken })
          })
          .then(res => res.json())
          .then((response: any) => {
            if (response.success) {
              const newToken = response.data.accessToken;
              this.storage.set('access_token', newToken);
              this.storage.set('refresh_token', response.data.refreshToken);
              this.refreshTokenSubject.next(newToken);
              this.isRefreshing = false;

              const authReq = req.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } });
              next.handle(authReq).subscribe(observer);
            } else {
              this.isRefreshing = false;
              this.storage.remove('access_token');
              this.storage.remove('refresh_token');
              observer.error(new Error('Token refresh failed'));
            }
          })
          .catch(err => {
            this.isRefreshing = false;
            observer.error(err);
          });
        });
      });
    }

    return this.refreshTokenSubject.pipe(
      filter(token => token !== null),
      take(1),
      switchMap(token => {
        const authReq = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
        return next.handle(authReq);
      })
    );
  }
}
