import { Component, inject, OnInit, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../services/admin.service';
import { UserDto, RoleDto } from '../../../models/admin.models';

@Component({
  selector: 'app-user-management',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.css'
})
export class UserManagementComponent implements OnInit {
  private adminService = inject(AdminService);
  private cdr = inject(ChangeDetectorRef);

  users: UserDto[] = [];
  roles: RoleDto[] = [];
  loading = false;
  errorMsg = '';
  successMsg = '';
  search = '';

  showModal = false;
  selectedUser: UserDto | null = null;
  selectedRoleName = '';
  showDeleteModal = false;
  userToDelete: UserDto | null = null;

  get filtered(): UserDto[] {
    const s = this.search.toLowerCase();
    return this.users.filter(u => !s || u.username.toLowerCase().includes(s) || u.email.toLowerCase().includes(s));
  }

  ngOnInit(): void {
    this.loading = true;
    this.adminService.getAllUsers().subscribe({
      next: (u) => { this.users = u; this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.loading = false; this.cdr.markForCheck(); }
    });
    this.adminService.getAllRoles().subscribe({ next: (r) => { this.roles = r; this.cdr.markForCheck(); }, error: () => { } });
  }

  openGrantModal(user: UserDto): void {
    this.selectedUser = user;
    this.selectedRoleName = '';
    this.successMsg = '';
    this.errorMsg = '';
    this.showModal = true;
  }

  grantRole(): void {
    if (!this.selectedUser || !this.selectedRoleName) return;
    const role = this.roles.find(r => r.roleName === this.selectedRoleName);
    if (!role) return;
    this.adminService.grantAccess(this.selectedUser.userId, role.roleId).subscribe({
      next: (res) => {
        this.successMsg = res.message;
        this.showModal = false;
        this.reload();
      },
      error: (err: { error?: { message?: string } }) => {
        this.errorMsg = err.error?.message || 'Failed to grant role.';
      }
    });
  }

  revokeRole(userId: string, roleName: string): void {
    const role = this.roles.find(r => r.roleName === roleName);
    if (!role) return;
    this.adminService.revokeAccess(userId, role.roleId).subscribe({
      next: (res) => { this.successMsg = res.message; this.reload(); },
      error: (err: { error?: { message?: string } }) => { this.errorMsg = err.error?.message || 'Failed.'; }
    });
  }

  openDeleteModal(user: UserDto): void {
    this.userToDelete = user;
    this.showDeleteModal = true;
  }

  confirmDeleteUser() {
    if (this.userToDelete) {
      this.deleteUser(this.userToDelete.userId);
      this.showDeleteModal = false;
      this.userToDelete = null;
    }
  }

  deleteUser(id: string): void {
    if (!confirm('Delete this user?')) return;
    this.adminService.deleteUser(id).subscribe({
      next: () => { this.successMsg = 'User deleted.'; this.reload(); },
      error: (err: { error?: { message?: string } }) => { this.errorMsg = err.error?.message || 'Failed.'; }
    });
  }

  private reload(): void {
    this.adminService.getAllUsers().subscribe({ next: (u) => { this.users = u; this.cdr.markForCheck(); }, error: () => { } });
  }
}
