import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserDto, RoleDto, AuditLogDto, AuditLogFilter, UpdateUserRequest } from '../models/admin.models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private http = inject(HttpClient);
  private readonly adminUrl = `${environment.apiUrl}/admin`;
  private readonly userUrl = `${environment.apiUrl}/user`;

  // Users
  getAllUsers(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(this.userUrl);
  }

  getUserById(userId: string): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.userUrl}/${userId}`);
  }

  updateUser(userId: string, request: UpdateUserRequest): Observable<string> {
    return this.http.put<string>(`${this.userUrl}/${userId}`, request, { responseType: 'text' as 'json' });
  }

  deleteUser(userId: string): Observable<string> {
    return this.http.delete<string>(`${this.userUrl}/${userId}`, { responseType: 'text' as 'json' });
  }

  // Roles
  getAllRoles(): Observable<RoleDto[]> {
    return this.http.get<RoleDto[]>(`${this.adminUrl}/roles`);
  }

  grantAccess(targetUserId: string, roleId: string): Observable<{ message: string }> {
    const params = new HttpParams().set('targetUserId', targetUserId).set('roleId', roleId);
    return this.http.post<{ message: string }>(`${this.adminUrl}/grant-access`, {}, { params });
  }

  revokeAccess(targetUserId: string, roleId: string): Observable<{ message: string }> {
    const params = new HttpParams().set('targetUserId', targetUserId).set('roleId', roleId);
    return this.http.delete<{ message: string }>(`${this.adminUrl}/revoke-access`, { params });
  }

  // Audit logs
  getAuditLogs(filter: AuditLogFilter): Observable<AuditLogDto[]> {
    let params = new HttpParams();
    if (filter.entity)    params = params.set('entity',    filter.entity);
    if (filter.entityId)  params = params.set('entityId',  filter.entityId);
    if (filter.userId)    params = params.set('userId',    filter.userId);
    if (filter.username)  params = params.set('username',  filter.username);
    if (filter.action)    params = params.set('action',    filter.action);
    if (filter.fromDate)  params = params.set('fromDate',  filter.fromDate);
    if (filter.toDate)    params = params.set('toDate',    filter.toDate);
    return this.http.get<AuditLogDto[]>(`${this.adminUrl}/audit-logs`, { params });
  }
}
