import { RoleName } from './roles';

export interface UserDto {
  userId: string;
  email: string;
  userName: string;
  roles: RoleName[];
  isInactive?: boolean;
}

export interface UserCreateRequest {
  userName: string;
  email?: string;
  password: string;
  roles: RoleName[];
}

export interface UserUpdateRequest {
  userName: string;
  email?: string | null;
  roles: RoleName[];
  password?: string;
}
