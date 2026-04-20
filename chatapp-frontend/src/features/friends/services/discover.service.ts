import axiosClient from "../../../lib/axiosClient";
import type { ApiResponse, PaginatedList } from "../../../types";
import type { DiscoverUser } from "../types";

export const discoverService = {
  getSuggestions: async (pageNumber = 1, pageSize = 10) => {
    return await axiosClient.get<
      ApiResponse<PaginatedList<DiscoverUser>>,
      ApiResponse<PaginatedList<DiscoverUser>>
    >(`/friends/suggestions?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  },

  searchUsers: async (keyword: string, pageNumber = 1, pageSize = 20) => {
    return await axiosClient.get<
      ApiResponse<PaginatedList<DiscoverUser>>,
      ApiResponse<PaginatedList<DiscoverUser>>
    >(
      `/friends/search?keyword=${encodeURIComponent(
        keyword,
      )}&pageNumber=${pageNumber}&pageSize=${pageSize}`,
    );
  },
};
