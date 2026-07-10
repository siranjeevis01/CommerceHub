import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, PaginatedResponse } from '../models';
import { environment } from '@env/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private http: HttpClient) {}

  private get baseUrl(): string {
    return environment.apiUrl;
  }

  get<T>(path: string, params?: HttpParams): Observable<ApiResponse<T>> {
    return this.http.get<ApiResponse<T>>(`${this.baseUrl}${path}`, { params });
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
}
