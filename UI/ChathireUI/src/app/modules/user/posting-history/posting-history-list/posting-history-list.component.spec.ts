import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PostingHistoryListComponent } from './posting-history-list.component';

describe('PostingHistoryListComponent', () => {
  let component: PostingHistoryListComponent;
  let fixture: ComponentFixture<PostingHistoryListComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [PostingHistoryListComponent]
    });
    fixture = TestBed.createComponent(PostingHistoryListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
