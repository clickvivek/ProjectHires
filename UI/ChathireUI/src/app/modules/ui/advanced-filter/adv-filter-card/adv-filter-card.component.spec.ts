import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdvFilterCardComponent } from './adv-filter-card.component';

describe('AdvFilterCardComponent', () => {
  let component: AdvFilterCardComponent;
  let fixture: ComponentFixture<AdvFilterCardComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [AdvFilterCardComponent]
    });
    fixture = TestBed.createComponent(AdvFilterCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
