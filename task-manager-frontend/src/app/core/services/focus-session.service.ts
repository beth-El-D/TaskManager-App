import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface FocusSession {
    id: string;
    taskId?: string;
    startTime: Date;
    endTime?: Date;
    duration: number;
    isCompleted: boolean;
    userId: string;
    createdAt: Date;
}

export interface CreateFocusSessionDto {
    taskId?: string;
    duration: number;
}

export interface CompleteFocusSessionDto {
    endTime: Date;
}

@Injectable({
    providedIn: 'root'
})
export class FocusSessionService {
    private apiUrl = 'http://localhost:5118/api/focussessions';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    private getHeaders() {
        const token = this.authService.getToken();
        return { 'Authorization': `Bearer ${token}` };
    }

    getSessions(): Observable<FocusSession[]> {
        return this.http.get<FocusSession[]>(this.apiUrl, { headers: this.getHeaders() });
    }

    getSession(id: string): Observable<FocusSession> {
        return this.http.get<FocusSession>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
    }

    startSession(session: CreateFocusSessionDto): Observable<FocusSession> {
        return this.http.post<FocusSession>(this.apiUrl, session, { headers: this.getHeaders() });
    }

    completeSession(id: string, data: CompleteFocusSessionDto): Observable<FocusSession> {
        return this.http.put<FocusSession>(`${this.apiUrl}/${id}/complete`, data, { headers: this.getHeaders() });
    }

    deleteSession(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
    }

    getTodaySessions(): Observable<FocusSession[]> {
        return this.http.get<FocusSession[]>(`${this.apiUrl}/today`, { headers: this.getHeaders() });
    }
}
