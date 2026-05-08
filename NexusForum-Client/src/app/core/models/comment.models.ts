export interface CommentDto {
  id: number;
  content: string;
  authorId: string;
  authorUsername: string;
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
