import { Component, OnInit, inject, ChangeDetectorRef, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ContentService } from '../../core/services/content.service';
import { InteractionService } from '../../core/services/interaction.service';

export interface ContentDetail {
  id: number;
  title: string;
  description: string;
  genres: string;
  releaseDate: string;
  posterUrl: string;
  
  
  developer?: string;
  publisher?: string;
  director?: string;
  author?: string;
  pages?: number;
  durationMinutes?: number;
  seasonCount?: number;
  status?: string;
  pegi?: string;
  
  siteAverageRating: number;
  userRating: number | null;
  similarContent: any[];
}

@Component({
  selector: 'app-content-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './content-detail.component.html',
  styleUrls: ['./content-detail.component.scss']
})
export class ContentDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private contentService = inject(ContentService);
  private interactionService = inject(InteractionService);
  private cdr = inject(ChangeDetectorRef);
  
  private platformId = inject(PLATFORM_ID);

  content: ContentDetail | null = null;
  isFavorite: boolean = false; 
  hoveredStar: number = 0;
  isLoading: boolean = true;

  ngOnInit() {
    
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.loadContentDetails(id);
        this.checkInteractionStatus(id);
      } else {
        this.router.navigate(['/home']);
      }
    });
  }

  loadContentDetails(id: string) {
    this.isLoading = true;

    
    if (isPlatformBrowser(this.platformId)) {
      window.scrollTo(0, 0);
    }

    this.contentService.getContentDetails(id).subscribe({
      next: (data: any) => {
        this.content = {
          id: data.Id || data.id,
          title: data.Title || data.title,
          description: data.Description || 'Опис відсутній',
          genres: data.Genres || 'Не вказано',
          releaseDate: data.ReleaseDate || 'Дата невідома',
          posterUrl: data.PosterUrl || 'https://placehold.co/400x600/1a1a1a/ffffff?text=No+Poster',
          
          developer: data.Developer,
          publisher: data.Publisher,
          director: data.Director,
          author: data.Author,
          pages: data.Pages,
          durationMinutes: data.DurationMinutes,
          seasonCount: data.SeasonCount,
          status: data.Status,
          pegi: data.Pegi,
          
          siteAverageRating: data.AverageRating || 0.0,
          userRating: null, 
          similarContent: []
        };
        
        this.loadRecommendations(this.content.id);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        console.error('Помилка завантаження контенту:', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  checkInteractionStatus(id: string) {
    this.interactionService.getInteractionStatus(id).subscribe({
      next: (status) => {
        this.isFavorite = status.IsFavorite;
        if (this.content) {
          this.content.userRating = status.UserScore;
        }
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        console.warn('Статус взаємодії недоступний (401/404):', err);
        this.isFavorite = false;
        if (this.content) this.content.userRating = 0;
        this.cdr.detectChanges();
      }
    });
  }

  loadRecommendations(contentId: number) {
    this.contentService.getRecommendations(contentId).subscribe({
      next: (similar: any[]) => {
        if (this.content) {
          this.content.similarContent = similar;
          this.cdr.detectChanges();
        }
      },
      error: (err: any) => {
        console.log('Рекомендації не знайдені або сервіс офлайн:', err);
        if (this.content) this.content.similarContent = [];
        this.cdr.detectChanges();
      }
    });
  }

  rateContent(stars: number) {
    if (!this.content) return;

    this.interactionService.rateContent(this.content.id, stars).subscribe({
      next: () => {
        this.content!.userRating = stars;
        this.cdr.detectChanges();
      },
      error: (err: any) => console.error('Помилка при виставленні оцінки:', err)
    });
  }

  toggleFavorite() {
    if (!this.content) return;
    this.interactionService.toggleFavorite(this.content.id).subscribe({
      next: (res) => {
        this.isFavorite = res.isFavorite;
        this.cdr.detectChanges();
      },
      error: (err: any) => console.error('Помилка при зміні стану Favorite:', err)
    });
  }

  onImageError(event: Event) {
    (event.target as HTMLImageElement).src = 'https://placehold.co/400x600/1a1a1a/ffffff?text=No+Poster';
  }

  logout() {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.clear();
    }
    this.router.navigate(['/']);
  }
}
