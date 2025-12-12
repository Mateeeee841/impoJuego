import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { SessionService } from '../services/session.service';

export const sessionInterceptor: HttpInterceptorFn = (req, next) => {
  const sessionService = inject(SessionService);
  const sessionId = sessionService.getSessionId();

  const clonedRequest = req.clone({
    setHeaders: {
      'X-Session-Id': sessionId
    }
  });

  return next(clonedRequest);
};
