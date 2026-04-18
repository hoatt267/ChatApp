import { useEffect } from "react";
import type { HubConnection } from "@microsoft/signalr";
import { useChatStore } from "../store/useChatStore";

export const useChatEvents = (connection: HubConnection | null) => {
  const { setOnlineUsers, addOnlineUser, removeOnlineUser, setTyping } =
    useChatStore();

  useEffect(() => {
    if (!connection) return;

    // Đăng ký sự kiện
    connection.on("GetOnlineUsers", setOnlineUsers);
    connection.on("UserIsOnline", addOnlineUser);
    connection.on("UserIsOffline", removeOnlineUser);
    connection.on("UserTyping", setTyping);

    // Hủy đăng ký khi component unmount hoặc connection thay đổi
    return () => {
      connection.off("GetOnlineUsers", setOnlineUsers);
      connection.off("UserIsOnline", addOnlineUser);
      connection.off("UserIsOffline", removeOnlineUser);
      connection.off("UserTyping", setTyping);
    };
  }, [connection, setOnlineUsers, addOnlineUser, removeOnlineUser, setTyping]);
};
