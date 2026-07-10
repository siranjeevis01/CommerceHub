import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@shared/services/auth.service';

@Component({
  standalone: false,
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavbarComponent {
  searchQuery = '';

  constructor(
    public auth: AuthService,
    private router: Router,
  ) {}

  onSearch(): void {
    if (this.searchQuery.trim()) {
      this.router.navigate(['/products'], { queryParams: { search: this.searchQuery } });
    }
  }

  logout(): void {
    this.auth.logout();
  }

  get userInitials(): string {
    const u = this.auth.currentUser;
    if (!u) return 'A';
    return `${u.firstName.charAt(0)}${u.lastName.charAt(0)}`;
  }
}
