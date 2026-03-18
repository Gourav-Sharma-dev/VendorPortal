import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { AuthResponse, LoginRequest, RegisterRequest, ResetPasswordRequest, DecodedToken } from '../models/auth.models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private readonly TOKEN_KEY = 'vp_token';
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  private currentUserSubject = new BehaviorSubject<DecodedToken | null>(this.decodeStoredToken());
  currentUser$ = this.currentUserSubject.asObservable();

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, request).pipe(
      tap((res) => {
        localStorage.setItem(this.TOKEN_KEY, res.token);
        this.currentUserSubject.next(this.decodeToken(res.token));
      })
    );
  }

  register(request: RegisterRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/register`, request);
  }

  resetPassword(request: ResetPasswordRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/reset-password`, request);
  }

  logout(): void {
    this.http.post(`${this.baseUrl}/logout`, {}).subscribe({ error: () => {} });
    localStorage.removeItem(this.TOKEN_KEY);
    this.currentUserSubject.next(null);
    this.router.navigate(['/auth/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;
    const decoded = this.decodeToken(token);
    if (!decoded) return false;
    return decoded.exp * 1000 > Date.now();
  }

  // Claim URI constants matching .NET ClaimTypes
  private readonly CLAIM_NAME = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';
  private readonly CLAIM_NAMEID = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
  private readonly CLAIM_ROLE = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

  getCurrentUser(): DecodedToken | null {
    return this.currentUserSubject.value;
  }

  get username(): string {
    const u = this.getCurrentUser();
    return (u?.[this.CLAIM_NAME] as string) ?? '';
  }

  get userId(): string {
    const u = this.getCurrentUser();
    return (u?.[this.CLAIM_NAMEID] as string) ?? '';
  }

  getRoles(): string[] {
    const user = this.getCurrentUser();
    if (!user) return [];
    const raw = user[this.CLAIM_ROLE] as string | string[] | undefined;
    if (!raw) return [];
    return Array.isArray(raw) ? raw : [raw];
  }

  hasRole(...roles: string[]): boolean {
    const userRoles = this.getRoles();
    return roles.some((r) => userRoles.includes(r));
  }

  private decodeToken(token: string): DecodedToken | null {
    try {
      const payload = token.split('.')[1];
      return JSON.parse(atob(payload)) as DecodedToken;
    } catch {
      return null;
    }
  }

  private decodeStoredToken(): DecodedToken | null {
    const token = localStorage.getItem(this.TOKEN_KEY);
    return token ? this.decodeToken(token) : null;
  }
}
