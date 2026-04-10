import React, { useRef, useState } from "react";
import { Send, Image as ImageIcon, X, Loader2 } from "lucide-react";
import { APP_CONFIG } from "../../../../config";

interface MessageInputProps {
  value: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onSubmit: (e: React.FormEvent) => void;
  onSendMedia: (file: File, content?: string) => Promise<void>;
}

export default function MessageInput({
  value,
  onChange,
  onSubmit,
  onSendMedia,
}: MessageInputProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const textInputRef = useRef<HTMLInputElement>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [isUploading, setIsUploading] = useState(false);

  // Xử lý khi user chọn file
  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      if (file.size > APP_CONFIG.MAX_FILE_SIZE_MB * 1024 * 1024) {
        alert(
          `File quá lớn! Vui lòng chọn file dưới ${APP_CONFIG.MAX_FILE_SIZE_MB}MB.`,
        );
        handleClearFile();
        return;
      }
      setSelectedFile(file);
      setTimeout(() => textInputRef.current?.focus(), 0);
    }
  };

  const handlePaste = (e: React.ClipboardEvent<HTMLInputElement>) => {
    const items = e.clipboardData?.items;
    if (!items) return;

    for (let i = 0; i < items.length; i++) {
      if (items[i].type.indexOf("image") !== -1) {
        const file = items[i].getAsFile();
        if (file) {
          if (file.size > APP_CONFIG.MAX_FILE_SIZE_MB * 1024 * 1024) {
            alert(
              `Ảnh dán vào vượt quá giới hạn ${APP_CONFIG.MAX_FILE_SIZE_MB}MB!`,
            );
            return;
          }
          setSelectedFile(file);
          e.preventDefault(); // Ngăn không cho dán tên file/text rác vào ô input
          break; // Chỉ lấy ảnh đầu tiên
        }
      }
    }
  };

  // Hủy bỏ file đã chọn
  const handleClearFile = () => {
    setSelectedFile(null);
    if (fileInputRef.current) fileInputRef.current.value = "";
  };

  // Xử lý Gửi (Phân nhánh: Gửi File hoặc Gửi Text)
  const handleFormSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (selectedFile) {
      setIsUploading(true);
      try {
        await onSendMedia(selectedFile, value.trim()); // Gọi hàm upload với nội dung
        handleClearFile(); // Upload xong thì xóa file đã chọn
      } finally {
        setIsUploading(false);
      }
    } else if (value.trim()) {
      onSubmit(e); // Nếu không có file thì gửi text bình thường
    }
  };

  return (
    <div className="bg-gray-50 flex flex-col z-10 border-t border-gray-200">
      {/*  PREVIEW FILE TRƯỚC KHI GỬI */}
      {selectedFile && (
        <div className="px-4 pt-3 pb-1 flex items-center">
          <div className="relative inline-block">
            {selectedFile.type.startsWith("image/") ? (
              <img
                src={URL.createObjectURL(selectedFile)}
                alt="preview"
                className="h-20 w-20 object-cover rounded-lg border shadow-sm"
              />
            ) : selectedFile.type.startsWith("video/") ? (
              <video
                src={URL.createObjectURL(selectedFile)}
                className="h-20 w-20 object-cover rounded-lg border shadow-sm"
              />
            ) : (
              <div className="h-20 w-20 bg-gray-200 flex items-center justify-center rounded-lg text-xs text-gray-500 font-medium px-2 text-center truncate shadow-sm">
                {selectedFile.name}
              </div>
            )}
            <button
              type="button"
              onClick={handleClearFile}
              className="absolute -top-2 -right-2 bg-red-500 text-white rounded-full p-1 shadow-md hover:bg-red-600"
            >
              <X size={14} />
            </button>
          </div>
        </div>
      )}

      {/* KHU VỰC NHẬP LIỆU */}
      <div className="h-[72px] flex items-center px-4 py-3">
        <form
          onSubmit={handleFormSubmit}
          className="flex-1 flex gap-2 items-center max-w-4xl mx-auto w-full"
        >
          {/* Nút bấm gọi Input File ẩn */}
          <button
            type="button"
            onClick={() => fileInputRef.current?.click()}
            className="p-3 text-gray-500 hover:bg-gray-200 rounded-full transition-colors"
          >
            <ImageIcon size={22} />
          </button>
          <input
            type="file"
            ref={fileInputRef}
            onChange={handleFileSelect}
            className="hidden"
            accept="image/*, video/*, .pdf, .doc, .docx, .txt"
          />

          <input
            type="text"
            ref={textInputRef}
            onPaste={handlePaste}
            value={value}
            onChange={onChange}
            disabled={isUploading}
            placeholder={
              selectedFile ? "Thêm chú thích cho tệp..." : "Nhập tin nhắn..."
            }
            className="flex-1 px-4 py-3 bg-white border border-gray-300 rounded-full focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100 disabled:text-gray-500"
          />

          <button
            type="submit"
            disabled={(!value.trim() && !selectedFile) || isUploading}
            className="p-3 bg-blue-500 text-white rounded-full hover:bg-blue-600 disabled:bg-blue-300 flex-shrink-0"
          >
            {isUploading ? (
              <Loader2 size={20} className="animate-spin" />
            ) : (
              <Send size={20} />
            )}
          </button>
        </form>
      </div>
    </div>
  );
}
