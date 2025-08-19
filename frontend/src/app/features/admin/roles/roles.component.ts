import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnInit,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { RolesService } from '../../../Core/services/roles.service';

// PrimeNG
import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { MultiSelectModule } from 'primeng/multiselect';
import { ConfirmationService, MessageService } from 'primeng/api';

import { finalize } from 'rxjs';
import { RoleDto } from '../../../Core/models/roles';

@Component({
  selector: 'admin-roles',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ToastModule,
    ConfirmDialogModule,
    ButtonModule,
    DialogModule,
    MultiSelectModule,
  ],
  templateUrl: './roles.component.html',
  styleUrls: ['./roles.component.css'],
  providers: [ConfirmationService, MessageService],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RolesComponent implements OnInit {
  private rolesSvc = inject(RolesService);
  private toast = inject(MessageService);
  private confirm = inject(ConfirmationService);
  private cdr = inject(ChangeDetectorRef);

  loading = false;

  roles: RoleDto[] = [];

  showCreate = false;
  newRoleName = '';

  showPerms = false;
  selectedRole: RoleDto | null = null;
  allPermissions: string[] = [];
  selectedPermissions: string[] = [];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;

    this.rolesSvc
      .getAll()
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: (data) => (this.roles = data),
        error: () =>
          this.toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'No se pudieron cargar los roles',
          }),
      });

    this.rolesSvc.getAllPermissions().subscribe({
      next: (p) => (this.allPermissions = p ?? []),
      error: () =>
        this.toast.add({
          severity: 'warn',
          summary: 'Permisos',
          detail: 'No se pudieron cargar los permisos',
        }),
      complete: () => this.cdr.markForCheck(),
    });
  }

  openCreate() {
    this.newRoleName = '';
    this.showCreate = true;
  }

  createRole() {
    const name = this.newRoleName?.trim();
    if (!name) return;

    this.loading = true;
    this.rolesSvc
      .create(name)
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: () => {
          this.toast.add({
            severity: 'success',
            summary: 'Rol',
            detail: 'Creado correctamente',
          });
          this.showCreate = false;
          this.load();
        },
        error: (err) =>
          this.toast.add({
            severity: 'error',
            summary: 'Error',
            detail: this.extractError(err) ?? 'No se pudo crear el rol',
          }),
      });
  }

  confirmDelete(role: RoleDto) {
    this.confirm.confirm({
      message: `¿Eliminar el rol "${role.roleName}"?`,
      header: 'Eliminar rol',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Eliminar',
      rejectLabel: 'Cancelar',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-secondary',
      accept: () => {
        this.loading = true;
        this.rolesSvc
          .delete(role.id)
          .pipe(
            finalize(() => {
              this.loading = false;
              this.cdr.markForCheck();
            })
          )
          .subscribe({
            next: () => {
              this.toast.add({
                severity: 'success',
                summary: 'Roles',
                detail: `Rol "${role.roleName}" eliminado`,
                life: 3500,
              });
              this.load();
            },
            error: (err) => {
              console.log('err', err);
              const detail = this.extractError(err.status);
              this.toast.add({
                severity: 'warn',
                summary: `No se pudo eliminar el rol "${role.roleName}"`,
                detail,
                life: 5000,
              });
            },
          });
      },
    });
  }

  openPerms(r: RoleDto) {
    this.selectedRole = r;
    this.selectedPermissions = [];
    this.showPerms = true;

    this.rolesSvc.getRolePermissions(r.roleName).subscribe({
      next: (perms) => {
        this.selectedPermissions = perms ?? [];
        const set = new Set([
          ...this.allPermissions,
          ...this.selectedPermissions,
        ]);
        this.allPermissions = Array.from(set);
      },
      error: () =>
        this.toast.add({
          severity: 'error',
          summary: 'Permisos',
          detail: 'No se pudieron cargar los permisos del rol',
        }),
      complete: () => this.cdr.markForCheck(),
    });
  }

  savePerms() {
    if (!this.selectedRole) return;
    this.loading = true;
    this.rolesSvc
      .setRolePermissions(this.selectedRole.roleName, this.selectedPermissions)
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: () => {
          this.toast.add({
            severity: 'success',
            summary: 'Permisos',
            detail: 'Actualizados correctamente',
          });
          this.showPerms = false;
        },
        error: (err) =>
          this.toast.add({
            severity: 'error',
            summary: 'Error',
            detail:
              this.extractError(err) ??
              'No se pudieron actualizar los permisos',
          }),
      });
  }

  private extractError(err: any): string {
    if (err?.error?.title) return err.error.title;
    if (typeof err?.error === 'string') return err.error;

    if (err?.error?.errors) {
      try {
        const flat = Object.values(err.error.errors).flat() as string[];
        if (flat.length) return flat.join(' ');
      } catch {}
    }

    if (Array.isArray(err?.error)) {
      const msgs = err.error
        .map((e: any) => e?.description || e?.code)
        .filter(Boolean);
      if (msgs.length) return msgs.join(' ');
    }

    if (err?.message) return err.message;
    return 'Ocurrió un error';
  }
}
