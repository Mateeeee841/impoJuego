import { TestBed } from '@angular/core/testing';
import { SessionService } from './session.service';

describe('SessionService', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
  });

  it('debería generar un UUID en el primer acceso', () => {
    const service = TestBed.inject(SessionService);
    const id = service.getSessionId();
    expect(id).toBeTruthy();
    expect(id.length).toBeGreaterThan(10);
  });

  it('debería reutilizar el mismo id entre llamadas', () => {
    const service = TestBed.inject(SessionService);
    const id1 = service.getSessionId();
    const id2 = service.getSessionId();
    expect(id1).toBe(id2);
  });

  it('resetSession debería generar un id distinto', () => {
    const service = TestBed.inject(SessionService);
    const oldId = service.getSessionId();
    const newId = service.resetSession();
    expect(newId).not.toBe(oldId);
    expect(service.getSessionId()).toBe(newId);
  });

  it('debería persistir el id en localStorage', () => {
    const service = TestBed.inject(SessionService);
    const id = service.getSessionId();
    expect(localStorage.getItem('impojuego_session_id')).toBe(id);
  });
});
