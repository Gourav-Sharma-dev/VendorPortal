export interface VendorBankDetail {
  bankName: string;
  accountNumber: string;
  ifscCode: string;
  branchName: string;
}

export interface VendorRequest {
  vendorName: string;
  vendorType: string;
  contactPerson: string;
  mobileNumber: string;
  emailAddress: string;
  companyName: string;
  companyAddress: string;
  city: string;
  state: string;
  country: string;
  pinCode: string;
  gstNumber: string;
  panNumber: string;
  bankDetail?: VendorBankDetail;
}

export interface VendorDto {
  vendorId: string;
  vendorCode?: string;
  vendorName: string;
  vendorType: string;
  status: string;
  contactPerson: string;
  mobileNumber: string;
  emailAddress: string;
  companyName: string;
  companyAddress: string;
  city: string;
  state: string;
  country: string;
  pinCode: string;
  gstNumber: string;
  panNumber: string;
  remarks?: string;
  bankDetail?: VendorBankDetail;
  procurementApproved?: boolean;
  procurementRemarks?: string;
  financeApproved?: boolean;
  financeRemarks?: string;
  createdAt: string;
  updatedAt: string;
}

export interface VendorDocumentDto {
  documentId: string;
  documentType: string;
  fileName: string;
  fileFormat: string;
  fileSize: number;
  isMandatory: boolean;
  uploadedAt: string;
}

export interface VendorStatusHistoryDto {
  oldStatus: string;
  newStatus: string;
  actionDate: string;
  actionBy: string;
  remarks?: string;
  approvalLevel?: string;
}

export type VendorStatus =
  | 'Submitted'
  | 'UnderReview'
  | 'FinanceVerification'
  | 'Approved'
  | 'Rejected'
  | 'CorrectionRequested'
  | 'Active';

export type VendorType = 'Manufacturer' | 'Distributor' | 'ServiceProvider';
