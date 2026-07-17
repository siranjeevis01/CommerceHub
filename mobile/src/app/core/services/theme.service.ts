import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { ThemeName } from '../models';

const THEMES: Record<ThemeName, Record<string, string>> = {
  dark: {
    '--ion-background-color': '#0f172a',
    '--ion-background-color-rgb': '15,23,42',
    '--ion-text-color': '#f8fafc',
    '--ion-text-color-rgb': '248,250,252',
    '--ion-item-background': '#1e293b',
    '--ion-card-background': '#1e293b',
    '--ion-toolbar-background': '#0f172a',
    '--ion-tab-bar-background': '#1e293b',
    '--ion-color-primary': '#6366f1',
    '--ion-color-primary-rgb': '99,102,241',
  },
  light: {
    '--ion-background-color': '#f8fafc',
    '--ion-background-color-rgb': '248,250,252',
    '--ion-text-color': '#0f172a',
    '--ion-text-color-rgb': '15,23,42',
    '--ion-item-background': '#ffffff',
    '--ion-card-background': '#ffffff',
    '--ion-toolbar-background': '#f8fafc',
    '--ion-tab-bar-background': '#ffffff',
    '--ion-color-primary': '#6366f1',
    '--ion-color-primary-rgb': '99,102,241',
  },
  cyberpunk: {
    '--ion-background-color': '#0a0a0f',
    '--ion-background-color-rgb': '10,10,15',
    '--ion-text-color': '#00ff88',
    '--ion-text-color-rgb': '0,255,136',
    '--ion-item-background': '#111118',
    '--ion-card-background': '#111118',
    '--ion-toolbar-background': '#0a0a0f',
    '--ion-tab-bar-background': '#111118',
    '--ion-color-primary': '#00ff88',
    '--ion-color-primary-rgb': '0,255,136',
  },
  neon: {
    '--ion-background-color': '#0d0221',
    '--ion-background-color-rgb': '13,2,33',
    '--ion-text-color': '#f0e7ff',
    '--ion-text-color-rgb': '240,231,255',
    '--ion-item-background': '#150533',
    '--ion-card-background': '#150533',
    '--ion-toolbar-background': '#0d0221',
    '--ion-tab-bar-background': '#150533',
    '--ion-color-primary': '#ff00ff',
    '--ion-color-primary-rgb': '255,0,255',
  },
  ocean: {
    '--ion-background-color': '#041424',
    '--ion-background-color-rgb': '4,20,36',
    '--ion-text-color': '#e0f2fe',
    '--ion-text-color-rgb': '224,242,254',
    '--ion-item-background': '#0a2540',
    '--ion-card-background': '#0a2540',
    '--ion-toolbar-background': '#041424',
    '--ion-tab-bar-background': '#0a2540',
    '--ion-color-primary': '#0ea5e9',
    '--ion-color-primary-rgb': '14,165,233',
  },
  forest: {
    '--ion-background-color': '#0a1a0f',
    '--ion-background-color-rgb': '10,26,15',
    '--ion-text-color': '#dcfce7',
    '--ion-text-color-rgb': '220,252,231',
    '--ion-item-background': '#132e1a',
    '--ion-card-background': '#132e1a',
    '--ion-toolbar-background': '#0a1a0f',
    '--ion-tab-bar-background': '#132e1a',
    '--ion-color-primary': '#22c55e',
    '--ion-color-primary-rgb': '34,197,94',
  },
  sunset: {
    '--ion-background-color': '#1a0a0a',
    '--ion-background-color-rgb': '26,10,10',
    '--ion-text-color': '#fef2f2',
    '--ion-text-color-rgb': '254,242,242',
    '--ion-item-background': '#2a1111',
    '--ion-card-background': '#2a1111',
    '--ion-toolbar-background': '#1a0a0a',
    '--ion-tab-bar-background': '#2a1111',
    '--ion-color-primary': '#f43f5e',
    '--ion-color-primary-rgb': '244,63,94',
  }
};

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private currentThemeSubject = new BehaviorSubject<ThemeName>('dark');
  currentTheme$ = this.currentThemeSubject.asObservable();

  constructor() {
    const saved = localStorage.getItem('theme') as ThemeName;
    if (saved && THEMES[saved]) {
      this.setTheme(saved);
    }
  }

  get currentTheme(): ThemeName {
    return this.currentThemeSubject.value;
  }

  get availableThemes(): { name: ThemeName; label: string; icon: string }[] {
    return [
      { name: 'dark', label: 'Dark', icon: 'moon' },
      { name: 'light', label: 'Light', icon: 'sunny' },
      { name: 'cyberpunk', label: 'Cyberpunk', icon: 'hardware-chip' },
      { name: 'neon', label: 'Neon', icon: 'flash' },
      { name: 'ocean', label: 'Ocean', icon: 'water' },
      { name: 'forest', label: 'Forest', icon: 'leaf' },
      { name: 'sunset', label: 'Sunset', icon: 'flame' },
    ];
  }

  setTheme(theme: ThemeName): void {
    const variables = THEMES[theme];
    if (!variables) return;

    const root = document.documentElement;
    Object.entries(variables).forEach(([key, value]) => {
      root.style.setProperty(key, value);
    });

    document.querySelector('meta[name="color-scheme"]')
      ?.setAttribute('content', theme === 'light' ? 'light' : 'dark');

    this.currentThemeSubject.next(theme);
    localStorage.setItem('theme', theme);
  }

  toggleTheme(): void {
    const themes = Object.keys(THEMES) as ThemeName[];
    const currentIndex = themes.indexOf(this.currentTheme);
    const nextIndex = (currentIndex + 1) % themes.length;
    this.setTheme(themes[nextIndex]);
  }
}
