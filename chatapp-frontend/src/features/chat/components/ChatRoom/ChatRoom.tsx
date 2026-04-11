// File: src/features/chat/components/ChatRoom/ChatRoom.tsx
import React, { useEffect, useState, useRef, useCallback } from "react";
import { useParams } from "react-router-dom";
import { Loader2 } from "lucide-react";
import * as signalR from "@microsoft/signalr";

import { useAuthStore } from "../../../auth/store/useAuthStore";
import { useChatStore } from "../../store/useChatStore";
import { chatService } from "../../services/chat.service";
import { MessageType, type Message } from "../../types";

import ChatHeader from "./ChatHeader";
import MessageList from "./MessageList";
import MessageInput from "./MessageInput";
import { APP_CONFIG } from "../../../../config";

export default function ChatRoom() {
  const { conversationId } = useParams<{ conversationId: string }>();
  const currentUser = useAuthStore((state) => state.user);
  const connection = useChatStore((state) => state.connection);
  const onlineUsers = useChatStore((state) => state.onlineUsers);
  const typingUsers = useChatStore((state) => state.typingUsers);
  const typingUsersInChat = typingUsers[conversationId || ""] || [];

  const [messages, setMessages] = useState<Message[]>([]);
  const [newMessage, setNewMessage] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isLoadingMore, setIsLoadingMore] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [chatInfo, setChatInfo] = useState<{
    name: string;
    avatarUrl: string;
  } | null>(null);

  const messagesEndRef = useRef<HTMLDivElement>(null);
  const chatContainerRef = useRef<HTMLDivElement>(null);
  const isLoadingRef = useRef(false);
  const typingTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const scrollToBottom = useCallback(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, []);

  // 1. Fetch data ban đầu (Thông tin phòng + Tin nhắn)
  useEffect(() => {
    let isMounted = true;
    const fetchInitialData = async () => {
      if (!conversationId) return;
      setIsLoading(true);
      setHasMore(true);
      try {
        const convsRes = await chatService.getConversationById(conversationId);
        const currentConv = convsRes.data;
        if (isMounted && currentConv) {
          const otherUser = currentConv.participants.find(
            (p) => p.userId !== currentUser?.id,
          );
          setChatInfo({
            name: currentConv.isGroup
              ? currentConv.title || "Nhóm Chat"
              : otherUser?.fullName || "Người dùng",
            avatarUrl: currentConv.isGroup ? "" : otherUser?.avatarUrl || "",
          });
        }
        const msgRes = await chatService.getMessages(
          conversationId,
          APP_CONFIG.MESSAGES_PER_PAGE,
        );
        if (isMounted) {
          if (msgRes.data.length < APP_CONFIG.MESSAGES_PER_PAGE)
            setHasMore(false);
          setMessages(msgRes.data);
        }
      } catch (error) {
        console.error("Lỗi:", error);
      } finally {
        if (isMounted) {
          setIsLoading(false);
          setTimeout(scrollToBottom, 100);
        }
      }
    };
    fetchInitialData();

    return () => {
      isMounted = false;
    };
  }, [conversationId, currentUser?.id, scrollToBottom]);

  // 2. Xử lý Infinite Scroll
  const handleScroll = async () => {
    if (
      !chatContainerRef.current ||
      isLoadingRef.current ||
      !hasMore ||
      !conversationId
    )
      return;
    if (chatContainerRef.current.scrollTop === 0) {
      const oldestMessage = messages[0];
      if (!oldestMessage) return;
      isLoadingRef.current = true;
      setIsLoadingMore(true);
      const previousScrollHeight = chatContainerRef.current.scrollHeight;
      try {
        const res = await chatService.getMessages(
          conversationId,
          APP_CONFIG.MESSAGES_PER_PAGE,
          oldestMessage.createdAt,
        );
        if (res.data.length < APP_CONFIG.MESSAGES_PER_PAGE) setHasMore(false);
        setMessages((prev) => {
          const newMsgs = res.data.filter(
            (m) => !prev.some((p) => p.id === m.id),
          );
          return [...newMsgs, ...prev];
        });
        requestAnimationFrame(() => {
          if (chatContainerRef.current)
            chatContainerRef.current.scrollTop =
              chatContainerRef.current.scrollHeight - previousScrollHeight;
        });
      } finally {
        isLoadingRef.current = false;
        setIsLoadingMore(false);
      }
    }
  };

  // 3. Xử lý gõ phím & NotifyTyping
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value;
    setNewMessage(val);
    if (!connection || !conversationId) return;
    if (!newMessage && val)
      connection.invoke("NotifyTyping", conversationId, true);
    if (typingTimeoutRef.current) clearTimeout(typingTimeoutRef.current);
    typingTimeoutRef.current = setTimeout(() => {
      connection.invoke("NotifyTyping", conversationId, false);
    }, 2000);
  };

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newMessage.trim() || !connection || !conversationId) return;
    await connection.invoke("SendMessage", conversationId, newMessage.trim());
    setNewMessage("");
    connection.invoke("NotifyTyping", conversationId, false);
    if (typingTimeoutRef.current) clearTimeout(typingTimeoutRef.current);
  };

  // 🌟 4. LẮNG NGHE SIGNALR VÀ JOIN PHÒNG
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

    // B. Lắng nghe tin nhắn tới
    const handleReceiveMessage = (res: { data: Message }) => {
      if (
        res.data.conversationId.toLowerCase() === conversationId.toLowerCase()
      ) {
        setMessages((prev) => {
          // 1. Nếu Axios đã nhanh chân ghi đè trước, ta bỏ qua không thêm nữa
          if (prev.some((m) => m.id === res.data.id)) return prev;

          //  2. TÌM VÀ ĐÈ TIN NHẮN ẢO (OPTIMISTIC)
          // Kiểm tra xem trên màn hình có tin nhắn ảo nào đang upload của chính mình gửi không
          const optimisticIndex = prev.findIndex(
            (m) =>
              m.isOptimistic &&
              m.senderId === res.data.senderId &&
              m.fileName === res.data.fileName,
          );

          if (optimisticIndex !== -1) {
            // SignalR về nhanh hơn Axios! Ta sẽ đè tin thật vào vị trí tin ảo ngay lập tức.
            const newMsgs = [...prev];
            newMsgs[optimisticIndex] = res.data;
            return newMsgs;
          }

          // 3. Nếu là tin nhắn bình thường của người khác
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
  }, [connection, conversationId, scrollToBottom]);

  // MarkAsRead khi vào phòng chat nếu có tin nhắn chưa đọc
  useEffect(() => {
    if (!connection || !conversationId || messages.length === 0) return;

    const hasUnread = messages.some(
      (m) =>
        m.senderId !== currentUser?.id &&
        !m.readBy?.includes(currentUser?.id || ""),
    );

    if (hasUnread) {
      connection.invoke("MarkAsRead", conversationId).catch(console.error);
    }
  }, [connection, conversationId, messages, currentUser?.id]);

  // Lắng nghe sự kiện UserHasReadMessages để cập nhật trạng thái đã đọc
  useEffect(() => {
    if (!connection || !conversationId) return;

    const handleUserHasRead = (
      readConversationId: string,
      readByUserId: string,
    ) => {
      if (readConversationId.toLowerCase() === conversationId.toLowerCase()) {
        setMessages((prev) =>
          prev.map((msg) => {
            // Nếu tin nhắn chưa có ID người này trong mảng readBy thì thêm vào
            if (!msg.readBy?.includes(readByUserId)) {
              return { ...msg, readBy: [...(msg.readBy || []), readByUserId] };
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
  }, [connection, conversationId]);

  const typingNames = typingUsersInChat
    .map((id) => onlineUsers.find((u) => u.userId === id)?.fullName)
    .filter((n): n is string => !!n);

  // Xử lý gửi File / Hình ảnh
  const handleSendMedia = async (file: File, content?: string) => {
    if (!conversationId || !currentUser) return;

    const msgType = file.type.startsWith("video/")
      ? MessageType.Video
      : file.type.startsWith("image/")
        ? MessageType.Image
        : MessageType.Document;
    // 1.Tạo một ID ảo và một đường link ảo đọc trực tiếp từ RAM
    const tempId = `temp-${Date.now()}`;
    const fileUrl = URL.createObjectURL(file);
    // 2. Tạo Optimistic Message
    const tempMsg: Message = {
      id: tempId,
      conversationId,
      senderId: currentUser.id,
      senderName: currentUser.fullName || "",
      senderAvatarUrl: currentUser.avatarUrl || "",
      content: content || "",
      type: msgType,
      fileUrl: fileUrl,
      fileName: file.name,
      createdAt: new Date().toISOString(),
      readBy: [],
      isOptimistic: true, //  Đánh dấu cờ
      progress: 0, //  Tiến trình ban đầu
    };
    // 3. Đưa ngay lên màn hình và cuộn xuống
    setMessages((prev) => [...prev, tempMsg]);
    setTimeout(scrollToBottom, 100);

    try {
      const res = await chatService.uploadMedia(
        conversationId,
        file,
        content,
        (progressEvent) => {
          const percentCompleted = Math.round(
            (progressEvent.loaded * 100) / (progressEvent.total || file.size),
          );

          // Cập nhật % vào đúng cái tin nhắn ảo đó
          setMessages((prev) =>
            prev.map((msg) =>
              msg.id === tempId ? { ...msg, progress: percentCompleted } : msg,
            ),
          );
        },
      );

      // 5. Thành công: Xử lý logic đè tin nhắn thông minh
      const realMsg = res.data;

      setMessages((prev) => {
        // Kiểm tra xem anh bạn SignalR đã chạy trước và ghi đè tin này chưa
        const isAlreadyAddedBySignalR = prev.some((m) => m.id === realMsg.id);

        if (isAlreadyAddedBySignalR) {
          // Nếu SignalR đã làm rồi, ta dọn dẹp cái id ảo (nếu nó còn sót lại)
          return prev.filter((m) => m.id !== tempId);
        } else {
          // Nếu Axios nhanh hơn, Axios sẽ tự thay thế ID ảo bằng ID thật
          return prev.map((msg) => (msg.id === tempId ? realMsg : msg));
        }
      });
    } catch (error) {
      console.error("Lỗi upload file:", error);
      // Lỗi thì xóa cái tin nhắn ảo đi và báo lỗi
      setMessages((prev) => prev.filter((msg) => msg.id !== tempId));
      alert("Tải tệp lên thất bại");
    }
  };

  if (isLoading)
    return (
      <div className="flex-1 flex items-center justify-center bg-[#e5ddd5]">
        <Loader2 className="w-8 h-8 text-blue-500 animate-spin" />
      </div>
    );

  return (
    <div className="flex-1 flex flex-col h-full bg-[#e5ddd5] relative">
      <ChatHeader
        name={chatInfo?.name || ""}
        avatarUrl={chatInfo?.avatarUrl || ""}
      />
      <MessageList
        messages={messages}
        currentUser={currentUser}
        isLoadingMore={isLoadingMore}
        typingNames={typingNames}
        chatContainerRef={chatContainerRef}
        messagesEndRef={messagesEndRef}
        onScroll={handleScroll}
      />
      <MessageInput
        value={newMessage}
        onChange={handleInputChange}
        onSubmit={handleSendMessage}
        onSendMedia={handleSendMedia}
      />
    </div>
  );
}
