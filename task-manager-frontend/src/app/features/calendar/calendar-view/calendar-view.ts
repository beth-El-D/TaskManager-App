import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
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

interface CalendarDay {
  date: Date;
  isCurrentMonth: boolean;
  isToday: boolean;
  tasks: Task[];
}

@Component({
  selector: 'app-calendar-view',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './calendar-view.html',
  styleUrl: './calendar-view.scss',
})
export class CalendarView implements OnInit {
  currentDate = new Date();
  currentMonth = new Date().getMonth();
  currentYear = new Date().getFullYear();

  calendarDays: CalendarDay[] = [];
  weekDays = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
  monthNames = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];

  tasks: Task[] = [];
  selectedDay: CalendarDay | null = null;
  showTaskModal = false;
  showCreateModal = false;

  loading = false;

  // View mode
  viewMode: 'month' | 'week' | 'day' = 'month';

  // Projects and Categories
  projects: Project[] = [];
  categories: Category[] = [];

  // New task
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
    private categoryService: CategoryService
  ) { }

  ngOnInit() {
    this.loadTasks();
    this.loadProjects();
    this.loadCategories();
    this.generateCalendar();
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
        this.generateCalendar();
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

  generateCalendar() {
    this.calendarDays = [];

    const firstDay = new Date(this.currentYear, this.currentMonth, 1);
    const lastDay = new Date(this.currentYear, this.currentMonth + 1, 0);
    const prevLastDay = new Date(this.currentYear, this.currentMonth, 0);

    const firstDayIndex = firstDay.getDay();
    const lastDayIndex = lastDay.getDay();
    const nextDays = 7 - lastDayIndex - 1;

    // Previous month days
    for (let i = firstDayIndex; i > 0; i--) {
      const date = new Date(this.currentYear, this.currentMonth, -i + 1);
      this.calendarDays.push({
        date,
        isCurrentMonth: false,
        isToday: this.isToday(date),
        tasks: this.getTasksForDate(date)
      });
    }

    // Current month days
    for (let i = 1; i <= lastDay.getDate(); i++) {
      const date = new Date(this.currentYear, this.currentMonth, i);
      this.calendarDays.push({
        date,
        isCurrentMonth: true,
        isToday: this.isToday(date),
        tasks: this.getTasksForDate(date)
      });
    }

    // Next month days
    for (let i = 1; i <= nextDays; i++) {
      const date = new Date(this.currentYear, this.currentMonth + 1, i);
      this.calendarDays.push({
        date,
        isCurrentMonth: false,
        isToday: this.isToday(date),
        tasks: this.getTasksForDate(date)
      });
    }
  }

  getTasksForDate(date: Date): Task[] {
    return this.tasks.filter(task => {
      const taskDate = new Date(task.dueDate);
      return taskDate.getDate() === date.getDate() &&
        taskDate.getMonth() === date.getMonth() &&
        taskDate.getFullYear() === date.getFullYear();
    });
  }

  isToday(date: Date): boolean {
    const today = new Date();
    return date.getDate() === today.getDate() &&
      date.getMonth() === today.getMonth() &&
      date.getFullYear() === today.getFullYear();
  }

  previousMonth() {
    if (this.currentMonth === 0) {
      this.currentMonth = 11;
      this.currentYear--;
    } else {
      this.currentMonth--;
    }
    this.generateCalendar();
  }

  nextMonth() {
    if (this.currentMonth === 11) {
      this.currentMonth = 0;
      this.currentYear++;
    } else {
      this.currentMonth++;
    }
    this.generateCalendar();
  }

  goToToday() {
    const today = new Date();
    this.currentMonth = today.getMonth();
    this.currentYear = today.getFullYear();
    this.generateCalendar();
  }

  selectDay(day: CalendarDay) {
    this.selectedDay = day;
    if (day.tasks.length > 0) {
      this.showTaskModal = true;
    }
  }

  openCreateModal(day?: CalendarDay) {
    if (day) {
      this.newTask.dueDate = day.date.toISOString().split('T')[0];
    }
    this.showCreateModal = true;
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

  getPriorityClass(priority: 'Low' | 'Medium' | 'High'): string {
    return `priority-${priority.toLowerCase()}`;
  }

  getMonthYear(): string {
    return `${this.monthNames[this.currentMonth]} ${this.currentYear}`;
  }

  getTaskCount(day: CalendarDay): number {
    return day.tasks.length;
  }

  getCompletedTaskCount(day: CalendarDay): number {
    return day.tasks.filter(t => t.isCompleted).length;
  }

  hasOverdueTasks(day: CalendarDay): boolean {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return day.date < today && day.tasks.some(t => !t.isCompleted);
  }

  trackByTaskId(index: number, task: Task): string {
    return task.id;
  }
}
