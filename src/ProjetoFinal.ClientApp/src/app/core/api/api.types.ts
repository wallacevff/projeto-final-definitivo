export interface ApiPageInfo {
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalItens: number;
}

export interface ApiPagedResponse<T> {
  dados?: T[];
  Dados?: T[];
  pageInfo?: ApiPageInfo;
  PageInfo?: ApiPageInfo;
}

export interface NormalizedPaged<T> {
  items: T[];
  pageInfo: ApiPageInfo;
}

export function normalizePagedResponse<T>(response: ApiPagedResponse<T>): NormalizedPaged<T> {
  const items = response.dados ?? response.Dados ?? [];
  const pageInfo = response.pageInfo ?? response.PageInfo ?? {
    pageNumber: 1,
    pageSize: items.length,
    totalPages: 1,
    totalItens: items.length
  };

  return { items, pageInfo };
}
