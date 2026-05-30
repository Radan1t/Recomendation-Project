import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router'; 
import { AdminService } from '../../core/services/admin.service';
import { RecommendationService } from '../../core/services/recommendation.service';

type ViewState = 'hub' | 'games' | 'movies' | 'series' | 'books';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit {
  currentView: ViewState = 'hub';
  loading = false; 
  isTableLoading = false; 

  allTableData: any[] = []; 
  filteredData: any[] = []; 
  currentTableData: any[] = []; 

  searchQuery: string = '';

  currentPage = 1;
  pageSize = 100;
  totalPages = 1;

  gamePage = 1;
  moviePage = 1;
  seriesPage = 1;
  bookSubject = 'fiction';
  bookStartIndex = 0;
  bookGenres = [
    { name: 'Fiction (Художня література)', value: 'fiction' },
    { name: 'Fantasy (Фентезі)', value: 'fantasy' },
    { name: 'Science Fiction (Наукова фантастика)', value: 'science fiction' },
    { name: 'Mystery (Містика / Детектив)', value: 'mystery' },
    { name: 'Thriller (Трилер)', value: 'thriller' },
    { name: 'Romance (Романтика)', value: 'romance' },
    { name: 'Horror (Жахи)', value: 'horror' },
    { name: 'Historical Fiction (Історичний роман)', value: 'historical fiction' },
    { name: 'Comics (Комікси)', value: 'comics' },
    { name: 'History (Історія)', value: 'history' },
    { name: 'Biography (Біографії)', value: 'biography' },
    { name: 'Psychology (Психологія)', value: 'psychology' },
    { name: 'Philosophy (Філософія)', value: 'philosophy' },
    { name: 'Business (Бізнес)', value: 'business' },
    { name: 'Art (Мистецтво)', value: 'art' },
    { name: 'Travel (Подорожі)', value: 'travel' },
    { name: 'Computers (Комп\'ютери та IT)', value: 'computers' },
    { name: 'Programming (Програмування)', value: 'programming' },
    { name: 'Science (Наука)', value: 'science' },
    { name: 'Mathematics (Математика)', value: 'mathematics' }
  ];

  constructor(
    private adminService: AdminService, 
    private cdr: ChangeDetectorRef,
    private router: Router,
    private recommendationService: RecommendationService
  ) {}

  ngOnInit(): void {}

  // =========================================
  // МЕТОДИ НАВІГАЦІЇ (ДЛЯ ХЕДЕРА)
  // =========================================
  goToHome() {
    this.router.navigate(['/home']);
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
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem('token'); 
    }
    this.router.navigate(['/login']);
  }

  // =========================================
  // ЛОГІКА АДМІНКИ
  // =========================================
  setView(view: ViewState) {
    this.currentView = view;
    if (view !== 'hub') {
      this.currentPage = 1;
      this.searchQuery = ''; 
      this.allTableData = [];
      this.filteredData = [];
      this.currentTableData = [];
      this.loadTableData(view);
    }
  }

  loadTableData(category: ViewState) {
    this.isTableLoading = true;
    this.searchQuery = ''; 
    this.cdr.detectChanges();

    this.adminService.getContent(category).subscribe({
      next: (data: any) => { 
        let finalData: any[] = [];
        
        if (Array.isArray(data)) {
          finalData = data;
        } else if (typeof data === 'string') {
          try { 
            let parsed = JSON.parse(data); 
            finalData = typeof parsed === 'string' ? JSON.parse(parsed) : parsed;
          } catch(e) { console.error('Парсинг помилка', e); }
        } else if (data && typeof data === 'object') {
          const arrayKey = Object.keys(data).find(key => Array.isArray(data[key]));
          if (arrayKey) finalData = data[arrayKey];
        }

        this.allTableData = finalData;
        this.filteredData = [...this.allTableData]; 
        this.updatePaginatedData();
        this.isTableLoading = false;
        this.cdr.detectChanges(); 
      },
      error: (err: any) => { 
        console.error('Помилка завантаження', err);
        this.isTableLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  onSearch() {
    this.currentPage = 1; 
    if (!this.searchQuery.trim()) {
      this.filteredData = [...this.allTableData]; 
    } else {
      const query = this.searchQuery.toLowerCase().trim();
      this.filteredData = this.allTableData.filter(item => {
        const title = (item.title || item.Title || '').toLowerCase();
        return title.includes(query); 
      });
    }
    this.updatePaginatedData();
  }

  updatePaginatedData() {
    this.totalPages = Math.ceil(this.filteredData.length / this.pageSize) || 1; 
    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.currentTableData = this.filteredData.slice(startIndex, endIndex);
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.updatePaginatedData();
      window.scrollTo(0, 0);
      this.cdr.detectChanges();
    }
  }

  prevPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.updatePaginatedData();
      window.scrollTo(0, 0);
      this.cdr.detectChanges();
    }
  }

  onAddManual() {
    const newItem = { 
      Id: Math.floor(Math.random() * 10000), 
      Title: 'Новий тестовий запис (' + this.currentView + ')'
    };
    this.allTableData.unshift(newItem);
    this.onSearch(); 
    this.cdr.detectChanges();
  }
  
  onEditItem(item: any) {
    const itemId = item.id || item.Id;
    alert(`Редагування ID: ${itemId} (У розробці 🛠️)`);
  }

  onDeleteItem(item: any) {
    const itemId = item.id || item.Id;
    const itemTitle = item.title || item.Title;

    if (confirm(`Видалити "${itemTitle}"?`)) {
      this.adminService.deleteContent(itemId).subscribe({
        next: () => {
          this.allTableData = this.allTableData.filter(i => (i.id || i.Id) !== itemId);
          this.onSearch(); 
          this.cdr.detectChanges();
          alert('Видалено!');
        },
        error: (err) => alert('Помилка при видаленні')
      });
    }
  }

  onImportGames() { this.runImport(this.adminService.importGames(this.gamePage)); }
  onImportMovies() { this.runImport(this.adminService.importMovies(this.moviePage)); }
  onImportSeries() { this.runImport(this.adminService.importSeries(this.seriesPage)); }
  onImportBooks() { 
      this.runImport(this.adminService.importBooks(this.bookSubject, this.bookStartIndex)); 
  }
  
  private runImport(obs: any) {
    this.loading = true;
    obs.subscribe({
      next: (res: any) => {
        alert(res.message || 'Успішно!');
        this.loading = false;
        this.loadTableData(this.currentView);
      },
      error: () => { alert('Помилка імпорту'); this.loading = false; }
    });
  }
}