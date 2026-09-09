import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdvFilterResetComponent } from './adv-filter-reset.component';

describe('AdvFilterResetComponent', () => {
  let component: AdvFilterResetComponent;
  let fixture: ComponentFixture<AdvFilterResetComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [AdvFilterResetComponent]
    });
    fixture = TestBed.createComponent(AdvFilterResetComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
