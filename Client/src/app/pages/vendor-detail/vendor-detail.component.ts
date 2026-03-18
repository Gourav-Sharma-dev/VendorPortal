import { Component, inject, OnInit, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { VendorService } from '../../services/vendor.service';
import { ApprovalService } from '../../services/approval.service';
import { AuthService } from '../../services/auth.service';
import { VendorDto, VendorStatusHistoryDto, VendorDocumentDto } from '../../models/vendor.models';

@Component({
  selector: 'app-vendor-detail',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './vendor-detail.component.html',
  styleUrl: './vendor-detail.component.css'
})
export class VendorDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private vendorService = inject(VendorService);
  private approvalService = inject(ApprovalService);
  private auth = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);
  private sanitizer = inject(DomSanitizer);

  vendor: VendorDto | null = null;
  history: VendorStatusHistoryDto[] = [];
  documents: VendorDocumentDto[] = [];
  loading = false;
  actionLoading = false;
  actionMsg = '';
  actionError = '';
  remarks = '';
  showRemarkBox = false;
  pendingAction = '';

  // Preview modal
  previewDoc$: VendorDocumentDto | null = null;
  previewUrl: SafeResourceUrl = '';

  hasRole(...roles: string[]): boolean { return this.auth.hasRole(...roles); }

  closePreview(): void {
    this.previewDoc$ = null;
    this.cdr.markForCheck();
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.loading = true;
    this.vendorService.getVendorById(id).subscribe({
      next: (v) => { this.vendor = v; this.loading = false; this.cdr.markForCheck(); this.loadHistory(id); this.loadDocuments(id); },
      error: () => { this.loading = false; this.cdr.markForCheck(); }
    });
  }

  loadHistory(id: string): void {
    this.approvalService.getStatusHistory(id).subscribe({
      next: (h) => { this.history = h; this.cdr.markForCheck(); }, error: () => { }
    });
  }

  loadDocuments(id: string): void {
    this.vendorService.getVendorDocuments(id).subscribe({
      next: (d) => { this.documents = d; this.cdr.markForCheck(); }, error: () => { }
    });
  }

  triggerAction(action: string): void {
    if (action === 'reject' || action === 'correction') {
      this.pendingAction = action;
      this.showRemarkBox = true;
    } else {
      this.executeAction(action, '');
    }
  }

  confirmAction(): void {
    this.executeAction(this.pendingAction, this.remarks);
    this.showRemarkBox = false;
  }

  private executeAction(action: string, remark: string): void {
    if (!this.vendor) return;
    this.actionLoading = true;
    this.actionMsg = '';
    this.actionError = '';
    const id = this.vendor.vendorId;
    let call;
    if (action === 'procurement') call = this.approvalService.procurementApproval(id, true, remark);
    else if (action === 'finance') call = this.approvalService.financeApproval(id, true, remark);
    else if (action === 'reject') call = this.approvalService.rejectVendor(id, remark);
    else if (action === 'correction') call = this.approvalService.requestCorrection(id, remark);
    else if (action === 'activate') call = this.approvalService.activateVendor(id);
    else return;

    call.subscribe({
      next: (msg: string) => {
        this.actionMsg = msg;
        this.actionLoading = false;
        this.remarks = '';
        this.cdr.markForCheck();
        this.vendorService.getVendorById(id).subscribe(v => { this.vendor = v; this.cdr.markForCheck(); });
        this.loadHistory(id);
      },
      error: (err: { error?: { message?: string } }) => {
        this.actionError = err.error?.message || 'Action failed.';
        this.actionLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  statusClass(status: string): string {
    const map: Record<string, string> = {
      'Submitted': 'bg-primary-subtle text-primary',
      'UnderReview': 'bg-warning-subtle text-warning',
      'FinanceVerification': 'bg-warning text-white',
      'CorrectionRequested': 'bg-danger-subtle text-danger',
      'Approved': 'bg-success-subtle text-success',
      'Active': 'bg-success text-white',
      'Rejected': 'bg-danger text-white'
    };
    return map[status] ?? 'bg-secondary-subtle text-secondary';
  }
  downloadAndPreview(doc: VendorDocumentDto): void {
    this.vendorService.downloadDocument(doc.fileName).subscribe(blob => {
      const url = URL.createObjectURL(blob);
      this.previewDoc$ = doc;
      this.previewUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
    });
  }

  downloadFile(doc: VendorDocumentDto): void {
    this.vendorService.downloadDocument(doc.fileName).subscribe(blob => {
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = doc.fileName;
      a.click();
      URL.revokeObjectURL(url);
    });
  }
  // Add this for modal download button
  downloadPreviewedFile(): void {
    if (!this.previewDoc$) return;
    this.vendorService.downloadDocument(this.previewDoc$.fileName).subscribe(blob => {
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = this.previewDoc$!.fileName;
      a.click();
      URL.revokeObjectURL(url);
    });
  }
}
