import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { HttpClient } from '@angular/common/http';
import { ProjectService, Project } from '../../../core/services/project.service';
import { CategoryService, Category } from '../../../core/services/category.service';
import { FocusSessionService, FocusSession } from '../../../core/services/focus-session.service';
import { UserGoalService, UserGoal } from '../../../core/services/user-goal.service';

interface Task {
  id: string;
  title: string;
  priority: 'Low' | 'Medium' | 'High';
  dueDate: Date;
  isCompleted: boolean;
  category: 'overdue' | 'today' | 'upcoming';
  createdAt: Date;
  status: number;
  project?: string; // Optional project field
}

interface DashboardStats {
  overdueTasks: Task[];
  todayTasks: Task[];
  upcomingTasks: Task[];
  totalTasksToday: number;
  completedToday: number;
  overdueCount: number;
  weeklyProgress: WeeklyProgress[];
  completionTrend: number;
}

interface WeeklyProgress {
  date: string;
  dayName: string;
  completionRate: number;
  totalTasks: number;
  completedTasks: number;
  isToday: boolean;
}

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard-home.html',
  styleUrl: './dashboard-home.scss',
})
export class DashboardHome implements OnInit {
  currentUser: any;
  currentDate = new Date();
  greeting = '';
  loading = true;

  // Quick add task
  newTaskTitle = '';
  newTaskPriority: 'Low' | 'Medium' | 'High' = 'Medium';
  selectedProjectId?: string;
  selectedCategoryId?: string;

  // Projects and Categories
  projects: Project[] = [];
  categories: Category[] = [];

  // Goals
  activeGoals: UserGoal[] = [];

  // Focus sessions
  currentFocusSession?: FocusSession;
  todayFocusSessions: FocusSession[] = [];

  // Dashboard stats
  dashboardStats: DashboardStats = {
    overdueTasks: [],
    todayTasks: [],
    upcomingTasks: [],
    totalTasksToday: 0,
    completedToday: 0,
    overdueCount: 0,
    weeklyProgress: [],
    completionTrend: 0
  };

  // Focus mode
  focusMode = {
    active: false,
    taskTitle: '',
    timeRemaining: 0,
    totalTime: 25 * 60, // 25 minutes in seconds
    isPaused: false
  };

  // Analytics
  weeklyProgress = 70;

  // UX enhancements
  isQuickAddExpanded = false;
  selectedPriority: 'Low' | 'Medium' | 'High' = 'Medium';
  selectedDueDate = '';
  draggedTask: Task | null = null;
  showCompletedTasks = false;
  searchQuery = '';
  filterPriority = 'All';
  sortBy = 'dueDate';

  private apiUrl = 'http://localhost:5118/api';

  constructor(
    private authService: AuthService,
    private http: HttpClient,
    private router: Router,
    private notification: NotificationService,
    private projectService: ProjectService,
    private categoryService: CategoryService,
    private focusSessionService: FocusSessionService,
    private userGoalService: UserGoalService
  ) { }

  ngOnInit() {
    this.currentUser = this.authService.getCurrentUser();
    this.setGreeting();
    this.loadDashboardData();
    this.loadProjects();
    this.loadCategories();
    this.loadActiveGoals();
    this.loadTodayFocusSessions();

    // Add keyboard event listeners
    document.addEventListener('keydown', this.onKeyDown.bind(this));
  }

  setGreeting() {
    const hour = new Date().getHours();
    if (hour < 12) {
      this.greeting = 'GOOD MORNING';
    } else if (hour < 17) {
      this.greeting = 'GOOD AFTERNOON';
    } else {
      this.greeting = 'GOOD EVENING';
    }
  }

  getUserDisplayName(): string {
    if (this.currentUser?.name) {
      return this.currentUser.name;
    } else if (this.currentUser?.email) {
      return this.currentUser.email.split('@')[0];
    }
    return 'User';
  }

