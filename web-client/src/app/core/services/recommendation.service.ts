import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class RecommendationService {
  private apiUrl = 'http://localhost:5000/api/v1';
  private platformId = inject(PLATFORM_ID);
  private http = inject(HttpClient);

  generateRecommendations(userId?: number): Observable<any> {
    if (isPlatformBrowser(this.platformId)) {
      const token = localStorage.getItem('token');
      if (!token) return of({ recommendations: [] });
      const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
      const uid = userId ?? (localStorage.getItem('user_id') ? Number(localStorage.getItem('user_id')) : null);
      if (uid) {
        return this.http.post(`${this.apiUrl}/recommendations/generate/${uid}`, {}, { headers });
      } else {
        // Fallback: try endpoint without explicit user id
        return this.http.post(`${this.apiUrl}/recommendations/generate`, {}, { headers });
      }
    }
    return of({ recommendations: [] });
  }
}
