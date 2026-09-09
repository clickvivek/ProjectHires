import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyhotlistTableviewComponent } from './myhotlist-tableview.component';

describe('MyhotlistTableviewComponent', () => {
  let component: MyhotlistTableviewComponent;
  let fixture: ComponentFixture<MyhotlistTableviewComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [MyhotlistTableviewComponent]
    });
    fixture = TestBed.createComponent(MyhotlistTableviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
