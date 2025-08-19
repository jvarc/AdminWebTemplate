import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  inject,
} from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../Core/services/auth.service';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { CardModule } from 'primeng/card';
@Component({
  selector: 'login-component',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    InputTextModule,
    PasswordModule,
    ButtonModule,
    DividerModule,
    CardModule,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private cdr = inject(ChangeDetectorRef);
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.minLength(3)]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  isLoading = false;
  errorMsg = '';

  onSubmit(): void {
    if (this.form.invalid) return;
    this.isLoading = true;
    this.errorMsg = '';

    const { email, password } = this.form.value;

    this.authService.login(email!, password!).subscribe({
      next: () => {
        if (
          this.authService.hasPermission('admin:access') ||
          this.authService.hasRole('Admin')
        ) {
          this.router.navigate(['/admin']);
        } else {
          this.router.navigate(['/dashboard']);
        }
      },
      error: (err) => {
        this.isLoading = false;
        if (err?.status === 403) {
          this.errorMsg =
            'Tu cuenta está inactiva o bloqueada. Contacta al administrador.';
        } else if (err?.status === 401) {
          this.errorMsg = 'Credenciales inválidas.';
        } else {
          this.errorMsg = 'Ha ocurrido un error. Intenta de nuevo.';
        }
        this.isLoading = false;
        this.cdr.markForCheck();
        console.log('login error ->', err?.status, err?.error);
      },
      complete: () => {
        this.isLoading = false;
        this.cdr.markForCheck();
      },
    });
  }
}
