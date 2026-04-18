export interface UserListResponse {
  totalCount: number;
  items: UserListItem[];
}

export interface UserListItem {
  userId: string;
  firstName: string;
  lastName: string;
  email: string | null;
  phoneNumber: string | null;
  roles: string[];
  isDeleted: boolean;
  createdDate: string;
  lastLoginTime: string | null;
}

export interface UserDetailResponse {
  userId: string;
  firstName: string;
  lastName: string;
  email: string | null;
  phoneNumber: string | null;
  roles: string[];
  isDeleted: boolean;
  createdDate: string;
  updatedDate: string | null;
  lastLoginTime: string | null;
}

export interface UpdateUserRoleRequest {
  roleNames: string[];
}
