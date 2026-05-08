import { CommentDto } from './comment.models';

export interface PostMemberDto {
  userId: string;
  username: string;
  invitedAt: string;
}

export interface PostSummaryDto {
  id: number;
  title: string;
  authorUsername: string;
  categoryName: string;
  isPrivate: boolean;
  commentCount: number;
  createdAt: string;
}

export interface PostDto {
  id: number;
  title: string;
  content: string;
  authorId: string;
  authorUsername: string;
  categoryId: number;
  categoryName: string;
  isPrivate: boolean;
  isRestricted: boolean;
  comments: CommentDto[];
  members: PostMemberDto[];
  createdAt: string;
  updatedAt?: string;
}

export interface CreatePostRequest {
  title: string;
  content: string;
  categoryId: number;
  isPrivate?: boolean;
}

export interface UpdatePostRequest {
  title: string;
  content: string;
  categoryId: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface InviteUserRequest {
  username: string;
}
