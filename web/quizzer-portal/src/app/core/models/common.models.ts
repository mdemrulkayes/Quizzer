export interface PaginatedList<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ApiResult<T> {
  isSuccess: boolean;
  value: T;
  error: ApiError | null;
}

export interface ApiError {
  code: string;
  description: string;
}
