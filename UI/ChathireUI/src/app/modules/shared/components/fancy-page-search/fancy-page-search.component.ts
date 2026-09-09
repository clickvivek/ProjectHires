import { Component, Input, Output, EventEmitter, ElementRef, HostListener, Renderer2 } from '@angular/core';

@Component({
  selector: 'fancy-page-search',
  templateUrl: './fancy-page-search.component.html',
  styleUrls: ['./fancy-page-search.component.scss']
})
export class FancyPageSearchComponent {

  @Input() placeholder;

  searchData:string = ""

  @Output() public outputData = new EventEmitter();

  constructor(
    private el: ElementRef, private renderer: Renderer2
  ) {

  }

  @HostListener('click') onClick() {
    
    const el = this.el.nativeElement;
    const childElem = this.el.nativeElement.querySelector('.form-group')
    if (childElem) {
      this.renderer.addClass(childElem, 'active');
    }

  }

  @HostListener('document:click', ['$event'])
    onDocumentClick(event:any) {

      const el = this.el.nativeElement;
      const childElem = this.el.nativeElement.querySelector('.form-group')

      const activeClass = childElem.classList.contains('active');

      if (!el.contains(event.target)) {

        if(activeClass) {
          this.renderer.removeClass(childElem, 'active');
        }

      }

   }

 
  onSearchData() {
    this.outputData.emit(this.searchData)
  }

}
