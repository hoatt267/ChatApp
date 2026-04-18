import { createBrowserRouter, Navigate } from "react-router-dom";
import Login from "../features/auth/components/Login/Login";
import ProtectedRoute from "./ProtectedRoute";
import PublicRoute from "./PublicRoute";
import Register from "../features/auth/components/Register/Register";
import MainLayout from "../layouts/MainLayout";
import ChatRoom from "../features/chat/components/ChatRoom/ChatRoom";
import FriendsPage from "../features/friends/components/FriendsPage";

export const router = createBrowserRouter([
  {
    path: "/login",
    element: (
      <PublicRoute>
        <Login />
      </PublicRoute>
    ),
  },
  {
    path: "/register",
    element: (
      <PublicRoute>
        <Register />
      </PublicRoute>
    ),
  },
  {
    // Route cha (Bọc bằng ProtectedRoute để bảo vệ)
    path: "/",
    element: (
      <ProtectedRoute>
        <MainLayout />
      </ProtectedRoute>
    ),
    // Route con: Nội dung sẽ được hiển thị vào chỗ <Outlet /> của MainLayout
    children: [
      {
        index: true, // Khi mới vào trang chủ, chưa chọn ai để chat
        element: (
          <div className="flex-1 flex items-center justify-center text-gray-500 bg-gray-50">
            <div className="text-center">
              <h2 className="text-2xl font-semibold mb-2">
                Chào mừng đến với ChatApp
              </h2>
              <p>Hãy chọn một đoạn chat hoặc bắt đầu cuộc trò chuyện mới</p>
            </div>
          </div>
        ),
      },
      {
        path: "chat/:conversationId",
        element: <ChatRoom />,
      },
      {
        path: "friends",
        element: <FriendsPage />,
      },
    ],
  },
  {
    path: "*",
    element: <Navigate to="/" replace />,
  },
]);
