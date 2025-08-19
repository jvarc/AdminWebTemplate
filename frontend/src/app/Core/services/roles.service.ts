import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/development';
import { Observable } from 'rxjs';
import { RoleDto } from '../models/roles';

@Injectable({ providedIn: 'root' })
export class RolesService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/roles`;

  getAll(): Observable<RoleDto[]> {
    return this.http.get<RoleDto[]>(this.base);
  }

  create(roleName: string): Observable<void | { message?: string }> {
    return this.http.post<void | { message?: string }>(`${this.base}/create`, {
      roleName,
    });
  }

  delete(roleId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${encodeURIComponent(roleId)}`);
  }

  getAllPermissions(): Observable<string[]> {
    return this.http.get<string[]>(`${this.base}/permissions`);
  }

  getRolePermissions(roleName: string): Observable<string[]> {
    return this.http.get<string[]>(
      `${this.base}/${encodeURIComponent(roleName)}/permissions`
    );
  }

  setRolePermissions(
    roleName: string,
    permissions: string[]
  ): Observable<void> {
    return this.http.put<void>(
      `${this.base}/${encodeURIComponent(roleName)}/permissions`,
      { permissions }
    );
  }
}
