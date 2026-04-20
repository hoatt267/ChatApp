import { useState } from "react";
import { toast } from "react-toastify";
import { useNavigate } from "react-router-dom";
import { useDiscover } from "../../hooks/useDiscover";
import { friendService } from "../../services/friend.service";
import { chatService } from "../../../chat/services/chat.service";
import SearchBar from "./SearchBar";
import UserCard from "./UserCard";
import { FriendshipStatus } from "../../types";

export default function DiscoverTab() {
  const navigate = useNavigate();
  const {
    keyword,
    setKeyword,
    suggestions,
    searchResults,
    loadingSuggestions,
    isSearching,
    loadMoreSuggestions,
    hasMoreSuggestions,
    updateUserStatus,
  } = useDiscover();

  const [processingId, setProcessingId] = useState<string | null>(null);

  const handleAddFriend = async (targetUserId: string) => {
    try {
      setProcessingId(targetUserId);
      await friendService.sendRequest(targetUserId);
      toast.success("Đã gửi lời mời kết bạn!");
      updateUserStatus(targetUserId, FriendshipStatus.Pending);
    } catch (error) {
      toast.error("Không thể gửi lời mời kết bạn.");
    } finally {
      setProcessingId(null);
    }
  };

  const handleChat = async (targetUserId: string) => {
    try {
      const response = await chatService.createPrivateChat(targetUserId);
      navigate(`/chat/${response.data?.id}`);
    } catch (error) {
      toast.error("Không thể mở phòng chat");
    }
  };

  const displayUsers = isSearching ? searchResults : suggestions;

  return (
    <div className="w-full">
      <SearchBar
        value={keyword}
        onChange={setKeyword}
        isLoading={loadingSuggestions}
      />

      {!isSearching && (
        <h3 className="text-lg font-bold text-gray-800 mb-4">
          Những người bạn có thể biết
        </h3>
      )}

      {isSearching && keyword.trim() !== "" && (
        <h3 className="text-lg font-bold text-gray-800 mb-4">
          Kết quả tìm kiếm cho "{keyword}"
        </h3>
      )}

      {loadingSuggestions && displayUsers.length === 0 ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
          {[1, 2, 3, 4].map((n) => (
            <div
              key={n}
              className="bg-white rounded-xl shadow-sm border border-gray-100 h-56 animate-pulse"
            ></div>
          ))}
        </div>
      ) : displayUsers.length === 0 ? (
        <div className="text-center text-gray-500 py-10 bg-white rounded-xl border border-dashed border-gray-300">
          Không tìm thấy người dùng nào.
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {displayUsers.map((user) => (
              <UserCard
                key={user.userId}
                user={user}
                onAddFriend={handleAddFriend}
                onChat={handleChat}
                isProcessing={processingId === user.userId}
              />
            ))}
          </div>
          {!isSearching && hasMoreSuggestions && (
            <div className="mt-8 text-center">
              <button
                onClick={loadMoreSuggestions}
                disabled={loadingSuggestions}
                className="px-6 py-2 bg-white border border-gray-300 text-gray-700 font-medium rounded-full hover:bg-gray-50 transition-colors shadow-sm disabled:opacity-50"
              >
                {loadingSuggestions ? "Đang tải..." : "Xem thêm"}
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
