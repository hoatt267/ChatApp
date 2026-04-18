import { useEffect, useState } from "react";
import { Loader2, UserMinus, Ban, MessageSquare } from "lucide-react";
import { toast } from "react-toastify";
import { useNavigate } from "react-router-dom";
import { friendService } from "../services/friend.service";
import { chatService } from "../../chat/services/chat.service";
import { FriendshipAction, type FriendProfile } from "../types";
import { useFriendStore } from "../store/useFriendStore";

export default function FriendList() {
  const refreshTrigger = useFriendStore((state) => state.refreshTrigger);
  const [friends, setFriends] = useState<FriendProfile[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  const fetchFriends = async () => {
    try {
      const res = await friendService.getFriends();
      setFriends(res.data);
    } catch (error) {
      toast.error("Lỗi khi tải danh sách bạn bè");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchFriends();
  }, [refreshTrigger]);

  const handleBlock = async (targetUserId: string) => {
    // Thêm confirm để tránh bấm nhầm
    if (
      !window.confirm(
        "Bạn có chắc chắn muốn chặn người này? Họ sẽ không thể nhắn tin cho bạn nữa.",
      )
    )
      return;

    try {
      await friendService.blockUser(targetUserId);
      toast.success("Đã chặn người dùng!");
      fetchFriends();
    } catch (error) {
      toast.error("Có lỗi xảy ra khi chặn");
    }
  };

  const handleUnfriend = async (targetUserId: string) => {
    if (!window.confirm("Bạn có chắc chắn muốn hủy kết bạn?")) return;

    try {
      // Dùng chung API removeFriendship cho cả Hủy kết bạn, Hủy lời mời, Từ chối
      await friendService.removeFriendship(
        targetUserId,
        FriendshipAction.Unfriend,
      );
      toast.info("Đã hủy kết bạn");
      fetchFriends();
    } catch (error) {
      toast.error("Có lỗi xảy ra");
    }
  };

  // Tính năng xịn xò: Bấm vào bạn bè là mở phòng chat
  const handleChat = async (targetUserId: string) => {
    try {
      const response = await chatService.createPrivateChat(targetUserId);
      navigate(`/chat/${response.data?.id}`);
    } catch (error) {
      toast.error("Không thể mở phòng chat");
    }
  };

  if (loading)
    return <Loader2 className="animate-spin text-blue-500 mx-auto mt-10" />;

  if (friends.length === 0) {
    return (
      <div className="text-center text-gray-500 mt-10">
        Bạn chưa có người bạn nào. Hãy tìm kiếm và kết bạn nhé!
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
      {friends.map((friend) => (
        <div
          key={friend.friendshipId}
          className="bg-white p-4 rounded-xl shadow-sm border border-gray-100 flex items-center gap-4 hover:shadow-md transition-shadow"
        >
          <img
            src={
              friend.avatarUrl ||
              `https://ui-avatars.com/api/?name=${friend.fullName}`
            }
            alt="Avatar"
            className="w-14 h-14 rounded-full object-cover border"
          />
          <div className="flex-1 min-w-0">
            <h4 className="font-bold text-gray-800 truncate">
              {friend.fullName}
            </h4>
            <p className="text-xs text-gray-500 truncate">{friend.email}</p>
          </div>

          <div className="flex gap-2">
            <button
              onClick={() => handleChat(friend.userId)}
              className="p-2 bg-blue-50 text-blue-600 rounded-full hover:bg-blue-100 transition-colors"
              title="Nhắn tin"
            >
              <MessageSquare size={18} />
            </button>
            <button
              onClick={() => handleUnfriend(friend.userId)}
              className="p-2 bg-gray-50 text-gray-600 rounded-full hover:bg-gray-200 transition-colors"
              title="Hủy kết bạn"
            >
              <UserMinus size={18} />
            </button>
            <button
              onClick={() => handleBlock(friend.userId)}
              className="p-2 bg-red-50 text-red-600 rounded-full hover:bg-red-100 transition-colors"
              title="Chặn"
            >
              <Ban size={18} />
            </button>
          </div>
        </div>
      ))}
    </div>
  );
}