  async loadDashboardData() {
    try {
      this.loading = true;
      const token = this.authService.getToken();
      const headers = { 'Authorization': `Bearer ${token}` };

      const response = await this.http.get<DashboardStats>(`${this.apiUrl}/tasks/dashboard`, { headers }).toPromise();

      if (response) {
        this.dashboardStats = response;
        // Convert date strings to Date objects and categorize
        this.dashboardStats.overdueTasks = this.dashboardStats.overdueTasks.map(task => ({
          ...task,
          dueDate: new Date(task.dueDate),
          createdAt: new Date(task.createdAt),
          category: 'overdue' as const
        }));

        this.dashboardStats.todayTasks = this.dashboardStats.todayTasks.map(task => ({
          ...task,
          dueDate: new Date(task.dueDate),
          createdAt: new Date(task.createdAt),
          category: 'today' as const
        }));

        this.dashboardStats.upcomingTasks = this.dashboardStats.upcomingTasks.map(task => ({
          ...task,
          dueDate: new Date(task.dueDate),
          createdAt: new Date(task.createdAt),
          category: 'upcoming' as const
        }));
      }
    } catch (error) {
      console.error('Error loading dashboard data:', error);
      // Fallback to sample data if API fails
      this.loadSampleData();
    } finally {
      this.loading = false;
    }
  }

  async loadProjects() {
    try {
      this.projects = await this.projectService.getProjects().toPromise() || [];
    } catch (error) {
      console.error('Error loading projects:', error);
      this.projects = [];
    }
  }

  async loadCategories() {
    try {
      this.categories = await this.categoryService.getCategories().toPromise() || [];
    } catch (error) {
      console.error('Error loading categories:', error);
      this.categories = [];
    }
  }

  async loadActiveGoals() {
    try {
      this.activeGoals = await this.userGoalService.getActiveGoals().toPromise() || [];
    } catch (error) {
      console.error('Error loading goals:', error);
      this.activeGoals = [];
    }
  }

  async loadTodayFocusSessions() {
    try {
      this.todayFocusSessions = await this.focusSessionService.getTodaySessions().toPromise() || [];
    } catch (error) {
      console.error('Error loading focus sessions:', error);
      this.todayFocusSessions = [];
    }
  }

  loadSampleData() {
    // Fallback sample data - should only be used if API fails
    this.dashboardStats = {
      overdueTasks: [],
      todayTasks: [],
      upcomingTasks: [],
      totalTasksToday: 0,
      completedToday: 0,
      overdueCount: 0,
      weeklyProgress: [],
      completionTrend: 0
    };
  }

  get overdueTasks() {
    return this.dashboardStats.overdueTasks.filter(task => !task.isCompleted);
  }

  get todayTasks() {
    return this.dashboardStats.todayTasks.filter(task => !task.isCompleted);
  }

  get upcomingTasks() {
    return this.dashboardStats.upcomingTasks.filter(task => !task.isCompleted);
  }

  get completedToday() {
    return this.dashboardStats.completedToday;
  }

  get tasksToday() {
    return this.dashboardStats.totalTasksToday;
  }

  async addQuickTask() {
    if (this.newTaskTitle.trim()) {
      try {
        // Check if user is authenticated
        if (!this.authService.isLoggedIn()) {
          this.notification.showError('Please log in to add tasks.');
          this.router.navigate(['/login']);
          return;
        }

        const taskData = {
          title: this.newTaskTitle.trim(),
          description: '', // Add empty description as it's optional
          priority: this.newTaskPriority, // Send as string: 'Low', 'Medium', 'High'
          dueDate: new Date().toISOString(),
          projectId: this.selectedProjectId || null,
          categoryId: this.selectedCategoryId || null
        };

        await this.http.post(`${this.apiUrl}/tasks`, taskData).toPromise();

        this.newTaskTitle = '';
        this.newTaskPriority = 'Medium';
        this.selectedProjectId = undefined;
        this.selectedCategoryId = undefined;
        this.notification.showSuccess('Task added successfully!');

        // Reload dashboard data
        this.loadDashboardData();
      } catch (error: any) {
        console.error('Error creating task:', error);

        let errorMessage = 'Failed to create task. Please try again.';

        if (error.status === 401) {
          errorMessage = 'Please log in to add tasks.';
          this.router.navigate(['/login']);
        } else if (error.status === 400) {
          if (error.error?.message) {
            errorMessage = error.error.message;
          } else {
            errorMessage = 'Invalid task data. Please check all fields.';
          }
        } else if (error.status === 0) {
          errorMessage = 'Unable to connect to server. Please check your connection.';
        }

        this.notification.showError(errorMessage);
      }
    } else {
      this.notification.showError('Please enter a task title.');
    }
  }

