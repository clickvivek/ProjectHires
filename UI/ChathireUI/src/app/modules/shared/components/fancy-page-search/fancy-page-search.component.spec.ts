import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FancyPageSearchComponent } from './fancy-page-search.component';

describe('FancyPageSearchComponent', () => {
  let component: FancyPageSearchComponent;
  let fixture: ComponentFixture<FancyPageSearchComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [FancyPageSearchComponent]
    });
    fixture = TestBed.createComponent(FancyPageSearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
