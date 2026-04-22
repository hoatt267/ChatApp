import { useNavigate } from "react-router-dom";
import { useAuthStore } from "../../../features/auth/store/useAuthStore";
import { useSignalRStore } from "../../../store/useSignalRStore";
import { useRecentChats } from "../../../features/chat/hooks/useRecentChats";

export default function RecentChats() {
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);
  const connection = useSignalRStore((state) => state.connection);

  const { conversations, getChatInfo, renderLastMessage } = useRecentChats(
    user,
    connection,
  );

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
            const hasUnread =
              !conv.lastMessage?.readBy?.some((r) => r.userId === user?.id) &&
              conv.lastMessage?.senderId !== user?.id;

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
                    className={`text-xs truncate mt-0.5 ${hasUnread ? "text-gray-900 font-bold" : "text-gray-500"}`}
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
