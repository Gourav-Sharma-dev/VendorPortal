export interface UserDto {
  userId: string;
  username: string;
  email: string;
  fullName: string;
  isActive: boolean;
  role: string;
  roles: string[];
  createdAt: string;
}

export interface RoleDto {
  roleId: string;
  roleName: string;
  roleDescription: string;
}

export interface AuditLogDto {
  action: string;
  entity: string;
  entityId: string;
  entityName: string;
  timestamp: string;
  details?: string;
  username?: string;
  userId?: string;
}

export interface AuditLogFilter {
  entity?: string;
  entityId?: string;
  userId?: string;
  action?: string;
  username?: string;
  fromDate?: string;
  toDate?: string;
}

export interface UpdateUserRequest {
  email: string;
  fullName: string;
  isActive: boolean;
}

export interface ReportFilter {
  from?: string;
  to?: string;
  status?: string;
  vendorType?: string;
  fromDate?: string;
  toDate?: string;
}

export interface VendorRegistrationReportDto {
  //vendorId: string;
  vendorCode?: string;
  vendorName: string;
  vendorType: string;
  registeredDate: string;
  status: string;
}

export interface VendorVerificationReportDto {
  //vendorId: string;
  vendorCode?: string;
  vendorName: string;
  procurementApproved?: boolean;
  financeApproved?: boolean;
  //approvedByUsername?: string;
  //approvedDate?: string;
  status: string;
  verifiedDate: string;
}
