import { Component, HostListener, OnInit } from '@angular/core';

export interface CandidateDemo {
  id: number;
  name: string;
  role: string;
  experience: string;
  visa: string;
  location: string;
  rate: string;
  skills: string[];
  status: string;
  isNew?: boolean;
}

export interface SubmissionDemo {
  id: number;
  consultantName: string;
  role: string;
  benchCompany: string;
  benchManager: string;
  rate: string;
  matchScore: number;
  experience: string;
  visa: string;
  location: string;
  status: 'new' | 'shortlisted' | 'interviewed';
}

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {

  isMobile: boolean = false;

  // Active persona: 'benchsales' | 'recruiter'
  activePersona: 'benchsales' | 'recruiter' = 'benchsales';

  // Stepper state
  benchStep: number = 1; // 1: Hotlist, 2: LinkedIn Image Export, 3: Inbound Calls & Resume Submit
  recruiterStep: number = 1; // 1: Post Job & Share LinkedIn, 2: Receive & Review Resumes

  // Demo interactive state - Bench Sales
  benchCandidates: CandidateDemo[] = [
    {
      id: 1,
      name: 'Rahul Sharma',
      role: 'Sr. Java AWS Full Stack Architect',
      experience: '10+ Yrs',
      visa: 'H1B / USC',
      location: 'Dallas, TX (Remote/Hybrid)',
      rate: '$80/hr C2C',
      skills: ['Java 17', 'Spring Boot', 'AWS', 'Microservices', 'Kafka', 'React'],
      status: 'Available'
    },
    {
      id: 2,
      name: 'Sneha Patel',
      role: 'Lead Data Engineer & Snowflake Specialist',
      experience: '8+ Yrs',
      visa: 'GC',
      location: 'Atlanta, GA (Open to Relocate)',
      rate: '$85/hr C2C',
      skills: ['PySpark', 'Snowflake', 'dbt', 'AWS Glue', 'Airflow', 'Python'],
      status: 'Available'
    },
    {
      id: 3,
      name: 'Amitabh Verma',
      role: 'Principal DevOps / SRE & Kubernetes Lead',
      experience: '11+ Yrs',
      visa: 'US Citizen',
      location: 'Chicago, IL (Remote)',
      rate: '$90/hr C2C',
      skills: ['Kubernetes', 'Terraform', 'CI/CD', 'Azure / AWS', 'Docker', 'Go'],
      status: 'Available'
    }
  ];

  isAddingCandidate: boolean = false;
  candidateAddedSuccess: boolean = false;
  imageGenerated: boolean = false;
  imageGenerating: boolean = false;
  linkedinShared: boolean = false;

  // Inbound Call / Chat simulation state
  callState: 'incoming' | 'connected' | 'submitted' = 'incoming';
  selectedQuickReply: string = '';
  resumeSubmitFeedback: string = '';

  // Demo interactive state - Recruiter
  recruiterJobPosted: boolean = false;
  recruiterLinkedInShared: boolean = false;

  recruiterSubmissions: SubmissionDemo[] = [
    {
      id: 101,
      consultantName: 'Karthik Raman',
      role: 'Lead Cloud .NET Azure Architect',
      benchCompany: 'Apex Tech Solutions Inc.',
      benchManager: 'Vikram Joshi (Bench Sales)',
      rate: '$85/hr C2C',
      matchScore: 98,
      experience: '10 Yrs',
      visa: 'H1B (Verified)',
      location: 'New York, NY (Hybrid)',
      status: 'new'
    },
    {
      id: 102,
      consultantName: 'Pooja Hegde',
      role: 'Senior Full Stack .NET Core & Angular Engineer',
      benchCompany: 'Vanguard Systems LLC',
      benchManager: 'Sunil Rao (Director Sales)',
      rate: '$78/hr C2C',
      matchScore: 94,
      experience: '8 Yrs',
      visa: 'US Citizen',
      location: 'Austin, TX (Remote)',
      status: 'new'
    },
    {
      id: 103,
      consultantName: 'Devendra Malik',
      role: 'Azure Microservices & DevOps Consultant',
      benchCompany: 'CloudScale Consulting',
      benchManager: 'Ananya Roy (Account Lead)',
      rate: '$82/hr C2C',
      matchScore: 91,
      experience: '9 Yrs',
      visa: 'Green Card',
      location: 'Charlotte, NC (Hybrid)',
      status: 'new'
    }
  ];

  activeResumeModal: SubmissionDemo | null = null;
  shortlistedIds: number[] = [];

  @HostListener('window:resize', ['$event'])
  onResize(event: any) {
    this.mobileScreen();
  }

  mobileScreen() {
    if (window.innerWidth <= 991) {
      this.isMobile = true;
    } else {
      this.isMobile = false;
    }
  }

  ngOnInit() {
    this.mobileScreen();
  }

  // Persona & Step Switchers
  setPersona(persona: 'benchsales' | 'recruiter') {
    this.activePersona = persona;
  }

  setBenchStep(step: number) {
    this.benchStep = step;
  }

  setRecruiterStep(step: number) {
    this.recruiterStep = step;
  }

  // Bench Sales: Add candidate simulation
  toggleAddCandidate() {
    this.isAddingCandidate = !this.isAddingCandidate;
  }

  addDemoCandidate() {
    const newDemo: CandidateDemo = {
      id: Date.now(),
      name: 'Vikas Nambiar',
      role: 'Senior Salesforce Technical Architect',
      experience: '9+ Yrs',
      visa: 'H1B / USC',
      location: 'San Jose, CA (Remote/Hybrid)',
      rate: '$85/hr C2C',
      skills: ['Salesforce Lightning', 'Apex', 'LWC', 'Integrations', 'Copado'],
      status: 'Available',
      isNew: true
    };
    this.benchCandidates.unshift(newDemo);
    this.candidateAddedSuccess = true;
    this.isAddingCandidate = false;

    setTimeout(() => {
      this.candidateAddedSuccess = false;
    }, 4000);
  }

  // Bench Sales: Image Generation Simulation
  generateSocialImage() {
    this.imageGenerating = true;
    setTimeout(() => {
      this.imageGenerating = false;
      this.imageGenerated = true;
    }, 600);
  }

  shareBenchToLinkedIn() {
    this.linkedinShared = true;
    setTimeout(() => {
      this.linkedinShared = false;
    }, 4500);
  }

  // Bench Sales: Inbound Call & Chat simulation
  acceptInboundCall() {
    this.callState = 'connected';
  }

  submitResumeWithRate(rate: string) {
    this.callState = 'submitted';
    this.resumeSubmitFeedback = `Resume submitted instantly at ${rate} with 1-click verified authorization!`;
  }

  resetCallDemo() {
    this.callState = 'incoming';
    this.resumeSubmitFeedback = '';
  }

  // Recruiter: Post Job & Share simulation
  publishRecruiterJob() {
    this.recruiterJobPosted = true;
  }

  shareRecruiterToLinkedIn() {
    this.recruiterLinkedInShared = true;
    setTimeout(() => {
      this.recruiterLinkedInShared = false;
    }, 4500);
  }

  // Recruiter: Review & Shortlist candidate
  openResumePreview(submission: SubmissionDemo) {
    this.activeResumeModal = submission;
  }

  closeResumePreview() {
    this.activeResumeModal = null;
  }

  shortlistSubmission(id: number) {
    if (!this.shortlistedIds.includes(id)) {
      this.shortlistedIds.push(id);
    }
  }

  isShortlisted(id: number): boolean {
    return this.shortlistedIds.includes(id);
  }
}

