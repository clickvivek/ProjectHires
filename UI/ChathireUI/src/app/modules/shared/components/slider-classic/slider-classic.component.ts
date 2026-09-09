import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'slider-classic',
  templateUrl: './slider-classic.component.html',
  styleUrls: ['./slider-classic.component.scss']
})
export class SliderClassicComponent {

  @Input() data;
  @Input() selectedItem;

  @Input() activeText;
  @Input() inActiveText="";


  @Output() outputparams = new EventEmitter();

  onSliderChange(status) {
    let newData = {
      item: this.selectedItem,
      status: status
    }
   this.outputparams.emit(newData)
  }

}
