import { Component } from '@angular/core';
import { Router, ActivatedRoute} from '@angular/router';

@Component({
  selector: 'adv-filter-reset',
  templateUrl: './adv-filter-reset.component.html',
  styleUrls: ['./adv-filter-reset.component.scss']
})
export class AdvFilterResetComponent {

  constructor(
    public router: Router,
    private route: ActivatedRoute
    ) {
    
  }


  resetFilter() {
    let currentParams = { ...this.route.snapshot.queryParams }
    if (currentParams.hasOwnProperty('visas')) {
      delete currentParams['visas']
    }
    if (currentParams.hasOwnProperty('wm')) {
      delete currentParams['wm']
    }
    if (currentParams.hasOwnProperty('exp')) {
      delete currentParams['exp']
    }
    if (currentParams.hasOwnProperty('date')) {
      delete currentParams['date']
    }

    this.router.navigate([], { relativeTo: this.route, queryParams: currentParams });
  }

}
