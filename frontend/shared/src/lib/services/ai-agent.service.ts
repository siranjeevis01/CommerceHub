import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models';
import { environment } from '@env/environment';

export interface ChatRequest {
  message: string;
  conversationId?: number;
  context?: string;
}

export interface ChatResponse {
  conversationId: number;
  reply: string;
  intent?: string;
  action?: string;
  data?: Record<string, any>;
  recommendations?: ProductRecommendation[];
  searchResults?: ProductSearchResult[];
}

export interface Conversation {
  id: number;
  userId: number;
  title: string;
  status: string;
  createdAt: string;
  lastActivityAt?: string;
  messageCount: number;
}

export interface Message {
  id: number;
  conversationId: number;
  role: string;
  content: string;
  intent?: string;
  createdAt: string;
}

export interface ProductRecommendation {
  productId: number;
  productName: string;
  imageUrl?: string;
  price: number;
  comparePrice?: number;
  score: number;
  reason?: string;
}

export interface ProductSearchResult {
  id: number;
  name: string;
  slug: string;
  price: number;
  comparePrice?: number;
  mainImageUrl?: string;
  categoryName?: string;
  vendorName?: string;
  relevanceScore?: number;
}

export interface SearchResult {
  items: ProductSearchResult[];
  totalCount: number;
  page: number;
  pageSize: number;
  correctedQuery?: string;
  intent?: string;
}

export interface SearchRequest {
  query: string;
  page?: number;
  pageSize?: number;
}

export interface RecordInteractionRequest {
  productId: number;
  interactionType: 'view' | 'purchase' | 'cart' | 'wishlist';
}

@Injectable({ providedIn: 'root' })
export class AIAgentService {
  constructor(private http: HttpClient) {}

  chat(request: ChatRequest): Observable<ApiResponse<ChatResponse>> {
    return this.http.post<ApiResponse<ChatResponse>>(
      `${environment.apiUrl}/api/v1/ai/chat`, request
    );
  }

  getConversations(page = 1, pageSize = 20): Observable<ApiResponse<Conversation[]>> {
    return this.http.get<ApiResponse<Conversation[]>>(
      `${environment.apiUrl}/api/v1/ai/conversations`, {
        params: { page: page.toString(), pageSize: pageSize.toString() }
      }
    );
  }

  getConversationMessages(conversationId: number): Observable<ApiResponse<Message[]>> {
    return this.http.get<ApiResponse<Message[]>>(
      `${environment.apiUrl}/api/v1/ai/conversations/${conversationId}/messages`
    );
  }

  search(request: SearchRequest): Observable<ApiResponse<SearchResult>> {
    return this.http.post<ApiResponse<SearchResult>>(
      `${environment.apiUrl}/api/v1/ai/search`, request
    );
  }

  getRecommendations(count = 10, type?: string): Observable<ApiResponse<ProductRecommendation[]>> {
    let params: any = { count: count.toString() };
    if (type) params.type = type;
    return this.http.get<ApiResponse<ProductRecommendation[]>>(
      `${environment.apiUrl}/api/v1/ai/recommendations`, { params }
    );
  }

  getTrending(count = 10): Observable<ApiResponse<ProductRecommendation[]>> {
    return this.http.get<ApiResponse<ProductRecommendation[]>>(
      `${environment.apiUrl}/api/v1/ai/recommendations/trending`, {
        params: { count: count.toString() }
      }
    );
  }

  recordInteraction(request: RecordInteractionRequest): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(
      `${environment.apiUrl}/api/v1/ai/interactions`, request
    );
  }
}