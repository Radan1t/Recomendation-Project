import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private gatewayUrl = 'http://localhost:5000/api/v1'; 

  constructor(private http: HttpClient) {}

  

  importGames(page: number = 1): Observable<any> {
    return this.http.post(`${this.gatewayUrl}/Games/import-from-rawg`, null, {
      params: new HttpParams().set('page', page.toString()).set('pageSize', '100')
    });
  }

  importMovies(page: number = 1): Observable<any> {
    return this.http.post(`${this.gatewayUrl}/TmdbImport/import-movies`, null, {
      params: new HttpParams().set('page', page.toString())
    });
  }

  importSeries(page: number = 1): Observable<any> {
    return this.http.post(`${this.gatewayUrl}/TmdbImport/import-series`, null, {
      params: new HttpParams().set('page', page.toString())
    });
  }

  importBooks(subject: string = 'fiction', startIndex: number = 0): Observable<any> {
    return this.http.post(`${this.gatewayUrl}/BooksImport/import`, null, {
      params: new HttpParams().set('subject', subject).set('startIndex', startIndex.toString())
    });
  }

  

  getContent(category: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.gatewayUrl}/content/admin/list?type=${category}`);
  }

  deleteContent(id: number): Observable<any> {
    return this.http.delete(`${this.gatewayUrl}/content/admin/${id}`);
  }
}
