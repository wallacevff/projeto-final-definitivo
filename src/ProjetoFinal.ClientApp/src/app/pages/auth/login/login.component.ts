import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

import { AuthService } from '../../../core/services/auth.service';
import { LoginCredentials } from '../../../core/api/auth.api';
import { UserRole } from '../../../core/api/users.api';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly toastr = inject(ToastrService);

  readonly isSubmitting = signal(false);
  readonly isRegistering = signal(false);

  readonly form = this.fb.group({
    username: this.fb.control('', { validators: [Validators.required] }),
    password: this.fb.control('', { validators: [Validators.required, Validators.minLength(6)] })
  });

  readonly registerForm = this.fb.group(
    {
      fullName: this.fb.control('', { validators: [Validators.required] }),
      email: this.fb.control('', { validators: [Validators.required, Validators.email] }),
      username: this.fb.control('', { validators: [Validators.required] }),
      password: this.fb.control('', { validators: [Validators.required, Validators.minLength(6)] }),
      confirmPassword: this.fb.control('', { validators: [Validators.required] }),
      role: this.fb.control(UserRole.Student, { validators: [Validators.required] })
    },
    { validators: [this.matchPasswords] }
  );

  readonly roleOptions = [
    { label: 'Aluno', value: UserRole.Student },
    { label: 'Professor', value: UserRole.Instructor }
  ];

  constructor() {
    if (this.authService.isAuthenticated()) {
      this.authService.navigateToHome();
    }
  }

  toggleMode(nextIsRegistering: boolean): void {
    this.isRegistering.set(nextIsRegistering);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const credentials = {
      username: this.form.controls.username.value?.trim() ?? '',
      password: this.form.controls.password.value ?? ''
    } satisfies LoginCredentials;

    this.isSubmitting.set(true);

    this.authService
      .login(credentials)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: () => this.toastr.success('Bem-vindo!'),
        error: () => this.toastr.error('Usuario ou senha invalidos.')
      });
  }

  submitRegistration(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    const payload = {
      FullName: this.registerForm.controls.fullName.value?.trim() ?? '',
      Email: this.registerForm.controls.email.value?.trim() ?? '',
      Username: this.registerForm.controls.username.value?.trim() ?? '',
      Password: this.registerForm.controls.password.value ?? '',
      Role: this.registerForm.controls.role.value ?? UserRole.Student
    };

    this.isSubmitting.set(true);

    this.authService
      .register(payload)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: () => {
          this.toastr.success('Cadastro realizado com sucesso!');
          this.form.controls.username.setValue(payload.Username);
          this.form.controls.password.setValue(payload.Password);
          this.isRegistering.set(false);
        },
        error: () => this.toastr.error('Nao foi possivel concluir o cadastro.')
      });
  }

  private matchPasswords(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value;
    const confirm = control.get('confirmPassword')?.value;
    return password && confirm && password !== confirm ? { passwordMismatch: true } : null;
  }
}
