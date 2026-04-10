import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { chatService } from "../../../features/chat/services/chat.service";
import { useAuthStore } from "../../../features/auth/store/useAuthStore";
import { useChatStore } from "../../../features/chat/store/useChatStore";
import type { Conversation, Message } from "../../../features/chat/types";

export default function RecentChats() {
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);
  const connection = useChatStore((state) => state.connection);

  const [conversations, setConversations] = useState<Conversation[]>([]);

  useEffect(() => {
    if (!user || !connection) return;

    let isMounted = true;

    const loadConversations = async () => {
      try {
        const res = await chatService.getConversations();
        if (isMounted) setConversations(res.data);
      } catch (err) {
        console.error("Lỗi lấy danh sách đoạn chat:", err);
      }
    };

    loadConversations();

    const handleReceiveMessage = (res: { data: Message }) => {
      const incomingMsg = res.data;

      setConversations((prev) => {
        const index = prev.findIndex(
          (c) => c.id === incomingMsg.conversationId,
        );

        if (index > -1) {
          // 🌟 CẬP NHẬT TRỰC TIẾP VÀO OBJECT lastMessage
          const updatedConv = {
            ...prev[index],
            lastMessage: incomingMsg,
          };

          const newList = [...prev];
          newList.splice(index, 1);
          newList.unshift(updatedConv);
          return newList;
        } else {
          loadConversations();
          return prev;
        }
      });
    };

    const handleNewConversation = () => {
      loadConversations();
    };

    const handleUserHasRead = (
      readConversationId: string,
      readByUserId: string,
    ) => {
      setConversations((prev) => {
        const index = prev.findIndex((c) => c.id === readConversationId);
        if (index > -1) {
          const conv = prev[index];
          // Chỉ cập nhật nếu có lastMessage và người đọc chưa có trong mảng readBy
          if (
            conv.lastMessage &&
            !conv.lastMessage.readBy?.includes(readByUserId)
          ) {
            const updatedConv = {
              ...conv,
              lastMessage: {
                ...conv.lastMessage,
                readBy: [...(conv.lastMessage.readBy || []), readByUserId],
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
      isMounted = false;
      connection.off("ReceiveMessage", handleReceiveMessage);
      connection.off("NewConversationCreated", handleNewConversation);
      connection.off("UserHasReadMessages", handleUserHasRead);
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

  const renderLastMessage = (conv: Conversation) => {
    if (!conv.lastMessage) return "Bắt đầu cuộc trò chuyện...";

    const msg = conv.lastMessage;
    const isMine = msg.senderId === user?.id;

    // Nếu mình là người gửi
    if (isMine) return `Bạn: ${msg.content}`;

    // Nếu là người khác gửi trong nhóm chat
    if (conv.isGroup) {
      const sender = conv.participants.find((p) => p.userId === msg.senderId);
      const shortName = sender?.fullName?.split(" ").pop() || "Ai đó";
      return `${shortName}: ${msg.content}`;
    }

    // Nếu là chat 1-1
    return msg.content;
  };

  const displayConversations = conversations.filter(
    (conv) => conv.isGroup || conv.lastMessage,
  );

  return (
    <div className="flex-1 overflow-y-auto p-2 custom-scrollbar">
      <h3 className="px-3 pt-4 pb-2 text-xs font-semibold text-gray-500 uppercase tracking-wider">
        Gần đây ({displayConversations.length})
      </h3>

      {displayConversations.length === 0 ? (
        <div className="text-center text-gray-400 text-sm mt-10">
          Chưa có cuộc trò chuyện nào
        </div>
      ) : (
        <div className="space-y-1">
          {displayConversations.map((conv) => {
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
                  <p
                    className={`text-xs truncate mt-0.5 ${!conv.lastMessage?.readBy?.includes(user?.id || "") && conv.lastMessage?.senderId !== user?.id ? "text-gray-900 font-bold" : "text-gray-500"}`}
                  >
                    {renderLastMessage(conv)}
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
