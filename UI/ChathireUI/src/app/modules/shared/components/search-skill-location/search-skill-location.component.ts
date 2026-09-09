import { Component, OnInit, Input, ViewChild, HostListener, Renderer2, OnChanges } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute, Params } from '@angular/router';
import { NgForm } from '@angular/forms';

import { CommonService } from 'src/app/api/api/common.service'

@Component({
  selector: 'search-skill-location',
  templateUrl: './search-skill-location.component.html',
  styleUrls: ['./search-skill-location.component.scss']
})
export class SearchSkillLocationComponent  {

  @Input() skill: any = ""
  @Input() location: any = "";
  @Input() cityId: any = null;
  @Input() stateId: any = null;
  @Input() type: any;
  @Input() page: string = "";

  formData = {
    selectedSkill: this.skill,
    selectedLocation: this.location,
  }
  
  skillList:any[] = [];
  locationList:any[] = [];
  
  skillElement:any;
  skillSearchDropdown:any;

  locationElement:any;
  locationSearchDropdown:any;

  charLength: number = 2
  
  @ViewChild ('searchSkill', {static: true}) skillInputElement:any;
  @ViewChild('searchLocation', { static: true }) locationInputElement: any;
  @ViewChild('searchSkillLocationForm', {static: false}) searchSkillLocationForm: NgForm;

  constructor(
    private commonService: CommonService,
    public router: Router,
    private route: ActivatedRoute,
    public renderer:Renderer2
    ) {
    
  }

  submitSkillLocationForm() {

    if (this.searchSkillLocationForm.valid) {
      if (this.type == 'jobs') {
        this.router.navigate(['/search-jobs'], { queryParamsHandling: 'merge', queryParams: {skill: this.skill, location: this.location, cid: this.cityId} });
      } 
      else {
        this.router.navigate(['/search-hotlist'], {queryParamsHandling: 'merge', queryParams: { skill: this.skill, location: this.location, cid: this.cityId } });
      }
    }

  }

  @HostListener('document:click', ['$event'])
    onDocumentClick(event:any) {
    
      const selectSkillClass = this.skillSearchDropdown.classList.contains('show');
      if (!this.skillElement.contains(event.target)) {
        if(selectSkillClass) {
          this.renderer.removeClass(this.skillSearchDropdown,'show');
        }
      }

      const selectLocationClass = this.locationSearchDropdown.classList.contains('show');
      if (!this.locationElement.contains(event.target)) {
        if(selectLocationClass) {
          this.renderer.removeClass(this.locationSearchDropdown,'show');
        }
      }
  }


  handleSearchSkill() {

    if(this.skill?.length > this.charLength) {
      this.renderer.addClass(this.skillSearchDropdown,'show');
      
      this.commonService.apiCommonSkillsGet(this.skill).subscribe({
        next: (res : any) => {
          this.skillList = res.value
        },
        error: (error:any) => {

        }
      })

      
    }
    else {
      this.renderer.removeClass(this.skillSearchDropdown,'show');
      this.skillList = []
    }

  }

  handleSearchLocation() {
    
    if (this.location?.length > this.charLength) {
      
      // If the user is entering a zipcode (only digits), wait until 5 digits
      const isZipCode = /^\d+$/.test(this.location);
      if (isZipCode && this.location.length < 5) {
        this.renderer.removeClass(this.locationSearchDropdown,'show');
        this.locationList = [];
        return;
      }

      this.renderer.addClass(this.locationSearchDropdown,'show');
      this.commonService.apiCommonCityGet(this.location).subscribe({
        next: (res : any) => {
          this.locationList = res.value
        },
        error: (error:any) => {

        }
      })
    }
    else {
      this.renderer.removeClass(this.locationSearchDropdown,'show');
      this.locationList = []
      this.location = ""
      this.cityId = null
      this.stateId = null
    }

  }

  getSkill(name, id) {
    this.skill = name;
  }

  getLocation(city, state, country, cityId, stateId) {
    this.location = city+" ,"+state+" ,"+country;
    this.cityId = cityId;
    this.stateId = stateId
  }

  ngOnInit() {

    

    this.skillElement = this.skillInputElement.nativeElement;
    this.skillSearchDropdown = this.skillElement.nextSibling.nextSibling;

    this.locationElement = this.locationInputElement.nativeElement;
    this.locationSearchDropdown = this.locationElement.nextSibling.nextSibling;

  }

  ngOnChanges() {
    if(this.skill == "")
    this.searchSkillLocationForm?.resetForm()
  }

}
