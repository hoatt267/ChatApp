export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: Record<string, string[]>;
}

export interface PaginatedList<T> {
  items: T[];
  count: number;
  pageNumber: number;
  pageSize: number;
}
