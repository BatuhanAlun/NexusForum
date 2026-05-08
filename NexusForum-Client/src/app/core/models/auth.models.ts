export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  id: string;
  username: string;
  email: string;
  role: string;
  token: string;
}

export interface AuthUser {
  id: string;
  username: string;
  email: string;
  role: string;
  token: string;
}
