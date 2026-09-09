import { Component, HostListener } from '@angular/core';

@Component({
  selector: 'adv-filter-card',
  templateUrl: './adv-filter-card.component.html',
  styleUrls: ['./adv-filter-card.component.scss']
})
export class AdvFilterCardComponent {

  isFixed: boolean = false;

  constructor(
    
  ) {
    
  }

  @HostListener('window:scroll', ['$event'])
  	onWindowScroll() {
    if (window.scrollY > 50) {
  			this.isFixed = true
  		}
  		else {
  			this.isFixed = false
  		}
   }

}
