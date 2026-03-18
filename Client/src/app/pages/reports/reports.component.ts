import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportService } from '../../services/report.service';
import { VendorRegistrationReportDto, VendorVerificationReportDto, ReportFilter } from '../../models/admin.models';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.css'
})
export class ReportsComponent implements OnInit {
  private reportService = inject(ReportService);

  activeTab = 'registration';
  registrationData: VendorRegistrationReportDto[] = [];
  verificationData: VendorVerificationReportDto[] = [];
  loading = false;
  filter: ReportFilter = {};

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
      this.reportService.getVendorRegistrationReport(this.filter).subscribe({
        next: (d) => { this.registrationData = d; this.loading = false; },
        error: () => { this.loading = false; }
      });
      this.reportService.getVendorVerificationReport(this.filter).subscribe({
        next: (d) => { this.verificationData = d; this.loading = false; },
        error: () => { this.loading = false; }
      });
  }

  switchTab(tab: string): void { this.activeTab = tab; this.filter = {}; this.load(); }
  reset(): void { this.filter = {}; this.load(); }

  countByStatuses(statuses: string[]): number {
    return this.registrationData.filter(r => statuses.includes(r.status)).length;
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
}
