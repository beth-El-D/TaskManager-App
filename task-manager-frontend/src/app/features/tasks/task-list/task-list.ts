import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ProjectService, Project } from '../../../core/services/project.service';
import { CategoryService, Category } from '../../../core/services/category.service';

interface Task {
  id: string;
  title: string;
  description?: string;
  priority: 'Low' | 'Medium' | 'High';
  status: number;
  dueDate: Date;
  isCompleted: boolean;
  createdAt: Date;
  completedAt?: Date;
  projectId?: string;
  categoryId?: string;
  projectName?: string;
  categoryName?: string;
  projectColor?: string;
  categoryColor?: string;
}

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './task-list.html',
  styleUrl: './task-list.scss',
})
export class TaskList implements OnInit {
  tasks: Task[] = [];
  filteredTasks: Task[] = [];
  projects: Project[] = [];
  categories: Category[] = [];

  loading = false;
  showCreateModal = false;
  showEditModal = false;

  // Filters
  searchQuery = '';
  filterStatus: 'all' | 'active' | 'completed' = 'all';
  filterPriority: 'all' | 'Low' | 'Medium' | 'High' = 'all';
  filterProject: string = 'all';
  filterCategory: string = 'all';
  sortBy: 'dueDate' | 'priority' | 'title' | 'createdAt' = 'dueDate';

  // New/Edit task
  currentTask: Partial<Task> = {};
  newTask = {
    title: '',
    description: '',
    priority: 'Medium' as 'Low' | 'Medium' | 'High',
    dueDate: new Date().toISOString().split('T')[0],
    projectId: null as string | null,
    categoryId: null as string | null
  };

