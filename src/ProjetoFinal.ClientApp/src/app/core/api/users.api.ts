export const enum UserRole {
  Student = 1,
  Instructor = 2,
  Administrator = 3
}

export interface UserDto {
  Id: string;
  ExternalId: string;
  FullName: string;
  Email: string;
  Role: UserRole;
  Biography?: string;
  AvatarUrl?: string;
  IsActive: boolean;
}
