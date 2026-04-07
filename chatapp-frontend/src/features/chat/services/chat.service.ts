import axiosClient from "../../../lib/axiosClient";
import type { ApiResponse } from "../../../types";
import type { Conversation, Message } from "../types";

export const chatService = {
  // 1. Lấy danh sách tất cả các cuộc trò chuyện của User hiện tại
  getConversations: async () => {
    return await axiosClient.get<
      ApiResponse<Conversation[]>,
      ApiResponse<Conversation[]>
    >("/conversations");
  },

  getMessages: async (conversationId: string, limit = 5, before?: string) => {
    const params = new URLSearchParams();
    params.append("limit", limit.toString());
    if (before) params.append("before", before);

    return await axiosClient.get<
      ApiResponse<Message[]>,
      ApiResponse<Message[]>
    >(`/conversations/${conversationId}/messages?${params.toString()}`);
  },

  // 3. Tạo hoặc lấy phòng chat 1-1
  createPrivateChat: async (targetUserId: string) => {
    return await axiosClient.post<
      ApiResponse<Conversation>,
      ApiResponse<Conversation>
    >("/conversations/private", `"${targetUserId}"`, {
      headers: {
        "Content-Type": "application/json",
      },
    });
  },

  // 4. Tạo phòng chat nhóm
  createGroupChat: async (title: string, targetUserIds: string[]) => {
    return await axiosClient.post<
      ApiResponse<Conversation>,
      ApiResponse<Conversation>
    >("/conversations/group", { title, targetUserIds });
  },
};
