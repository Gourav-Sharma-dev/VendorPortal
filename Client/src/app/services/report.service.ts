import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ReportFilter, VendorRegistrationReportDto, VendorVerificationReportDto } from '../models/admin.models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/report`;

  getVendorRegistrationReport(filter: ReportFilter): Observable<VendorRegistrationReportDto[]> {
    let params = new HttpParams();
    if (filter.fromDate) params = params.set('fromDate', filter.fromDate);
    if (filter.toDate) params = params.set('toDate', filter.toDate);
    if (filter.status) params = params.set('status', filter.status);
    if (filter.vendorType) params = params.set('vendorType', filter.vendorType);
    return this.http.get<VendorRegistrationReportDto[]>(`${this.baseUrl}/vendor-registration`, { params });
  }

  getVendorVerificationReport(filter: ReportFilter): Observable<VendorVerificationReportDto[]> {
    let params = new HttpParams();
    if (filter.fromDate) params = params.set('fromDate', filter.fromDate);
    if (filter.toDate) params = params.set('toDate', filter.toDate);
    if (filter.status) params = params.set('status', filter.status);
    return this.http.get<VendorVerificationReportDto[]>(`${this.baseUrl}/vendor-verification`, { params });
  }
}
