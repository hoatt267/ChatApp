import React, { useEffect, useState, useRef } from "react"; // 🌟 THÊM useRef
import { Outlet, useNavigate } from "react-router-dom";
import { LogOut, User as UserIcon, Camera, Loader2 } from "lucide-react"; // 🌟 THÊM Camera, Loader2

import { useAuthStore } from "../features/auth/store/useAuthStore";
import { userService } from "../features/auth/services/user.service";

export default function MainLayout() {
  const navigate = useNavigate();
  const { user, setUser, logout: clearStore } = useAuthStore();

  // 🌟 STATE VÀ REF CHO TÍNH NĂNG UPLOAD AVATAR
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isUploading, setIsUploading] = useState(false);

  // CHỈ BẬT LOADING NẾU TRONG LOCAL STORAGE CHƯA CÓ DỮ LIỆU
  const [loading, setLoading] = useState(!user);

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const response = await userService.getMe();
        setUser(response.data);
      } catch (error) {
        console.error("Lỗi lấy thông tin user:", error);
        clearStore();
        navigate("/login");
      } finally {
        setLoading(false);
      }
    };
    fetchProfile();
  }, [setUser, clearStore, navigate]);

  const handleLogout = async () => {
    try {
      await userService.logout();
    } catch (error) {
      console.error("Lỗi khi logout backend:", error);
    } finally {
      clearStore();
      navigate("/login");
    }
  };

  const handleAvatarClick = () => {
    if (isUploading) return;
    fileInputRef.current?.click();
  };

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    if (file.size > 5 * 1024 * 1024) {
      alert("File quá lớn. Vui lòng chọn ảnh dưới 5MB.");
      return;
    }

    setIsUploading(true);
    try {
      const response = await userService.uploadAvatar(file);
      const newAvatarUrl = response.data; // API Backend trả về chuỗi URL ảnh

      if (user) {
        setUser({ ...user, avatarUrl: newAvatarUrl });
      }
    } catch (error) {
      console.error("Lỗi upload avatar:", error);
      alert("Upload ảnh thất bại! Vui lòng thử lại.");
    } finally {
      setIsUploading(false);
      // Reset lại input để người dùng có thể chọn lại chính bức ảnh đó nếu muốn
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
    }
  };

  if (loading) {
    return (
      <div className="flex h-screen items-center justify-center bg-gray-100">
        <div className="animate-pulse flex flex-col items-center">
          <div className="w-12 h-12 bg-blue-400 rounded-full mb-4"></div>
          <div className="text-gray-500 font-medium">
            Đang tải dữ liệu ChatApp...
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="flex h-screen bg-gray-100 overflow-hidden">
      <aside className="w-1/4 min-w-[300px] max-w-[400px] bg-white border-r border-gray-200 flex flex-col">
        <div className="h-16 flex items-center justify-between px-4 border-b border-gray-200 bg-gray-50">
          <div className="flex items-center gap-3">
            {/* 🌟 GIAO DIỆN AVATAR MỚI CÓ HIỆU ỨNG HOVER VÀ LOADING */}
            <div
              className="relative cursor-pointer group w-10 h-10 rounded-full"
              onClick={handleAvatarClick}
              title="Nhấn để đổi ảnh đại diện"
            >
              {user?.avatarUrl ? (
                <img
                  src={user.avatarUrl}
                  alt="Avatar"
                  className="w-full h-full rounded-full object-cover border border-gray-300 shadow-sm"
                />
              ) : (
                <div className="w-full h-full rounded-full bg-gradient-to-tr from-blue-500 to-blue-400 text-white flex items-center justify-center shadow-sm">
                  <UserIcon size={20} />
                </div>
              )}

              <div className="absolute inset-0 bg-black bg-opacity-40 rounded-full flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
                {isUploading ? (
                  <Loader2 className="animate-spin text-white w-5 h-5" />
                ) : (
                  <Camera className="text-white w-5 h-5" />
                )}
              </div>

              <input
                type="file"
                ref={fileInputRef}
                onChange={handleFileChange}
                accept="image/*"
                className="hidden"
              />
            </div>

            <div className="flex flex-col">
              <span className="font-bold text-gray-800 text-sm">
                {user?.fullName || "Người dùng"}
              </span>
              <span className="text-xs text-gray-500 truncate max-w-[150px]">
                {user?.email}
              </span>
            </div>
          </div>

          <button
            onClick={handleLogout}
            className="p-2 text-gray-500 hover:text-red-600 hover:bg-red-50 rounded-full transition-colors"
            title="Đăng xuất"
          >
            <LogOut size={20} />
          </button>
        </div>

        <div className="flex-1 overflow-y-auto p-4 flex items-center justify-center text-gray-400">
          <p>Danh sách đoạn chat sẽ nằm ở đây</p>
        </div>
      </aside>

      <main className="flex-1 flex flex-col bg-[#e5ddd5]">
        <Outlet />
      </main>
    </div>
  );
}
