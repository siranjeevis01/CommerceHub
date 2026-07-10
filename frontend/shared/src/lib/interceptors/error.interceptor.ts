import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private router: Router) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        let message = 'An unexpected error occurred';
        if (error.error?.message) {
          message = error.error.message;
        } else if (error.error?.errors) {
          message = Object.values(error.error.errors).flat().join(', ');
        } else if (error.status === 403) {
          message = 'You do not have permission to perform this action';
        } else if (error.status === 404) {
          message = 'The requested resource was not found';
        } else if (error.status === 429) {
          message = 'Too many requests. Please try again later.';
        } else if (error.status >= 500) {
          message = 'Server error. Please try again later.';
        }
        return throwError(() => new Error(message));
      })
    );
  }
}
