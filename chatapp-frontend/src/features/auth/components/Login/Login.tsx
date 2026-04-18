import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { LogIn } from "lucide-react";
import { AxiosError } from "axios";
import { useForm } from "react-hook-form";

import { authService } from "../../services/auth.service";
import { useAuthStore } from "../../store/useAuthStore";

import { PageWrapper, FormCard, SubmitButton } from "../Auth.styled";
import { IconWrapper, IconCircle, ErrorBox, RegisterLink } from "./styled";
import type { LoginCredentials } from "../../types";

export default function Login() {
  const navigate = useNavigate();
  const setAccessToken = useAuthStore((state) => state.setAccessToken);

  const [apiError, setApiError] = useState("");

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginCredentials>();

  const onSubmit = async (data: LoginCredentials) => {
    setApiError("");
    try {
      const response = await authService.login(data);
      const token = response.data.accessToken;

      setAccessToken(token);
      navigate("/");
    } catch (err) {
      const axiosError = err as AxiosError<{ message: string }>;
      setApiError(
        axiosError.response?.data?.message ||
          "Đăng nhập thất bại. Vui lòng thử lại!",
      );
    }
  };

  return (
    <PageWrapper>
      <FormCard>
        <IconWrapper>
          <IconCircle>
            <LogIn size={32} />
          </IconCircle>
        </IconWrapper>

        <h2 className="text-2xl font-bold text-center text-gray-800 mb-6">
          Đăng nhập ChatApp
        </h2>

        {apiError && <ErrorBox>{apiError}</ErrorBox>}

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div>
            <label className="block text-gray-700 text-sm font-bold mb-2">
              Email
            </label>
            <input
              type="email"
              className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 ${
                errors.email
                  ? "border-red-500 focus:ring-red-500"
                  : "focus:ring-blue-500"
              }`}
              {...register("email", {
                required: "Vui lòng nhập email",
                pattern: {
                  value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                  message: "Email không hợp lệ",
                },
              })}
            />
            {/* Hiển thị lỗi validation ngay dưới ô input */}
            {errors.email && (
              <p className="text-red-500 text-xs mt-1">
                {errors.email.message}
              </p>
            )}
          </div>

          <div>
            <label className="block text-gray-700 text-sm font-bold mb-2">
              Mật khẩu
            </label>
            <input
              type="password"
              className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 ${
                errors.password
                  ? "border-red-500 focus:ring-red-500"
                  : "focus:ring-blue-500"
              }`}
              {...register("password", {
                required: "Vui lòng nhập mật khẩu",
              })}
            />
            {errors.password && (
              <p className="text-red-500 text-xs mt-1">
                {errors.password.message}
              </p>
            )}
          </div>

          <SubmitButton
            type="submit"
            disabled={isSubmitting}
            $loading={isSubmitting}
          >
            {isSubmitting ? "Đang xử lý..." : "Đăng nhập"}
          </SubmitButton>
        </form>

        <p className="text-center text-gray-600 mt-6 text-sm">
          Chưa có tài khoản?{" "}
          <RegisterLink onClick={() => navigate("/register")}>
            Đăng ký ngay
          </RegisterLink>
        </p>
      </FormCard>
    </PageWrapper>
  );
}
