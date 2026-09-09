import { Component, HostListener } from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {

  isMobile: boolean = false;

  @HostListener('window:resize', ['$event'])
  onResize(event: any) {
      this.mobileScreen()
  }

  mobileScreen() {
    if(window.innerWidth <= 991)
      this.isMobile = true;
    else
      this.isMobile = false;
  }

  ngOnInit() {

    this.mobileScreen();


  }

}
