import { Component, Input } from '@angular/core';

@Component({
  selector: 'slider-large',
  templateUrl: './slider-large.component.html',
  styleUrls: ['./slider-large.component.scss']
})
export class SliderLargeComponent {

  @Input() data;
  @Input() activeText;
  @Input() inActiveText="";

}
