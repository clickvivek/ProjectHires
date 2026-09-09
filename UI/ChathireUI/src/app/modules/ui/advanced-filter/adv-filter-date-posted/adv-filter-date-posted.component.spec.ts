import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdvFilterDatePostedComponent } from './adv-filter-date-posted.component';

describe('AdvFilterDatePostedComponent', () => {
  let component: AdvFilterDatePostedComponent;
  let fixture: ComponentFixture<AdvFilterDatePostedComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [AdvFilterDatePostedComponent]
    });
    fixture = TestBed.createComponent(AdvFilterDatePostedComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
