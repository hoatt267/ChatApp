import { create } from "zustand";

export interface OnlineUser {
  userId: string;
  fullName: string;
  avatarUrl: string;
}

interface ChatState {
  onlineUsers: OnlineUser[];
  typingUsers: Record<string, string[]>;

  // Setters
  setOnlineUsers: (users: OnlineUser[]) => void;
  addOnlineUser: (user: OnlineUser) => void;
  removeOnlineUser: (userId: string) => void;
  setTyping: (
    conversationId: string,
    userId: string,
    isTyping: boolean,
  ) => void;
  clearChatState: () => void;
}

export const useChatStore = create<ChatState>((set) => ({
  onlineUsers: [],
  typingUsers: {},

  setOnlineUsers: (users) => set({ onlineUsers: users }),

  addOnlineUser: (newUser) =>
    set((state) => ({
      onlineUsers: [
        ...state.onlineUsers.filter((u) => u.userId !== newUser.userId),
        newUser,
      ],
    })),

  removeOnlineUser: (offlineUserId) =>
    set((state) => ({
      onlineUsers: state.onlineUsers.filter((u) => u.userId !== offlineUserId),
    })),

  setTyping: (conversationId, userId, isTyping) =>
    set((state) => {
      const currentTyping = state.typingUsers[conversationId] || [];
      const newTyping = isTyping
        ? [...new Set([...currentTyping, userId])]
        : currentTyping.filter((id) => id !== userId);

      return {
        typingUsers: {
          ...state.typingUsers,
          [conversationId]: newTyping,
        },
      };
    }),

  clearChatState: () => set({ onlineUsers: [], typingUsers: {} }),
}));
