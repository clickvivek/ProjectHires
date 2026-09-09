import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'inbox-resume-contact',
  templateUrl: './inbox-resume-contact.component.html',
  styleUrls: ['./inbox-resume-contact.component.scss']
})
export class InboxResumeContactComponent {

  @Input() resume;
  @Output() hideContact = new EventEmitter();

  isLoaded:boolean = false;

  contact:any = null

  closeContact() {
    this.hideContact.emit(true)
  }

  ngOnInit() {
    this.isLoaded = true
    this.contact = this.resume.consultancyUser
  }

}
