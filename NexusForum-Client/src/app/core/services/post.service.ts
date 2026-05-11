import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreatePostRequest, InviteUserRequest, PagedResult, PostDto, PostMemberDto, PostSummaryDto, UpdatePostRequest } from '../models/post.models';

@Injectable({ providedIn: 'root' })
export class PostService {
  private http = inject(HttpClient);

  getPaged(page = 1, pageSize = 15, categoryId?: number): Observable<PagedResult<PostSummaryDto>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (categoryId) params = params.set('categoryId', categoryId);
    return this.http.get<PagedResult<PostSummaryDto>>('/api/posts', { params });
  }

  getById(id: number): Observable<PostDto> {
    return this.http.get<PostDto>(`/api/posts/${id}`);
  }

  create(req: CreatePostRequest): Observable<PostDto> {
    return this.http.post<PostDto>('/api/posts', req);
  }

  update(id: number, req: UpdatePostRequest): Observable<PostDto> {
    return this.http.put<PostDto>(`/api/posts/${id}`, req);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`/api/posts/${id}`);
  }

  getMembers(postId: number): Observable<PostMemberDto[]> {
    return this.http.get<PostMemberDto[]>(`/api/posts/${postId}/members`);
  }

  invite(postId: number, req: InviteUserRequest): Observable<PostMemberDto> {
    return this.http.post<PostMemberDto>(`/api/posts/${postId}/members`, req);
  }

  removeMember(postId: number, userId: string): Observable<void> {
    return this.http.delete<void>(`/api/posts/${postId}/members/${userId}`);
  }

  generateInviteLink(postId: number): Observable<{ token: string }> {
    return this.http.post<{ token: string }>(`/api/posts/${postId}/invite-link`, {});
  }

  redeemInvite(token: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`/api/invites/${token}/redeem`, {});
  }

  reactToComment(commentId: number, reactionType: 'up' | 'down'): Observable<{ upCount: number; downCount: number; myReaction: string | null }> {
    return this.http.post<{ upCount: number; downCount: number; myReaction: string | null }>(`/api/comments/${commentId}/react`, { reactionType });
  }
}
