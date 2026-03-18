import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { VendorService } from '../../../services/vendor.service';
import { VendorRequest } from '../../../models/vendor.models';

@Component({
  selector: 'app-vendor-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './vendor-register.component.html',
  styleUrl: './vendor-register.component.css'
})
export class VendorRegisterComponent {
  private fb = inject(FormBuilder);
  private vendorService = inject(VendorService);
  private router = inject(Router);

  currentStep = 1;
  steps = [
    { id: 1, label: 'Basic Details' },
    { id: 2, label: 'Business Details' },
    { id: 3, label: 'Tax Details' },
    { id: 4, label: 'Bank Details' }
  ];
  loading = false;
  errorMsg = '';
  successMsg = '';

  form: FormGroup = this.fb.group({
    vendorName: ['', Validators.required],
    vendorType: ['', Validators.required],
    contactPerson: ['', Validators.required],
    mobileNumber: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
    emailAddress: ['', [Validators.required, Validators.email]],
    companyName: ['', Validators.required],
    companyAddress: ['', Validators.required],
    city: ['', Validators.required],
    state: ['', Validators.required],
    country: ['', Validators.required],
    pinCode: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
    gstNumber: ['', [Validators.required, Validators.pattern(/^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$/)]],
    panNumber: ['', [Validators.required, Validators.pattern(/^[A-Z]{5}[0-9]{4}[A-Z]{1}$/)]],
    bankDetail: this.fb.group({
      bankName: ['', Validators.required],
      accountNumber: ['', Validators.required],
      ifscCode: ['', [Validators.required, Validators.pattern(/^[A-Z]{4}0[A-Z0-9]{6}$/)]],
      branchName: ['', Validators.required]
    })
  });

  get f(): { [key: string]: any } { return this.form.controls; }
  get bankDetail(): { [key: string]: any } { return (this.form.get('bankDetail') as FormGroup).controls; }

  nextStep(): void {
    const stepFields: { [key: number]: string[] } = {
      1: ['vendorName', 'vendorType', 'contactPerson', 'mobileNumber', 'emailAddress'],
      2: ['companyName', 'companyAddress', 'city', 'state', 'country', 'pinCode'],
      3: ['gstNumber', 'panNumber']
    };
    const fields = stepFields[this.currentStep];
    if (fields) {
      fields.forEach(f => this.form.get(f)?.markAsTouched());
      if (fields.some(f => this.form.get(f)?.invalid)) return;
    }
    this.currentStep++;
  }

  prevStep(): void { this.currentStep--; }

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    this.errorMsg = '';
    this.vendorService.registerVendor(this.form.value as VendorRequest).subscribe({
      next: (res) => {
        this.successMsg = res.message;
        this.loading = false;
        setTimeout(() => this.router.navigate(['/app/vendor/dashboard']), 1500);
      },
      error: (err: { error?: { message?: string } }) => {
        this.errorMsg = err.error?.message || 'Registration failed. Please try again.';
        this.loading = false;
      }
    });
  }
}
