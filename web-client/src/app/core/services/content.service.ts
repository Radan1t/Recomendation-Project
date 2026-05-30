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

  getGenres(type?: string): Observable<Genre[]> {
    let url = `${this.apiUrl}/genres`;
    if (type) url += `?type=${encodeURIComponent(type)}`;
    return this.http.get<Genre[]>(url);
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

  getContentList(type: string, q?: string, genreId?: number, page: number = 1, pageSize: number = 20): Observable<any> {
    let params = `?type=${encodeURIComponent(type)}&page=${page}&pageSize=${pageSize}`;
    if (q) params += `&q=${encodeURIComponent(q)}`;
    if (genreId) params += `&genreId=${genreId}`;
    return this.http.get<any>(`${this.apiUrl}/list${params}`);
  }

  
  searchGlobalContent(query: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/search?q=${encodeURIComponent(query)}`);
  }

  
  
  
  getRecommendations(contentId: number | string): Observable<any[]> {
    
    return this.http.get<any[]>(`${this.recommendationUrl}/${contentId}`);
  }
}
