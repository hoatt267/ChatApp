// File: src/routes/index.tsx
import { createBrowserRouter, Navigate } from "react-router-dom";
import Login from "../features/auth/components/Login/Login";
import ProtectedRoute from "./ProtectedRoute";
import PublicRoute from "./PublicRoute";
import Register from "../features/auth/components/Register/Register";

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
    path: "/",
    element: (
      <ProtectedRoute>
        <div className="min-h-screen flex items-center justify-center bg-green-50">
          <h1 className="text-3xl font-bold text-green-600">
            Đăng nhập thành công! Chào mừng đến ChatApp
          </h1>
        </div>
      </ProtectedRoute>
    ),
  },
  {
    path: "*",
    element: <Navigate to="/" replace />,
  },
]);
