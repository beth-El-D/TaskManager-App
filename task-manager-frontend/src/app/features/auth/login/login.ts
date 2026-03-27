import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.html'
})
export class Login {

  email = '';
  password = '';

  constructor(private auth: AuthService, private router: Router) { }

  login() {
    this.auth.login({ email: this.email, password: this.password })
      .subscribe({
        next: (response) => {
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          let errorMessage = 'Login failed. Please try again.';

          if (error.error) {
            if (typeof error.error === 'string') {
              errorMessage = error.error;
            } else if (error.error.message) {
              errorMessage = error.error.message;
            }
          }

          // Show user-friendly error message
          alert('❌ ' + errorMessage);
        }
      });
  }
}
