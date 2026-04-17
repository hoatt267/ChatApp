import { create } from "zustand";
import * as signalR from "@microsoft/signalr";
import { APP_CONFIG } from "../../../config";
import { useAuthStore } from "../../auth/store/useAuthStore";
import { toast } from "react-toastify";

export interface OnlineUser {
  userId: string;
  fullName: string;
  avatarUrl: string;
}

interface ChatState {
  connection: signalR.HubConnection | null;
  onlineUsers: OnlineUser[];

  connect: () => void;
  disconnect: () => void;

  typingUsers: Record<string, string[]>;
}

export const useChatStore = create<ChatState>((set, get) => ({
  connection: null,
  onlineUsers: [],

  connect: () => {
    // Nếu đã có kết nối rồi hoặc đang tạo thì không tạo lại nữa
    if (get().connection) return;

    //  Lấy token từ Auth Store để nhét vào WebSocket
    const token = useAuthStore.getState().accessToken;
    if (!token) return;

    // 1. Khởi tạo đường ống kết nối
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(APP_CONFIG.SIGNALR_URL, {
        accessTokenFactory: () => token,
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Error)
      .build();

    set({ connection: newConnection });

    // 2. Lắng nghe các sự kiện từ ChatHub.cs bên Backend

    newConnection.on("GetOnlineUsers", (users: OnlineUser[]) => {
      set({ onlineUsers: users });
    });

    // Khi có một người MỚI vừa online
    newConnection.on("UserIsOnline", (newUser: OnlineUser) => {
      set((state) => ({
        // Thêm người mới vào danh sách
        onlineUsers: [
          ...state.onlineUsers.filter((u) => u.userId !== newUser.userId),
          newUser,
        ],
      }));
    });

    // Khi có một người vừa tắt tab/offline
    newConnection.on("UserIsOffline", (offlineUserId: string) => {
      set((state) => ({
        onlineUsers: state.onlineUsers.filter(
          (u) => u.userId !== offlineUserId,
        ),
      }));
    });

    // 3. Bắt đầu chạy kết nối
    newConnection
      .start()
      .then(() => {
        console.log("🟢 BẮT ĐẦU KẾT NỐI SIGNALR THÀNH CÔNG!");
        set({ connection: newConnection });
      })
      .catch((err) => {
        console.error("🔴 LỖI KẾT NỐI SIGNALR:", err);
        set({ connection: null });
      });

    // Lắng nghe sự kiện "UserTyping" để cập nhật trạng thái gõ chữ của người dùng
    newConnection.on(
      "UserTyping",
      (conversationId: string, userId: string, isTyping: boolean) => {
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
        });
      },
    );

    //  Lắng nghe: Có người gửi lời mời kết bạn
    newConnection.on(
      "ReceiveFriendRequest",
      (data: {
        userId: string;
        fullName: string;
        avatarUrl: string;
        message: string;
      }) => {
        console.log("🔔 CÓ LỜI MỜI KẾT BẠN MỚI:", data);
        // Tại đây bạn có thể dùng thư viện react-toastify hoặc react-hot-toast để bật popup lên:
        toast.info(data.message);

        // (Tùy chọn) Có thể lưu vào 1 mảng notifications trong store để hiện chấm đỏ ở Navbar
      },
    );

    //  Lắng nghe: Lời mời kết bạn đã được đồng ý
    newConnection.on(
      "FriendRequestAccepted",
      (data: { userId: string; fullName: string; message: string }) => {
        console.log("✅ ĐÃ TRỞ THÀNH BẠN BÈ:", data);
        toast.success(data.message);

        // (Tùy chọn) Cập nhật lại danh sách bạn bè online nếu cần
      },
    );
  },

  disconnect: () => {
    const { connection } = get();
    if (connection) {
      connection.stop();
      set({ connection: null, onlineUsers: [] });
      console.log("⚪ ĐÃ NGẮT KẾT NỐI SIGNALR");
    }
  },

  typingUsers: {},
}));
