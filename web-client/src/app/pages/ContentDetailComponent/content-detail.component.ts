import { Component, OnInit, inject, ChangeDetectorRef, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ContentService } from '../../core/services/content.service';
import { InteractionService } from '../../core/services/interaction.service';
import { RecommendationService } from '../../core/services/recommendation.service';

export interface ContentDetail {
  id: number;
  title: string;
  description: string;
  genres: string;
  releaseDate: string;
  posterUrl: string;
  contentType?: string;

  // Опціональні поля для різних типів контенту
  developer?: string;
  publisher?: string;
  director?: string;
  author?: string;
  pages?: number;
  durationMinutes?: number;
  seasonCount?: number;
  episodeCount?: number;
  status?: string;
  pegi?: string;
  cast?: string; 
  country?: string;
  creator?: string;
  isbn?: string;

  // Рейтинги
  siteAverageRating: number; // Зовнішній загальний рейтинг (RAWG/TMDB)
  platformRating: number;    // Внутрішній рейтинг платформи
  userRating: number | null; // Оцінка поточного користувача
  
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
  private recommendationService = inject(RecommendationService);
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
          description: data.Description || data.description || 'Опис відсутній',
          genres: data.Genres || data.genres || 'Не вказано',
          releaseDate: data.ReleaseDate || data.releaseDate || 'Дата невідома',
          posterUrl: data.PosterUrl || data.posterUrl || 'https://placehold.co/400x600/1a1a1a/ffffff?text=No+Poster',
          contentType: data.ContentType || data.contentType || data.type || '',

          developer: data.Developer || data.developer,
          publisher: data.Publisher || data.publisher,
          director: data.Director || data.director,
          author: data.Author || data.author,
          pages: data.Pages || data.pages,
          durationMinutes: data.DurationMinutes || data.durationMinutes,
          seasonCount: data.SeasonCount || data.seasonCount,
          episodeCount: data.EpisodeCount || data.episodeCount || data.EpisodesCount,
          status: data.Status || data.status,
          pegi: data.Pegi || data.pegi,
          cast: data.Cast || data.cast || data.Crew,
          country: data.Country || data.country,
          creator: data.Creator || data.creator,
          isbn: data.ISBN || data.Isbn || data.isbn,

          // Мапінг трьох видів рейтингу (враховуємо опечатку AvarageRating в бекенді)
          siteAverageRating: data.AvarageRating || data.AverageRating || data.averageRating || 0.0,
          platformRating: data.PlatformRating || data.platformRating || 0.0,
          userRating: null, 
          
          similarContent: []
        };
        
        this.loadRecommendations(this.content.id);

        // Завантажити середній рейтинг платформи (з InteractionService)
        this.interactionService.getContentAverage(this.content.id).subscribe({
          next: (res: any) => {
            // API may return PascalCase or camelCase
            this.content!.platformRating = res?.average ?? res?.Average ?? res?.avg ?? 0.0;
            this.cdr.detectChanges();

            // Після того як отримали середній рейтинг, також перевірити статус поточного користувача
            // (це допомагає коли запит статусу міг повернутися раніше, але токен/кліма ще не були доступні)
            this.checkInteractionStatus(String(this.content!.id));
          },
          error: (err: any) => {
            console.warn('Не вдалося отримати середній рейтинг платформи:', err);
            this.content!.platformRating = 0.0;
            this.cdr.detectChanges();

            // Спробувати все одно перевірити статус
            this.checkInteractionStatus(String(this.content!.id));
          }
        });

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
        // Support multiple casing variants from different backends (PascalCase, camelCase)
        this.isFavorite = (status?.IsFavorite ?? status?.isFavorite ?? false) as boolean;
        const usrScore = status?.UserScore ?? status?.userScore ?? status?.score ?? null;
        if (this.content) {
          this.content.userRating = usrScore;
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

  isType(t: string) {
    const ct = (this.content?.contentType || '').toLowerCase();
    return !!ct && ct.includes(t.toLowerCase());
  }

  // =========================================
  // МЕТОДИ НАВІГАЦІЇ
  // =========================================
  goToHome() {
    this.router.navigate(['/home']);
  }

  goToProfile() {
    this.router.navigate(['/profile']);
  }

  navigateTo(category: string) {
    this.router.navigate(['/browse'], { queryParams: { type: category } });
  }

  unexpectedRecommendation() {
    const uidStr = typeof localStorage !== 'undefined' ? localStorage.getItem('user_id') : null;
    const uid = uidStr ? Number(uidStr) : null;
    if (!uid) {
      alert('User ID not found. Please login as a user to get recommendations.');
      return;
    }
    this.recommendationService.generateRecommendations(uid).subscribe({
      next: (res: any) => {
        const recs = res?.recommendations || [];
        if (!recs.length) {
          alert('No recommendations available.');
          return;
        }
        const rand = recs[Math.floor(Math.random() * recs.length)];
        const cid = rand.ContentID || rand.contentID || rand.id;
        if (cid) {
          this.router.navigate(['/content', cid]);
        } else {
          alert('Cannot determine content id from recommendation.');
        }
      },
      error: (err: any) => {
        console.error('Recommendation error', err);
        alert('Failed to fetch recommendations.');
      }
    });
  }

  logout() {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.clear();
    }
    this.router.navigate(['/']);
  }
}