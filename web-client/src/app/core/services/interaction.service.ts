import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class InteractionService {
  private http = inject(HttpClient);
  private auth = inject(AuthService);
  
  private apiUrl = 'http://localhost:5000/api/v1/interactions';

  private buildHeaders() {
    const uid = this.auth.getUserId();
    return uid ? new HttpHeaders({ 'X-User-Id': uid }) : undefined;
  }

  rateContent(contentId: number, score: number) {
    const headers = this.buildHeaders();
    const options = headers ? { headers } : {};
    return this.http.post(`${this.apiUrl}/rate`, { contentId, score }, options);
  }

  toggleFavorite(contentId: number) {
    const headers = this.buildHeaders();
    const options = headers ? { headers } : {};
    return this.http.post<any>(`${this.apiUrl}/favorite`, { contentId }, options);
  }

  getInteractionStatus(contentId: string) {
    const headers = this.buildHeaders();
    const options = headers ? { headers } : {};
    return this.http.get<any>(`${this.apiUrl}/status/${contentId}`, options);
  }

  getUserRatings() {
    const headers = this.buildHeaders();
    const options = headers ? { headers } : {};
    return this.http.get<any[]>(`${this.apiUrl}/user/ratings`, options);
  }

  getUserFavorites() {
    const headers = this.buildHeaders();
    const options = headers ? { headers } : {};
    return this.http.get<any[]>(`${this.apiUrl}/user/favorites`, options);
  }

  getContentAverage(contentId: number) {
    const headers = this.buildHeaders();
    const options = headers ? { headers } : {};
    return this.http.get<any>(`${this.apiUrl}/content/${contentId}/average`, options);
  }
}
