import { useEffect, useRef } from "react";
import * as signalR from "@microsoft/signalr";
import type { Message } from "../types";
import type { UserResponse } from "../../auth/types";
import { toast } from "react-toastify";

export const useChatSignalR = (
  connection: signalR.HubConnection | null,
  conversationId: string | undefined,
  currentUser: UserResponse | null,
  messages: Message[],
  setMessages: React.Dispatch<React.SetStateAction<Message[]>>,
  scrollToBottom: () => void,
  isBlocked: boolean,
  markAsBlocked: () => void,
) => {
  const typingTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  // 1. Join phòng và Nhận tin nhắn
  useEffect(() => {
    if (!connection || !conversationId) return;
    let isMounted = true;

    const joinRoom = async () => {
      try {
        while (connection.state === signalR.HubConnectionState.Connecting) {
          await new Promise((resolve) => setTimeout(resolve, 100));
        }
        if (
          connection.state === signalR.HubConnectionState.Connected &&
          isMounted
        ) {
          await connection.invoke("JoinConversation", conversationId);
        }
      } catch (err) {
        console.error("Lỗi Join phòng:", err);
      }
    };
    joinRoom();

    const handleReceiveMessage = (res: { data: Message }) => {
      if (
        res.data.conversationId.toLowerCase() === conversationId.toLowerCase()
      ) {
        setMessages((prev) => {
          if (prev.some((m) => m.id === res.data.id)) return prev;

          const optimisticIndex = prev.findIndex(
            (m) =>
              m.isOptimistic &&
              m.senderId === res.data.senderId &&
              m.fileName === res.data.fileName,
          );

          if (optimisticIndex !== -1) {
            const newMsgs = [...prev];
            newMsgs[optimisticIndex] = res.data;
            return newMsgs;
          }
          return [...prev, res.data];
        });
        setTimeout(scrollToBottom, 100);
      }
    };

    connection.on("ReceiveMessage", handleReceiveMessage);
    return () => {
      isMounted = false;
      connection.off("ReceiveMessage", handleReceiveMessage);
    };
  }, [connection, conversationId, setMessages, scrollToBottom]);

  // 2. Mark as Read
  useEffect(() => {
    if (!connection || !conversationId || messages.length === 0) return;

    const hasUnread = messages.some(
      (m) =>
        m.senderId !== currentUser?.id &&
        !m.readBy?.some((r) => r.userId === currentUser?.id),
    );

    if (hasUnread && !isBlocked) {
      connection.invoke("MarkAsRead", conversationId).catch(console.error);
    }
  }, [connection, conversationId, messages, currentUser?.id, isBlocked]);

  // 3. Lắng nghe người khác đã đọc
  useEffect(() => {
    if (!connection || !conversationId) return;

    const handleUserHasRead = (
      readConversationId: string,
      readByUserId: string,
      readAt: string,
    ) => {
      if (readConversationId.toLowerCase() === conversationId.toLowerCase()) {
        setMessages((prev) =>
          prev.map((msg) => {
            if (!msg.readBy?.some((r) => r.userId === readByUserId)) {
              return {
                ...msg,
                readBy: [
                  ...(msg.readBy || []),
                  { userId: readByUserId, readAt: readAt },
                ],
              };
            }
            return msg;
          }),
        );
      }
    };

    connection.on("UserHasReadMessages", handleUserHasRead);
    return () => {
      connection.off("UserHasReadMessages", handleUserHasRead);
    };
  }, [connection, conversationId, setMessages]);

  // 4. Lắng nghe lỗi từ server (ví dụ: bị chặn)
  useEffect(() => {
    if (!connection) return;

    const handleReceiveError = (errorMessage: string) => {
      toast.error(errorMessage);

      if (errorMessage.toLowerCase().includes("chặn")) {
        markAsBlocked();
      }
    };

    connection.on("ReceiveError", handleReceiveError);

    return () => {
      connection.off("ReceiveError", handleReceiveError);
    };
  }, [connection, markAsBlocked]);

  // 4. Các hàm thao tác gửi đi
  const sendMessage = async (content: string) => {
    if (!content.trim() || !connection || !conversationId) return;
    await connection.invoke("SendMessage", conversationId, content.trim());
    connection.invoke("NotifyTyping", conversationId, false);
    if (typingTimeoutRef.current) clearTimeout(typingTimeoutRef.current);
  };

  const notifyTyping = (isTyping: boolean) => {
    if (!connection || !conversationId) return;
    connection.invoke("NotifyTyping", conversationId, isTyping);

    if (typingTimeoutRef.current) clearTimeout(typingTimeoutRef.current);
    if (isTyping) {
      typingTimeoutRef.current = setTimeout(() => {
        connection.invoke("NotifyTyping", conversationId, false);
      }, 2000);
    }
  };

  return { sendMessage, notifyTyping };
};
