import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';

import { Organization } from '../../models/organization.model';
import { OrganizationFormComponent } from './organization-form.component';

const organization: Organization = {
  id: 'org-1',
  ownerUserId: 'u-9',
  name: 'River Cleaners',
  type: 'Nonprofit',
  causeTags: ['environment'],
  location: 'Dhaka',
  description: 'Cleaning rivers.',
  verifiedAtUtc: null,
  isVerified: false,
};

describe('OrganizationFormComponent', () => {
  let fixture: ComponentFixture<OrganizationFormComponent>;
  let element: HTMLElement;

  async function setup(input: Organization | null): Promise<void> {
    await TestBed.configureTestingModule({
      imports: [OrganizationFormComponent],
      providers: [provideNoopAnimations()],
    }).compileComponents();

    fixture = TestBed.createComponent(OrganizationFormComponent);
    fixture.componentRef.setInput('organization', input);
    fixture.detectChanges();
    element = fixture.nativeElement as HTMLElement;
  }

  function setInput(testid: string, value: string): void {
    const input = element.querySelector<HTMLInputElement | HTMLTextAreaElement>(
      `[data-testid="${testid}"]`,
    )!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  it('prefills the form when an organization is provided', async () => {
    await setup(organization);

    expect(element.querySelector<HTMLInputElement>('[data-testid="org-name"]')?.value).toBe(
      'River Cleaners',
    );
    expect(element.querySelector('[data-testid="org-save"]')?.textContent).toContain(
      'Save changes',
    );
  });

  it('labels the submit button for registration when empty', async () => {
    await setup(null);

    expect(element.querySelector('[data-testid="org-save"]')?.textContent).toContain(
      'Register organization',
    );
  });

  it('emits parsed cause tags on save', async () => {
    await setup(null);
    const emitted: unknown[] = [];
    fixture.componentInstance.save.subscribe((payload) => emitted.push(payload));

    setInput('org-name', 'River Cleaners');
    setInput('org-type', 'Nonprofit');
    setInput('org-cause-tags', 'environment, education');
    setInput('org-location', 'Dhaka');
    setInput('org-description', 'Cleaning rivers.');

    element.querySelector<HTMLButtonElement>('[data-testid="org-save"]')!.click();
    fixture.detectChanges();

    expect(emitted).toEqual([
      {
        name: 'River Cleaners',
        type: 'Nonprofit',
        causeTags: ['environment', 'education'],
        location: 'Dhaka',
        description: 'Cleaning rivers.',
      },
    ]);
  });

  it('does not emit when required fields are missing', async () => {
    await setup(null);
    const emitted: unknown[] = [];
    fixture.componentInstance.save.subscribe((payload) => emitted.push(payload));

    element.querySelector<HTMLButtonElement>('[data-testid="org-save"]')!.click();
    fixture.detectChanges();

    expect(emitted).toHaveLength(0);
    expect(element.querySelector('[data-testid="org-name-error"]')).not.toBeNull();
  });
});
