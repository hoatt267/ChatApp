import { Loader2, User as UserIcon, Users, Check, X } from "lucide-react";
import { useAuthStore } from "../../../features/auth/store/useAuthStore";
import { useChatStore } from "../../../features/chat/store/useChatStore";
import { useOnlineUsers } from "../../../features/chat/hooks/useOnlineUser";

export default function OnlineUsers() {
  const user = useAuthStore((state) => state.user);
  const onlineUsers = useChatStore((state) => state.onlineUsers);

  const {
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
  } = useOnlineUsers(user, onlineUsers);

  return (
    <div className="max-h-[40%] flex flex-col p-2 border-b border-gray-200">
      <div className="flex items-center justify-between px-3 pt-2 pb-2">
        <h3 className="text-xs font-semibold text-gray-500 uppercase tracking-wider">
          Đang hoạt động ({otherOnlineUsers.length})
        </h3>
        {!isGroupMode && otherOnlineUsers.length >= 2 && (
          <button
            onClick={() => setIsGroupMode(true)}
            className="text-blue-500 hover:text-blue-700 bg-blue-50 hover:bg-blue-100 p-1.5 rounded-md transition-colors flex items-center gap-1 text-xs font-medium"
          >
            <Users size={14} /> Tạo nhóm
          </button>
        )}
      </div>

      {isGroupMode && (
        <div className="px-3 pb-3 space-y-2 animate-in slide-in-from-top-2 duration-200">
          <input
            type="text"
            placeholder="Nhập tên nhóm..."
            value={groupTitle}
            onChange={(e) => setGroupTitle(e.target.value)}
            disabled={isCreatingGroup}
            className="w-full text-sm px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          <div className="flex items-center justify-between mt-2">
            <span className="text-xs text-gray-500">
              Đã chọn:{" "}
              <span className="font-bold text-blue-600">
                {selectedUsers.length}
              </span>{" "}
              người
            </span>
            <div className="flex items-center gap-2">
              <button
                onClick={handleCancelGroupMode}
                disabled={isCreatingGroup}
                className="p-1.5 text-gray-500 hover:bg-gray-200 rounded-md transition-colors"
              >
                <X size={16} />
              </button>
              <button
                onClick={handleCreateGroup}
                disabled={
                  isCreatingGroup ||
                  selectedUsers.length < 2 ||
                  !groupTitle.trim()
                }
                className="px-3 py-1.5 bg-blue-500 text-white text-xs font-medium rounded-md hover:bg-blue-600 disabled:bg-gray-300 disabled:cursor-not-allowed flex items-center gap-1"
              >
                {isCreatingGroup ? (
                  <Loader2 size={14} className="animate-spin" />
                ) : (
                  <Check size={14} />
                )}{" "}
                Tạo
              </button>
            </div>
          </div>
        </div>
      )}

      <div className="flex-1 overflow-y-auto custom-scrollbar">
        {otherOnlineUsers.length === 0 ? (
          <div className="text-center text-gray-400 text-sm mt-4 mb-4">
            Chưa có ai online
          </div>
        ) : (
          <div className="space-y-1">
            {otherOnlineUsers.map((onlineUser) => {
              const isSelected = selectedUsers.includes(onlineUser.userId);
              return (
                <div
                  key={onlineUser.userId}
                  onClick={() =>
                    isGroupMode
                      ? toggleUserSelection(onlineUser.userId)
                      : handleStartChat(onlineUser.userId)
                  }
                  className={`flex items-center gap-3 p-3 rounded-lg cursor-pointer transition-colors ${creatingChatId === onlineUser.userId ? "opacity-70 pointer-events-none" : ""} ${isGroupMode ? (isSelected ? "bg-blue-50 border border-blue-200" : "hover:bg-gray-100 border border-transparent") : "hover:bg-gray-100"}`}
                >
                  <div className="relative">
                    {onlineUser.avatarUrl ? (
                      <img
                        src={onlineUser.avatarUrl}
                        alt="Avatar"
                        className="w-12 h-12 rounded-full object-cover border border-gray-200"
                      />
                    ) : (
                      <div className="w-12 h-12 rounded-full bg-gray-200 text-gray-500 flex items-center justify-center">
                        <UserIcon size={24} />
                      </div>
                    )}
                    <div className="absolute bottom-0 right-0 w-3.5 h-3.5 bg-green-500 border-2 border-white rounded-full"></div>
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="font-semibold text-gray-800 truncate">
                      {onlineUser.fullName}
                    </p>
                    <p className="text-xs text-green-600 font-medium truncate">
                      Đang hoạt động
                    </p>
                  </div>
                  {isGroupMode && (
                    <div
                      className={`w-5 h-5 rounded-full border flex items-center justify-center ${isSelected ? "bg-blue-500 border-blue-500" : "border-gray-300"}`}
                    >
                      {isSelected && <Check size={12} className="text-white" />}
                    </div>
                  )}
                  {creatingChatId === onlineUser.userId && (
                    <Loader2 className="w-5 h-5 text-blue-500 animate-spin" />
                  )}
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}
