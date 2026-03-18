import { Component, inject, OnInit, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../services/admin.service';
import { AuditLogDto, AuditLogFilter } from '../../../models/admin.models';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, FormsModule],
  templateUrl: './audit-logs.component.html',
  styleUrl: './audit-logs.component.css'
})
export class AuditLogsComponent implements OnInit {
  private adminService = inject(AdminService);
  private cdr = inject(ChangeDetectorRef);

  logs: AuditLogDto[] = [];
  loading = false;
  filter: AuditLogFilter = {};

  // Pagination
  pageSize = 20;
  page = 1;

  get totalPages(): number { return Math.max(1, Math.ceil(this.logs.length / this.pageSize)); }
  get paged(): AuditLogDto[] {
    const start = (this.page - 1) * this.pageSize;
    return this.logs.slice(start, start + this.pageSize);
  }
  get pages(): number[] { return Array.from({ length: this.totalPages }, (_, i) => i + 1); }

  ngOnInit(): void {
    // Default: last 7 days
    const today = new Date();
    const sevenDaysAgo = new Date();
    sevenDaysAgo.setDate(today.getDate() - 7);
    today.setDate(today.getDate() + 1); // Include today
    this.filter.toDate   = today.toISOString().split('T')[0];
    this.filter.fromDate = sevenDaysAgo.toISOString().split('T')[0];
    this.load();
  }

  load(): void {
    this.loading = true;
    this.page = 1;
    this.adminService.getAuditLogs(this.filter).subscribe({
      next: (l) => { this.logs = l; this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.loading = false; this.cdr.markForCheck(); }
    });
  }

  reset(): void {
    const today = new Date();
    const sevenDaysAgo = new Date();
    sevenDaysAgo.setDate(today.getDate() - 7);
    this.filter = {
      toDate:   today.toISOString().split('T')[0],
      fromDate: sevenDaysAgo.toISOString().split('T')[0]
    };
    this.load();
  }

  actionBadgeClass(action: string): string {
    if (action.toLowerCase().includes('reject')) return 'bg-danger-subtle text-danger';
    if (action.toLowerCase().includes('approv') || action.toLowerCase().includes('activat')) return 'bg-success-subtle text-success';
    if (action.toLowerCase().includes('correction') || action.toLowerCase().includes('update')) return 'bg-warning-subtle text-warning';
    return 'bg-primary-subtle text-primary';
  }
}
