// File: src/layouts/MainLayout.tsx
import { useEffect, useState } from "react";
import { Outlet, useNavigate } from "react-router-dom";

import { useAuthStore } from "../features/auth/store/useAuthStore";
import { userService } from "../features/auth/services/user.service";
import { useChatStore } from "../features/chat/store/useChatStore";

import SidebarHeader from "./components/Sidebar/SidebarHeader";
import OnlineUsers from "./components/Sidebar/OnlineUsers";
import RecentChats from "./components/Sidebar/RecentChats";

export default function MainLayout() {
  const navigate = useNavigate();
  const { user, setUser, logout: clearStore } = useAuthStore();
  const { connect, disconnect } = useChatStore();

  const [loading, setLoading] = useState(!user);

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const response = await userService.getMe();
        setUser(response.data);
        connect();
      } catch (error) {
        console.error("Lỗi khi lấy thông tin người dùng:", error);
        clearStore();
        navigate("/login");
      } finally {
        setLoading(false);
      }
    };
    fetchProfile();

    return () => {
      disconnect();
    };
  }, [setUser, clearStore, navigate, connect, disconnect]);

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
        <SidebarHeader />
        <OnlineUsers />
        <RecentChats />
      </aside>

      <main className="flex-1 flex flex-col bg-[#e5ddd5]">
        <Outlet />
      </main>
    </div>
  );
}
