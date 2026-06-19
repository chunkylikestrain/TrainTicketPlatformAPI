export type LoginRequest = {
  email: string;
  password: string;
};

export type LoginResponse = {
  token: string;
  userId: number;
  role: string;
};

export type RegisterRequest = {
  email: string;
  password: string;
  phone: string;
};

export type CurrentUser = {
  id: number;
  email: string;
  phone: string;
  role: string;
};
