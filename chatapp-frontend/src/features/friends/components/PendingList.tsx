import { useEffect, useState } from "react";
import { Loader2, Check, X } from "lucide-react";
import { toast } from "react-toastify";
import { friendService } from "../services/friend.service";
import { FriendshipAction, type FriendProfile } from "../types";
import { useFriendStore } from "../store/useFriendStore";

export default function PendingList() {
  const refreshTrigger = useFriendStore((state) => state.refreshTrigger);
  const [requests, setRequests] = useState<FriendProfile[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchPending = async () => {
    try {
      const res = await friendService.getPendingRequests();
      setRequests(res.data);
    } catch (error) {
      toast.error("Lỗi khi tải danh sách lời mời");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchPending();
  }, [refreshTrigger]);

  const handleAccept = async (friendshipId: string) => {
    try {
      await friendService.acceptRequest(friendshipId);
      toast.success("Đã chấp nhận kết bạn!");
      fetchPending(); // Tải lại danh sách
    } catch (error) {
      toast.error("Có lỗi xảy ra khi chấp nhận");
    }
  };

  const handleRemove = async (targetUserId: string, isSender: boolean) => {
    try {
      await friendService.removeFriendship(
        targetUserId,
        isSender ? FriendshipAction.Cancel : FriendshipAction.Reject,
      );
      toast.info(isSender ? "Đã hủy lời mời kết bạn" : "Đã từ chối lời mời");
      fetchPending();
    } catch (error) {
      toast.error("Có lỗi xảy ra");
    }
  };

  if (loading)
    return <Loader2 className="animate-spin text-blue-500 mx-auto mt-10" />;

  if (requests.length === 0) {
    return (
      <div className="text-center text-gray-500 mt-10">
        Không có lời mời kết bạn nào.
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
      {requests.map((req) => (
        <div
          key={req.friendshipId}
          className="bg-white p-4 rounded-xl shadow-sm border border-gray-100 flex items-center gap-4"
        >
          <img
            src={
              req.avatarUrl ||
              `https://ui-avatars.com/api/?name=${req.fullName}`
            }
            alt="Avatar"
            className="w-14 h-14 rounded-full object-cover border"
          />
          <div className="flex-1 min-w-0">
            <h4 className="font-bold text-gray-800 truncate">{req.fullName}</h4>
            <p className="text-xs text-gray-500 truncate">
              {req.isRequester
                ? "Bạn đã gửi lời mời"
                : "Đã gửi lời mời cho bạn"}
            </p>
          </div>

          <div className="flex gap-2">
            {!req.isRequester ? (
              // Nút Chấp nhận & Từ chối (Cho người nhận)
              <>
                <button
                  onClick={() => handleAccept(req.friendshipId)}
                  className="p-2 bg-blue-100 text-blue-600 rounded-full hover:bg-blue-200"
                  title="Chấp nhận"
                >
                  <Check size={20} />
                </button>
                <button
                  onClick={() => handleRemove(req.userId, false)}
                  className="p-2 bg-gray-100 text-gray-600 rounded-full hover:bg-gray-200"
                  title="Từ chối"
                >
                  <X size={20} />
                </button>
              </>
            ) : (
              // Nút Hủy (Cho người gửi)
              <button
                onClick={() => handleRemove(req.userId, true)}
                className="px-3 py-1.5 bg-red-50 text-red-600 rounded-lg text-sm font-medium hover:bg-red-100"
              >
                Hủy lời mời
              </button>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}
