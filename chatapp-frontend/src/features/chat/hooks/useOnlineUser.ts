import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { chatService } from "../services/chat.service";
import type { UserResponse } from "../../auth/types";
import type { OnlineUser } from "../store/useChatStore";

export const useOnlineUsers = (
  user: UserResponse | null,
  onlineUsers: OnlineUser[],
) => {
  const navigate = useNavigate();
  const otherOnlineUsers = onlineUsers.filter((u) => u.userId !== user?.id);

  const [creatingChatId, setCreatingChatId] = useState<string | null>(null);
  const [isGroupMode, setIsGroupMode] = useState(false);
  const [groupTitle, setGroupTitle] = useState("");
  const [selectedUsers, setSelectedUsers] = useState<string[]>([]);
  const [isCreatingGroup, setIsCreatingGroup] = useState(false);

  const handleStartChat = async (targetUserId: string) => {
    if (creatingChatId || isGroupMode) return;
    try {
      setCreatingChatId(targetUserId);
      const response = await chatService.createPrivateChat(targetUserId);
      navigate(`/chat/${response.data?.id}`);
    } catch (error) {
      console.error("Lỗi khi tạo/lấy phòng chat:", error);
      alert("Không thể bắt đầu cuộc trò chuyện. Vui lòng thử lại sau.");
    } finally {
      setCreatingChatId(null);
    }
  };

  const toggleUserSelection = (userId: string) => {
    setSelectedUsers((prev) =>
      prev.includes(userId)
        ? prev.filter((id) => id !== userId)
        : [...prev, userId],
    );
  };

  const handleCreateGroup = async () => {
    if (!groupTitle.trim() || selectedUsers.length < 2) return;
    setIsCreatingGroup(true);
    try {
      const response = await chatService.createGroupChat(
        groupTitle.trim(),
        selectedUsers,
      );
      setIsGroupMode(false);
      setGroupTitle("");
      setSelectedUsers([]);
      navigate(`/chat/${response.data?.id}`);
    } catch {
      alert("Tạo nhóm thất bại!");
    } finally {
      setIsCreatingGroup(false);
    }
  };

  const handleCancelGroupMode = () => {
    setIsGroupMode(false);
    setGroupTitle("");
    setSelectedUsers([]);
  };

  return {
    otherOnlineUsers,
    creatingChatId,
    isGroupMode,
    setIsGroupMode,
    groupTitle,
    setGroupTitle,
    selectedUsers,
    isCreatingGroup,
    handleStartChat,
    toggleUserSelection,
    handleCreateGroup,
    handleCancelGroupMode,
  };
};
