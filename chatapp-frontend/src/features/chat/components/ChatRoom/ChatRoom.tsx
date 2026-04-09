// File: src/features/chat/components/ChatRoom/ChatRoom.tsx
import React, { useEffect, useState, useRef, useCallback } from "react";
import { useParams } from "react-router-dom";
import { Loader2 } from "lucide-react";
import * as signalR from "@microsoft/signalr";

import { useAuthStore } from "../../../auth/store/useAuthStore";
import { useChatStore } from "../../store/useChatStore";
import { chatService } from "../../services/chat.service";
import type { Message } from "../../types";

import ChatHeader from "./ChatHeader";
import MessageList from "./MessageList";
import MessageInput from "./MessageInput";

const MESSAGES_PER_PAGE = 10;

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
    const fetchInitialData = async () => {
      if (!conversationId) return;
      setIsLoading(true);
      setHasMore(true);
      try {
        const convsRes = await chatService.getConversations();
        const currentConv = convsRes.data.find((c) => c.id === conversationId);
        if (currentConv) {
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
          MESSAGES_PER_PAGE,
        );
        if (msgRes.data.length < MESSAGES_PER_PAGE) setHasMore(false);
        setMessages(msgRes.data);
      } catch (error) {
        console.error("Lỗi:", error);
      } finally {
        setIsLoading(false);
        setTimeout(scrollToBottom, 100);
      }
    };
    fetchInitialData();
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
          MESSAGES_PER_PAGE,
          oldestMessage.createdAt,
        );
        if (res.data.length < MESSAGES_PER_PAGE) setHasMore(false);
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
        setMessages((prev) => [...prev, res.data]);
        setTimeout(scrollToBottom, 100);
      }
    };

    connection.on("ReceiveMessage", handleReceiveMessage);

    return () => {
      isMounted = false;
      connection.off("ReceiveMessage", handleReceiveMessage);
    };
  }, [connection, conversationId, scrollToBottom]);

  const typingNames = typingUsersInChat
    .map((id) => onlineUsers.find((u) => u.userId === id)?.fullName)
    .filter((n): n is string => !!n);

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
      />
    </div>
  );
}
