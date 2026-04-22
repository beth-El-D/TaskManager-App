import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface Category {
    id: string;
    name: string;
    description?: string;
    color: string;
    icon?: string;
    userId: string;
    createdAt: Date;
}

export interface CreateCategoryDto {
    name: string;
    description?: string;
    color: string;
    icon?: string;
}

@Injectable({
    providedIn: 'root'
})
export class CategoryService {
    private apiUrl = 'http://localhost:5118/api/categories';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    private getHeaders() {
        const token = this.authService.getToken();
        return { 'Authorization': `Bearer ${token}` };
    }

    getCategories(): Observable<Category[]> {
        return this.http.get<Category[]>(this.apiUrl, { headers: this.getHeaders() });
    }

    getCategory(id: string): Observable<Category> {
        return this.http.get<Category>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
    }

    createCategory(category: CreateCategoryDto): Observable<Category> {
        return this.http.post<Category>(this.apiUrl, category, { headers: this.getHeaders() });
    }

    updateCategory(id: string, category: Partial<CreateCategoryDto>): Observable<Category> {
        return this.http.put<Category>(`${this.apiUrl}/${id}`, category, { headers: this.getHeaders() });
    }

    deleteCategory(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
    }
}
