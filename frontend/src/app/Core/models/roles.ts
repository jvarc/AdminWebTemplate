export type RoleName = string;

// DTO tal como viene del backend
export interface RoleDto {
  id: string;
  roleName: string;
  usersCount: number;
}

export const toRoleNames = (roles: RoleDto[]): RoleName[] =>
  roles.map((r) => r.roleName);
