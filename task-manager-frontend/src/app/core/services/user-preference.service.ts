import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface UserPreference {
    id: string;
    theme: string;
    defaultFocusDuration: number;
    notificationsEnabled: boolean;
    weeklyGoal: number;
    userId: string;
    createdAt: Date;
    updatedAt: Date;
}

export interface UpdateUserPreferenceDto {
    theme?: string;
    defaultFocusDuration?: number;
    notificationsEnabled?: boolean;
    weeklyGoal?: number;
}

@Injectable({
    providedIn: 'root'
})
export class UserPreferenceService {
    private apiUrl = 'http://localhost:5118/api/userpreferences';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    private getHeaders() {
        const token = this.authService.getToken();
        return { 'Authorization': `Bearer ${token}` };
    }

    getPreferences(): Observable<UserPreference> {
        return this.http.get<UserPreference>(this.apiUrl, { headers: this.getHeaders() });
    }

    updatePreferences(preferences: UpdateUserPreferenceDto): Observable<UserPreference> {
        return this.http.put<UserPreference>(this.apiUrl, preferences, { headers: this.getHeaders() });
    }
}
