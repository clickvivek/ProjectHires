import { Component, Input, ViewChild, ElementRef, SimpleChanges  } from '@angular/core';

@Component({
  selector: 'view-resume',
  templateUrl: './view-resume.component.html',
  styleUrls: ['./view-resume.component.scss']
})
export class ViewResumeComponent {

  @Input() doc: any;
  fileType: string = ""
  fileUrl: string = ""
  isViewer: boolean = false;

  loadDoc:boolean = false;

  defaultFileTypes = ['ppt', 'pptx', 'doc', 'docx', 'xls', 'xlsx', 'pdf']

  @ViewChild('viewResumeImgElem') viewResumeImgElem: ElementRef;
  

  constructor(
   
  ) {

  }

  onSidenavOpened() {
    return true
  }

  handleResume() {
    this.isViewer = !this.isViewer
    setTimeout(() => {
      this.loadDoc = true
    })
  }

  isDoc() {
   return this.defaultFileTypes.some(item => item === this.fileType);
  }

  closeViewer() {
    this.isViewer = false
    setTimeout(() => {
      this.loadDoc = false
    })
  }

  ngOnInit() {
    const resume = this.doc.split('.')
    this.fileType = resume[1].toLowerCase()
    this.fileUrl = `https://hiresblob.blob.core.windows.net/resumes/${this.doc}`
  }

  ngOnChanges(changes: SimpleChanges) {
		
  }

}
