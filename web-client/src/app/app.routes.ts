import { Routes } from '@angular/router';
import { LandingComponent } from './pages/landing/landing.component';
import { HomeComponent } from './pages/home/home.component'; 
import { ContentDetailComponent } from './pages/ContentDetailComponent/content-detail.component';
import { AnalyticsComponent } from './pages/analytics/analytics.component';
import { AdminDashboardComponent } from './pages/admin-dashboard/admin-dashboard.component';

export const routes: Routes = [
  { path: '', component: LandingComponent },
  { path: 'home', component: HomeComponent },
  { path: 'content/:id', component: ContentDetailComponent },
  { path: 'analytics', component: AnalyticsComponent  },
  { path: 'admin', component: AdminDashboardComponent },
  { path: '**', redirectTo: '' }
];
