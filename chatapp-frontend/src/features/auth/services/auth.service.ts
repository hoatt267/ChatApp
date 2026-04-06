import axiosClient from "../../../lib/axiosClient";
import type { ApiResponse } from "../../../types";
import type {
  LoginCredentials,
  LoginResponse,
  RegisterData,
  UserResponse,
} from "../types";

export const authService = {
  login: async (credentials: LoginCredentials) => {
    // Đã sửa lại đường dẫn chuẩn khớp với UsersController bên C#
    return await axiosClient.post<
      ApiResponse<LoginResponse>,
      ApiResponse<LoginResponse>
    >("/users/login", credentials);
  },

  register: async (data: RegisterData) => {
    return await axiosClient.post<
      ApiResponse<UserResponse>,
      ApiResponse<UserResponse>
    >("/users/register", data);
  },
};
