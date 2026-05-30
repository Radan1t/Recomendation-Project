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
          if (typeof window !== 'undefined') {
            localStorage.setItem('token', response.token);
            localStorage.setItem('user_priorities', JSON.stringify(response.priorities));

            // try to extract user id from token
            try {
              const payload = JSON.parse(atob(response.token.split('.')[1]));
              const uid = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload['nameid'] || payload['sub'] || payload['NameIdentifier'] || payload['name_identifier'] || payload['nameidentifier'];
              if (uid) localStorage.setItem('user_id', uid.toString());
            } catch (e) { /* ignore decoding errors */ }
          }
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
    localStorage.removeItem('token');
    localStorage.removeItem('user_id');
    localStorage.removeItem('user_priorities');
    window.location.reload();
  }

  getUserId(): string | null {
    if (typeof window === 'undefined') return null;
    const uid = localStorage.getItem('user_id');
    if (uid) return uid;

    const token = localStorage.getItem('token');
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const uidVal = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload['nameid'] || payload['sub'] || payload['NameIdentifier'] || payload['name_identifier'] || payload['nameidentifier'];
      if (uidVal) {
        localStorage.setItem('user_id', uidVal.toString());
        return uidVal.toString();
      }
    } catch { }
    return null;
  }
}
