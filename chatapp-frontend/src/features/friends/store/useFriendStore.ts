import { create } from "zustand";

interface FriendState {
  refreshTrigger: number;
  triggerRefresh: () => void;
}

// Bất cứ khi nào gọi triggerRefresh, con số này sẽ tăng lên 1
export const useFriendStore = create<FriendState>((set) => ({
  refreshTrigger: 0,
  triggerRefresh: () =>
    set((state) => ({ refreshTrigger: state.refreshTrigger + 1 })),
}));
