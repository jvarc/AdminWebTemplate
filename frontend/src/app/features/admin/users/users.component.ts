import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
  ChangeDetectorRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { UsersService } from '../../../Core/services/users.service';
import { RolesService } from '../../../Core/services/roles.service';
import {
  UserCreateRequest,
  UserDto,
  UserUpdateRequest,
} from '../../../Core/models/users';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';

// PrimeNG
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { MultiSelectModule } from 'primeng/multiselect';
import { TagModule } from 'primeng/tag';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService, MessageService } from 'primeng/api';

// RxJS
import { forkJoin, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { RoleDto, RoleName } from '../../../Core/models/roles';

@Component({
  selector: 'admin-users',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    PasswordModule,
    MultiSelectModule,
    TagModule,
    ConfirmDialogModule,
    ToastModule,
  ],
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css'],
  providers: [ConfirmationService, MessageService],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsersComponent implements OnInit {
  private usersSvc = inject(UsersService);
  private rolesSvc = inject(RolesService);
  private fb = inject(FormBuilder);
  private toast = inject(MessageService);
  private cdr = inject(ChangeDetectorRef);

  users: UserDto[] = [];
  roles: RoleDto[] = [];
  loading = false;
  busy = new Set<string>();

  currentUserId: string | null = null;
  showForm = false;
  mode: 'create' | 'edit' = 'create';

  form = this.fb.group({
    userName: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.email]],
    password: [''],
    roles: [[] as RoleName[]],
  });

  ngOnInit() {
    this.loadData();
  }

  private applyCreatePasswordValidators() {
    const ctrl = this.form.get('password');
    ctrl?.setValidators([Validators.required, Validators.minLength(8)]);
    ctrl?.updateValueAndValidity({ emitEvent: false });
  }

  private clearPasswordValidators() {
    const ctrl = this.form.get('password');
    ctrl?.clearValidators();
    ctrl?.updateValueAndValidity({ emitEvent: false });
  }

  loadData() {
    this.loading = true;

    forkJoin({
      users: this.usersSvc.getAll().pipe(
        catchError(() => {
          this.toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'No se pudieron cargar usuarios',
          });
          return of([] as UserDto[]);
        })
      ),
      roles: this.rolesSvc.getAll().pipe(
        catchError(() => {
          this.toast.add({
            severity: 'warn',
            summary: 'Roles',
            detail: 'No se pudieron cargar roles',
          });
          return of([] as RoleDto[]);
        })
      ),
    })
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe(({ users, roles }) => {
        this.users = users;
        this.roles = roles;
      });
  }

  openCreate() {
    this.mode = 'create';
    this.currentUserId = null;
    this.form.reset({ userName: '', email: '', password: '', roles: [] });
    this.applyCreatePasswordValidators();
    this.showForm = true;
  }

  onSubmit() {
    if (this.form.invalid) return;

    if (this.mode === 'create') {
      const payload = this.form.value as UserCreateRequest;
      this.loading = true;
      this.usersSvc
        .create(payload)
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
              summary: 'Usuario',
              detail: 'Creado correctamente',
            });
            this.showForm = false;
            this.loadData();
          },
          error: (err) =>
            this.toast.add({
              severity: 'error',
              summary: 'Error',
              detail: err?.error || 'No se pudo crear',
            }),
        });
    } else {
      const id = this.currentUserId!;
      const { userName, email, roles, password } = this.form.value;
      const payload: UserUpdateRequest = {
        userName: userName!,
        email: email || '',
        roles: (roles as RoleName[]) ?? [],
        password: password || undefined,
      };

      this.loading = true;
      this.usersSvc
        .update(id, payload)
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
              summary: 'Usuario',
              detail: 'Actualizado correctamente',
            });
            this.showForm = false;
            this.loadData();
          },
          error: (err) =>
            this.toast.add({
              severity: 'error',
              summary: 'Error',
              detail: err?.error || 'No se pudo actualizar',
            }),
        });
    }
  }

  openEdit(u: UserDto) {
    this.mode = 'edit';
    this.currentUserId = u.userId;
    this.form.reset({
      userName: u.userName,
      email: u.email ?? '',
      password: '',
      roles: [...(u.roles as RoleName[])],
    });
    this.clearPasswordValidators();
    this.showForm = true;
  }

  toggleActive(u: UserDto) {
    const willBeActive = !!u.isInactive;
    this.busy.add(u.userId);

    this.usersSvc.setStatus(u.userId, willBeActive).subscribe({
      next: (res) => {
        u.isInactive = !res.active;
        this.toast.add({
          severity: 'success',
          summary: 'Estado',
          detail: res.active ? 'Activado' : 'Desactivado',
        });
      },
      error: () => {
        this.toast.add({
          severity: 'error',
          summary: 'Error',
          detail: 'No se pudo cambiar el estado',
        });
      },
      complete: () => this.busy.delete(u.userId),
    });
  }
}
