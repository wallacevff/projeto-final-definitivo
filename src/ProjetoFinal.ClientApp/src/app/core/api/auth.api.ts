export interface LoginCredentials {
  username: string;
  password: string;
}

export interface LoginPayload {
  Username: string;
  Password: string;
}

export interface LoginResponse {
  Token: string;
  ExpiresAt: string;
  User: AuthenticatedUserDto;
}

export interface AuthenticatedUserDto {
  Id: string;
  ExternalId: string;
  Username: string;
  FullName: string;
  Email: string;
  Role: number;
}

export interface AuthUser {
  id: string;
  externalId: string;
  username: string;
  fullName: string;
  email: string;
  role: number;
}

export interface RegisterPayload {
  FullName: string;
  Email: string;
  Username: string;
  Password: string;
  Role: number;
  Biography?: string;
  AvatarUrl?: string;
}
