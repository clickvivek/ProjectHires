import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdvFilterExperienceLevelComponent } from './adv-filter-experience-level.component';

describe('AdvFilterExperienceLevelComponent', () => {
  let component: AdvFilterExperienceLevelComponent;
  let fixture: ComponentFixture<AdvFilterExperienceLevelComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [AdvFilterExperienceLevelComponent]
    });
    fixture = TestBed.createComponent(AdvFilterExperienceLevelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
