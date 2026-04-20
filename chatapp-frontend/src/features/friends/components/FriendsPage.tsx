import { useState } from "react";
import { Users, UserPlus, Compass } from "lucide-react";
import FriendList from "./FriendList";
import PendingList from "./PendingList";
import DiscoverTab from "./Discover/DiscoverTab";

export default function FriendsPage() {
  const [activeTab, setActiveTab] = useState<
    "friends" | "pending" | "discover"
  >("friends");

  return (
    <div className="flex-1 flex flex-col h-full bg-gray-50">
      {/* Header của trang */}
      <div className="h-16 border-b border-gray-200 flex items-center px-6 bg-white shadow-sm z-10 gap-6">
        <h2 className="font-bold text-xl text-gray-800 border-r pr-6">
          Bạn bè
        </h2>

        {/* Tabs */}
        <div className="flex gap-2">
          <button
            onClick={() => setActiveTab("friends")}
            className={`flex items-center gap-2 px-4 py-2 rounded-lg font-medium transition-colors ${
              activeTab === "friends"
                ? "bg-blue-50 text-blue-600"
                : "text-gray-600 hover:bg-gray-100"
            }`}
          >
            <Users size={18} /> Danh sách bạn bè
          </button>
          <button
            onClick={() => setActiveTab("pending")}
            className={`flex items-center gap-2 px-4 py-2 rounded-lg font-medium transition-colors ${
              activeTab === "pending"
                ? "bg-blue-50 text-blue-600"
                : "text-gray-600 hover:bg-gray-100"
            }`}
          >
            <UserPlus size={18} /> Lời mời kết bạn
          </button>
          <button
            onClick={() => setActiveTab("discover")}
            className={`flex items-center gap-2 px-4 py-2 rounded-lg font-medium transition-colors ${
              activeTab === "discover"
                ? "bg-blue-50 text-blue-600"
                : "text-gray-600 hover:bg-gray-100"
            }`}
          >
            <Compass size={18} /> Khám phá
          </button>
        </div>
      </div>

      {/* Nội dung Tab */}
      <div className="flex-1 overflow-y-auto p-6">
        <div className="max-w-4xl mx-auto">
          {activeTab === "friends" ? (
            <FriendList />
          ) : activeTab === "pending" ? (
            <PendingList />
          ) : (
            <DiscoverTab />
          )}
        </div>
      </div>
    </div>
  );
}
