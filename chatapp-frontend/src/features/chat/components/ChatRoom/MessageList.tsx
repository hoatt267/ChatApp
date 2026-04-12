import React, { useState, useMemo } from "react";
import { Loader2 } from "lucide-react";
import type { Message, Participant, ReadReceipt } from "../../types";
import type { UserResponse } from "../../../auth/types";
import MessageItem from "./MessageItem"; // 🌟 Import component con vào
import SeenByModal from "./SeenByModal";

interface MessageListProps {
  messages: Message[];
  currentUser: UserResponse | null;
  participants: Participant[];
  isLoadingMore: boolean;
  typingNames: string[];
  chatContainerRef: React.RefObject<HTMLDivElement | null>;
  messagesEndRef: React.RefObject<HTMLDivElement | null>;
  onScroll: (e: React.UIEvent<HTMLDivElement>) => void;
}

export default function MessageList({
  messages,
  currentUser,
  participants,
  isLoadingMore,
  typingNames,
  chatContainerRef,
  messagesEndRef,
  onScroll,
}: MessageListProps) {
  const [seenByModalData, setSeenByModalData] = useState<ReadReceipt[] | null>(
    null,
  );

  // Thuật toán tìm tin cuối cùng
  const lastReadMsgIds = useMemo(() => {
    const map: Record<string, string> = {};
    messages.forEach((msg) => {
      msg.readBy?.forEach((receipt) => {
        map[receipt.userId] = msg.id;
      });
    });
    return map;
  }, [messages]);

  return (
    <div
      ref={chatContainerRef}
      onScroll={onScroll}
      className="flex-1 overflow-y-auto p-4 space-y-4 custom-scrollbar relative"
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
        messages.map((msg) => {
          const isMine = msg.senderId === currentUser?.id;

          const displayAvatars = (msg.readBy || []).filter((r) => {
            return (
              r.userId !== currentUser?.id &&
              r.userId !== msg.senderId &&
              lastReadMsgIds[r.userId] === msg.id
            );
          });

          return (
            <MessageItem
              key={msg.id}
              msg={msg}
              isMine={isMine}
              currentUser={currentUser}
              participants={participants}
              displayAvatars={displayAvatars}
              onOpenSeenBy={setSeenByModalData}
            />
          );
        })
      )}

      {typingNames.length > 0 && (
        <div className="flex items-center gap-2 text-xs text-gray-500 italic ml-12 mb-2">
          {typingNames.join(", ")} đang nhập...
        </div>
      )}

      <div ref={messagesEndRef} />

      {/* MODAL DANH SÁCH NGƯỜI XEM */}
      <SeenByModal
        isOpen={!!seenByModalData}
        onClose={() => setSeenByModalData(null)}
        receipts={seenByModalData}
        participants={participants}
      />
    </div>
  );
}