  private mapPriorityToNumber(priority: string): number {
    // This method is no longer needed but kept for backward compatibility
    switch (priority.toLowerCase()) {
      case 'low': return 1;
      case 'medium': return 2;
      case 'high': return 3;
      default: return 2;
    }
  }

  async toggleTask(taskId: string) {
    try {
      // Check if user is authenticated
      if (!this.authService.isLoggedIn()) {
        this.notification.showError('Please log in to update tasks.');
        this.router.navigate(['/login']);
        return;
      }

      // Find the task to get its current state
      const allTasks = [...this.overdueTasks, ...this.todayTasks, ...this.upcomingTasks];
      const task = allTasks.find(t => t.id === taskId);

      if (task) {
        const updateData = {
          isCompleted: !task.isCompleted
        };

        await this.http.put(`${this.apiUrl}/tasks/${taskId}`, updateData).toPromise();

        // Reload dashboard data
        this.loadDashboardData();
        this.notification.showSuccess(task.isCompleted ? 'Task marked as incomplete' : 'Task completed!');
      }
    } catch (error: any) {
      console.error('Error updating task:', error);

      let errorMessage = 'Failed to update task. Please try again.';

      if (error.status === 401) {
        errorMessage = 'Please log in to update tasks.';
        this.router.navigate(['/login']);
      } else if (error.status === 404) {
        errorMessage = 'Task not found.';
      } else if (error.status === 0) {
        errorMessage = 'Unable to connect to server. Please check your connection.';
      }

      this.notification.showError(errorMessage);
    }
  }

  async startFocusMode() {
    if (this.todayTasks.length > 0) {
      try {
        const firstTask = this.todayTasks[0];

        // Create focus session in backend
        const sessionData = {
          taskId: firstTask.id,
          duration: this.focusMode.totalTime
        };

        this.currentFocusSession = await this.focusSessionService.startSession(sessionData).toPromise();

        this.focusMode.active = true;
        this.focusMode.taskTitle = firstTask.title;
        this.focusMode.timeRemaining = this.focusMode.totalTime;
        this.startFocusTimer();

        this.notification.showSuccess('Focus session started!');
      } catch (error) {
        console.error('Error starting focus session:', error);
        this.notification.showError('Failed to start focus session.');
      }
    } else {
      this.notification.showError('No tasks available for focus mode.');
    }
  }

  async stopFocusMode() {
    if (this.currentFocusSession) {
      try {
        // Complete the session in backend
        await this.focusSessionService.completeSession(
          this.currentFocusSession.id,
          { endTime: new Date() }
        ).toPromise();

        this.notification.showSuccess('Focus session completed!');
        this.loadTodayFocusSessions();
      } catch (error) {
        console.error('Error completing focus session:', error);
      }
    }

    this.focusMode.active = false;
    this.focusMode.timeRemaining = 0;
    this.currentFocusSession = undefined;
  }

  private startFocusTimer() {
    const timer = setInterval(() => {
      if (this.focusMode.timeRemaining > 0) {
        this.focusMode.timeRemaining--;
      } else {
        clearInterval(timer);
        this.focusMode.active = false;
        alert('🎉 Focus session completed!');
      }
    }, 1000);
  }

  formatTime(seconds: number): string {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
  }

  trackByTaskId(index: number, task: Task): string {
    return task.id;
  }

  getCompletionPercentage(): number {
    if (this.tasksToday === 0) return 0;
    return Math.round((this.completedToday / this.tasksToday) * 100);
  }

