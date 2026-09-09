import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PostingHistoryComponent } from './posting-history.component';

describe('PostingHistoryComponent', () => {
  let component: PostingHistoryComponent;
  let fixture: ComponentFixture<PostingHistoryComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [PostingHistoryComponent]
    });
    fixture = TestBed.createComponent(PostingHistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
