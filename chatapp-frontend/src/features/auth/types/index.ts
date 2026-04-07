export interface LoginCredentials {
  email: string;
  password: string;
}

export interface RegisterData {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface UserResponse {
  id: string;
  email: string;
  fullName?: string;
  avatarUrl?: string;
  role?: string;
}

export interface LoginResponse {
  accessToken: string;
  user: UserResponse;
}
