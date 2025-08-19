import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/development';
import { Observable, tap } from 'rxjs';
import { jwtDecode } from 'jwt-decode';

export type AppJwtPayload = {
  exp?: number;
  name?: string;
  sub?: string;
  role?: string | string[];
  roles?: string[];
  permissions?: string[];
  perms?: string[];
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?:
    | string
    | string[];
};

type LoginResponse = {
  access_token: string;
  token_type: string; // "Bearer"
  expires_at: string; // ISO 8601 UTC
};

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/auth`;
  private tokenKey = 'access_token';
  private expKey = 'expires_at';
  private storage = sessionStorage;

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.apiUrl}/login`, { email, password })
      .pipe(
        tap((res) => {
          if (res?.access_token) {
            this.storage.setItem(this.tokenKey, res.access_token);
            this.storage.setItem(this.expKey, res.expires_at);
          }
        })
      );
  }

  logout(): void {
    this.storage.removeItem(this.tokenKey);
    this.storage.removeItem(this.expKey);
  }

  getToken(): string | null {
    return this.storage.getItem(this.tokenKey);
  }

  private getPayload(): AppJwtPayload | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      return jwtDecode<AppJwtPayload>(token);
    } catch {
      return null;
    }
  }

  private normalizeToArray(v: unknown): string[] {
    if (!v) return [];
    if (Array.isArray(v))
      return v.filter((x): x is string => typeof x === 'string');
    if (typeof v === 'string') return [v];
    return [];
  }

  // Valida contra exp del token y, como fallback, contra expires_at del backend
  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;

    const p = this.getPayload();
    // 1) Si el JWT trae exp, úsalo como autoridad
    if (p?.exp && Date.now() >= p.exp * 1000) return false;

    // 2) Fallback: expires_at que vino en la respuesta de login
    const expiresAt = this.storage.getItem(this.expKey);
    if (expiresAt) {
      const ts = Date.parse(expiresAt);
      if (!Number.isNaN(ts) && Date.now() >= ts) return false;
    }

    return true;
  }

  private getRoles(): string[] {
    const p = this.getPayload();
    if (!p) return [];

    const msft =
      p['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    const fromMsft = this.normalizeToArray(msft);
    if (fromMsft.length) return fromMsft;

    const fromRoles = this.normalizeToArray(p.roles);
    if (fromRoles.length) return fromRoles;

    const fromRole = this.normalizeToArray(p.role);
    if (fromRole.length) return fromRole;

    return [];
  }

  hasRole(requiredRole: string): boolean {
    return this.getRoles().includes(requiredRole);
  }

  hasPermission(perm: string): boolean {
    const list = this.getPermissions();
    return list.includes(perm);
  }

  hasAllPermissions(perms: string[] | string): boolean {
    const list = this.getPermissions();
    const required = Array.isArray(perms) ? perms : [perms];
    return required.every((p) => list.includes(p));
  }

  hasAnyPermission(perms: string[] | string): boolean {
    const list = this.getPermissions();
    const required = Array.isArray(perms) ? perms : [perms];
    return required.some((p) => list.includes(p));
  }

  private getPermissions(): string[] {
    const p = this.getPayload() as any;
    const list = p?.perm ?? p?.perms ?? p?.permissions ?? [];
    return Array.isArray(list) ? list : [];
  }

  getUserName(): string | null {
    const p = this.getPayload();
    return p?.name ?? null;
  }
}
