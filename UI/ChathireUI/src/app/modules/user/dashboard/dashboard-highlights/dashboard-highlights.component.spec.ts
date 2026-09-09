import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DashboardHighlightsComponent } from './dashboard-highlights.component';

describe('DashboardHighlightsComponent', () => {
  let component: DashboardHighlightsComponent;
  let fixture: ComponentFixture<DashboardHighlightsComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [DashboardHighlightsComponent]
    });
    fixture = TestBed.createComponent(DashboardHighlightsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
