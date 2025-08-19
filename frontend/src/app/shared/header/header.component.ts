import {
  ChangeDetectionStrategy,
  Component,
  inject,
  ViewChild,
} from '@angular/core';
import { AuthService } from '../../Core/services/auth.service';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CommonModule } from '@angular/common';
import { AvatarModule } from 'primeng/avatar';
import { Menu, MenuModule } from 'primeng/menu';

type MenuMeta = {
  label: string;
  icon?: string;
  showInUserMenu?: boolean;
  requiredPerms?: string[];
};

@Component({
  selector: 'header-component',
  standalone: true,
  imports: [CommonModule, MenuModule, ButtonModule, AvatarModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HeaderComponent {
  private router = inject(Router);
  private auth = inject(AuthService);

  @ViewChild('userMenu') userMenu!: Menu;

  items: any[] = [];
  userName: string | null = null;
  isLogged = false;

  ngOnInit(): void {
    this.isLogged = this.auth.isLoggedIn();
    this.userName = this.auth.getUserName();

    if (this.isLogged) {
      this.items = this.buildUserMenu();
    }
  }

  onClick() {
    this.router.navigate(['/']);
  }

  toggleMenu(event: Event) {
    this.userMenu.toggle(event);
  }

  login() {
    this.router.navigate(['/login']);
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  private buildUserMenu() {
    const out: any[] = [];

    for (const r of this.router.config) {
      const data = (r as any).data as { menu?: MenuMeta } | undefined;
      if (!data?.menu?.showInUserMenu) continue;

      const m = data.menu;
      const required = m.requiredPerms ?? [];

      if (!this.auth.hasAllPermissions(required)) continue;

      const routerLink = '/' + (r.path ?? '');
      out.push({
        label: m.label,
        icon: m.icon ?? 'pi pi-angle-right',
        routerLink,
        command: () => this.userMenu.hide(),
      });
    }

    // separador y botón salir
    out.push({ separator: true });
    out.push({
      label: 'Salir',
      icon: 'pi pi-sign-out',
      command: () => this.logout(),
    });

    return out;
  }
}
