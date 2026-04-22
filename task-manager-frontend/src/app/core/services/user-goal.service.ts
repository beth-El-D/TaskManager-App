import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface UserGoal {
    id: string;
    title: string;
    description?: string;
    targetValue: number;
    currentValue: number;
    unit: string;
    deadline?: Date;
    isCompleted: boolean;
    userId: string;
    createdAt: Date;
}

export interface CreateUserGoalDto {
    title: string;
    description?: string;
    targetValue: number;
    currentValue?: number;
    unit: string;
    deadline?: Date;
}

export interface UpdateGoalProgressDto {
    currentValue: number;
}

@Injectable({
    providedIn: 'root'
})
export class UserGoalService {
    private apiUrl = 'http://localhost:5118/api/usergoals';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    private getHeaders() {
        const token = this.authService.getToken();
        return { 'Authorization': `Bearer ${token}` };
    }

    getGoals(): Observable<UserGoal[]> {
        return this.http.get<UserGoal[]>(this.apiUrl, { headers: this.getHeaders() });
    }

    getGoal(id: string): Observable<UserGoal> {
        return this.http.get<UserGoal>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
    }

    createGoal(goal: CreateUserGoalDto): Observable<UserGoal> {
        return this.http.post<UserGoal>(this.apiUrl, goal, { headers: this.getHeaders() });
    }

    updateGoal(id: string, goal: Partial<CreateUserGoalDto>): Observable<UserGoal> {
        return this.http.put<UserGoal>(`${this.apiUrl}/${id}`, goal, { headers: this.getHeaders() });
    }

    updateProgress(id: string, progress: UpdateGoalProgressDto): Observable<UserGoal> {
        return this.http.put<UserGoal>(`${this.apiUrl}/${id}/progress`, progress, { headers: this.getHeaders() });
    }

    deleteGoal(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
    }

    getActiveGoals(): Observable<UserGoal[]> {
        return this.http.get<UserGoal[]>(`${this.apiUrl}/active`, { headers: this.getHeaders() });
    }
}
