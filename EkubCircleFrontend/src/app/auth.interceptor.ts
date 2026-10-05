import { HttpInterceptorFn } from '@angular/common/http';

const API_BASE = 'http://localhost:5000';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('ekub_token');
  const isApiRequest = req.url.startsWith('/api') || req.url.startsWith(`${API_BASE}/api`);
  if (!token || !isApiRequest) return next(req);
  return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
