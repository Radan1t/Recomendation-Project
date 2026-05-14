import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';


export interface LoginResponse {
  token: string;
  priorities: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5000/api/v1/auth'; 

  register(dto: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, dto);
  }

  
  login(credentials: any): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        if (response.token) {
          localStorage.setItem('token', response.token);
          localStorage.setItem('user_priorities', JSON.stringify(response.priorities));
        }
      })
    );
  }

  getPriorities(): string[] {
    if (typeof window !== 'undefined') {
      const p = localStorage.getItem('user_priorities');
      return p ? JSON.parse(p) : ['Games', 'Films', 'Series', 'Books'];
    }
    return ['Games', 'Films', 'Series', 'Books'];
  }

  logout() {
    localStorage.clear();
    window.location.reload();
  }
}
