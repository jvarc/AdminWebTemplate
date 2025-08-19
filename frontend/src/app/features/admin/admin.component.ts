import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HeaderComponent } from '../../shared/header/header.component';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'admin-component',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, HeaderComponent],
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css'],
})
export class AdminComponent {
  title = 'Panel de Administración';
}
