import { useState, useRef, useCallback } from "react";
import { chatService } from "../services/chat.service";
import { MessageType, type Message, type Participant } from "../types";
import type { UserResponse } from "../../auth/types";
import { APP_CONFIG } from "../../../config";

export const useChatMessages = (
  conversationId: string | undefined,
  currentUser: UserResponse | null,
  scrollToBottom: () => void,
) => {
  const [messages, setMessages] = useState<Message[]>([]);
  const [participants, setParticipants] = useState<Participant[]>([]);
  const [chatInfo, setChatInfo] = useState<{
    name: string;
    avatarUrl: string;
    isBlocked?: boolean;
  } | null>(null);

  const [isLoading, setIsLoading] = useState(true);
  const [isLoadingMore, setIsLoadingMore] = useState(false);
  const [hasMore, setHasMore] = useState(true);

  const isLoadingRef = useRef(false);

  const markAsBlocked = useCallback(() => {
    setChatInfo((prev) => (prev ? { ...prev, isBlocked: true } : null));
  }, []);

  const fetchInitialData = useCallback(async () => {
    if (!conversationId) return;
    setIsLoading(true);
    setHasMore(true);
    try {
      const convsRes = await chatService.getConversationById(conversationId);
      const currentConv = convsRes.data;

      if (currentConv) {
        setParticipants(currentConv.participants);
        const otherUser = currentConv.participants.find(
          (p) => p.userId !== currentUser?.id,
        );
        setChatInfo({
          name: currentConv.isGroup
            ? currentConv.title || "Nhóm Chat"
            : otherUser?.fullName || "Người dùng",
          avatarUrl: currentConv.isGroup ? "" : otherUser?.avatarUrl || "",
          isBlocked: currentConv.isBlocked || false,
        });
      }

      const msgRes = await chatService.getMessages(
        conversationId,
        APP_CONFIG.MESSAGES_PER_PAGE,
      );
      if (msgRes.data.length < APP_CONFIG.MESSAGES_PER_PAGE) setHasMore(false);
      setMessages(msgRes.data);
    } catch (error) {
      console.error("Lỗi fetch data:", error);
    } finally {
      setIsLoading(false);
      setTimeout(scrollToBottom, 100);
    }
  }, [conversationId, currentUser?.id, scrollToBottom]);

  const loadMoreMessages = async (
    chatContainerRef: React.RefObject<HTMLDivElement | null>,
  ) => {
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

      isLoadingRef.current = true;
      setIsLoadingMore(true);
      const previousScrollHeight = chatContainerRef.current.scrollHeight;

      try {
        const res = await chatService.getMessages(
          conversationId,
          APP_CONFIG.MESSAGES_PER_PAGE,
          oldestMessage.createdAt,
        );
        if (res.data.length < APP_CONFIG.MESSAGES_PER_PAGE) setHasMore(false);
        setMessages((prev) => {
          const newMsgs = res.data.filter(
            (m) => !prev.some((p) => p.id === m.id),
          );
          return [...newMsgs, ...prev];
        });

        requestAnimationFrame(() => {
          if (chatContainerRef.current) {
            chatContainerRef.current.scrollTop =
              chatContainerRef.current.scrollHeight - previousScrollHeight;
          }
        });
      } finally {
        isLoadingRef.current = false;
        setIsLoadingMore(false);
      }
    }
  };

  const handleSendMedia = async (file: File, content?: string) => {
    if (!conversationId || !currentUser) return;

    const msgType = file.type.startsWith("video/")
      ? MessageType.Video
      : file.type.startsWith("image/")
        ? MessageType.Image
        : MessageType.Document;

    const tempId = `temp-${Date.now()}`;
    const tempMsg: Message = {
      id: tempId,
      conversationId,
      senderId: currentUser.id,
      senderName: currentUser.fullName || "",
      senderAvatarUrl: currentUser.avatarUrl || "",
      content: content || "",
      type: msgType,
      fileUrl: URL.createObjectURL(file),
      fileName: file.name,
      createdAt: new Date().toISOString(),
      readBy: [],
      isOptimistic: true,
      progress: 0,
    };

    setMessages((prev) => [...prev, tempMsg]);
    setTimeout(scrollToBottom, 100);

    try {
      const res = await chatService.uploadMedia(
        conversationId,
        file,
        content,
        (progressEvent) => {
          const percentCompleted = Math.round(
            (progressEvent.loaded * 100) / (progressEvent.total || file.size),
          );
          setMessages((prev) =>
            prev.map((msg) =>
              msg.id === tempId ? { ...msg, progress: percentCompleted } : msg,
            ),
          );
        },
      );

      const realMsg = res.data;
      setMessages((prev) => {
        const isAlreadyAddedBySignalR = prev.some((m) => m.id === realMsg.id);
        return isAlreadyAddedBySignalR
          ? prev.filter((m) => m.id !== tempId)
          : prev.map((msg) => (msg.id === tempId ? realMsg : msg));
      });
    } catch (error) {
      console.error("Lỗi upload file:", error);
      setMessages((prev) => prev.filter((msg) => msg.id !== tempId));
      alert("Tải tệp lên thất bại");
    }
  };

  return {
    messages,
    setMessages,
    participants,
    chatInfo,
    isLoading,
    isLoadingMore,
    hasMore,
    fetchInitialData,
    loadMoreMessages,
    handleSendMedia,
    markAsBlocked,
  };
};
