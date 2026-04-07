import React, { useEffect, useState, useRef } from "react";
import { useParams } from "react-router-dom";
import { Send, Loader2, User as UserIcon } from "lucide-react";
import * as signalR from "@microsoft/signalr";

import { useAuthStore } from "../../../auth/store/useAuthStore";
import { useChatStore } from "../../store/useChatStore";
import { chatService } from "../../services/chat.service";
import type { Message } from "../../types";

const MESSAGES_PER_PAGE = 10;

export default function ChatRoom() {
  const { conversationId } = useParams<{ conversationId: string }>();
  const currentUser = useAuthStore((state) => state.user);
  const connection = useChatStore((state) => state.connection);

  const [messages, setMessages] = useState<Message[]>([]);
  const [newMessage, setNewMessage] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  // STATE PHÂN TRANG
  const [isLoadingMore, setIsLoadingMore] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [chatInfo, setChatInfo] = useState<{
    name: string;
    avatarUrl: string;
  } | null>(null);

  const messagesEndRef = useRef<HTMLDivElement>(null);
  const chatContainerRef = useRef<HTMLDivElement>(null);

  // 🌟 KHÓA BẢO VỆ (MUTEX LOCK) CHỐNG SPAM API KHI SCROLL
  const isLoadingRef = useRef(false);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    const fetchInitialData = async () => {
      if (!conversationId) return;
      setIsLoading(true);
      setHasMore(true);

      try {
        const convsRes = await chatService.getConversations();
        const currentConv = convsRes.data.find((c) => c.id === conversationId);

        if (currentConv) {
          if (currentConv.isGroup) {
            setChatInfo({
              name: currentConv.title || "Nhóm Chat",
              avatarUrl: "",
            });
          } else {
            const otherUser = currentConv.participants.find(
              (p) => p.userId !== currentUser?.id,
            );
            setChatInfo({
              name: otherUser?.fullName || "Người dùng ẩn danh",
              avatarUrl: otherUser?.avatarUrl || "",
            });
          }
        }

        const msgRes = await chatService.getMessages(
          conversationId,
          MESSAGES_PER_PAGE,
        );
        if (msgRes.data.length < MESSAGES_PER_PAGE) setHasMore(false);

        // Gán thẳng data vì BE đã Reverse rồi
        setMessages(msgRes.data);
      } catch (error) {
        console.error("Lỗi tải dữ liệu phòng chat:", error);
      } finally {
        setIsLoading(false);
        setTimeout(scrollToBottom, 100);
      }
    };

    fetchInitialData();
  }, [conversationId, currentUser?.id]);

  // 🌟 HÀM SCROLL CỰC KỲ CHẶT CHẼ
  const handleScroll = async () => {
    if (
      !chatContainerRef.current ||
      isLoadingRef.current ||
      !hasMore ||
      !conversationId
    )
      return;

    if (chatContainerRef.current.scrollTop === 0) {
      const oldestMessage = messages[0];
      if (!oldestMessage) return;

      // 1. Khóa lập tức để chặn các sự kiện scroll tiếp theo
      isLoadingRef.current = true;
      setIsLoadingMore(true);

      const scrollContainer = chatContainerRef.current;
      const previousScrollHeight = scrollContainer.scrollHeight;

      try {
        const res = await chatService.getMessages(
          conversationId,
          MESSAGES_PER_PAGE,
          oldestMessage.createdAt,
        );

        if (res.data.length < MESSAGES_PER_PAGE) setHasMore(false);

        const olderMessages = res.data;

        setMessages((prev) => {
          // 2. Tường lửa: Lọc sạch các tin nhắn trùng lặp (Phòng hờ BE code sai logic)
          const newMessages = olderMessages.filter(
            (oldMsg) => !prev.some((p) => p.id === oldMsg.id),
          );

          // Nếu API trả về data nhưng toàn tin trùng -> Báo hết tin để ép dừng vòng lặp vô tận
          if (newMessages.length === 0) {
            setHasMore(false);
            return prev;
          }

          return [...newMessages, ...prev];
        });

        // 3. Dùng requestAnimationFrame giúp UI không bị giật, mượt hơn setTimeout
        requestAnimationFrame(() => {
          scrollContainer.scrollTop =
            scrollContainer.scrollHeight - previousScrollHeight;
        });
      } catch (error) {
        console.error("Lỗi load thêm tin nhắn:", error);
      } finally {
        // 4. Mở khóa
        isLoadingRef.current = false;
        setIsLoadingMore(false);
      }
    }
  };

  // 🌟 FIX LỖI SIGNALR: PHẢI CHỜ CONNECTED MỚI JOIN PHÒNG
  useEffect(() => {
    if (!connection || !conversationId) return;

    let isMounted = true;

    const joinRoom = async () => {
      try {
        // Chờ đến khi ống nước được nối thông hoàn toàn
        while (connection.state === signalR.HubConnectionState.Connecting) {
          await new Promise((resolve) => setTimeout(resolve, 100));
        }

        if (
          connection.state === signalR.HubConnectionState.Connected &&
          isMounted
        ) {
          await connection.invoke("JoinConversation", conversationId);
        }
      } catch (err) {
        console.error("Lỗi Join phòng:", err);
      }
    };

    joinRoom();

    const handleReceiveMessage = (response: { data: Message }) => {
      const incomingMessage = response.data;
      // Dùng toLowerCase() an toàn để so khớp GUID giữa C# và JS
      if (
        incomingMessage.conversationId.toLowerCase() ===
        conversationId.toLowerCase()
      ) {
        setMessages((prev) => [...prev, incomingMessage]);
        setTimeout(scrollToBottom, 100);
      }
    };

    connection.on("ReceiveMessage", handleReceiveMessage);

    return () => {
      isMounted = false;
      connection.off("ReceiveMessage", handleReceiveMessage);
    };
  }, [connection, conversationId]);

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newMessage.trim() || !connection || !conversationId) return;

    try {
      await connection.invoke("SendMessage", conversationId, newMessage.trim());
      setNewMessage("");
    } catch (error) {
      console.error("Lỗi gửi tin nhắn:", error);
    }
  };

  if (isLoading) {
    return (
      <div className="flex-1 flex items-center justify-center bg-[#e5ddd5]">
        <Loader2 className="w-8 h-8 text-blue-500 animate-spin" />
      </div>
    );
  }

  return (
    <div className="flex-1 flex flex-col h-full bg-[#e5ddd5] relative">
      <div className="h-16 border-b border-gray-200 flex items-center px-4 bg-white shadow-sm z-10 gap-3">
        {chatInfo?.avatarUrl ? (
          <img
            src={chatInfo.avatarUrl}
            alt="Avatar"
            className="w-10 h-10 rounded-full object-cover border border-gray-200"
          />
        ) : (
          <div className="w-10 h-10 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center font-bold">
            {chatInfo?.name.charAt(0).toUpperCase() || <UserIcon size={20} />}
          </div>
        )}
        <div>
          <h2 className="font-semibold text-lg text-gray-800">
            {chatInfo?.name || "Đang tải..."}
          </h2>
        </div>
      </div>

      <div
        ref={chatContainerRef}
        onScroll={handleScroll}
        className="flex-1 overflow-y-auto p-4 space-y-4 custom-scrollbar"
      >
        {isLoadingMore && (
          <div className="flex justify-center py-2">
            <Loader2 className="w-5 h-5 text-blue-500 animate-spin" />
          </div>
        )}

        {messages.length === 0 ? (
          <div className="text-center text-gray-500 bg-white/60 py-2 px-4 rounded-full w-max mx-auto shadow-sm text-sm">
            Hãy là người đầu tiên gửi tin nhắn!
          </div>
        ) : (
          messages.map((msg) => {
            const isMine = msg.senderId === currentUser?.id;
            return (
              <div
                key={msg.id}
                className={`flex ${isMine ? "justify-end" : "justify-start"}`}
              >
                <div
                  className={`max-w-[70%] flex gap-2 ${isMine ? "flex-row-reverse" : "flex-row"}`}
                >
                  {!isMine && (
                    <img
                      src={
                        msg.senderAvatarUrl || "https://via.placeholder.com/150"
                      }
                      alt="avatar"
                      className="w-8 h-8 rounded-full object-cover flex-shrink-0 mt-1"
                    />
                  )}
                  <div
                    className={`px-4 py-2 rounded-2xl shadow-sm ${
                      isMine
                        ? "bg-blue-500 text-white rounded-tr-sm"
                        : "bg-white text-gray-800 rounded-tl-sm border border-gray-100"
                    }`}
                  >
                    {!isMine && (
                      <p className="text-xs text-blue-600 font-bold mb-1">
                        {msg.senderName}
                      </p>
                    )}
                    <p className="text-sm break-words whitespace-pre-wrap">
                      {msg.content}
                    </p>
                    <p
                      className={`text-[10px] text-right mt-1 ${isMine ? "text-blue-100" : "text-gray-400"}`}
                    >
                      {new Date(msg.createdAt).toLocaleTimeString([], {
                        hour: "2-digit",
                        minute: "2-digit",
                      })}
                    </p>
                  </div>
                </div>
              </div>
            );
          })
        )}
        <div ref={messagesEndRef} />
      </div>

      <div className="h-[72px] bg-gray-50 flex items-center px-4 py-3 z-10">
        <form
          onSubmit={handleSendMessage}
          className="flex-1 flex gap-2 items-end max-w-4xl mx-auto w-full"
        >
          <input
            type="text"
            value={newMessage}
            onChange={(e) => setNewMessage(e.target.value)}
            placeholder="Nhập tin nhắn..."
            className="flex-1 px-4 py-3 bg-white border border-gray-300 rounded-full focus:outline-none focus:ring-2 focus:ring-blue-500 transition-shadow shadow-sm"
          />
          <button
            type="submit"
            disabled={!newMessage.trim()}
            className="p-3 bg-blue-500 text-white rounded-full hover:bg-blue-600 disabled:bg-gray-300 transition-colors shadow-sm flex-shrink-0"
          >
            <Send size={20} />
          </button>
        </form>
      </div>
    </div>
  );
}
