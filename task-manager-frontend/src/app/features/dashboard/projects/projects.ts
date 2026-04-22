import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProjectService, Project, CreateProjectDto } from '../../../core/services/project.service';
import { CategoryService, Category, CreateCategoryDto } from '../../../core/services/category.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
    selector: 'app-projects',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './projects.html',
    styleUrl: './projects.scss'
})
export class ProjectsComponent implements OnInit {
    projects: Project[] = [];
    categories: Category[] = [];

    showProjectModal = false;
    showCategoryModal = false;

    newProject: CreateProjectDto = {
        name: '',
        description: '',
        color: '#6366f1',
        icon: '📁'
    };

    newCategory: CreateCategoryDto = {
        name: '',
        description: '',
        color: '#10b981',
        icon: '🏷️'
    };

    loading = false;

    constructor(
        private projectService: ProjectService,
        private categoryService: CategoryService,
        private notification: NotificationService
    ) { }

    ngOnInit() {
        this.loadProjects();
        this.loadCategories();
    }

    async loadProjects() {
        try {
            this.loading = true;
            this.projects = await this.projectService.getProjects().toPromise() || [];
        } catch (error) {
            console.error('Error loading projects:', error);
            this.notification.showError('Failed to load projects');
        } finally {
            this.loading = false;
        }
    }

    async loadCategories() {
        try {
            this.categories = await this.categoryService.getCategories().toPromise() || [];
        } catch (error) {
            console.error('Error loading categories:', error);
            this.notification.showError('Failed to load categories');
        }
    }

    async createProject() {
        if (!this.newProject.name.trim()) {
            this.notification.showError('Please enter a project name');
            return;
        }

        try {
            await this.projectService.createProject(this.newProject).toPromise();
            this.notification.showSuccess('Project created successfully!');
            this.showProjectModal = false;
            this.resetProjectForm();
            this.loadProjects();
        } catch (error) {
            console.error('Error creating project:', error);
            this.notification.showError('Failed to create project');
        }
    }

    async createCategory() {
        if (!this.newCategory.name.trim()) {
            this.notification.showError('Please enter a category name');
            return;
        }

        try {
            await this.categoryService.createCategory(this.newCategory).toPromise();
            this.notification.showSuccess('Category created successfully!');
            this.showCategoryModal = false;
            this.resetCategoryForm();
            this.loadCategories();
        } catch (error) {
            console.error('Error creating category:', error);
            this.notification.showError('Failed to create category');
        }
    }

    async deleteProject(id: string) {
        if (confirm('Are you sure you want to delete this project?')) {
            try {
                await this.projectService.deleteProject(id).toPromise();
                this.notification.showSuccess('Project deleted successfully!');
                this.loadProjects();
            } catch (error) {
                console.error('Error deleting project:', error);
                this.notification.showError('Failed to delete project');
            }
        }
    }

    async deleteCategory(id: string) {
        if (confirm('Are you sure you want to delete this category?')) {
            try {
                await this.categoryService.deleteCategory(id).toPromise();
                this.notification.showSuccess('Category deleted successfully!');
                this.loadCategories();
            } catch (error) {
                console.error('Error deleting category:', error);
                this.notification.showError('Failed to delete category');
            }
        }
    }

    resetProjectForm() {
        this.newProject = {
            name: '',
            description: '',
            color: '#6366f1',
            icon: '📁'
        };
    }

    resetCategoryForm() {
        this.newCategory = {
            name: '',
            description: '',
            color: '#10b981',
            icon: '🏷️'
        };
    }

    openProjectModal() {
        this.resetProjectForm();
        this.showProjectModal = true;
    }

    openCategoryModal() {
        this.resetCategoryForm();
        this.showCategoryModal = true;
    }
}
