import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UpdateProfileRequest, UserProfileDto } from '../models/user.models';

@Injectable({ providedIn: 'root' })
export class UserService {
  private http = inject(HttpClient);

  getProfile(username: string): Observable<UserProfileDto> {
    return this.http.get<UserProfileDto>(`/api/users/${username}`);
  }

  updateProfile(req: UpdateProfileRequest): Observable<UserProfileDto> {
    return this.http.put<UserProfileDto>('/api/users/me', req);
  }
}
