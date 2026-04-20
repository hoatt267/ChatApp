import { useState, useEffect, useCallback } from "react";
import { discoverService } from "../services/discover.service";
import type { DiscoverUser, FriendshipStatus } from "../types";
import { toast } from "react-toastify";
import { useFriendStore } from "../store/useFriendStore";

export const useDiscover = () => {
  const refreshTrigger = useFriendStore((state) => state.refreshTrigger);
  const [keyword, setKeyword] = useState("");
  const [debouncedKeyword, setDebouncedKeyword] = useState("");

  const [suggestions, setSuggestions] = useState<DiscoverUser[]>([]);
  const [searchResults, setSearchResults] = useState<DiscoverUser[]>([]);

  // State phân trang
  const [suggestionPage, setSuggestionPage] = useState(1);
  const [hasMoreSuggestions, setHasMoreSuggestions] = useState(true);
  const [loadingSuggestions, setLoadingSuggestions] = useState(true);

  // 1. Debounce
  useEffect(() => {
    const timer = setTimeout(() => setDebouncedKeyword(keyword), 500);
    return () => clearTimeout(timer);
  }, [keyword]);

  // 2. Fetch/Load More Gợi ý kết bạn
  const fetchSuggestions = useCallback(async (page = 1, isLoadMore = false) => {
    try {
      setLoadingSuggestions(true);
      const res = await discoverService.getSuggestions(page, 10);
      const newItems = res.data.items;

      if (isLoadMore) {
        setSuggestions((prev) => [...prev, ...newItems]);
      } else {
        setSuggestions(newItems);
      }

      setHasMoreSuggestions(newItems.length === 10);
    } catch (error) {
      toast.error("Không tải được danh sách gợi ý.");
    } finally {
      setLoadingSuggestions(false);
    }
  }, []);

  // Gọi lần đầu
  useEffect(() => {
    fetchSuggestions(1, false);
  }, [fetchSuggestions]);

  // 3. LẮNG NGHE REFRESH TRIGGER CỦA SIGNALR
  useEffect(() => {
    setSuggestionPage(1); // Reset về trang 1
    fetchSuggestions(1, false);
  }, [fetchSuggestions, refreshTrigger]);

  const loadMoreSuggestions = () => {
    const nextPage = suggestionPage + 1;
    setSuggestionPage(nextPage);
    fetchSuggestions(nextPage, true);
  };

  // 3. Fetch Kết quả tìm kiếm (Tạm thời chỉ lấy trang 1 cho Search)
  useEffect(() => {
    const fetchSearch = async () => {
      if (!debouncedKeyword.trim()) {
        setSearchResults([]);
        return;
      }
      try {
        setLoadingSuggestions(true); // Dùng chung hiệu ứng loading cho gọn
        const res = await discoverService.searchUsers(debouncedKeyword, 1, 20);
        setSearchResults(res.data.items);
      } catch (error) {
        toast.error("Lỗi khi tìm kiếm người dùng.");
      } finally {
        setLoadingSuggestions(false);
      }
    };
    fetchSearch();
  }, [debouncedKeyword, refreshTrigger]);

  //Update UI sau khi gửi lời mời hoặc mở chat
  const updateUserStatus = useCallback(
    (userId: string, status: FriendshipStatus) => {
      setSuggestions((prev) =>
        prev.map((user) =>
          user.userId === userId ? { ...user, friendshipStatus: status } : user,
        ),
      );
      setSearchResults((prev) =>
        prev.map((user) =>
          user.userId === userId ? { ...user, friendshipStatus: status } : user,
        ),
      );
    },
    [],
  );

  return {
    keyword,
    setKeyword,
    suggestions,
    searchResults,
    loadingSuggestions,
    isSearching: debouncedKeyword.trim().length > 0,
    hasMoreSuggestions,
    loadMoreSuggestions,
    updateUserStatus,
  };
};
