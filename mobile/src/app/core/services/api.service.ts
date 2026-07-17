import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, PaginatedResponse } from '../models';
import { environment } from '@env/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private get baseUrl(): string {
    return environment.apiUrl;
  }

  constructor(private http: HttpClient) {}

  get<T>(path: string, params?: HttpParams | Record<string, any>): Observable<ApiResponse<T>> {
    let httpParams = new HttpParams();
    if (params instanceof HttpParams) {
      httpParams = params;
    } else if (params) {
      Object.keys(params).forEach(key => {
        const val = params[key];
        if (val !== undefined && val !== null) {
          httpParams = httpParams.set(key, String(val));
        }
      });
    }
    return this.http.get<ApiResponse<T>>(`${this.baseUrl}${path}`, { params: httpParams });
  }

  post<T>(path: string, body?: any): Observable<ApiResponse<T>> {
    return this.http.post<ApiResponse<T>>(`${this.baseUrl}${path}`, body);
  }

  put<T>(path: string, body?: any): Observable<ApiResponse<T>> {
    return this.http.put<ApiResponse<T>>(`${this.baseUrl}${path}`, body);
  }

  delete<T>(path: string, body?: any): Observable<ApiResponse<T>> {
    return this.http.request<ApiResponse<T>>('DELETE', `${this.baseUrl}${path}`, { body });
  }

  getPaginated<T>(path: string, page: number, pageSize: number, params?: HttpParams): Observable<PaginatedResponse<T>> {
    let p = params || new HttpParams();
    p = p.set('page', page.toString()).set('pageSize', pageSize.toString());
    return this.http.get<PaginatedResponse<T>>(`${this.baseUrl}${path}`, { params: p });
  }

  upload<T>(path: string, formData: FormData): Observable<ApiResponse<T>> {
    return this.http.post<ApiResponse<T>>(`${this.baseUrl}${path}`, formData, {
      reportProgress: false
    });
  }
}
