import { HttpInterceptorFn } from '@angular/common/http';
import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  
  const platformId = inject(PLATFORM_ID);

  let token = null;

  
  if (isPlatformBrowser(platformId)) {
    token = localStorage.getItem('token');
  }

  
  if (token) {
  const uid = isPlatformBrowser(platformId) ? localStorage.getItem('user_id') : null;
  const headers: any = { Authorization: `Bearer ${token}` };
  if (uid) headers['X-User-Id'] = uid;
  const authReq = req.clone({
    setHeaders: headers
  });
  return next(authReq);
  }

  // Also forward X-User-Id when present even without token (best-effort)
  if (isPlatformBrowser(platformId)) {
  const uid = localStorage.getItem('user_id');
  if (uid) {
    const reqWithUid = req.clone({ setHeaders: { 'X-User-Id': uid } });
    return next(reqWithUid);
  }
  }

  return next(req);
};
