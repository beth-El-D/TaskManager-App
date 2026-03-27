import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, RouterModule],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {

  email = '';
  password = '';
  isLoading = false;

  constructor(
    private auth: AuthService,
    private router: Router,
    private notification: NotificationService
  ) { }

  register() {
    if (this.isLoading) return;

    // Basic validation
    if (!this.email.trim()) {
      this.notification.showError('Please enter your email address.');
      return;
    }

    if (!this.password.trim()) {
      this.notification.showError('Please enter a password.');
      return;
    }

    if (this.password.length < 6) {
      this.notification.showError('Password must be at least 6 characters long.');
      return;
    }

    this.isLoading = true;
    const payload = {
      email: this.email.trim(),
      password: this.password
    };

    this.auth.register(payload).subscribe({
      next: (response) => {
        this.notification.showSuccess('Registration successful! Please login to continue.');
        this.router.navigate(['/login']);
      },
      error: (error) => {
        let errorMessage = 'Registration failed. Please try again.';

        if (error.error) {
          if (typeof error.error === 'string') {
            errorMessage = error.error;
          } else if (error.error.message) {
            errorMessage = error.error.message;
          }
        }

        this.notification.showError(errorMessage);
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }
}