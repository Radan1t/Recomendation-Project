import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';


import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http'; 
import { authInterceptor } from './core/interceptors/auth.interceptor';


import { provideCharts, withDefaultRegisterables } from 'ng2-charts';

export const appConfig: ApplicationConfig = {
  providers: [
    
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor])
    ),
    
    
    provideRouter(routes),
    
    
    provideCharts(withDefaultRegisterables()) 
  ]
};
