import { useEffect, useState, useCallback } from "react";
import * as signalR from "@microsoft/signalr";
import { chatService } from "../services/chat.service";
import type { Conversation, Message } from "../types";
import type { UserResponse } from "../../auth/types";

export const useRecentChats = (
  user: UserResponse | null,
  connection: signalR.HubConnection | null,
) => {
  const [conversations, setConversations] = useState<Conversation[]>([]);

  const loadConversations = useCallback(async () => {
    try {
      const res = await chatService.getConversations();
      setConversations(res.data);
    } catch (err) {
      console.error("Lỗi lấy danh sách đoạn chat:", err);
    }
  }, []);

  // Lắng nghe SignalR cho Sidebar
  useEffect(() => {
    if (!user || !connection) return;

    const fetchInitialData = async () => {
      await loadConversations();
    };
    fetchInitialData();

    const handleReceiveMessage = (res: { data: Message }) => {
      const incomingMsg = res.data;
      setConversations((prev) => {
        const index = prev.findIndex(
          (c) => c.id === incomingMsg.conversationId,
        );
        if (index > -1) {
          const updatedConv = { ...prev[index], lastMessage: incomingMsg };
          const newList = [...prev];
          newList.splice(index, 1);
          newList.unshift(updatedConv);
          return newList;
        } else {
          // Bọc lại nếu gọi trong callback
          const fetchNewConv = async () => {
            await loadConversations();
          };
          fetchNewConv();
          return prev;
        }
      });
    };

    const handleNewConversation = () => {
      const fetchNewConv = async () => {
        await loadConversations();
      };
      fetchNewConv();
    };

    const handleUserHasRead = (
      readConvId: string,
      readByUserId: string,
      readAt: string,
    ) => {
      setConversations((prev) => {
        const index = prev.findIndex((c) => c.id === readConvId);
        if (index > -1) {
          const conv = prev[index];
          if (
            conv.lastMessage &&
            !conv.lastMessage.readBy?.some((r) => r.userId === readByUserId)
          ) {
            const updatedConv = {
              ...conv,
              lastMessage: {
                ...conv.lastMessage,
                readBy: [
                  ...(conv.lastMessage.readBy || []),
                  { userId: readByUserId, readAt },
                ],
              },
            };
            const newList = [...prev];
            newList[index] = updatedConv;
            return newList;
          }
        }
        return prev;
      });
    };

    connection.on("ReceiveMessage", handleReceiveMessage);
    connection.on("NewConversationCreated", handleNewConversation);
    connection.on("UserHasReadMessages", handleUserHasRead);

    return () => {
      connection.off("ReceiveMessage", handleReceiveMessage);
      connection.off("NewConversationCreated", handleNewConversation);
      connection.off("UserHasReadMessages", handleUserHasRead);
    };
  }, [user, connection, loadConversations]);

  const getChatInfo = (conv: Conversation) => {
    if (conv.isGroup) return { name: conv.title || "Nhóm Chat", avatarUrl: "" };
    const otherUser = conv.participants.find((p) => p.userId !== user?.id);
    return {
      name: otherUser?.fullName || "Người dùng ẩn danh",
      avatarUrl: otherUser?.avatarUrl || "",
    };
  };

  const renderLastMessage = (conv: Conversation) => {
    if (!conv.lastMessage) return "Bắt đầu cuộc trò chuyện...";
    const msg = conv.lastMessage;
    const isMine = msg.senderId === user?.id;

    let displayContent = msg.content;
    if ((!displayContent || displayContent === msg.fileName) && msg.fileUrl) {
      if (msg.fileUrl.match(/\.(jpeg|jpg|gif|png|webp)(\?.*)?$/i))
        displayContent = "[Hình ảnh]";
      else if (msg.fileUrl.match(/\.(mp4|webm|ogg)(\?.*)?$/i))
        displayContent = "[Video]";
      else displayContent = "[Tệp đính kèm]";
    }

    if (isMine) return `Bạn: ${displayContent}`;
    if (conv.isGroup) {
      const sender = conv.participants.find((p) => p.userId === msg.senderId);
      const shortName = sender?.fullName?.split(" ").pop() || "Ai đó";
      return `${shortName}: ${displayContent}`;
    }
    return displayContent;
  };

  return { conversations, getChatInfo, renderLastMessage };
};
