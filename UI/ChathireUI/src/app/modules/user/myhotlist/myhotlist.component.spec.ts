import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyhotlistComponent } from './myhotlist.component';

describe('MyhotlistComponent', () => {
  let component: MyhotlistComponent;
  let fixture: ComponentFixture<MyhotlistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MyhotlistComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MyhotlistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
