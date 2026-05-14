import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Genre {
  GenreID: number;
  Name: string;
}

export interface Language {
  LanguageID: number;
  Code: string;
  Name: string;
}

@Injectable({
  providedIn: 'root' 
})
export class ContentService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5000/api/v1/content'; 
  
  private recommendationUrl = 'http://localhost:5000/api/v1/recommendations'; 

  getRandomPosters(count: number): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/random-posters?count=${count}`);
  }

  getGenres(): Observable<Genre[]> {
    return this.http.get<Genre[]>(`${this.apiUrl}/genres`);
  }

  getLanguages(): Observable<Language[]> {
    return this.http.get<Language[]>(`${this.apiUrl}/languages`);
  }
  
  getContentDetailsByIds(ids: number[]): Observable<any[]> {
    const params = ids.map(id => `ids=${id}`).join('&');
    return this.http.get<any[]>(`${this.apiUrl}/details?${params}`);
  }

  getContentDetails(id: string | number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  
  
  
  searchGlobalContent(query: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/search?q=${encodeURIComponent(query)}`);
  }

  
  
  
  getRecommendations(contentId: number | string): Observable<any[]> {
    
    return this.http.get<any[]>(`${this.recommendationUrl}/${contentId}`);
  }
}