  getCompletionTrend(): string {
    const trend = this.dashboardStats.completionTrend;
    if (trend > 0) {
      return `+${trend}%`;
    } else if (trend < 0) {
      return `${trend}%`;
    } else {
      return '0%';
    }
  }

  getTrendClass(): string {
    const trend = this.dashboardStats.completionTrend;
    if (trend > 0) return 'positive';
    if (trend < 0) return 'negative';
    return 'neutral';
  }

  getDefaultFocusTime(): string {
    if (this.focusMode.active) {
      return this.formatTime(this.focusMode.timeRemaining);
    }
    return this.formatTime(this.focusMode.totalTime);
  }

  getTasksRemaining(): number {
    return Math.max(0, this.tasksToday - this.completedToday);
  }

  toggleFocusMode() {
    if (this.focusMode.active) {
      this.stopFocusMode();
    } else {
      this.startFocusMode();
    }
  }

  getPriorityClass(priority: string): string {
    return `priority-${priority?.toLowerCase() || 'medium'}`;
  }

  // Enhanced UX Methods
  expandQuickAdd() {
    this.isQuickAddExpanded = true;
  }

  collapseQuickAdd() {
    this.isQuickAddExpanded = false;
    this.selectedPriority = 'Medium';
    this.selectedDueDate = '';
  }

  async addTaskWithDetails() {
    if (this.newTaskTitle.trim()) {
      try {
        if (!this.authService.isLoggedIn()) {
          this.notification.showError('Please log in to add tasks.');
          this.router.navigate(['/login']);
          return;
        }

        const taskData = {
          title: this.newTaskTitle.trim(),
          description: '',
          priority: this.selectedPriority, // Send as string: 'Low', 'Medium', 'High'
          dueDate: this.selectedDueDate ? new Date(this.selectedDueDate).toISOString() : new Date().toISOString()
        };

        await this.http.post(`${this.apiUrl}/tasks`, taskData).toPromise();

        this.newTaskTitle = '';
        this.collapseQuickAdd();
        this.notification.showSuccess('Task added successfully!');
        this.loadDashboardData();
      } catch (error: any) {
        console.error('Error creating task:', error);
        this.notification.showError('Failed to create task. Please try again.');
      }
    }
  }

