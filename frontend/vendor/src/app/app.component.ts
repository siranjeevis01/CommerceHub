import { ChangeDetectionStrategy, Component } from '@angular/core';
import { AuthService } from '@shared/services/auth.service';
import { Router } from '@angular/router';

@Component({
  standalone: false,
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  sidebarCollapsed = false;
  showNotifications = false;

  constructor(public auth: AuthService, private router: Router) {}

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/auth/login']);
  }
}
