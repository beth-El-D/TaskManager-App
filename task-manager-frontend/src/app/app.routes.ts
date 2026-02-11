import { Routes } from '@angular/router';

import { LoginComponent } from './features/auth/login/login';
import { Register} from './features/auth/register/register';
import { DashboardHome } from './features/dashboard/dashboard-home/dashboard-home';
import { TaskList } from './features/tasks/task-list/task-list';
import { ProjectList } from './features/projects/project-list/project-list';
import { CalendarView } from './features/calendar/calendar-view/calendar-view';
import { FocusPage } from './features/focus/focus-page/focus-page';
import { AnalyticsDashboard } from './features/analytics/analytics-dashboard/analytics-dashboard';
import { ProfileSettings } from './features/settings/profile-settings/profile-settings';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: Register },
  { path: 'dashboard', component: DashboardHome, canActivate: [authGuard] },
  { path: 'tasks', component: TaskList },
  { path: 'projects', component: ProjectList },
  { path: 'calendar', component: CalendarView },
  { path: 'focus', component: FocusPage },
  { path: 'analytics', component: AnalyticsDashboard },
  { path: 'settings/profile', component: ProfileSettings },

  { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
];
