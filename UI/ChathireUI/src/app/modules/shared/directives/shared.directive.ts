
import { Directive, OnInit, Input, Inject, ElementRef, Renderer2, Output, NgZone, EventEmitter, HostListener, OnChanges, SimpleChanges } from '@angular/core';
import { NgModel } from '@angular/forms';
import { DOCUMENT } from '@angular/common';

declare const bootstrap: any;
declare var $:JQueryStatic;

@Directive({
  selector: '[AwakeClass]'
})

export class AwakeClassDirective {

  constructor(private element: ElementRef, private renderer: Renderer2) { }


  @HostListener('window:scroll', ['$event'])
  	onWindowScroll() {
    if (window.scrollY > 100) {
  			this.addClass('awake');
  		}
  		else {
  			this.removeClass('awake');
  		}
   }

   addClass(className: string) {
      this.renderer.addClass(this.element.nativeElement, className);
   }

   removeClass(className: string) {
       this.renderer.removeClass(this.element.nativeElement,className);
   }

}

@Directive({
  selector: '.select',
  providers : [NgModel]
})

export class SelectDirective implements OnInit {

  @Output() ngModelChange = new EventEmitter();

  constructor(
    private element: ElementRef,
    private renderer: Renderer2,
    private ngModel : NgModel
    ) {
  }

  @HostListener("click", ["$event"])
   onClick(event:any) {
      this.enableSelect();
   }

   enableSelect() {

     var el = this.element.nativeElement;
      var arrowElement = el.nextSibling;
      var selectDropdown = arrowElement.nextSibling;

      const arrowClass = arrowElement.classList.contains('up');
      const selectClass = selectDropdown.classList.contains('show');

      if(arrowClass) {
        this.renderer.removeClass(arrowElement,"up");
      }
      else {
        this.renderer.addClass(arrowElement,"up");
      }

      if(selectClass) {
        this.renderer.removeClass(selectDropdown,"show");
      }
      else {
        this.renderer.addClass(selectDropdown,"show");
      }

   }


   @HostListener('document:click', ['$event'])
    onDocumentClick(event:any) {

      var el = this.element.nativeElement;
      var arrowElement = el.nextSibling;

      const arrowClass = arrowElement.classList.contains('up');

      if (!el.contains(event.target)) {

        if(arrowClass) {
          this.enableSelect();
        }

      }

   }

   ngOnInit() {

   }

}


@Directive({
  selector: '.search',
  providers : [NgModel]
})

export class SearchDirective {

  constructor(
    private element: ElementRef,
    private renderer: Renderer2
    ) {

  }

  @HostListener("click", ["$event"])
   onClick(event:any) {

    var el = this.element.nativeElement;
    var arrowElement = el.nextSibling;
    var selectDropdown = arrowElement.nextSibling;

    if(selectDropdown.classList) {
      this.renderer.addClass(selectDropdown,"show");
    }

   }


   @HostListener('document:click', ['$event'])
    onDocumentClick(event:any) {

      var el = this.element.nativeElement;
      var arrowElement = el.nextSibling;
      var selectDropdown = arrowElement.nextSibling;
     
      if (!el.contains(event.target)) {
        if(selectDropdown.classList) {
          this.renderer.removeClass(selectDropdown,"show");
        }
      }

   }

}


@Directive({
  selector: '.filter-item',
  providers : [NgModel]
})

export class FilterBoxDirective {

  constructor(
    private element: ElementRef,
    private renderer: Renderer2
    ) {

  }

  @HostListener("click", ["$event"])
  onClick(event: any) {
    
    var el = this.element.nativeElement;

    if (!el.classList.contains('reset-item')) {
     var selectDropdown = el.children[1];
     if(selectDropdown?.classList && !selectDropdown?.classList.contains('show')) {
      this.renderer.addClass(selectDropdown,"show");
     }
     else {
       if(!selectDropdown.contains(event.target))
        this.renderer.removeClass(selectDropdown,"show");
      }
    }
    
   }

   @HostListener('document:click', ['$event'])
    onDocumentClick(event:any) {
     
     var el = this.element.nativeElement;
     var selectDropdown = el.children[1];
     
     if (window.innerWidth >= 991) {
      if (!el.contains(event.target)) {
        if(selectDropdown?.classList) {
          this.renderer.removeClass(selectDropdown,"show");
        }
      }
     }

     
     
   
     
   }

}

