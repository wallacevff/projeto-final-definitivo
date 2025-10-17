import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

import { AuthService } from '../../../core/services/auth.service';
import { LoginCredentials } from '../../../core/api/auth.api';

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

  readonly form = this.fb.group({
    username: this.fb.control('', { validators: [Validators.required] }),
    password: this.fb.control('', { validators: [Validators.required, Validators.minLength(6)] })
  });

  constructor() {
    if (this.authService.isAuthenticated()) {
      this.authService.navigateToHome();
    }
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
}
