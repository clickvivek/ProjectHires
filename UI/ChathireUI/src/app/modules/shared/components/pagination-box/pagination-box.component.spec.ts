import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PaginationBoxComponent } from './pagination-box.component';

describe('PaginationBoxComponent', () => {
  let component: PaginationBoxComponent;
  let fixture: ComponentFixture<PaginationBoxComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PaginationBoxComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PaginationBoxComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
