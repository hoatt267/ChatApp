import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { chatService } from "../../../features/chat/services/chat.service";
import { useAuthStore } from "../../../features/auth/store/useAuthStore";
import { useChatStore } from "../../../features/chat/store/useChatStore";
import type { Conversation, Message } from "../../../features/chat/types";

// 🌟 1. MỞ RỘNG KIỂU DỮ LIỆU ĐỂ LƯU THÊM TIN NHẮN CUỐI
type ChatItem = Conversation & {
  lastMessageContent?: string;
};

export default function RecentChats() {
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);
  const connection = useChatStore((state) => state.connection);

  const [conversations, setConversations] = useState<ChatItem[]>([]);

  // 🌟 2. HÀM TẢI DANH SÁCH (Dùng useCallback để tối ưu)
  useEffect(() => {
    if (!user || !connection) return;

    let isMounted = true;

    // Khai báo hàm fetch ngay bên trong để an toàn tuyệt đối
    const loadConversations = async () => {
      try {
        const res = await chatService.getConversations();
        if (isMounted) setConversations(res.data);
      } catch (err) {
        console.error("Lỗi lấy danh sách đoạn chat:", err);
      }
    };

    // 1. Tải danh sách lần đầu tiên
    loadConversations();

    // 2. Đăng ký sự kiện SignalR
    const handleReceiveMessage = (res: { data: Message }) => {
      const incomingMsg = res.data;

      setConversations((prev) => {
        const index = prev.findIndex(
          (c) => c.id === incomingMsg.conversationId,
        );

        if (index > -1) {
          const updatedConv = {
            ...prev[index],
            lastMessageContent: incomingMsg.content,
          };
          const newList = [...prev];
          newList.splice(index, 1);
          newList.unshift(updatedConv);
          return newList;
        } else {
          // Nếu có phòng mới chưa từng chat -> Gọi API lấy lại list
          loadConversations();
          return prev;
        }
      });
    };

    const handleNewConversation = () => {
      loadConversations();
    };

    connection.on("ReceiveMessage", handleReceiveMessage);
    connection.on("NewConversationCreated", handleNewConversation);

    // Dọn dẹp tai nghe khi component bị hủy
    return () => {
      isMounted = false;
      connection.off("ReceiveMessage", handleReceiveMessage);
      connection.off("NewConversationCreated", handleNewConversation);
    };
  }, [user, connection]);

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
                    {/* 🌟 Nếu có tin nhắn mới thì in ra, không thì in chữ Bắt đầu */}
                    {conv.lastMessageContent || conv.lastMessage?.content ? (
                      <span className="text-gray-700 font-medium">
                        {conv.lastMessage?.content || conv.lastMessageContent}
                      </span>
                    ) : (
                      "Bắt đầu cuộc trò chuyện..."
                    )}
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
