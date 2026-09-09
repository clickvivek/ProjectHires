import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdvFilterWorkModelComponent } from './adv-filter-work-model.component';

describe('AdvFilterWorkModelComponent', () => {
  let component: AdvFilterWorkModelComponent;
  let fixture: ComponentFixture<AdvFilterWorkModelComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [AdvFilterWorkModelComponent]
    });
    fixture = TestBed.createComponent(AdvFilterWorkModelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
