import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { Store } from '@ngrx/store';
import { MockStore, provideMockStore } from '@ngrx/store/testing';

import { AuthActions } from '../../store/auth.actions';
import { initialAuthState } from '../../store/auth.reducer';
import { selectAuthError } from '../../store/auth.selectors';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  let fixture: ComponentFixture<LoginComponent>;
  let store: MockStore;
  let element: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        provideMockStore({ initialState: { auth: initialAuthState } }),
      ],
    }).compileComponents();

    store = TestBed.inject(MockStore);
    vi.spyOn(store, 'dispatch');
    fixture = TestBed.createComponent(LoginComponent);
    fixture.detectChanges();
    element = fixture.nativeElement as HTMLElement;
  });

  function setInput(testid: string, value: string): void {
    const input = element.querySelector<HTMLInputElement>(`[data-testid="${testid}"]`)!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  function submit(): void {
    element.querySelector<HTMLButtonElement>('[data-testid="login-submit"]')!.click();
    fixture.detectChanges();
  }

  it('renders the login form with a link to registration', () => {
    expect(element.querySelector('[data-testid="login-email"]')).not.toBeNull();
    expect(element.querySelector('[data-testid="login-password"]')).not.toBeNull();
    expect(element.querySelector('[data-testid="login-google"]')).not.toBeNull();
    expect(element.querySelector('[data-testid="login-to-register"]')).not.toBeNull();
  });

  it('does not dispatch when the form is invalid', () => {
    submit();

    expect(store.dispatch).not.toHaveBeenCalled();
    expect(element.querySelector('[data-testid="login-email-error"]')).not.toBeNull();
  });

  it('dispatches login with the entered credentials', () => {
    setInput('login-email', 'a@example.com');
    setInput('login-password', 'password123');
    submit();

    expect(store.dispatch).toHaveBeenCalledWith(
      AuthActions.login({ email: 'a@example.com', password: 'password123' }),
    );
  });

  it('shows the store error message', () => {
    store.overrideSelector(selectAuthError, 'Invalid credentials.');
    store.refreshState();
    fixture.detectChanges();

    expect(element.querySelector('[data-testid="login-error"]')?.textContent).toContain(
      'Invalid credentials.',
    );
  });

  it('explains that Google sign-in is not configured yet', async () => {
    element.querySelector<HTMLButtonElement>('[data-testid="login-google"]')!.click();
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(element.querySelector('[data-testid="login-google-error"]')?.textContent).toContain(
      'not configured',
    );
    expect(store.dispatch).not.toHaveBeenCalledWith(
      expect.objectContaining({ type: AuthActions.googleLogin.type }),
    );
  });
});
