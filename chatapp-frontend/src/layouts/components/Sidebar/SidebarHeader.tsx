import React, { useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { LogOut, User as UserIcon, Camera, Loader2 } from "lucide-react";
import { useAuthStore } from "../../../features/auth/store/useAuthStore";
import { useChatStore } from "../../../features/chat/store/useChatStore";
import { userService } from "../../../features/auth/services/user.service";

export default function SidebarHeader() {
  const navigate = useNavigate();
  const { user, setUser, logout: clearStore } = useAuthStore();
  const { disconnect } = useChatStore();

  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isUploading, setIsUploading] = useState(false);

  const handleLogout = async () => {
    try {
      await userService.logout();
    } catch (error) {
      console.error("Lỗi khi logout backend:", error);
    } finally {
      clearStore();
      disconnect();
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
      if (user) setUser({ ...user, avatarUrl: response.data });
    } catch (error) {
      console.error("Lỗi upload ảnh:", error);
      alert("Upload ảnh thất bại! Vui lòng thử lại.");
    } finally {
      setIsUploading(false);
      if (fileInputRef.current) fileInputRef.current.value = "";
    }
  };

  return (
    <div className="h-16 flex items-center justify-between px-4 border-b border-gray-200 bg-gray-50 flex-shrink-0">
      <div className="flex items-center gap-3">
        <div
          className="relative cursor-pointer group w-10 h-10 rounded-full"
          onClick={handleAvatarClick}
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
      >
        <LogOut size={20} />
      </button>
    </div>
  );
}
