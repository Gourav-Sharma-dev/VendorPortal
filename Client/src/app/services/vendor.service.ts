import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { VendorDto, VendorRequest, VendorDocumentDto, VendorStatusHistoryDto } from '../models/vendor.models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class VendorService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/vendor`;

  registerVendor(request: VendorRequest): Observable<{ vendorId: string; message: string }> {
    return this.http.post<{ vendorId: string; message: string }>(`${this.baseUrl}/register`, request);
  }

  getAllVendors(): Observable<VendorDto[]> {
    return this.http.get<VendorDto[]>(this.baseUrl);
  }

  getMyVendors(): Observable<VendorDto[]> {
    return this.http.get<VendorDto[]>(`${this.baseUrl}/mylist`);
  }

  getVendorById(vendorId: string): Observable<VendorDto> {
    return this.http.get<VendorDto>(`${this.baseUrl}/${vendorId}`);
  }

  getVendorsByStatus(status: string): Observable<VendorDto[]> {
    return this.http.get<VendorDto[]>(`${this.baseUrl}/status/${status}`);
  }

  updateVendor(vendorId: string, request: VendorRequest): Observable<string> {
    return this.http.put<string>(`${this.baseUrl}/${vendorId}`, request);
  }

  uploadDocument(vendorId: string, documentType: string, file: File): Observable<string> {
    const formData = new FormData();
    formData.append('documentType', documentType);
    formData.append('file', file);
    return this.http.post<string>(`${this.baseUrl}/${vendorId}/upload-document`, formData);
  }

  getVendorDocuments(vendorId: string): Observable<VendorDocumentDto[]> {
    return this.http.get<VendorDocumentDto[]>(`${this.baseUrl}/${vendorId}/documents`);
  }
  downloadDocument(fileName: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/download/${encodeURIComponent(fileName)}`, { responseType: 'blob' });
  }
}
