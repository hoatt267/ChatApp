import axiosClient from "../../../lib/axiosClient";
import type { ApiResponse } from "../../../types";
import type { UserResponse } from "../types";

export const userService = {
  getMe: async () => {
    return await axiosClient.get<
      ApiResponse<UserResponse>,
      ApiResponse<UserResponse>
    >("/profiles/me");
  },

  logout: async () => {
    return await axiosClient.post<ApiResponse<null>, ApiResponse<null>>(
      "/users/logout",
    );
  },

  uploadAvatar: async (file: File) => {
    const formData = new FormData();
    formData.append("file", file);

    return await axiosClient.post<ApiResponse<string>, ApiResponse<string>>(
      "/profiles/avatar",
      formData,
      {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      },
    );
  },
};
