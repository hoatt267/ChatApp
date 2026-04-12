import {
  MessageType,
  type Message,
  type Participant,
  type ReadReceipt,
} from "../../types";
import type { UserResponse } from "../../../auth/types";

interface MessageItemProps {
  msg: Message;
  isMine: boolean;
  currentUser: UserResponse | null;
  participants: Participant[];
  displayAvatars: ReadReceipt[]; // Nhận danh sách avatar đã được tính toán từ cha
  onOpenSeenBy: (receipts: ReadReceipt[]) => void;
}

export default function MessageItem({
  msg,
  isMine,
  participants,
  displayAvatars,
  onOpenSeenBy,
}: MessageItemProps) {
  return (
    <div className={`flex flex-col ${isMine ? "items-end" : "items-start"}`}>
      <div
        className={`max-w-[70%] flex gap-2 ${isMine ? "flex-row-reverse" : "flex-row"}`}
      >
        {/* AVATAR NGƯỜI GỬI (Nếu không phải mình) */}
        {!isMine && (
          <img
            src={
              msg.senderAvatarUrl ||
              `https://ui-avatars.com/api/?name=${msg.senderName}`
            }
            alt="avatar"
            className="w-8 h-8 rounded-full object-cover flex-shrink-0 mt-1"
          />
        )}

        {/* BONG BÓNG CHAT */}
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

          {/* MEDIA & TEXT */}
          {msg.fileUrl ? (
            <div
              className={`flex flex-col gap-1 ${msg.isOptimistic ? "opacity-60 grayscale-[20%]" : ""}`}
            >
              {msg.type === MessageType.Video ? (
                <video
                  src={msg.fileUrl}
                  controls
                  className="max-w-[200px] md:max-w-xs rounded-lg object-contain bg-black"
                />
              ) : msg.type === MessageType.Image ? (
                <img
                  src={msg.fileUrl}
                  className="max-w-[200px] md:max-w-xs rounded-lg object-contain cursor-pointer"
                  alt=""
                />
              ) : (
                <a
                  href={msg.fileUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className={`flex items-center gap-2 p-2 rounded-lg underline text-sm break-all ${isMine ? "bg-blue-600 text-white" : "bg-gray-100 text-blue-600"}`}
                >
                  📎 {msg.fileName || "Tệp đính kèm"}
                </a>
              )}
              {msg.content && msg.content !== msg.fileName && (
                <p className="text-sm mt-1">{msg.content}</p>
              )}

              {/* THANH TIẾN TRÌNH UPLOAD */}
              {msg.isOptimistic && (
                <div className="w-full bg-gray-200 rounded-full h-1 mt-1 overflow-hidden">
                  <div
                    className="bg-blue-600 h-1 rounded-full transition-all duration-200"
                    style={{ width: `${msg.progress || 0}%` }}
                  ></div>
                </div>
              )}
            </div>
          ) : (
            <p className="text-sm break-words whitespace-pre-wrap">
              {msg.content}
            </p>
          )}

          {/* THỜI GIAN GỬI */}
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

      {/* WATERMARK AVATAR: Chỉ nằm bên góc phải dưới */}
      {displayAvatars.length > 0 && (
        <div
          className="flex items-center justify-end w-full gap-1 mt-1 cursor-pointer"
          onClick={() => onOpenSeenBy(displayAvatars)}
        >
          <div className="flex -space-x-1">
            {displayAvatars.slice(0, 3).map((receipt, idx) => {
              const reader = participants.find(
                (p) => p.userId === receipt.userId,
              );
              return (
                <img
                  key={receipt.userId}
                  src={
                    reader?.avatarUrl ||
                    `https://ui-avatars.com/api/?name=${reader?.fullName}`
                  }
                  alt={reader?.fullName}
                  title={`Đã xem bởi ${reader?.fullName}`}
                  className="w-3.5 h-3.5 rounded-full object-cover border border-[#e5ddd5] shadow-sm relative"
                  style={{ zIndex: 3 - idx }}
                />
              );
            })}
            {displayAvatars.length > 3 && (
              <div className="w-3.5 h-3.5 rounded-full bg-gray-300 border border-[#e5ddd5] text-[7px] font-bold flex items-center justify-center text-gray-700 shadow-sm relative">
                +{displayAvatars.length - 3}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
