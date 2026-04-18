import axiosClient from "../../../lib/axiosClient";
import type { ApiResponse } from "../../../types";
import type {
  FriendshipAction,
  FriendProfile,
  FriendshipResponse,
} from "../types";

export const friendService = {
  // 1. Lấy danh sách bạn bè đã kết bạn
  getFriends: async () => {
    return await axiosClient.get<
      ApiResponse<FriendProfile[]>,
      ApiResponse<FriendProfile[]>
    >("/friends/accepted");
  },

  // 2. Lấy danh sách lời mời (cả gửi và nhận)
  getPendingRequests: async () => {
    return await axiosClient.get<
      ApiResponse<FriendProfile[]>,
      ApiResponse<FriendProfile[]>
    >("/friends/pending");
  },

  // 3. Gửi lời mời kết bạn
  sendRequest: async (targetUserId: string) => {
    return await axiosClient.post<
      ApiResponse<FriendshipResponse>,
      ApiResponse<FriendshipResponse>
    >("/friends/requests", { targetUserId });
  },

  // 4. Chấp nhận lời mời kết bạn
  acceptRequest: async (friendshipId: string) => {
    return await axiosClient.post<
      ApiResponse<FriendshipResponse>,
      ApiResponse<FriendshipResponse>
    >(`/friends/requests/${friendshipId}/accept`);
  },

  // 5. Xóa mối quan hệ (Hủy kết bạn, Hủy lời mời, Từ chối)
  removeFriendship: async (
    targetUserId: string,
    actionType: FriendshipAction,
  ) => {
    return await axiosClient.delete<ApiResponse<boolean>, ApiResponse<boolean>>(
      `/friends/${targetUserId}?actionType=${actionType}`,
    );
  },

  // 6. Block người dùng
  blockUser: async (targetUserId: string) => {
    return await axiosClient.post<ApiResponse<boolean>, ApiResponse<boolean>>(
      `/friends/block/${targetUserId}`,
    );
  },
};
