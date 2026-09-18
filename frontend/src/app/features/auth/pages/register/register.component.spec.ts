import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { MockStore, provideMockStore } from '@ngrx/store/testing';

import { AuthActions } from '../../store/auth.actions';
import { initialAuthState } from '../../store/auth.reducer';
import { RegisterComponent } from './register.component';

describe('RegisterComponent', () => {
  let fixture: ComponentFixture<RegisterComponent>;
  let store: MockStore;
  let element: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegisterComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        provideMockStore({ initialState: { auth: initialAuthState } }),
      ],
    }).compileComponents();

    store = TestBed.inject(MockStore);
    vi.spyOn(store, 'dispatch');
    fixture = TestBed.createComponent(RegisterComponent);
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
    element.querySelector<HTMLButtonElement>('[data-testid="register-submit"]')!.click();
    fixture.detectChanges();
  }

  it('offers volunteer and organization admin roles', async () => {
    element.querySelector<HTMLElement>('[data-testid="register-role"]')!.click();
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const options = document.querySelectorAll('[data-testid^="register-role-"]');
    const labels = Array.from(options).map((option) => option.textContent?.trim());
    expect(labels).toContain('Volunteer');
    expect(labels).toContain('Organization admin');
  });

  it('does not dispatch when the password is too short', () => {
    setInput('register-email', 'a@example.com');
    setInput('register-password', 'short');
    submit();

    expect(store.dispatch).not.toHaveBeenCalled();
    expect(element.querySelector('[data-testid="register-password-error"]')).not.toBeNull();
  });

  it('dispatches register with the entered details and default role', () => {
    setInput('register-email', 'a@example.com');
    setInput('register-password', 'password123');
    submit();

    expect(store.dispatch).toHaveBeenCalledWith(
      AuthActions.register({
        email: 'a@example.com',
        password: 'password123',
        role: 'volunteer',
      }),
    );
  });
});
