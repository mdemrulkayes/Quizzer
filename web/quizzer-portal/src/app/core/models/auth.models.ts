export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  password: string;
  confirmPassword: string;
  userType: UserType;
}

export interface AccessTokenResponse {
  token: string;
  tokenType: string;
  refreshToken: string;
  refreshTokenExpiryDate: string;
  expiresIn: number;
}

export interface RefreshTokenRequest {
  accessToken: string;
  refreshToken: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface UserProfile {
  userId: string;
  firstName: string;
  lastName: string;
  email: string | null;
  roles: string[];
}

export enum UserType {
  SuperAdmin = 'SuperAdmin',
  SupportAdmin = 'SupportAdmin',
  QuizAuthor = 'QuizAuthor',
  Examine = 'Examine',
}

export enum UserRole {
  SuperAdmin = 'SuperAdmin',
  SupportAdmin = 'SupportAdmin',
  QuizAuthor = 'QuizAuthor',
  Examine = 'Examine',
}
