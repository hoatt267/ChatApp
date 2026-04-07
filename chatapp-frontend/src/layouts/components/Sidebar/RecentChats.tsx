import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { chatService } from "../../../features/chat/services/chat.service";
import { useAuthStore } from "../../../features/auth/store/useAuthStore";
import type { Conversation } from "../../../features/chat/types";

export default function RecentChats() {
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);
  const [conversations, setConversations] = useState<Conversation[]>([]);

  useEffect(() => {
    if (user) {
      chatService
        .getConversations()
        .then((res) => setConversations(res.data))
        .catch((err) => console.error("Lỗi lấy danh sách đoạn chat:", err));
    }
  }, [user]);

  const getChatInfo = (conv: Conversation) => {
    if (conv.isGroup) return { name: conv.title || "Nhóm Chat", avatarUrl: "" };
    const otherUser = conv.participants.find((p) => p.userId !== user?.id);
    return {
      name: otherUser?.fullName || "Người dùng ẩn danh",
      avatarUrl: otherUser?.avatarUrl || "",
    };
  };

  return (
    <div className="flex-1 overflow-y-auto p-2 custom-scrollbar">
      <h3 className="px-3 pt-4 pb-2 text-xs font-semibold text-gray-500 uppercase tracking-wider">
        Gần đây ({conversations.length})
      </h3>

      {conversations.length === 0 ? (
        <div className="text-center text-gray-400 text-sm mt-10">
          Chưa có cuộc trò chuyện nào
        </div>
      ) : (
        <div className="space-y-1">
          {conversations.map((conv) => {
            const { name, avatarUrl } = getChatInfo(conv);
            return (
              <div
                key={conv.id}
                className="flex items-center gap-3 p-3 hover:bg-gray-100 rounded-lg cursor-pointer transition-colors"
                onClick={() => navigate(`/chat/${conv.id}`)}
              >
                <div className="relative flex-shrink-0">
                  {avatarUrl ? (
                    <img
                      src={avatarUrl}
                      alt="Avatar"
                      className="w-12 h-12 rounded-full object-cover border border-gray-200"
                    />
                  ) : (
                    <div className="w-12 h-12 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center font-bold text-lg">
                      {name.charAt(0).toUpperCase()}
                    </div>
                  )}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="font-semibold text-gray-800 truncate">{name}</p>
                  <p className="text-xs text-gray-500 truncate mt-0.5">
                    Bắt đầu cuộc trò chuyện...
                  </p>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
