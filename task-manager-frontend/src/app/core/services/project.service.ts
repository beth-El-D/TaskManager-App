import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface Project {
    id: string;
    name: string;
    description?: string;
    color: string;
    icon?: string;
    userId: string;
    createdAt: Date;
}

export interface CreateProjectDto {
    name: string;
    description?: string;
    color: string;
    icon?: string;
}

@Injectable({
    providedIn: 'root'
})
export class ProjectService {
    private apiUrl = 'http://localhost:5118/api/projects';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    private getHeaders() {
        const token = this.authService.getToken();
        return { 'Authorization': `Bearer ${token}` };
    }

    getProjects(): Observable<Project[]> {
        return this.http.get<Project[]>(this.apiUrl, { headers: this.getHeaders() });
    }

    getProject(id: string): Observable<Project> {
        return this.http.get<Project>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
    }

    createProject(project: CreateProjectDto): Observable<Project> {
        return this.http.post<Project>(this.apiUrl, project, { headers: this.getHeaders() });
    }

    updateProject(id: string, project: Partial<CreateProjectDto>): Observable<Project> {
        return this.http.put<Project>(`${this.apiUrl}/${id}`, project, { headers: this.getHeaders() });
    }

    deleteProject(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
    }
}
