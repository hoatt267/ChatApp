import { useEffect } from "react";
import type { HubConnection } from "@microsoft/signalr";
import { toast } from "react-toastify";
import { useFriendStore } from "../store/useFriendStore";

interface FriendRequestData {
  userId: string;
  fullName: string;
  avatarUrl?: string;
  message: string;
}

interface FriendAcceptedData {
  userId: string;
  fullName: string;
  message: string;
}

export const useFriendEvents = (connection: HubConnection | null) => {
  const triggerRefresh = useFriendStore((state) => state.triggerRefresh);
  useEffect(() => {
    if (!connection) return;

    const handleReceiveFriendRequest = (data: FriendRequestData) => {
      console.log("🔔 CÓ LỜI MỜI KẾT BẠN MỚI:", data);
      toast.info(data.message);
      triggerRefresh();
    };

    const handleFriendRequestAccepted = (data: FriendAcceptedData) => {
      console.log("✅ ĐÃ TRỞ THÀNH BẠN BÈ:", data);
      toast.success(data.message);
      triggerRefresh();
    };

    const handleFriendshipRemoved = () => {
      triggerRefresh();
    };

    connection.on("ReceiveFriendRequest", handleReceiveFriendRequest);
    connection.on("FriendRequestAccepted", handleFriendRequestAccepted);
    connection.on("FriendshipRemoved", handleFriendshipRemoved);

    return () => {
      connection.off("ReceiveFriendRequest", handleReceiveFriendRequest);
      connection.off("FriendRequestAccepted", handleFriendRequestAccepted);
      connection.off("FriendshipRemoved", handleFriendshipRemoved);
    };
  }, [connection, triggerRefresh]);
};
