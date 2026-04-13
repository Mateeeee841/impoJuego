import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('sin sesión previa isLoggedIn() === false', () => {
    expect(service.isLoggedIn()).toBeFalse();
  });

  it('login exitoso guarda token y user', () => {
    const fakeUser = { id: 1, email: 'a@b.com', role: 'User' as const };
    const fakeResponse = { success: true, message: 'ok', data: { token: 't0k3n', user: fakeUser } };

    service.login('a@b.com', 'password123').subscribe();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/auth/login`);
    expect(req.request.method).toBe('POST');
    req.flush(fakeResponse);

    expect(localStorage.getItem('impojuego_token')).toBe('t0k3n');
    expect(service.isLoggedIn()).toBeTrue();
    expect(service.user()?.email).toBe('a@b.com');
  });

  it('logout limpia storage y signal', () => {
    localStorage.setItem('impojuego_token', 'old-token');
    localStorage.setItem('impojuego_user', JSON.stringify({ id: 1, email: 'x', role: 'User' }));

    // re-crear service para que pickup desde storage
    service = TestBed.inject(AuthService);

    service.logout();

    expect(localStorage.getItem('impojuego_token')).toBeNull();
    expect(service.isLoggedIn()).toBeFalse();
  });
});
