import { useEffect, useState } from "react";
import { Loader2, Unlock } from "lucide-react";
import { toast } from "react-toastify";
import { friendService } from "../services/friend.service";
import { FriendshipAction, type FriendProfile } from "../types";
import { useFriendStore } from "../store/useFriendStore";

export default function BlockList() {
  const refreshTrigger = useFriendStore((state) => state.refreshTrigger);
  const [blockedUsers, setBlockedUsers] = useState<FriendProfile[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchBlockedUsers = async () => {
    try {
      const res = await friendService.getBlockedUsers();
      setBlockedUsers(res.data);
    } catch (error) {
      toast.error("Lỗi khi tải danh sách chặn");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchBlockedUsers();
  }, [refreshTrigger]);

  const handleUnblock = async (targetUserId: string) => {
    if (!window.confirm("Bạn có muốn bỏ chặn người này?")) return;
    try {
      await friendService.removeFriendship(
        targetUserId,
        FriendshipAction.Unfriend,
      );
      toast.success("Đã bỏ chặn người dùng.");
      fetchBlockedUsers();
    } catch (error) {
      toast.error("Có lỗi xảy ra khi bỏ chặn");
    }
  };

  if (loading)
    return <Loader2 className="animate-spin text-blue-500 mx-auto mt-10" />;

  if (blockedUsers.length === 0) {
    return (
      <div className="text-center text-gray-500 mt-10">
        Danh sách chặn trống.
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
      {blockedUsers.map((user) => (
        <div
          key={user.friendshipId}
          className="bg-white p-4 rounded-xl shadow-sm border border-gray-100 flex items-center gap-4 opacity-75"
        >
          <img
            src={
              user.avatarUrl ||
              `https://ui-avatars.com/api/?name=${user.fullName}`
            }
            alt="Avatar"
            className="w-14 h-14 rounded-full object-cover border grayscale"
          />
          <div className="flex-1 min-w-0">
            <h4 className="font-bold text-gray-800 truncate line-through">
              {user.fullName}
            </h4>
            <p className="text-xs text-red-500 truncate">Đã chặn</p>
          </div>
          <button
            onClick={() => handleUnblock(user.userId)}
            className="px-3 py-1.5 bg-gray-100 text-gray-700 rounded-lg text-sm font-medium hover:bg-gray-200 flex items-center gap-2"
          >
            <Unlock size={16} /> Bỏ chặn
          </button>
        </div>
      ))}
    </div>
  );
}
