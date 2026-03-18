import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { VendorStatusHistoryDto } from '../models/vendor.models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApprovalService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/approval`;

  procurementApproval(vendorId: string, isApproved: boolean, remarks?: string): Observable<string> {
    let params = new HttpParams().set('isApproved', isApproved.toString());
    if (remarks) params = params.set('remarks', remarks);
    return this.http.post<string>(`${this.baseUrl}/${vendorId}/procurement-approval`, {}, { params, responseType: 'text' as 'json' });
  }

  financeApproval(vendorId: string, isApproved: boolean, remarks?: string): Observable<string> {
    let params = new HttpParams().set('isApproved', isApproved.toString());
    if (remarks) params = params.set('remarks', remarks);
    return this.http.post<string>(`${this.baseUrl}/${vendorId}/finance-approval`, {}, { params, responseType: 'text' as 'json' });
  }

  rejectVendor(vendorId: string, remarks: string): Observable<string> {
    const params = new HttpParams().set('remarks', remarks);
    return this.http.post<string>(`${this.baseUrl}/${vendorId}/reject`, {}, { params, responseType: 'text' as 'json' });
  }

  requestCorrection(vendorId: string, remarks: string): Observable<string> {
    const params = new HttpParams().set('remarks', remarks);
    return this.http.post<string>(`${this.baseUrl}/${vendorId}/request-correction`, {}, { params, responseType: 'text' as 'json' });
  }

  activateVendor(vendorId: string): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/${vendorId}/activate`, {}, { responseType: 'text' as 'json' });
  }

  getStatusHistory(vendorId: string): Observable<VendorStatusHistoryDto[]> {
    return this.http.get<VendorStatusHistoryDto[]>(`${this.baseUrl}/${vendorId}/history`);
  }
}
