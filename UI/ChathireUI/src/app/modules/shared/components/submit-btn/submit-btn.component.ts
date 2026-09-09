import { Component, Input } from '@angular/core';

@Component({
  selector: 'submit-btn',
  templateUrl: './submit-btn.component.html',
  styleUrls: ['./submit-btn.component.scss']
})
export class SubmitBtnComponent {

  @Input() isEdit;
  @Input() isSubmit;

  @Input() defaultText;
  @Input() editText?;

  @Input() defaultActionText?;
  @Input() editActionText?;

  @Input() buttonType = 'primary';

}
