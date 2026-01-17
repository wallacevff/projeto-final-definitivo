import { HttpClient } from '@angular/common/http';
import { inject, Injectable, computed, signal } from '@angular/core';
import { Router } from '@angular/router';
import { map, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  AuthUser,
  LoginCredentials,
  LoginPayload,
  LoginResponse,
  RegisterPayload
} from '../api/auth.api';

interface AuthState {
  token: string;
  expiresAt: string;
  user: AuthUser;
}

const ROLE_LABEL: Record<number, string> = {
  1: 'Aluno',
  2: 'Professor',
  3: 'Administrador'
};

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly baseUrl = environment.baseUrl;
  private readonly storageKey = 'ses-ead-auth-state';
  private redirectUrl: string | null = null;

  private readonly authState = signal<AuthState | null>(this.restoreState());

  readonly currentUser = computed(() => this.authState()?.user ?? null);
  private readonly instructorRole = computed(() => this.currentUser()?.role === 2);
  private readonly studentRole = computed(() => this.currentUser()?.role === 1);
  private readonly authenticated = computed(() => this.isTokenValid(this.authState()));

  login(credentials: LoginCredentials) {
    const payload: LoginPayload = {
      Username: credentials.username.trim().toLowerCase(),
      Password: credentials.password
    };

    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/login`, payload).pipe(
      tap(response => this.persist(response)),
      tap(() => this.navigateAfterLogin()),
      map(() => void 0)
    );
  }

  register(payload: RegisterPayload) {
    return this.http.post(`${this.baseUrl}/auth/register`, payload).pipe(map(() => void 0));
  }

  logout(redirectToLogin = true): void {
    this.clearState(redirectToLogin);
  }

  forceLogout(): void {
    this.clearState(true);
  }

  isAuthenticated(): boolean {
    const authenticated = this.authenticated();
    if (!authenticated && this.authState() !== null) {
      this.clearState(false);
    }
    return authenticated;
  }

  getToken(): string | null {
    const state = this.authState();
    if (!this.isTokenValid(state)) {
      this.clearState(false);
      return null;
    }
    return state?.token ?? null;
  }

  setRedirectUrl(url: string): void {
    this.redirectUrl = url;
  }

  navigateToHome(): void {
    this.redirectUrl = null;
    this.router.navigateByUrl('/dashboard');
  }

  isInstructorRole(): boolean {
    return this.instructorRole();
  }

  isStudentRole(): boolean {
    return this.studentRole();
  }

  getRoleLabel(role: number): string {
    return ROLE_LABEL[role] ?? 'Usuario';
  }

  private navigateAfterLogin(): void {
    const destination = this.redirectUrl && this.redirectUrl !== '/auth/login' ? this.redirectUrl : '/dashboard';
    this.redirectUrl = null;
    this.router.navigateByUrl(destination);
  }

  private persist(response: LoginResponse): void {
    const state: AuthState = {
      token: response.Token,
      expiresAt: response.ExpiresAt,
      user: {
        id: response.User.Id,
        externalId: response.User.ExternalId,
        username: response.User.Username,
        fullName: response.User.FullName,
        email: response.User.Email,
        role: response.User.Role
      }
    };

    this.authState.set(state);
    localStorage.setItem(this.storageKey, JSON.stringify(state));
  }

  private restoreState(): AuthState | null {
    const stored = localStorage.getItem(this.storageKey);
    if (!stored) {
      return null;
    }

    try {
      const parsed = JSON.parse(stored) as AuthState;
      if (!this.isTokenValid(parsed)) {
        localStorage.removeItem(this.storageKey);
        return null;
      }
      return parsed;
    } catch {
      localStorage.removeItem(this.storageKey);
      return null;
    }
  }

  private clearState(redirect: boolean): void {
    this.authState.set(null);
    localStorage.removeItem(this.storageKey);
    this.redirectUrl = null;
    if (redirect) {
      this.router.navigate(['/auth/login']);
    }
  }

  private isTokenValid(state: AuthState | null): boolean {
    if (!state?.token || !state.expiresAt) {
      return false;
    }

    const expiresAt = Date.parse(state.expiresAt);
    if (Number.isNaN(expiresAt)) {
      return false;
    }

    return expiresAt > Date.now();
  }
}
