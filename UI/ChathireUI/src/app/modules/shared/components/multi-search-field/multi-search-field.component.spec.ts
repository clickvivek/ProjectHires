import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MultiSearchFieldComponent } from './multi-search-field.component';

describe('MultiSearchFieldComponent', () => {
  let component: MultiSearchFieldComponent;
  let fixture: ComponentFixture<MultiSearchFieldComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MultiSearchFieldComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MultiSearchFieldComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
