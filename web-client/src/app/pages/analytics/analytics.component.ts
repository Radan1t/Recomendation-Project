import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router'; // Додано
import { AnalyticsService } from '../../core/services/analytics.service';
import { BaseChartDirective } from 'ng2-charts';
import { Chart, registerables, ChartConfiguration, ChartType, ChartData } from 'chart.js';
import { RecommendationService } from '../../core/services/recommendation.service';

Chart.register(...registerables);

@Component({
  selector: 'app-analytics',
  templateUrl: './analytics.component.html',
  styleUrls: ['./analytics.component.scss'],
  standalone: true,
  imports: [CommonModule, BaseChartDirective]
})
export class AnalyticsComponent implements OnInit {
  
  public pieChartType: ChartType = 'pie';
  public pieChartData: ChartData<'pie'> = { labels: [], datasets: [] };

  public lineChartType: ChartType = 'line';
  public lineChartData: ChartData<'line'> = { labels: [], datasets: [] };

  public chartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { 
        display: true, 
        position: 'bottom',
        labels: { color: '#ffffff', font: { family: 'Verdana', size: 12 } }
      }
    },
    scales: {
      x: { grid: { color: '#333' }, ticks: { color: '#aaa' } },
      y: { grid: { color: '#333' }, ticks: { color: '#aaa' } }
    }
  };

  constructor(
    private analyticsService: AnalyticsService,
    private cdr: ChangeDetectorRef,
    private router: Router, // Додано
    private recommendationService: RecommendationService
  ) {}

  ngOnInit(): void {
    this.loadGenreDistribution();
    this.loadActivityDynamics();
  }

  // =========================================
  // МЕТОДИ НАВІГАЦІЇ (ДЛЯ ХЕДЕРА)
  // =========================================
  goToHome() {
    this.router.navigate(['/home']);
  }
  // ДОДАНО: Перехід назад до адмін-панелі
  goToAdmin() {
    this.router.navigate(['/admin']); // Переконайтеся, що шлях відповідає вашому роутингу (наприклад '/admin')
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

  loadGenreDistribution() {
    this.analyticsService.getGenreDistribution().subscribe((data: any[]) => {
      if (data && data.length > 0) {
        this.pieChartData = {
          labels: data.map(d => d.name),
          datasets: [{
            data: data.map(d => d.value),
            backgroundColor: ['#e74c3c', '#3498db', '#f1c40f', '#2ecc71', '#9b59b6'],
            borderColor: '#1a1a1a',
            borderWidth: 2
          }]
        };
        this.cdr.detectChanges(); 
      }
    });
  }

  loadActivityDynamics() {
    this.analyticsService.getActivityDynamics().subscribe({
      next: (data: any[]) => {
        if (data && data.length > 0) {
          const normalized = data.map(d => ({ date: d.date ?? d.Date ?? d.DateRated ?? d.dateRated, count: d.count ?? d.Count ?? 0, type: d.type ?? d.Type ?? '' }));
          const labels = [...new Set(normalized.map(d => {
            const v = d.date;
            return v ? new Date(v).toLocaleDateString() : '';
          }))].filter(l => l);

          this.lineChartData = {
            labels: labels,
            datasets: [
              {
                data: labels.map(l => normalized.filter(d => d.type === 'Оцінки' && (new Date(d.date).toLocaleDateString()) === l).reduce((sum, x) => sum + (x.count || 0), 0)),
                label: 'Рейтинги',
                borderColor: '#f1c40f',
                backgroundColor: 'rgba(241, 196, 15, 0.1)',
                fill: true,
                tension: 0.4
              },
              {
                data: labels.map(l => normalized.filter(d => d.type === 'Обране' && (new Date(d.date).toLocaleDateString()) === l).reduce((sum, x) => sum + (x.count || 0), 0)),
                label: 'Обране',
                borderColor: '#ffffff',
                backgroundColor: 'rgba(255, 255, 255, 0.05)',
                fill: true,
                tension: 0.4
              }
            ]
          };
          this.cdr.detectChanges();
        }
      },
      error: (err: any) => {
        console.error('Failed to load activity dynamics', err);
      }
    });
  }
  
}