// File: src/features/auth/components/Register/Register.tsx
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { UserPlus } from "lucide-react";
import { AxiosError } from "axios";
import { useForm } from "react-hook-form";

import { authService } from "../../services/auth.service";
import { PageWrapper, FormCard, SubmitButton } from "../Auth.styled";
import type { RegisterData } from "../../types";
import {
  IconWrapper,
  IconCircle,
  ErrorBox,
  RegisterLink,
} from "../Login/styled";

export default function Register() {
  const navigate = useNavigate();
  const [apiError, setApiError] = useState("");

  const {
    register,
    handleSubmit,
    watch, // Hàm watch dùng để theo dõi giá trị đang nhập (Rất hữu ích để check Confirm Password)
    formState: { errors, isSubmitting },
  } = useForm<RegisterData>();

  const onSubmit = async (data: RegisterData) => {
    setApiError("");
    try {
      await authService.register(data);
      // Đăng ký thành công thì đá về trang Login kèm thông báo (Hoặc dùng toast/alert)
      alert("Đăng ký thành công! Vui lòng đăng nhập.");
      navigate("/login");
    } catch (err) {
      const axiosError = err as AxiosError<{
        message: string;
        errors?: Record<string, string[]>;
      }>;

      setApiError(
        axiosError.response?.data?.message ||
          "Đăng ký thất bại. Vui lòng thử lại!",
      );
    }
  };

  return (
    <PageWrapper>
      <FormCard>
        <IconWrapper>
          <IconCircle>
            <UserPlus size={32} />
          </IconCircle>
        </IconWrapper>

        <h2 className="text-2xl font-bold text-center text-gray-800 mb-6">
          Tạo tài khoản mới
        </h2>

        {apiError && <ErrorBox>{apiError}</ErrorBox>}

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div>
            <label className="block text-gray-700 text-sm font-bold mb-2">
              Họ và tên
            </label>
            <input
              type="text"
              className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Nguyễn Văn A"
              {...register("fullName")}
            />
          </div>

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
                minLength: { value: 6, message: "Mật khẩu phải từ 6 ký tự" },
              })}
            />
            {errors.password && (
              <p className="text-red-500 text-xs mt-1">
                {errors.password.message}
              </p>
            )}
          </div>

          <div>
            <label className="block text-gray-700 text-sm font-bold mb-2">
              Nhập lại Mật khẩu
            </label>
            <input
              type="password"
              className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 ${
                errors.confirmPassword
                  ? "border-red-500 focus:ring-red-500"
                  : "focus:ring-blue-500"
              }`}
              {...register("confirmPassword", {
                required: "Vui lòng xác nhận mật khẩu",
                validate: (val: string) => {
                  // Theo dõi giá trị của ô password để so sánh
                  if (watch("password") != val) {
                    return "Mật khẩu không khớp";
                  }
                },
              })}
            />
            {errors.confirmPassword && (
              <p className="text-red-500 text-xs mt-1">
                {errors.confirmPassword.message}
              </p>
            )}
          </div>

          <SubmitButton
            type="submit"
            disabled={isSubmitting}
            $loading={isSubmitting}
          >
            {isSubmitting ? "Đang xử lý..." : "Đăng ký"}
          </SubmitButton>
        </form>

        <p className="text-center text-gray-600 mt-6 text-sm">
          Đã có tài khoản?{" "}
          <RegisterLink onClick={() => navigate("/login")}>
            Đăng nhập
          </RegisterLink>
        </p>
      </FormCard>
    </PageWrapper>
  );
}
