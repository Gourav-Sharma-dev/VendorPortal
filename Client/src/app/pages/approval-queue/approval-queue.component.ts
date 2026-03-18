import { Component, inject, OnInit, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { VendorService } from '../../services/vendor.service';
import { AuthService } from '../../services/auth.service';
import { ApprovalService } from '../../services/approval.service';
import { VendorDto } from '../../models/vendor.models';

@Component({
  selector: 'app-approval-queue',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, RouterLink],
  templateUrl: './approval-queue.component.html',
  styleUrl: './approval-queue.component.css'
})
export class ApprovalQueueComponent implements OnInit {
  private vendorService = inject(VendorService);
  private auth = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);

  vendors: VendorDto[] = [];
  loading = false;
  activeTab = 'Submitted';

  tabs = [
    { key: 'Submitted',           label: 'Submitted',    icon: 'bi-send' },
    { key: 'UnderReview',         label: 'Under Review', icon: 'bi-search' },
    { key: 'FinanceVerification', label: 'Finance Verify', icon: 'bi-bank' },
    { key: 'CorrectionRequested', label: 'Correction',   icon: 'bi-pencil-square' },
    { key: 'Approved',            label: 'Approved',     icon: 'bi-check-circle' },
    { key: 'Rejected',            label: 'Rejected',     icon: 'bi-x-circle' },
    { key: 'Active',              label: 'Active',       icon: 'bi-lightning-charge' }
  ];

  hasRole(...roles: string[]): boolean { return this.auth.hasRole(...roles); }

  get filtered(): VendorDto[] {
    return this.vendors.filter(v => v.status === this.activeTab);
  }

  count(status: string): number {
    return this.vendors.filter(v => v.status === status).length;
  }

  private readonly tabColors: Record<string, [string, string]> = {
    'Submitted':           ['#eff6ff', '#1a56db'],
    'UnderReview':         ['#fffbeb', '#d97706'],
    'FinanceVerification': ['#f0f9ff', '#0284c7'],
    'CorrectionRequested': ['#fff7ed', '#ea580c'],
    'Approved':            ['#f0fdf4', '#16a34a'],
    'Rejected':            ['#fff1f2', '#dc2626'],
    'Active':              ['#fdf4ff', '#9333ea'],
  };
  tabIconBg(key: string): string    { return this.tabColors[key]?.[0] ?? '#f8fafc'; }
  tabIconColor(key: string): string { return this.tabColors[key]?.[1] ?? '#6c757d'; }

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

  ngOnInit(): void {
    this.loading = true;
    this.vendorService.getAllVendors().subscribe({
      next: (v) => { this.vendors = v; this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.loading = false; this.cdr.markForCheck(); }
    });
  }
}
