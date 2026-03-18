import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { VendorService } from '../../../services/vendor.service';
import { AuthService } from '../../../services/auth.service';
import { VendorDto, VendorDocumentDto, VendorRequest } from '../../../models/vendor.models';

@Component({
  selector: 'app-vendor-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, FormsModule],
  templateUrl: './vendor-dashboard.component.html',
  styleUrl: './vendor-dashboard.component.css'
})
export class VendorDashboardComponent implements OnInit {
  private vendorService = inject(VendorService);
  private auth = inject(AuthService);
  private sanitizer = inject(DomSanitizer);
  private fb = inject(FormBuilder);

  /** All vendors belonging to the logged-in user */
  allVendors: VendorDto[] = [];
  vendor: VendorDto | null = null;
  documents: VendorDocumentDto[] = [];
  loading = false;
  uploadLoading = false;
  uploadMsg = '';
  uploadError = '';

  selectedDocType = '';
  selectedFile: File | null = null;

  // Preview modal
  previewDoc$: VendorDocumentDto | null = null;
  previewUrl: SafeResourceUrl = '';

  // Edit mode (shown when CorrectionRequested)
  editMode = false;
  editLoading = false;
  editMsg = '';
  editError = '';
  editStep = 1;
  editSteps = [
    { id: 1, label: 'Basic Details' },
    { id: 2, label: 'Business Details' },
    { id: 3, label: 'Tax Details' },
    { id: 4, label: 'Bank Details' }
  ];
  editForm!: FormGroup;

  // 7-step workflow labels
  workflowSteps = [
    { key: 'Registration',      label: 'Registration',        icon: 'bi-person-plus' },
    { key: 'DocumentUpload',    label: 'Document Upload',     icon: 'bi-upload' },
    { key: 'DataSubmission',    label: 'Data Submission',     icon: 'bi-send' },
    { key: 'ProcurementReview', label: 'Procurement Review',  icon: 'bi-search' },
    { key: 'FinanceVerify',     label: 'Finance Verification',icon: 'bi-bank' },
    { key: 'FinalApproval',     label: 'Final Approval',      icon: 'bi-check-circle' },
    { key: 'Activation',        label: 'Vendor Activation',   icon: 'bi-lightning-charge' }
  ];

  /** Map VendorStatus → which workflow step index is current (0-based) */
  get workflowIndex(): number {
    if (!this.vendor) return 0;
    const map: Record<string, number> = {
      'Draft': 1,
      'Submitted': 2,
      'UnderReview': 3,
      'CorrectionRequested': 3,
      'FinanceVerification': 4,
      'Approved': 5,
      'Active': 6,
      'Rejected': -1
    };
    return map[this.vendor.status] ?? 0;
  }

  workflowStepState(index: number): 'done' | 'current' | 'pending' | 'rejected' {
    if (!this.vendor) return 'pending';
    if (this.vendor.status === 'Rejected') return 'rejected';
    const cur = this.workflowIndex;
    if (index < cur) return 'done';
    if (index === cur) return 'current';
    return 'pending';
  }

  // Masked bank fields
  showAccountNo = false;
  showIfsc = false;

  get hasMandatoryDocs(): boolean {
    const types = this.documents.map(d => d.documentType.toLowerCase());
    return types.some(t => t.includes('gst')) && types.some(t => t.includes('pan'));
  }

  // Legacy statusFlow kept for edit form step reuse
  statusFlow = ['Submitted', 'UnderReview', 'FinanceVerification', 'Approved', 'Active'];

  ngOnInit(): void {
    this.loading = true;
    this.buildEditForm();
    this.vendorService.getMyVendors().subscribe({
      next: (v) => {
        this.allVendors = v;
        this.vendor = v[0] ?? null;
        this.loading = false;
        if (this.vendor) this.loadDocs(this.vendor.vendorId);
      },
      error: () => { this.loading = false; }
    });
  }

  selectVendor(v: VendorDto): void {
    this.vendor = v;
    this.editMode = false;
    this.editMsg = '';
    this.editError = '';
    this.loadDocs(v.vendorId);
  }

  loadDocs(id: string): void {
    this.vendorService.getVendorDocuments(id).subscribe({
      next: (d) => { this.documents = d; },
      error: () => { }
    });
  }

  // ── Edit / Correction ──────────────────────────────────────────────

  private buildEditForm(): void {
    this.editForm = this.fb.group({
      vendorName:     ['', Validators.required],
      vendorType:     ['', Validators.required],
      contactPerson:  ['', Validators.required],
      mobileNumber:   ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
      emailAddress:   ['', [Validators.required, Validators.email]],
      companyName:    ['', Validators.required],
      companyAddress: ['', Validators.required],
      city:           ['', Validators.required],
      state:          ['', Validators.required],
      country:        ['', Validators.required],
      pinCode:        ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
      gstNumber:      ['', [Validators.required, Validators.pattern(/^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$/)]],
      panNumber:      ['', [Validators.required, Validators.pattern(/^[A-Z]{5}[0-9]{4}[A-Z]{1}$/)]],
      bankDetail: this.fb.group({
        bankName:      ['', Validators.required],
        accountNumber: ['', Validators.required],
        ifscCode:      ['', [Validators.required, Validators.pattern(/^[A-Z]{4}0[A-Z0-9]{6}$/)]],
        branchName:    ['', Validators.required]
      })
    });
  }

