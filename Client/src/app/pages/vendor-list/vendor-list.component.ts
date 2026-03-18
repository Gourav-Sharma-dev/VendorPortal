import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { VendorService } from '../../services/vendor.service';
import { VendorDto } from '../../models/vendor.models';

@Component({
  selector: 'app-vendor-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './vendor-list.component.html',
  styleUrl: './vendor-list.component.css'
})
export class VendorListComponent implements OnInit {
  private vendorService = inject(VendorService);

  vendors: VendorDto[] = [];
  loading = false;
  search = '';
  statusFilter = '';
  typeFilter = '';

  get filtered(): VendorDto[] {
    return this.vendors.filter(v => {
      const s = this.search.toLowerCase();
      const matchSearch = !s || v.vendorName.toLowerCase().includes(s) || v.emailAddress.toLowerCase().includes(s);
      const matchStatus = !this.statusFilter || v.status === this.statusFilter;
      const matchType = !this.typeFilter || v.vendorType === this.typeFilter;
      return matchSearch && matchStatus && matchType;
    });
  }

  ngOnInit(): void {
    this.loading = true;
    this.vendorService.getAllVendors().subscribe({
      next: (v) => { this.vendors = v; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  statusClass(status: string): string {
    const map: Record<string, string> = {
      'Submitted': 'bg-primary-subtle text-primary',
      'UnderReview': 'bg-warning-subtle text-warning',
      'FinanceVerification': 'bg-orange-subtle text-orange',
      'CorrectionRequested': 'bg-purple-subtle text-purple',
      'Approved': 'bg-success-subtle text-success',
      'Active': 'bg-success text-white',
      'Rejected': 'bg-danger-subtle text-danger'
    };
    return map[status] ?? 'bg-secondary-subtle text-secondary';
  }
}
