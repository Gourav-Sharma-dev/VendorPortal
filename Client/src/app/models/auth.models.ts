export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  password: string;
  email: string;
  fullName: string;
}

export interface ResetPasswordRequest {
  email: string;
  newPassword: string;
  otp: string;
}

export interface AuthResponse {
  token: string;
  username: string;
  role: string;
  message?: string;
}

export interface DecodedToken {
  exp: number;
  // JWT payload can have any key — AuthService reads claims via its own constants
  [key: string]: unknown;
}
