import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Opportunity } from '../../models/opportunity.model';
import { OpportunityListComponent } from './opportunity-list.component';

const opportunities: readonly Opportunity[] = [
  {
    id: '1',
    title: 'Beach Cleanup',
    description: 'Collect plastic waste.',
    location: "Cox's Bazar",
    isRemote: false,
    startsAtUtc: '2026-06-01T09:30:00Z',
  },
  {
    id: '2',
    title: 'Accessibility Audit',
    description: 'Review a charity site.',
    location: 'Remote',
    isRemote: true,
    startsAtUtc: '2026-07-01T09:30:00Z',
  },
];

describe('OpportunityListComponent', () => {
  let fixture: ComponentFixture<OpportunityListComponent>;

  function render(input: readonly Opportunity[]): HTMLElement {
    fixture.componentRef.setInput('opportunities', input);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [OpportunityListComponent] }).compileComponents();
    fixture = TestBed.createComponent(OpportunityListComponent);
  });

  it('renders one entry per opportunity', () => {
    const element = render(opportunities);

    const titles = Array.from(element.querySelectorAll('[data-testid="opportunity-title"]'));
    expect(titles.map((t) => t.textContent?.trim())).toEqual(['Beach Cleanup', 'Accessibility Audit']);
  });

  it('labels remote and on-site opportunities differently', () => {
    const element = render(opportunities);

    expect(element.textContent).toContain('On site');
    expect(element.textContent).toContain('Remote');
  });

  it('renders nothing when given an empty list', () => {
    const element = render([]);

    expect(element.querySelectorAll('[data-testid="opportunity-title"]').length).toBe(0);
  });
});
