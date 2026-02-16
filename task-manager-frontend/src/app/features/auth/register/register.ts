import { Component } from '@angular/core';

@Component({
  selector: 'app-register',
  imports: [],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {

  email = '';
  password = '';

  constructor(private auth: AuthService, private router: Router) {}

  register() {
    const payload = {
      email: this.email,
      password: this.password
    };

    this.auth.register(payload).subscribe({
      next: () => {
        alert('Registration successful');
        this.router.navigate(['/login']);
      },
      error: () => alert('Registration failed')
    });
  }
}