@Directive({
  selector: '.filter-cancel',
  providers: [NgModel]
})

export class filterCancelDirective {

  constructor(
    private element: ElementRef,
    private renderer: Renderer2
    ) {

  }

  @HostListener("click", ["$event"])
   onClick(event:any) {
    
    event.stopPropagation();

    var el = this.element.nativeElement;
    var parentElem = el.parentNode;
    const previousSibling = parentElem.previousElementSibling;
    const finalParentNode = previousSibling.parentNode;
    this.renderer.removeClass(finalParentNode,"show");

   }

}

@Directive({
  selector: '[showAdvFilter]',
})

export class showAdvFilterDirective {

  constructor(
    private element: ElementRef,
    private renderer: Renderer2
    ) {
  }

  @HostListener("click", ["$event"])
  onClick(event: any) {
   
      let filterElem = document.querySelector('.adv-filter-container')
      
      if (!filterElem?.classList.contains('show')) {
        this.renderer.addClass(filterElem, 'show')
      }
    
    }

}

@Directive({
  selector: '[hideAdvFilter]',
})

export class hideAdvFilterDirective {

  constructor(
    private element: ElementRef,
    private renderer: Renderer2
    ) {
  }

  @HostListener("click", ["$event"])
  onClick(event: any) {
   
      let filterElem = document.querySelector('.adv-filter-container')
      this.renderer.removeClass(filterElem, 'show')
    
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: Event): void {
    
    let filterElem = document.querySelector('.adv-filter-container')
    if (window.innerWidth >= 991) {
      this.renderer.removeClass(filterElem, 'show')
    }

  }

}

@Directive({
  selector: '.scroller'
})

export class ScrollerDirective {

  constructor(
    private renderer: Renderer2,
    @Inject(DOCUMENT) public document: Document
    ) { }


  @HostListener('mouseover')
  onMouseOver() {
    this.renderer.addClass(document.body, 'ov-h')
  }

  @HostListener('mouseout')
  onMouseOut() {
    this.renderer.removeClass(document.body, 'ov-h')
  }

  ngOnInit() {
  }

}

@Directive({
  selector: '[sidenav]'
})

export class SideNavDirective {

  @Input() sidenav: boolean;

  constructor(
    private element: ElementRef,
    private renderer: Renderer2,
    ) { 
    }

  ngOnInit() {

  }

  ngOnChanges(changes: SimpleChanges) {
    if(this.sidenav) {
      this.renderer.addClass(document.body, 'mat-sidenav')
    }
    else {
      this.renderer.removeClass(document.body, 'mat-sidenav')
    }
  }

}

@Directive({
  selector: '[profileMapColor]'
})

export class ProfileMapColorDirective {

  @Input() profileMapColor: any;

  constructor(
    private element: ElementRef,
    private renderer: Renderer2,
    ) { 
    }

  ngOnInit() {

    let status = ""
    switch (this.profileMapColor) {
      case 1:
        status = 'text-primary-600'
        break;
      case 2:
        status = 'text-secondary-600'
        break;
      case 3:
        status = 'text-dark-600'
        break;
      case 4:
        status = 'text-warning-600'
        break;
      case 5:
          status = 'text-primary-600'
          break;
      case 6:
        status = 'text-danger-600'
        break;
      default:
        status = 'text-dark-600'
    }

    this.renderer.addClass(this.element.nativeElement, status)

  }

  ngOnChanges(changes: SimpleChanges) {
    
  }

}


@Directive({
  selector: '[customTooltip]'
})
export class CustomTooltipDirective implements OnInit {
  @Input() tooltipTitle: string = '';
  @Input() tooltipPlacement: string = 'top';

  constructor(private el: ElementRef) {}

  ngOnInit() {
    this.el.nativeElement.setAttribute('data-bs-toggle', 'tooltip');
    this.el.nativeElement.setAttribute('data-bs-title', this.tooltipTitle);
    this.el.nativeElement.setAttribute('data-bs-placement', this.tooltipPlacement);
    new bootstrap.Tooltip(this.el.nativeElement);
  }
}

@Directive({
  selector: '[externalLink]'
})
export class ExternalLinkDirective {
  @Input('externalLink') externalLink: string;

  @HostListener('click', ['$event'])
  onClick(event: Event): void {
    console.log(event)
    if (this.externalLink) {
      window.open(this.externalLink, '_blank');
    }
  }
}

