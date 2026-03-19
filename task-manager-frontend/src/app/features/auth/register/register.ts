import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

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

  constructor(private auth: AuthService, private router: Router) { }

  register() {
    const payload = {
      email: this.email,
      password: this.password
    };

    this.auth.register(payload).subscribe({
      next: (response) => {
        alert('Registration successful! Please login to continue.');
        this.router.navigate(['/login']);
      },
      error: (error) => {
        const errorMessage = error.error || 'Registration failed';
        alert(errorMessage);
      }
    });
  }
}