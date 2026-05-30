import { Component, OnInit, inject, ChangeDetectorRef, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ContentService } from '../../core/services/content.service';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../core/services/auth.service';
import { InteractionService } from '../../core/services/interaction.service';

interface RatedContent {
  contentId: number;
  title: string;
  description: string;
  posterUrl: string;
  score: number;
  dateRated: string;
  contentType: string;
}

interface FavoriteContent {
  contentId: number;
  title: string;
  description: string;
  posterUrl: string;
  dateAdded: string;
  contentType: string;
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);
  private contentService = inject(ContentService);
  private http = inject(HttpClient);
  private auth = inject(AuthService);
  private interactionService = inject(InteractionService);
  private platformId = inject(PLATFORM_ID);

  // basic profile fields
  displayName: string = '';
  email: string = '';
  bio: string = '';

  // tags/genres
  allGenres: any[] = [];
  selectedGenreIds: number[] = [];
  tagsText: string = '';

  // Ratings and Favorites
  ratedContents: RatedContent[] = [];
  favoriteContents: FavoriteContent[] = [];
  isLoading: boolean = false;
  activeTab: string = 'ratings';

  ngOnInit() {
    this.loadProfile();
    this.loadGenres();
    if (isPlatformBrowser(this.platformId)) {
      this.loadRatingsAndFavorites();
    }
  }

  loadGenres() {
    this.contentService.getGenres().subscribe(g => {
      this.allGenres = g || [];
      this.cdr.detectChanges();
    }, err => { this.allGenres = []; });
  }

  loadProfile() {
    const userId = this.auth.getUserId() || '1';

    this.http.get(`http://localhost:5000/api/v1/user/profile/${userId}`).subscribe({
      next: (res: any) => {
        this.displayName = res.displayName || '';
        this.email = res.email || '';
        this.bio = res.bio || '';
        this.selectedGenreIds = res.selectedGenreIds || [];
        this.tagsText = (res.tags || []).join(', ');
        this.cdr.detectChanges();
      },
      error: () => {
        if (isPlatformBrowser(this.platformId)) {
          const raw = localStorage.getItem('user_profile');
          if (raw) {
            try {
              const p = JSON.parse(raw);
              this.displayName = p.displayName || '';
              this.email = p.email || '';
              this.bio = p.bio || '';
              this.selectedGenreIds = p.selectedGenreIds || [];
              this.tagsText = (p.tags || []).join(', ');
            } catch { /* ignore */ }
          }
        }
        this.cdr.detectChanges();
      }
    });
  }

  loadRatingsAndFavorites() {
    this.isLoading = true;
    
    this.interactionService.getUserRatings().subscribe({
      next: (ratings: any[]) => {
        this.loadRatingDetails(ratings);
      },
      error: (err) => {
        console.error('Failed to load ratings', err);
        this.ratedContents = [];
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });

    this.interactionService.getUserFavorites().subscribe({
      next: (favorites: any[]) => {
        this.loadFavoriteDetails(favorites);
      },
      error: (err) => {
        console.error('Failed to load favorites', err);
        this.favoriteContents = [];
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  private loadRatingDetails(ratings: any[]) {
    if (!ratings || ratings.length === 0) {
      this.ratedContents = [];
      this.isLoading = false;
      this.cdr.detectChanges();
      return;
    }

    const contentIds = ratings.map(r => r.contentId);
    this.contentService.getContentDetailsByIds(contentIds).subscribe({
      next: (contents: any[]) => {
        this.ratedContents = ratings.map(rating => {
          const content = contents.find(c => (c.contentId ?? c.ContentID ?? c.id ?? c.Id) === rating.contentId);
          return {
            contentId: rating.contentId,
            title: content?.title || content?.Title || content?.name || 'Unknown',
            description: content?.description || content?.Description || '',
            posterUrl: content?.posterUrl || content?.PosterUrl || content?.PosterURL || '',
            score: rating.score,
            dateRated: rating.dateRated,
            contentType: content?.contentType || content?.ContentType || 'Unknown'
          };
        });
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.ratedContents = ratings.map(r => ({
          ...r,
          title: 'Unknown',
          description: '',
          posterUrl: '',
          contentType: 'Unknown'
        }));
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  private loadFavoriteDetails(favorites: any[]) {
    if (!favorites || favorites.length === 0) {
      this.favoriteContents = [];
      this.isLoading = false;
      this.cdr.detectChanges();
      return;
    }

    const contentIds = favorites.map(f => f.contentId);
    this.contentService.getContentDetailsByIds(contentIds).subscribe({
      next: (contents: any[]) => {
        this.favoriteContents = favorites.map(favorite => {
          const content = contents.find(c => (c.contentId ?? c.ContentID ?? c.id ?? c.Id) === favorite.contentId);
          return {
            contentId: favorite.contentId,
            title: content?.title || content?.Title || content?.name || 'Unknown',
            description: content?.description || content?.Description || '',
            posterUrl: content?.posterUrl || content?.PosterUrl || content?.PosterURL || '',
            dateAdded: favorite.dateAdded,
            contentType: content?.contentType || content?.ContentType || 'Unknown'
          };
        });
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.favoriteContents = favorites.map(f => ({
          ...f,
          title: 'Unknown',
          description: '',
          posterUrl: '',
          contentType: 'Unknown'
        }));
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  saveProfile() {
    const dto = {
      displayName: this.displayName,
      email: this.email,
      selectedGenreIds: this.selectedGenreIds,
      tags: this.tagsText.split(',').map(t => t.trim()).filter(t => t)
    };

    const userId = this.auth.getUserId() || '1';
    this.http.put(`http://localhost:5000/api/v1/user/profile/${userId}`, dto).subscribe({
      next: () => {
        if (isPlatformBrowser(this.platformId)) {
          localStorage.setItem('user_profile', JSON.stringify(dto));
        }
        alert('Profile saved to server and locally.');
      },
      error: (err) => {
        console.error('Failed to save profile to server', err);
        if (isPlatformBrowser(this.platformId)) {
          localStorage.setItem('user_profile', JSON.stringify(dto));
        }
        alert('Saved locally (server unavailable).');
      }
    });
  }

  toggleGenre(id: number) {
    const idx = this.selectedGenreIds.indexOf(id);
    if (idx >= 0) this.selectedGenreIds.splice(idx, 1);
    else this.selectedGenreIds.push(id);
  }

  switchTab(tab: string) {
    this.activeTab = tab;
    this.cdr.detectChanges();
  }

  openAdminAccess() {
    const password = prompt('Введіть пароль для доступу до адміністраторської панелі:');
    if (password === 'admin123') {
      this.router.navigate(['/admin']);
    } else if (password !== null) {
      alert('Невірний пароль!');
    }
  }

  goHome() { this.router.navigate(['/home']); }

  goToContent(contentId: number) { 
    this.router.navigate(['/content', contentId]);
  }

  removeFavorite(contentId: number) {
    this.favoriteContents = this.favoriteContents.filter(f => f.contentId !== contentId);
    this.cdr.detectChanges();
  }

  removeRating(contentId: number) {
    this.ratedContents = this.ratedContents.filter(r => r.contentId !== contentId);
    this.cdr.detectChanges();
  }
}
