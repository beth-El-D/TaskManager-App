import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { HttpClient } from '@angular/common/http';

interface Task {
  id: string;
  title: string;
  priority: 'Low' | 'Medium' | 'High';
  dueDate: Date;
  isCompleted: boolean;
  category: 'overdue' | 'today' | 'upcoming';
  createdAt: Date;
  status: number;
}

interface DashboardStats {
  overdueTasks: Task[];
  todayTasks: Task[];
  upcomingTasks: Task[];
  totalTasksToday: number;
  completedToday: number;
  overdueCount: number;
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

  // Dashboard stats
  dashboardStats: DashboardStats = {
    overdueTasks: [],
    todayTasks: [],
    upcomingTasks: [],
    totalTasksToday: 0,
    completedToday: 0,
    overdueCount: 0
  };

  // Focus mode
  focusMode = {
    active: false,
    taskTitle: '',
    timeRemaining: 0,
    totalTime: 25 * 60 // 25 minutes in seconds
  };

  // Analytics
  weeklyProgress = 70;

  private apiUrl = 'http://localhost:5118/api';

  constructor(private authService: AuthService, private http: HttpClient) { }

  ngOnInit() {
    this.currentUser = this.authService.getCurrentUser();
    this.setGreeting();
    this.loadDashboardData();
  }

  setGreeting() {
    const hour = new Date().getHours();
    if (hour < 12) {
      this.greeting = 'Good Morning';
    } else if (hour < 17) {
      this.greeting = 'Good Afternoon';
    } else {
      this.greeting = 'Good Evening';
    }
  }

  getUserDisplayName(): string {
    if (this.currentUser?.email) {
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

  loadSampleData() {
    // Fallback sample data
    this.dashboardStats = {
      overdueTasks: [
        {
          id: '1',
          title: 'Complete API integration',
          priority: 'High',
          dueDate: new Date(Date.now() - 86400000),
          isCompleted: false,
          category: 'overdue',
          createdAt: new Date(),
          status: 1
        }
      ],
      todayTasks: [
        {
          id: '2',
          title: 'Update documentation',
          priority: 'Medium',
          dueDate: new Date(),
          isCompleted: false,
          category: 'today',
          createdAt: new Date(),
          status: 1
        }
      ],
      upcomingTasks: [
        {
          id: '3',
          title: 'Plan next sprint',
          priority: 'Low',
          dueDate: new Date(Date.now() + 86400000),
          isCompleted: false,
          category: 'upcoming',
          createdAt: new Date(),
          status: 1
        }
      ],
      totalTasksToday: 2,
      completedToday: 0,
      overdueCount: 1
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
        const token = this.authService.getToken();
        const headers = { 'Authorization': `Bearer ${token}` };

        const taskData = {
          title: this.newTaskTitle.trim(),
          priority: this.newTaskPriority,
          dueDate: new Date().toISOString()
        };

        await this.http.post(`${this.apiUrl}/tasks`, taskData, { headers }).toPromise();

        this.newTaskTitle = '';
        this.newTaskPriority = 'Medium';

        // Reload dashboard data
        this.loadDashboardData();
      } catch (error) {
        console.error('Error creating task:', error);
        alert('Failed to create task. Please try again.');
      }
    }
  }

  async toggleTask(taskId: string) {
    try {
      const token = this.authService.getToken();
      const headers = { 'Authorization': `Bearer ${token}` };

      // Find the task to get its current state
      const allTasks = [...this.overdueTasks, ...this.todayTasks, ...this.upcomingTasks];
      const task = allTasks.find(t => t.id === taskId);

      if (task) {
        const updateData = {
          isCompleted: !task.isCompleted
        };

        await this.http.put(`${this.apiUrl}/tasks/${taskId}`, updateData, { headers }).toPromise();

        // Reload dashboard data
        this.loadDashboardData();
      }
    } catch (error) {
      console.error('Error updating task:', error);
      alert('Failed to update task. Please try again.');
    }
  }

  startFocusMode() {
    if (this.todayTasks.length > 0) {
      this.focusMode.active = true;
      this.focusMode.taskTitle = this.todayTasks[0].title;
      this.focusMode.timeRemaining = this.focusMode.totalTime;
      this.startFocusTimer();
    }
  }

  stopFocusMode() {
    this.focusMode.active = false;
    this.focusMode.timeRemaining = 0;
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

  getPriorityClass(priority: string): string {
    return `priority-${priority.toLowerCase()}`;
  }

  trackByTaskId(index: number, task: Task): string {
    return task.id;
  }
}
