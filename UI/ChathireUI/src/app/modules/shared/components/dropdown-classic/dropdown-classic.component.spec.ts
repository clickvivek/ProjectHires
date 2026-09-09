import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DropdownClassicComponent } from './dropdown-classic.component';

describe('DropdownClassicComponent', () => {
  let component: DropdownClassicComponent;
  let fixture: ComponentFixture<DropdownClassicComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [DropdownClassicComponent]
    });
    fixture = TestBed.createComponent(DropdownClassicComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
