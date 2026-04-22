import React, { useEffect, useState, useRef, useCallback } from "react";
import { useParams } from "react-router-dom";
import { Loader2, ShieldAlert } from "lucide-react";

import { useAuthStore } from "../../../auth/store/useAuthStore";
import { useChatStore } from "../../store/useChatStore";
import { useSignalRStore } from "../../../../store/useSignalRStore";

import { useChatSignalR } from "../../hooks/useChatSignalR";

import ChatHeader from "./ChatHeader";
import MessageList from "./MessageList";
import MessageInput from "./MessageInput";
import { useChatMessages } from "../../hooks/useChatMessage";

export default function ChatRoom() {
  const { conversationId } = useParams<{ conversationId: string }>();
  const currentUser = useAuthStore((state) => state.user);
  const connection = useSignalRStore((state) => state.connection);

  const onlineUsers = useChatStore((state) => state.onlineUsers);
  const typingUsers = useChatStore((state) => state.typingUsers);
  const typingUsersInChat = typingUsers[conversationId || ""] || [];

  const [newMessage, setNewMessage] = useState("");
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const chatContainerRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = useCallback(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, []);

  // 1. Dùng Hook quản lý Data (Axios)
  const {
    messages,
    setMessages,
    participants,
    chatInfo,
    isLoading,
    isLoadingMore,
    fetchInitialData,
    loadMoreMessages,
    handleSendMedia,
    markAsBlocked,
  } = useChatMessages(conversationId, currentUser, scrollToBottom);

  // 2. Dùng Hook quản lý Socket (SignalR)
  const { sendMessage, notifyTyping } = useChatSignalR(
    connection,
    conversationId,
    currentUser,
    messages,
    setMessages,
    scrollToBottom,
    chatInfo?.isBlocked || false,
    markAsBlocked,
  );

  useEffect(() => {
    fetchInitialData();
  }, [fetchInitialData]);

  // Xử lý Input
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value;
    setNewMessage(val);
    if (!newMessage && val) notifyTyping(true);
  };

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    await sendMessage(newMessage);
    setNewMessage("");
  };

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
        onScroll={() => loadMoreMessages(chatContainerRef)}
        participants={participants}
      />

      {/* KIỂM TRA BLOCK TỪ BACKEND ĐỂ ẨN INPUT */}
      {chatInfo?.isBlocked ? (
        <div className="h-[72px] bg-gray-50 flex items-center justify-center border-t border-gray-200 z-10">
          <p className="text-gray-500 font-medium bg-white px-6 py-2 rounded-full shadow-sm flex items-center gap-2">
            <ShieldAlert size={18} className="text-red-400" />
            Bạn không thể trả lời cuộc trò chuyện này.
          </p>
        </div>
      ) : (
        <MessageInput
          value={newMessage}
          onChange={handleInputChange}
          onSubmit={handleSendMessage}
          onSendMedia={handleSendMedia}
        />
      )}
    </div>
  );
}
