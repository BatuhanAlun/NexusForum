export interface UserProfileDto {
  id: string;
  username: string;
  role: string;
  bio: string;
  avatarUrl?: string;
  createdAt: string;
  postCount: number;
  commentCount: number;
}

export interface UpdateProfileRequest {
  username: string;
  bio?: string;
  avatarUrl?: string;
}
