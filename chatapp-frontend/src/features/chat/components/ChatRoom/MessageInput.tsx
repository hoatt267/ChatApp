import { Send } from "lucide-react";

interface MessageInputProps {
  value: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onSubmit: (e: React.FormEvent) => void;
}

export default function MessageInput({
  value,
  onChange,
  onSubmit,
}: MessageInputProps) {
  return (
    <div className="h-[72px] bg-gray-50 flex items-center px-4 py-3 z-10">
      <form
        onSubmit={onSubmit}
        className="flex-1 flex gap-2 items-end max-w-4xl mx-auto w-full"
      >
        <input
          type="text"
          value={value}
          onChange={onChange}
          placeholder="Nhập tin nhắn..."
          className="flex-1 px-4 py-3 bg-white border border-gray-300 rounded-full focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <button
          type="submit"
          disabled={!value.trim()}
          className="p-3 bg-blue-500 text-white rounded-full hover:bg-blue-600"
        >
          <Send size={20} />
        </button>
      </form>
    </div>
  );
}
