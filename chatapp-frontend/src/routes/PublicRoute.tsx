import { Navigate } from "react-router-dom";
import type { JSX } from "react";
import { useAuthStore } from "../features/auth/store/useAuthStore";

export default function PublicRoute({ children }: { children: JSX.Element }) {
  const token = useAuthStore((state) => state.accessToken);
  return !token ? children : <Navigate to="/" replace />;
}
