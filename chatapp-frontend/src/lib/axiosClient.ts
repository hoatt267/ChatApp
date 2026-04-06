import axios from "axios";
import { useAuthStore } from "../features/auth/store/useAuthStore";
import { APP_CONFIG } from "../config";

// 1. Khởi tạo instance cơ bản
const axiosClient = axios.create({
  baseURL: APP_CONFIG.API_URL,
  headers: {
    "Content-Type": "application/json",
  },
  withCredentials: true,
});

// 2. REQUEST INTERCEPTOR: Tự động đính kèm Access Token trước khi gửi
axiosClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// 3. RESPONSE INTERCEPTOR: Bắt lỗi 401 và tự động Refresh Token
axiosClient.interceptors.response.use(
  (response) => {
    return response.data;
  },
  async (error) => {
    const originalRequest = error.config;

    // Nếu lỗi 401 (Hết hạn token) và chưa từng thử retry
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const res = await axios.post(
          "http://localhost:5000/api/auth/refresh-token",
          {},
          { withCredentials: true },
        );

        // Lấy token mới và lưu vào store
        const newToken = res.data.data.accessToken;
        useAuthStore.getState().setAccessToken(newToken);

        // Cập nhật lại token mới vào request bị lỗi và GỌI LẠI request đó
        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        return axiosClient(originalRequest);
      } catch (refreshError) {
        useAuthStore.getState().logout();
        window.location.href = "/login";
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  },
);

export default axiosClient;
