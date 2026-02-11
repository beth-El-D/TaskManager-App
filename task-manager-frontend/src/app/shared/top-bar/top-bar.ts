import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-top-bar',
  standalone: true,
  imports: [RouterModule, NgIf],
  templateUrl: './top-bar.html',
  styleUrls: ['./top-bar.scss']
})
export class TopBarComponent {
  menuOpen = false;
  profileOpen = false;

  toggleMenu() {
    this.menuOpen = !this.menuOpen;
  }

  toggleProfile() {
    this.profileOpen = !this.profileOpen;
  }

  toggleTheme() {
    document.body.classList.toggle('dark-theme');
  }
}
