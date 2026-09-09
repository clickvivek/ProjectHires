import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdvFilterVisaComponent } from './adv-filter-visa.component';

describe('AdvFilterVisaComponent', () => {
  let component: AdvFilterVisaComponent;
  let fixture: ComponentFixture<AdvFilterVisaComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [AdvFilterVisaComponent]
    });
    fixture = TestBed.createComponent(AdvFilterVisaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
