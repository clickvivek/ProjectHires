import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HomeSearchTabsComponent } from './home-search-tabs.component';

describe('HomeSearchTabsComponent', () => {
  let component: HomeSearchTabsComponent;
  let fixture: ComponentFixture<HomeSearchTabsComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [HomeSearchTabsComponent]
    });
    fixture = TestBed.createComponent(HomeSearchTabsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
