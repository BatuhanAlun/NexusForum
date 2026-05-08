import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CommentDto, CreateCommentRequest, UpdateCommentRequest } from '../models/comment.models';

@Injectable({ providedIn: 'root' })
export class CommentService {
  private http = inject(HttpClient);

  create(postId: number, req: CreateCommentRequest): Observable<CommentDto> {
    return this.http.post<CommentDto>(`/api/posts/${postId}/comments`, req);
  }

  update(id: number, req: UpdateCommentRequest): Observable<CommentDto> {
    return this.http.put<CommentDto>(`/api/comments/${id}`, req);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`/api/comments/${id}`);
  }
}