  // Drag and Drop functionality
  onDragStart(event: DragEvent, task: Task) {
    this.draggedTask = task;
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
      event.dataTransfer.setData('text/html', task.id);
    }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'move';
    }
  }

  onDrop(event: DragEvent, targetStatus: 'today' | 'upcoming' | 'completed') {
    event.preventDefault();
    if (this.draggedTask) {
      this.moveTask(this.draggedTask, targetStatus);
      this.draggedTask = null;
    }
  }

  async moveTask(task: Task, targetStatus: string) {
    try {
      const updateData = {
        isCompleted: targetStatus === 'completed',
        dueDate: targetStatus === 'today' ? new Date().toISOString() :
          targetStatus === 'upcoming' ? new Date(Date.now() + 86400000).toISOString() :
            task.dueDate.toISOString()
      };

      await this.http.put(`${this.apiUrl}/tasks/${task.id}`, updateData).toPromise();
      this.loadDashboardData();
      this.notification.showSuccess(`Task moved to ${targetStatus}!`);
    } catch (error) {
      this.notification.showError('Failed to move task.');
    }
  }

  // Enhanced Focus Mode
  pauseFocusMode() {
    this.focusMode.isPaused = true;
  }

  resumeFocusMode() {
    this.focusMode.isPaused = false;
  }

  resetFocusMode() {
    this.focusMode.active = false;
    this.focusMode.isPaused = false;
    this.focusMode.timeRemaining = this.focusMode.totalTime;
  }

  setFocusTime(minutes: number) {
    this.focusMode.totalTime = minutes * 60;
    this.focusMode.timeRemaining = this.focusMode.totalTime;
  }

  // Task Management
  async duplicateTask(task: Task) {
    try {
      const taskData = {
        title: `${task.title} (Copy)`,
        description: '',
        priority: this.mapPriorityToNumber(task.priority),
        dueDate: task.dueDate.toISOString()
      };

      await this.http.post(`${this.apiUrl}/tasks`, taskData).toPromise();
      this.loadDashboardData();
      this.notification.showSuccess('Task duplicated successfully!');
    } catch (error) {
      this.notification.showError('Failed to duplicate task.');
    }
  }

  async deleteTask(taskId: string) {
    if (confirm('Are you sure you want to delete this task?')) {
      try {
        await this.http.delete(`${this.apiUrl}/tasks/${taskId}`).toPromise();
        this.loadDashboardData();
        this.notification.showSuccess('Task deleted successfully!');
      } catch (error) {
        this.notification.showError('Failed to delete task.');
      }
    }
  }

  // Filtering and Sorting
  get filteredTasks() {
    let tasks = [...this.todayTasks];

    if (this.searchQuery) {
      tasks = tasks.filter(task =>
        task.title.toLowerCase().includes(this.searchQuery.toLowerCase())
      );
    }

    if (this.filterPriority !== 'All') {
      tasks = tasks.filter(task => task.priority === this.filterPriority);
    }

    if (!this.showCompletedTasks) {
      tasks = tasks.filter(task => !task.isCompleted);
    }

    // Sort tasks
    tasks.sort((a, b) => {
      switch (this.sortBy) {
        case 'priority':
          const priorityOrder = { 'High': 3, 'Medium': 2, 'Low': 1 };
          return priorityOrder[b.priority] - priorityOrder[a.priority];
        case 'title':
          return a.title.localeCompare(b.title);
        case 'dueDate':
        default:
          return new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime();
      }
    });

    return tasks;
  }

  // Quick Actions
  markAllAsCompleted() {
    if (confirm('Mark all today\'s tasks as completed?')) {
      this.todayTasks.forEach(task => {
        if (!task.isCompleted) {
          this.toggleTask(task.id);
        }
      });
    }
  }

  clearCompletedTasks() {
    if (confirm('Delete all completed tasks?')) {
      const completedTasks = this.todayTasks.filter(task => task.isCompleted);
      completedTasks.forEach(task => {
        this.deleteTask(task.id);
      });
    }
  }

  // Keyboard shortcuts
  onKeyDown(event: KeyboardEvent) {
    if (event.ctrlKey || event.metaKey) {
      switch (event.key) {
        case 'n':
          event.preventDefault();
          this.expandQuickAdd();
          break;
        case 'f':
          event.preventDefault();
          this.toggleFocusMode();
          break;
        case '/':
          event.preventDefault();
          const searchInput = document.querySelector('.search-input') as HTMLInputElement;
          if (searchInput) searchInput.focus();
          break;
      }
    }

    if (event.key === 'Escape') {
      this.collapseQuickAdd();
    }
  }

  // Statistics
  getProductivityScore(): number {
    const totalTasks = this.tasksToday;
    const completed = this.completedToday;
    const overdue = this.overdueTasks.length;

    if (totalTasks === 0) return 100;

    const completionRate = (completed / totalTasks) * 100;
    const overdueRate = (overdue / totalTasks) * 100;

    return Math.max(0, Math.round(completionRate - (overdueRate * 0.5)));
  }

  getTimeOfDayGreeting(): string {
    const hour = new Date().getHours();
    if (hour < 6) return 'Working late';
    if (hour < 12) return 'Good morning';
    if (hour < 17) return 'Good afternoon';
    if (hour < 22) return 'Good evening';
    return 'Working late';
  }

  // Animation helpers
  getTaskItemClass(task: Task): string {
    let classes = 'task-item';
    if (task.isCompleted) classes += ' completed';
    if (task.priority === 'High') classes += ' high-priority';
    if (new Date(task.dueDate) < new Date()) classes += ' overdue';
    return classes;
  }

  navigateToProjects() {
    this.router.navigate(['/projects']);
  }
}
