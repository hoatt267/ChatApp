import { UserPlus, MessageSquare, Clock } from "lucide-react";
import type { DiscoverUser } from "../../types";
import { FriendshipStatus } from "../../types";

interface UserCardProps {
  user: DiscoverUser;
  onAddFriend: (userId: string) => void;
  onChat: (userId: string) => void;
  isProcessing: boolean;
}

export default function UserCard({
  user,
  onAddFriend,
  onChat,
  isProcessing,
}: UserCardProps) {
  // Logic render nút bấm dựa trên trạng thái
  const renderActionButton = () => {
    switch (user.friendshipStatus) {
      case FriendshipStatus.None:
        return (
          <button
            onClick={() => onAddFriend(user.userId)}
            disabled={isProcessing}
            className="w-full flex items-center justify-center gap-2 px-4 py-2 bg-blue-50 text-blue-600 font-medium rounded-lg hover:bg-blue-100 transition-colors disabled:opacity-50"
          >
            <UserPlus size={18} /> Thêm bạn bè
          </button>
        );
      case FriendshipStatus.Pending:
        return (
          <button
            disabled
            className="w-full flex items-center justify-center gap-2 px-4 py-2 bg-gray-100 text-gray-500 font-medium rounded-lg cursor-not-allowed"
          >
            <Clock size={18} /> Đang chờ...
          </button>
        );
      case FriendshipStatus.Accepted:
        return (
          <button
            onClick={() => onChat(user.userId)}
            className="w-full flex items-center justify-center gap-2 px-4 py-2 bg-green-50 text-green-600 font-medium rounded-lg hover:bg-green-100 transition-colors"
          >
            <MessageSquare size={18} /> Nhắn tin
          </button>
        );
      default:
        return null; // Không hiển thị nút nếu bị Block
    }
  };

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-md transition-shadow flex flex-col">
      {/* Ảnh bìa ảo */}
      <div className="h-16 bg-gradient-to-r from-blue-100 to-blue-50"></div>

      <div className="px-4 pb-4 flex-1 flex flex-col items-center -mt-8">
        <img
          src={
            user.avatarUrl ||
            `https://ui-avatars.com/api/?name=${user.fullName}&size=128`
          }
          alt={user.fullName}
          className="w-20 h-20 rounded-full object-cover border-4 border-white shadow-sm mb-3"
        />
        <h3 className="font-bold text-gray-800 text-center text-lg truncate w-full">
          {user.fullName}
        </h3>
        <p className="text-xs text-gray-500 mb-4 truncate w-full text-center">
          {user.email}
        </p>

        <div className="mt-auto w-full">{renderActionButton()}</div>
      </div>
    </div>
  );
}
