import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/development';
import { Observable } from 'rxjs';
import { UserCreateRequest, UserDto, UserUpdateRequest } from '../models/users';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/users`;

  getAll(includeInactive = true): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(
      `${this.base}?includeInactive=${includeInactive}`
    );
  }

  create(payload: UserCreateRequest): Observable<any> {
    return this.http.post(`${this.base}/create`, payload);
  }

  update(id: string, payload: UserUpdateRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, payload);
  }

  setStatus(id: string, active: boolean): Observable<any> {
    return this.http.post<{ active: boolean }>(`${this.base}/${id}/status`, {
      active,
    });
  }
}
