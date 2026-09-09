import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SliderClassicComponent } from './slider-classic.component';

describe('SliderClassicComponent', () => {
  let component: SliderClassicComponent;
  let fixture: ComponentFixture<SliderClassicComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [SliderClassicComponent]
    });
    fixture = TestBed.createComponent(SliderClassicComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
