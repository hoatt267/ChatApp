import { UserIcon } from "lucide-react";

interface ChatHeaderProps {
  name: string;
  avatarUrl: string;
}

export default function ChatHeader({ name, avatarUrl }: ChatHeaderProps) {
  return (
    <div className="h-16 border-b border-gray-200 flex items-center px-4 bg-white shadow-sm z-10 gap-3">
      {avatarUrl ? (
        <img
          src={avatarUrl}
          alt="Avatar"
          className="w-10 h-10 rounded-full object-cover border border-gray-200"
        />
      ) : (
        <div className="w-10 h-10 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center font-bold">
          {name.charAt(0).toUpperCase() || <UserIcon size={20} />}
        </div>
      )}
      <h2 className="font-semibold text-lg text-gray-800">{name}</h2>
    </div>
  );
}
