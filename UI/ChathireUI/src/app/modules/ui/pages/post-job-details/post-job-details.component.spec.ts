import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PostJobDetailsComponent } from './post-job-details.component';

describe('PostJobDetailsComponent', () => {
  let component: PostJobDetailsComponent;
  let fixture: ComponentFixture<PostJobDetailsComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [PostJobDetailsComponent]
    });
    fixture = TestBed.createComponent(PostJobDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
