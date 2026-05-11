export interface CommentDto {
  id: number;
  content: string;
  authorId: string;
  authorUsername: string;
  upCount: number;
  downCount: number;
  postId: number;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateCommentRequest {
  content: string;
}

export interface UpdateCommentRequest {
  content: string;
}
