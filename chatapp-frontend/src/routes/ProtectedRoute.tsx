import { Navigate } from "react-router-dom";
import type { JSX } from "react";
import { useAuthStore } from "../features/auth/store/useAuthStore";

export default function ProtectedRoute({
  children,
}: {
  children: JSX.Element;
}) {
  const token = useAuthStore((state) => state.accessToken);
  return token ? children : <Navigate to="/login" replace />;
}