  openEditMode(): void {
    if (!this.vendor) return;
    this.editMode = true;
    this.editStep = 1;
    this.editMsg = '';
    this.editError = '';
    // Pre-populate form
    this.editForm.patchValue({
      vendorName:     this.vendor.vendorName,
      vendorType:     this.vendor.vendorType,
      contactPerson:  this.vendor.contactPerson,
      mobileNumber:   this.vendor.mobileNumber,
      emailAddress:   this.vendor.emailAddress,
      companyName:    this.vendor.companyName,
      companyAddress: this.vendor.companyAddress,
      city:           this.vendor.city,
      state:          this.vendor.state,
      country:        this.vendor.country,
      pinCode:        this.vendor.pinCode,
      gstNumber:      this.vendor.gstNumber,
      panNumber:      this.vendor.panNumber,
      bankDetail: {
        bankName:      this.vendor.bankDetail?.bankName ?? '',
        accountNumber: this.vendor.bankDetail?.accountNumber ?? '',
        ifscCode:      this.vendor.bankDetail?.ifscCode ?? '',
        branchName:    this.vendor.bankDetail?.branchName ?? ''
      }
    });
  }

  cancelEdit(): void {
    this.editMode = false;
    this.editMsg = '';
    this.editError = '';
  }

  get ef(): { [key: string]: any } { return this.editForm.controls; }
  get eBank(): { [key: string]: any } { return (this.editForm.get('bankDetail') as FormGroup).controls; }

  editNextStep(): void {
    const stepFields: { [key: number]: string[] } = {
      1: ['vendorName','vendorType','contactPerson','mobileNumber','emailAddress'],
      2: ['companyName','companyAddress','city','state','country','pinCode'],
      3: ['gstNumber','panNumber']
    };
    const fields = stepFields[this.editStep];
    if (fields) {
      fields.forEach(f => this.editForm.get(f)?.markAsTouched());
      if (fields.some(f => this.editForm.get(f)?.invalid)) return;
    }
    this.editStep++;
  }

  editPrevStep(): void { this.editStep--; }

  submitEdit(): void {
    if (this.editForm.invalid) { this.editForm.markAllAsTouched(); return; }
    if (!this.vendor) return;
    this.editLoading = true;
    this.editMsg = '';
    this.editError = '';

    this.vendorService.updateVendor(this.vendor.vendorId, this.editForm.value as VendorRequest).subscribe({
      next: () => {
        this.editLoading = false;
        this.editMode = false;
        this.editMsg = 'Application updated and re-submitted for review.';
        // Reload vendor
        this.vendorService.getMyVendors().subscribe(v => {
          this.allVendors = v;
          this.vendor = v.find(x => x.vendorId === this.vendor?.vendorId) ?? v[0] ?? null;
        });
      },
      error: (err: { error?: { message?: string } }) => {
        this.editError = err.error?.message || 'Update failed. Please try again.';
        this.editLoading = false;
      }
    });
  }

  // ── Document upload ────────────────────────────────────────────────

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
  }

  upload(): void {
    if (!this.vendor || !this.selectedFile || !this.selectedDocType) return;
    this.uploadLoading = true;
    this.uploadMsg = '';
    this.uploadError = '';
    this.vendorService.uploadDocument(this.vendor.vendorId, this.selectedDocType, this.selectedFile).subscribe({
      next: () => {
        this.uploadMsg = 'Document uploaded successfully.';
        this.uploadLoading = false;
        this.selectedFile = null;
        this.selectedDocType = '';
        this.loadDocs(this.vendor!.vendorId);
      },
      error: (err: { error?: { message?: string } }) => {
        this.uploadError = err.error?.message || 'Upload failed.';
        this.uploadLoading = false;
      }
    });
  }

  // ── Status / Step helpers ──────────────────────────────────────────

  stepStatus(step: string): 'done' | 'current' | 'pending' | 'rejected' {
    if (!this.vendor) return 'pending';
    if (this.vendor.status === 'Rejected') {
      // All steps before the rejected point are 'done'; mark all as rejected-path
      return 'rejected';
    }
    const idx = this.statusFlow.indexOf(this.vendor.status);
    const stepIdx = this.statusFlow.indexOf(step);
    if (stepIdx < idx) return 'done';
    if (stepIdx === idx) return 'current';
    return 'pending';
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

  // ── Preview ────────────────────────────────────────────────────────

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