  private apiUrl = 'http://localhost:5118/api';

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private notification: NotificationService,
    private projectService: ProjectService,
    private categoryService: CategoryService,
    private router: Router
  ) { }

  ngOnInit() {
    this.loadTasks();
    this.loadProjects();
    this.loadCategories();
  }

  async loadTasks() {
    try {
      this.loading = true;
      const token = this.authService.getToken();
      const headers = { 'Authorization': `Bearer ${token}` };

      const response = await this.http.get<Task[]>(`${this.apiUrl}/tasks`, { headers }).toPromise();

      if (response) {
        this.tasks = response.map(task => ({
          ...task,
          dueDate: new Date(task.dueDate),
          createdAt: new Date(task.createdAt),
          completedAt: task.completedAt ? new Date(task.completedAt) : undefined
        }));
        this.applyFilters();
      }
    } catch (error) {
      console.error('Error loading tasks:', error);
      this.notification.showError('Failed to load tasks');
    } finally {
      this.loading = false;
    }
  }

  async loadProjects() {
    try {
      this.projects = await this.projectService.getProjects().toPromise() || [];
    } catch (error) {
      console.error('Error loading projects:', error);
    }
  }

  async loadCategories() {
    try {
      this.categories = await this.categoryService.getCategories().toPromise() || [];
    } catch (error) {
      console.error('Error loading categories:', error);
    }
  }

  applyFilters() {
    let filtered = [...this.tasks];

    // Search filter
    if (this.searchQuery.trim()) {
      const query = this.searchQuery.toLowerCase();
      filtered = filtered.filter(task =>
        task.title.toLowerCase().includes(query) ||
        task.description?.toLowerCase().includes(query)
      );
    }

    // Status filter
    if (this.filterStatus === 'active') {
      filtered = filtered.filter(task => !task.isCompleted);
    } else if (this.filterStatus === 'completed') {
      filtered = filtered.filter(task => task.isCompleted);
    }

    // Priority filter
    if (this.filterPriority !== 'all') {
      filtered = filtered.filter(task => task.priority === this.filterPriority);
    }

    // Project filter
    if (this.filterProject !== 'all') {
      filtered = filtered.filter(task => task.projectId === this.filterProject);
    }

    // Category filter
    if (this.filterCategory !== 'all') {
      filtered = filtered.filter(task => task.categoryId === this.filterCategory);
    }

    // Sort
    filtered.sort((a, b) => {
      switch (this.sortBy) {
        case 'priority':
          const priorityOrder = { 'High': 3, 'Medium': 2, 'Low': 1 };
          return priorityOrder[b.priority] - priorityOrder[a.priority];
        case 'title':
          return a.title.localeCompare(b.title);
        case 'createdAt':
          return b.createdAt.getTime() - a.createdAt.getTime();
        case 'dueDate':
        default:
          return a.dueDate.getTime() - b.dueDate.getTime();
      }
    });

    this.filteredTasks = filtered;
  }

  async createTask() {
    if (!this.newTask.title.trim()) {
      this.notification.showError('Please enter a task title');
      return;
    }

    try {
      const token = this.authService.getToken();
      const headers = { 'Authorization': `Bearer ${token}` };

      const taskData = {
        title: this.newTask.title.trim(),
        description: this.newTask.description.trim(),
        priority: this.newTask.priority,
        dueDate: new Date(this.newTask.dueDate).toISOString(),
        projectId: this.newTask.projectId || undefined,
        categoryId: this.newTask.categoryId || undefined
      };

      await this.http.post(`${this.apiUrl}/tasks`, taskData, { headers }).toPromise();

      this.notification.showSuccess('Task created successfully!');
      this.showCreateModal = false;
      this.resetNewTask();
      this.loadTasks();
    } catch (error) {
      console.error('Error creating task:', error);
      this.notification.showError('Failed to create task');
    }
  }

  async toggleTask(taskId: string) {
    try {
      const token = this.authService.getToken();
      const headers = { 'Authorization': `Bearer ${token}` };

      const task = this.tasks.find(t => t.id === taskId);
      if (!task) return;

      const updateData = {
        isCompleted: !task.isCompleted
      };

      await this.http.put(`${this.apiUrl}/tasks/${taskId}`, updateData, { headers }).toPromise();

      this.notification.showSuccess(task.isCompleted ? 'Task marked as incomplete' : 'Task completed!');
      this.loadTasks();
    } catch (error) {
      console.error('Error updating task:', error);
      this.notification.showError('Failed to update task');
    }
  }

  async deleteTask(taskId: string) {
    if (!confirm('Are you sure you want to delete this task?')) return;

    try {
      const token = this.authService.getToken();
      const headers = { 'Authorization': `Bearer ${token}` };

      await this.http.delete(`${this.apiUrl}/tasks/${taskId}`, { headers }).toPromise();

      this.notification.showSuccess('Task deleted successfully!');
      this.loadTasks();
    } catch (error) {
      console.error('Error deleting task:', error);
      this.notification.showError('Failed to delete task');
    }
  }

  openCreateModal() {
    this.resetNewTask();
    this.showCreateModal = true;
  }

  resetNewTask() {
    this.newTask = {
      title: '',
      description: '',
      priority: 'Medium',
      dueDate: new Date().toISOString().split('T')[0],
      projectId: null,
      categoryId: null
    };
  }

  getPriorityLabel(priority: 'Low' | 'Medium' | 'High'): string {
    return priority;
  }

  getPriorityClass(priority: 'Low' | 'Medium' | 'High'): string {
    return `priority-${priority.toLowerCase()}`;
  }

  isOverdue(task: Task): boolean {
    return !task.isCompleted && task.dueDate < new Date();
  }

  onSearchChange() {
    this.applyFilters();
  }

  onFilterChange() {
    this.applyFilters();
  }

  trackByTaskId(index: number, task: Task): string {
    return task.id;
  }

  get activeTasks(): number {
    return this.tasks.filter(t => !t.isCompleted).length;
  }

  get completedTasks(): number {
    return this.tasks.filter(t => t.isCompleted).length;
  }
}
