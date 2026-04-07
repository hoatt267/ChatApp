import { Loader2, User as UserIcon } from "lucide-react";
import { useAuthStore } from "../../../features/auth/store/useAuthStore";
import { useChatStore } from "../../../features/chat/store/useChatStore";
import { useState } from "react";
import { chatService } from "../../../features/chat/services/chat.service";
import { useNavigate } from "react-router";

export default function OnlineUsers() {
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);
  const onlineUsers = useChatStore((state) => state.onlineUsers);
  const otherOnlineUsers = onlineUsers.filter((u) => u.userId !== user?.id);

  const [creatingChatId, setCreatingChatId] = useState<string | null>(null);
  const handleStartChat = async (targetUserId: string) => {
    if (creatingChatId) return; // Chống spam click

    try {
      setCreatingChatId(targetUserId);

      const response = await chatService.createPrivateChat(targetUserId);
      const conversationId = response.data.id;

      // Nhảy sang URL của phòng chat đó
      navigate(`/chat/${conversationId}`);
    } catch (error) {
      console.error("Lỗi khi tạo/lấy phòng chat:", error);
      alert("Không thể bắt đầu cuộc trò chuyện. Vui lòng thử lại sau.");
    } finally {
      setCreatingChatId(null);
    }
  };

  return (
    <div className="max-h-[30%] overflow-y-auto p-2 custom-scrollbar border-b border-gray-200">
      <h3 className="px-3 pt-2 pb-2 text-xs font-semibold text-gray-500 uppercase tracking-wider">
        Đang hoạt động ({otherOnlineUsers.length})
      </h3>

      {otherOnlineUsers.length === 0 ? (
        <div className="text-center text-gray-400 text-sm mt-4 mb-4">
          Chưa có ai online
        </div>
      ) : (
        <div className="space-y-1">
          {otherOnlineUsers.map((onlineUser) => (
            <div
              key={onlineUser.userId}
              onClick={() => handleStartChat(onlineUser.userId)}
              className={`flex items-center gap-3 p-3 hover:bg-gray-100 rounded-lg cursor-pointer transition-colors ${
                creatingChatId === onlineUser.userId
                  ? "opacity-70 pointer-events-none"
                  : ""
              }`}
            >
              <div className="relative">
                {onlineUser.avatarUrl ? (
                  <img
                    src={onlineUser.avatarUrl}
                    alt="Avatar"
                    className="w-12 h-12 rounded-full object-cover border border-gray-200"
                  />
                ) : (
                  <div className="w-12 h-12 rounded-full bg-gray-200 text-gray-500 flex items-center justify-center">
                    <UserIcon size={24} />
                  </div>
                )}
                <div className="absolute bottom-0 right-0 w-3.5 h-3.5 bg-green-500 border-2 border-white rounded-full"></div>
              </div>
              <div className="flex-1 min-w-0">
                <p className="font-semibold text-gray-800 truncate">
                  {onlineUser.fullName}
                </p>
                <p className="text-xs text-green-600 font-medium truncate">
                  Đang hoạt động
                </p>
              </div>
              {creatingChatId === onlineUser.userId && (
                <Loader2 className="w-5 h-5 text-blue-500 animate-spin" />
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
