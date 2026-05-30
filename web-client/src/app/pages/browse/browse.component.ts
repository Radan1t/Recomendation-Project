import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, debounceTime } from 'rxjs';
import { ContentService } from '../../core/services/content.service';
import { RecommendationService } from '../../core/services/recommendation.service';

@Component({
  selector: 'app-browse',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './browse.component.html',
  styleUrls: ['./browse.component.scss']
})
export class BrowseComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);
  private contentService = inject(ContentService);
  private recommendationService = inject(RecommendationService);

  // ДОДАНО: Створюємо посилання на глобальний об'єкт Math, щоб він був доступний у HTML
  Math = Math;

  type: string = 'all';
  items: any[] = [];
  genres: any[] = [];
  selectedGenreId?: number;
  searchQuery: string = '';
  private searchSubject = new Subject<string>();

  // pagination
  page: number = 1;
  pageSize: number = 20;
  total: number = 0;

  get totalPages(): number { return Math.max(1, Math.ceil(this.total / this.pageSize)); }
  get pageOptions(): number[] { return Array.from({length: this.totalPages}, (_, i) => i + 1); }
  get placeholders(): any[] { const n = Math.max(0, this.pageSize - (this.items?.length || 0)); return new Array(n); }

  ngOnInit() {
    // Підписуємося на зміни параметрів URL (наприклад, ?type=games)
    this.route.queryParams.subscribe(params => {
      const newType = params['type'] || 'all';
      
      // ДОДАНО: Скидання вибраного жанру при переході на інший тип контенту
      if (this.type !== newType) {
        this.selectedGenreId = undefined;
      }
      
      this.type = newType;
      this.page = 1;
      this.loadGenres();
      this.loadList();
    });

    // Налаштовуємо затримку для пошуку, щоб не спамити бекенд запитами
    this.searchSubject.pipe(debounceTime(300)).subscribe(q => {
      this.page = 1;
      this.loadList(q);
    });
  }

  // Завантажити жанри для фільтрації
  loadGenres() {
    const typeParam = this.type && this.type !== 'all' ? this.type : undefined;
    this.contentService.getGenres(typeParam).subscribe({
      next: (g) => {
        this.genres = g || [];
        this.cdr.detectChanges();
      },
      error: (err) => { 
        this.genres = []; 
      }
    });
  }

  // Обробник вводу в поле пошуку
  onSearchChange() {
    this.searchSubject.next(this.searchQuery);
  }

  onGenreChange(id?: number) {
    this.selectedGenreId = id;
    this.page = 1;
    this.loadList();
  }

  // Завантаження списку контенту з бекенду
  loadList(q?: string) {
    const t = this.type === 'all' ? 'all' : this.type;
    this.contentService.getContentList(t, q || undefined, this.selectedGenreId, this.page, this.pageSize).subscribe({
        next: (res) => {
          // support both PascalCase (backend) and camelCase (frontend) JSON
          this.items = (res && (res.items || res.Items)) || [];
          this.total = (res && (res.total || res.Total)) || 0;
          this.cdr.detectChanges();
        },
        error: (err) => {
        console.error('Failed to load list', err);
        this.items = [];
        this.total = 0;
        this.cdr.detectChanges();
      }
    });
  }

  prevPage() {
    if (this.page > 1) {
      this.page -= 1;
      this.loadList();
    }
  }

  nextPage() {
    const totalPages = Math.max(1, Math.ceil(this.total / this.pageSize));
    if (this.page < totalPages) {
      this.page += 1;
      this.loadList();
    }
  }

  // =========================================
  // МЕТОДИ НАВІГАЦІЇ (ДЛЯ ХЕДЕРА ТА КАРТОК)
  // =========================================

  // Перехід на детальну сторінку контенту
  goToContent(id: number) {
    this.router.navigate(['/content', id]);
  }

  // Перехід на головну сторінку (натискання на логотип)
  goToHome() {
    this.router.navigate(['/home']);
  }

  // Перехід на сторінку профілю (натискання на аватар)
  goToProfile() {
    this.router.navigate(['/profile']);
  }

  // Зміна категорії через випадаюче меню в хедері
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

  // Логіка виходу з акаунту
  logout() {
    // Якщо у вас є AuthService, краще викликати this.authService.logout()
    // Наразі просто очищаємо токен та переходимо на сторінку логіну
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem('token'); 
    }
    this.router.navigate(['/login']);
  }

  // Обробка помилки завантаження зображення
  onImageError(event: Event) {
    const target = event.target as HTMLImageElement;
    target.src = 'https://placehold.co/220x330/222/555?text=No+Image';
  }
}