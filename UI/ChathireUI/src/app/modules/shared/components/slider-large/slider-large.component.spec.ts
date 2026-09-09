import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SliderLargeComponent } from './slider-large.component';

describe('SliderLargeComponent', () => {
  let component: SliderLargeComponent;
  let fixture: ComponentFixture<SliderLargeComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [SliderLargeComponent]
    });
    fixture = TestBed.createComponent(SliderLargeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
