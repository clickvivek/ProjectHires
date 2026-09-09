import { Component, OnInit, Inject, Optional } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-hotlist-switch-confirmation-modal',
  templateUrl: './hotlist-switch-confirmation-modal.component.html',
  styleUrls: ['./hotlist-switch-confirmation-modal.component.scss']
})
export class HotlistSwitchConfirmationModalComponent implements OnInit {

  constructor(
    @Optional() @Inject(MAT_DIALOG_DATA) public statusId: any,
  ) { }

  ngOnInit(): void {
  }

}
