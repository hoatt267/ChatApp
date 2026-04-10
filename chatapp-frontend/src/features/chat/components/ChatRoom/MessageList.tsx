// File: src/features/chat/components/ChatRoom/MessageList.tsx
import React from "react";
import { Loader2 } from "lucide-react";
import type { Message } from "../../types";
import type { UserResponse } from "../../../auth/types";

interface MessageListProps {
  messages: Message[];
  currentUser: UserResponse | null;
  isLoadingMore: boolean;
  typingNames: string[];
  chatContainerRef: React.RefObject<HTMLDivElement | null>;
  messagesEndRef: React.RefObject<HTMLDivElement | null>;
  onScroll: (e: React.UIEvent<HTMLDivElement>) => void;
}

export default function MessageList({
  messages,
  currentUser,
  isLoadingMore,
  typingNames,
  chatContainerRef,
  messagesEndRef,
  onScroll,
}: MessageListProps) {
  return (
    <div
      ref={chatContainerRef}
      onScroll={onScroll}
      className="flex-1 overflow-y-auto p-4 space-y-4 custom-scrollbar"
    >
      {isLoadingMore && (
        <div className="flex justify-center py-2">
          <Loader2 className="w-5 h-5 text-blue-500 animate-spin" />
        </div>
      )}

      {messages.length === 0 ? (
        <div className="text-center text-gray-500 bg-white/60 py-2 px-4 rounded-full w-max mx-auto shadow-sm text-sm">
          Hãy là người đầu tiên gửi tin nhắn!
        </div>
      ) : (
        messages.map((msg, index) => {
          const isMine = msg.senderId === currentUser?.id;
          const isLastMessage = index === messages.length - 1;
          const isReadByOther = isMine && msg.readBy && msg.readBy.length > 0;
          return (
            <div
              key={msg.id}
              className={`flex flex-col ${isMine ? "items-end" : "items-start"}`}
            >
              <div
                className={`max-w-[70%] flex gap-2 ${isMine ? "flex-row-reverse" : "flex-row"}`}
              >
                {!isMine && (
                  <img
                    src={
                      msg.senderAvatarUrl || "https://via.placeholder.com/150"
                    }
                    alt="avatar"
                    className="w-8 h-8 rounded-full object-cover flex-shrink-0 mt-1"
                  />
                )}
                <div
                  className={`px-4 py-2 rounded-2xl shadow-sm ${
                    isMine
                      ? "bg-blue-500 text-white rounded-tr-sm"
                      : "bg-white text-gray-800 rounded-tl-sm border border-gray-100"
                  }`}
                >
                  {!isMine && (
                    <p className="text-xs text-blue-600 font-bold mb-1">
                      {msg.senderName}
                    </p>
                  )}
                  <p className="text-sm break-words whitespace-pre-wrap">
                    {msg.content}
                  </p>
                  <p
                    className={`text-[10px] text-right mt-1 ${isMine ? "text-blue-100" : "text-gray-400"}`}
                  >
                    {new Date(msg.createdAt).toLocaleTimeString([], {
                      hour: "2-digit",
                      minute: "2-digit",
                    })}
                  </p>
                </div>
              </div>
              {isLastMessage && isReadByOther && (
                <div className="text-[11px] text-gray-500 mt-1 mr-2 flex items-center gap-1">
                  ✓ Đã xem
                </div>
              )}
            </div>
          );
        })
      )}

      {typingNames.length > 0 && (
        <div className="flex items-center gap-2 text-xs text-gray-500 italic ml-12 mb-2">
          <div className="flex gap-1">
            <span className="w-1 h-1 bg-gray-400 rounded-full animate-bounce"></span>
            <span className="w-1 h-1 bg-gray-400 rounded-full animate-bounce [animation-delay:0.2s]"></span>
            <span className="w-1 h-1 bg-gray-400 rounded-full animate-bounce [animation-delay:0.4s]"></span>
          </div>
          {typingNames.join(", ")} đang nhập...
        </div>
      )}

      <div ref={messagesEndRef} />
    </div>
  );
}
