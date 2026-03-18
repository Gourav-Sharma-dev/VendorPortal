import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { VendorService } from '../../services/vendor.service';
import { VendorDto } from '../../models/vendor.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  private auth = inject(AuthService);
  private vendorService = inject(VendorService);

  vendors: VendorDto[] = [];
  myVendor: VendorDto | null = null;

  get username(): string { return this.auth.username; }
  get totalVendors(): number { return this.vendors.length; }
  get pendingCount(): number { return this.vendors.filter(v => ['Submitted','UnderReview','FinanceVerification'].includes(v.status)).length; }
  get activeCount(): number { return this.vendors.filter(v => v.status === 'Active').length; }
  get rejectedCount(): number { return this.vendors.filter(v => v.status === 'Rejected').length; }
  hasRole(...roles: string[]): boolean { return this.auth.hasRole(...roles); }

  ngOnInit(): void {
    if (this.auth.hasRole('Admin', 'Procurement', 'Finance')) {
      this.vendorService.getAllVendors().subscribe({ next: (v) => { this.vendors = v; }, error: () => {} });
    }
    if (this.auth.hasRole('Vendor')) {
      this.vendorService.getMyVendors().subscribe({ next: (v) => { this.myVendor = v[0] ?? null; }, error: () => {} });
    }
  }
}


