import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MultiSelectCheckFieldComponent } from './multi-select-check-field.component';

describe('MultiSelectCheckFieldComponent', () => {
  let component: MultiSelectCheckFieldComponent;
  let fixture: ComponentFixture<MultiSelectCheckFieldComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [MultiSelectCheckFieldComponent]
    });
    fixture = TestBed.createComponent(MultiSelectCheckFieldComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
