import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { AuthService } from '../../Core/services/auth.service';

@Component({
  selector: 'logout-button',
  standalone: true,
  imports: [ButtonModule],
  templateUrl: './logout-button.Component.html',
  styleUrl: './logout-button.Component.css',
})
export class LogoutButtonComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  logout() {
    this.auth.logout();
    this.router.navigate(['/login'], { replaceUrl: true });
  }
}
