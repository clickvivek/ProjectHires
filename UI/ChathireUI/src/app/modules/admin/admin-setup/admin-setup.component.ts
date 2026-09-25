import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { ConsultancyService } from 'src/app/api/api/consultancy.service';
import { picUrl, defaultProfilePic } from 'src/app/data/various';
import { environment } from 'src/environments/environment';
import { PromocodeService, PromocodeDto, CreatePromocodeDto } from 'src/app/core/services/promocode.service';
import {
  EmailJobPostingService,
  EmailJobPostingQueueDto,
  EmailJobPostingStatsDto,
  SimulateInboundEmailDto,
  ApproveEmailJobPostingDto
} from 'src/app/core/services/email-job-posting.service';

export interface CsvCompanyRow {
  name: string;
  email: string;
  address: string;
  phone: string;
  active: boolean;
  updated: string;
  website: string;
  linkedin: string;
  logo: string;
  isValid?: boolean;
  errors?: string[];
}

@Component({
  selector: 'app-admin-setup',
  templateUrl: './admin-setup.component.html',
  styleUrls: ['./admin-setup.component.scss']
})
export class AdminSetupComponent implements OnInit {
  @ViewChild('fileInput') fileInput!: ElementRef;
  @ViewChild('logoInput') logoInput!: ElementRef;

  activeTab: 'dau-dashboard' | 'bulk-upload' | 'view-edit' | 'review-companies' | 'user-quotas' | 'admin-users' | 'linkedin-scraper' | 'promo-codes' | 'jobposting-by-email' = 'dau-dashboard';

  // --- Tab: Admin Users State ---
  adminUsers: any[] = [];
  isLoadingAdminUsers = false;
  isCreatingAdmin = false;
  adminUserSearch = '';
  showAdminPassword = false;
  createAdminForm = {
    userName: '',
    password: '',
    fname: '',
    lname: '',
    phone: ''
  };

  // --- Tab: Review User-Added Companies State ---
  reviewSearchTerm = '';
  reviewStatusFilter: 'all' | 'active' | 'inactive' = 'all';
  reviewCurrentPage = 1;
  reviewPageSize = 25;
  reviewPageSizeOptions = [10, 25, 50, 100];

  // --- Tab 3: User Quotas State ---
  isLoadingQuotas = false;
  userQuotas: any[] = [];
  quotaTotalCount = 0;
  quotaTotalPages = 1;
  quotaSearch = '';
  quotaFilter = 'all';
  quotaCurrentPage = 1;
  quotaPageSize = 100;
  quotaPageSizeOptions = [25, 50, 100, 200];

  // Edit Quota Modal State
  isEditQuotaModalOpen = false;
  isSavingQuota = false;
  editingQuotaUser: any = null;
  editQuotaForm: {
    userId: number;
    actualJobPosting: number;
    actualDownloads: number;
    dailyChatLimit: number;
    startDate: string;
    endDate: string;
    isFree: boolean;
  } = {
    userId: 0,
    actualJobPosting: 15,
    actualDownloads: 10,
    dailyChatLimit: 20,
    startDate: '',
    endDate: '',
    isFree: true
  };

  // --- Tab 0: DAU Dashboard State ---
  dauTimeframe: 'day' | 'week' | 'month' = 'day';
  isLoadingDau = false;
  dauSummary: any = {
    dauCount: 0,
    wauCount: 0,
    mauCount: 0,
    activeNowCount: 0,
    totalLoginsInPeriod: 0,
    totalRegisteredUsers: 0
  };
  dauTrends: any[] = [];
  allUserActivities: any[] = [];
  filteredUserActivities: any[] = [];
  userActivitySearch = '';
  roleFilter = 'all';
  onlineOnlyFilter = false;
  activityCurrentPage = 1;
  activityPageSize = 25;
  activityPageSizeOptions = [10, 25, 50, 100];
  trendMetric: 'users' | 'logins' = 'users';

  // --- Tab 1: Bulk Upload State ---
  selectedFile: File | null = null;
  csvRows: CsvCompanyRow[] = [];
  isParsingCsv = false;
  isUploadingBulk = false;
  csvCurrentPage = 1;
  csvPageSize = 100;
  csvUploadError = '';
  isDragOver = false;

  // --- Tab 2: View / Edit State ---
  isLoadingCompanies = false;
  allCompanies: any[] = [];
  filteredCompanies: any[] = [];
  searchTerm = '';
  statusFilter: 'all' | 'active' | 'inactive' = 'all';
  currentPage = 1;
  pageSize = 100;
  pageSizeOptions = [25, 50, 100, 200, 500];

  // Edit Modal State
  isEditModalOpen = false;
  isSavingCompany = false;
  isUploadingLogo = false;
  editingCompany: any = null;
  selectedLogoFile: File | null = null;
  logoPreviewUrl: string | null = null;

  defaultLogo = defaultProfilePic;
  picBaseUrl = picUrl;
  Math = Math;

  constructor(
    private consultancyService: ConsultancyService,
    private promocodeService: PromocodeService,
    private emailJobPostingService: EmailJobPostingService,
    private http: HttpClient,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadDauStats();
    this.loadAdminUsers();
  }

  setTab(tab: 'dau-dashboard' | 'bulk-upload' | 'view-edit' | 'review-companies' | 'user-quotas' | 'admin-users' | 'linkedin-scraper' | 'promo-codes' | 'jobposting-by-email') {
    this.activeTab = tab;
    if (tab === 'dau-dashboard' && !this.dauSummary.totalRegisteredUsers) {
      this.loadDauStats();
    } else if ((tab === 'view-edit' || tab === 'review-companies') && this.allCompanies.length === 0) {
      this.loadCompanies();
    } else if (tab === 'user-quotas' && this.userQuotas.length === 0) {
      this.loadUserQuotas();
    } else if (tab === 'admin-users') {
      this.loadAdminUsers();
    } else if (tab === 'promo-codes') {
      this.loadPromoCodes();
    } else if (tab === 'jobposting-by-email') {
      this.loadEmailJobPostings();
      this.loadEmailJobStats();
    }
  }

  // ==========================================
  // TAB 1: CSV BULK UPLOAD
  // ==========================================

  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
    if (event.dataTransfer && event.dataTransfer.files.length > 0) {
      const file = event.dataTransfer.files[0];
      if (file.name.endsWith('.csv')) {
        this.processCsvFile(file);
      } else {
        this.toastr.error('Please upload a valid .csv file.', 'Invalid File Type');
      }
    }
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      if (file.name.endsWith('.csv')) {
        this.processCsvFile(file);
      } else {
        this.toastr.error('Please select a valid .csv file.', 'Invalid File Type');
        if (this.fileInput) {
          this.fileInput.nativeElement.value = '';
        }
      }
    }
  }

  processCsvFile(file: File) {
    this.selectedFile = file;
    this.csvUploadError = '';
    this.isParsingCsv = true;
    this.csvCurrentPage = 1;

    const reader = new FileReader();
    reader.onload = (e: any) => {
      try {
        const text = e.target.result;
        this.parseCsvText(text);
      } catch (err: any) {
        this.csvUploadError = 'Failed to parse CSV file: ' + (err.message || 'Unknown format error');
        this.toastr.error(this.csvUploadError, 'Parse Error');
      } finally {
        this.isParsingCsv = false;
      }
    };
    reader.onerror = () => {
      this.csvUploadError = 'Failed to read file.';
      this.isParsingCsv = false;
      this.toastr.error(this.csvUploadError, 'Read Error');
    };
    reader.readAsText(file);
  }

  parseCsvText(csvText: string) {
    const lines = this.splitCsvLines(csvText.trim());
    if (lines.length < 2) {
      this.csvUploadError = 'CSV file is empty or does not contain data rows.';
      this.csvRows = [];
      return;
    }

    const headers = this.parseCsvLine(lines[0]).map(h => h.trim().toLowerCase());

    const nameIdx = headers.findIndex(h => h === 'name' || h === 'company name' || h === 'company');
    const emailIdx = headers.findIndex(h => h === 'email' || h === 'email address');
    const addressIdx = headers.findIndex(h => h === 'address' || h === 'location');
    const phoneIdx = headers.findIndex(h => h === 'phone' || h === 'phone number');
    const activeIdx = headers.findIndex(h => h === 'active' || h === 'isactive' || h === 'status');
    const updatedIdx = headers.findIndex(h => h === 'updated' || h === 'updated date');
    const websiteIdx = headers.findIndex(h => h === 'website' || h === 'websiteurl' || h === 'url');
    const linkedinIdx = headers.findIndex(h => h === 'linkedin' || h === 'linkedinurl');
    const logoIdx = headers.findIndex(h => h === 'logo' || h === 'logourl');

    if (nameIdx === -1) {
      this.csvUploadError = 'CSV header must contain a "Name" column.';
      this.csvRows = [];
      return;
    }

    const parsedRows: CsvCompanyRow[] = [];

    for (let i = 1; i < lines.length; i++) {
      const line = lines[i].trim();
      if (!line) continue;

      const cols = this.parseCsvLine(line);
      const name = nameIdx >= 0 && cols[nameIdx] ? cols[nameIdx].trim() : '';
      const email = emailIdx >= 0 && cols[emailIdx] ? cols[emailIdx].trim() : '';
      const address = addressIdx >= 0 && cols[addressIdx] ? cols[addressIdx].trim() : '';
      const phone = phoneIdx >= 0 && cols[phoneIdx] ? cols[phoneIdx].trim() : '';
      const activeRaw = activeIdx >= 0 && cols[activeIdx] ? cols[activeIdx].trim().toLowerCase() : 'true';
      const updated = updatedIdx >= 0 && cols[updatedIdx] ? cols[updatedIdx].trim() : '';
      const website = websiteIdx >= 0 && cols[websiteIdx] ? cols[websiteIdx].trim() : '';
      const linkedin = linkedinIdx >= 0 && cols[linkedinIdx] ? cols[linkedinIdx].trim() : '';
      const logo = logoIdx >= 0 && cols[logoIdx] ? cols[logoIdx].trim() : '';

      const active = activeRaw === 'true' || activeRaw === '1' || activeRaw === 'yes' || activeRaw === 'y' || activeRaw === 'active';

      const errors: string[] = [];
      if (!name) {
        errors.push('Company name is required');
      }

      parsedRows.push({
        name,
        email,
        address,
        phone,
        active,
        updated,
        website,
        linkedin,
        logo,
        isValid: errors.length === 0,
        errors
      });
    }

    this.csvRows = parsedRows;
    if (this.csvRows.length === 0) {
      this.csvUploadError = 'No valid rows found in the CSV file.';
    } else {
      this.toastr.info(`Parsed ${this.csvRows.length} rows from CSV file.`, 'CSV Ready');
    }
  }

  private splitCsvLines(text: string): string[] {
    const lines: string[] = [];
    let curLine = '';
    let inQuotes = false;

    for (let i = 0; i < text.length; i++) {
      const ch = text[i];
      if (ch === '"') {
        inQuotes = !inQuotes;
        curLine += ch;
      } else if ((ch === '\r' || ch === '\n') && !inQuotes) {
        if (ch === '\r' && text[i + 1] === '\n') {
          i++;
        }
        if (curLine.length > 0) {
          lines.push(curLine);
          curLine = '';
        }
      } else {
        curLine += ch;
      }
    }
    if (curLine.length > 0) {
      lines.push(curLine);
    }
    return lines;
  }

  private parseCsvLine(line: string): string[] {
    const result: string[] = [];
    let curVal = '';
    let inQuotes = false;

    for (let i = 0; i < line.length; i++) {
      const ch = line[i];
      if (ch === '"') {
        if (inQuotes && textHasNextQuote(line, i)) {
          curVal += '"';
          i++; // Skip escaped quote
        } else {
          inQuotes = !inQuotes;
        }
      } else if (ch === ',' && !inQuotes) {
        result.push(curVal);
        curVal = '';
      } else {
        curVal += ch;
      }
    }
    result.push(curVal);
    return result;

    function textHasNextQuote(str: string, index: number): boolean {
      return index + 1 < str.length && str[index + 1] === '"';
    }
  }

  get csvValidCount(): number {
    return this.csvRows.filter(r => r.isValid).length;
  }

  get csvInvalidCount(): number {
    return this.csvRows.filter(r => !r.isValid).length;
  }

  get paginatedCsvRows(): CsvCompanyRow[] {
    const start = (this.csvCurrentPage - 1) * this.csvPageSize;
    return this.csvRows.slice(start, start + this.csvPageSize);
  }

  get totalCsvPages(): number {
    return Math.ceil(this.csvRows.length / this.csvPageSize) || 1;
  }

  clearCsvUpload() {
    this.selectedFile = null;
    this.csvRows = [];
    this.csvUploadError = '';
    this.csvCurrentPage = 1;
    if (this.fileInput) {
      this.fileInput.nativeElement.value = '';
    }
  }

  downloadSampleCsv() {
    const headers = 'Name,Email,Address,Phone,Active,Updated,website,Linkedin,Logo\n';
    const sampleRows = [
      'Acme Technologies,contact@acmetech.com,"123 Innovation Way, Tech Park, San Jose, CA",408-555-0199,true,2026-09-16,https://acmetech.com,https://linkedin.com/company/acme-tech,\n',
      'Global Staffing Solutions,info@globalstaffing.com,"456 Corporate Blvd, Suite 200, Dallas, TX",214-555-0144,true,2026-09-16,https://globalstaffing.com,https://linkedin.com/company/global-staffing,\n',
      'Apex IT Solutions,hr@apexit.com,"789 Enterprise Dr, Chicago, IL",312-555-0188,true,2026-09-16,https://apexit.com,https://linkedin.com/company/apex-it,\n'
    ].join('');

    const blob = new Blob([headers + sampleRows], { type: 'text/csv;charset=utf-8;' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'company_bulk_upload_template.csv';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }

  submitBulkUpload() {
    const validRows = this.csvRows.filter(r => r.isValid);
    if (validRows.length === 0) {
      this.toastr.warning('No valid rows to upload.', 'Validation');
      return;
    }

    this.isUploadingBulk = true;

    const payload = validRows.map(row => ({
      name: row.name,
      email: row.email || null,
      address: row.address || null,
      phone: row.phone || null,
      active: row.active,
      updated: row.updated || null,
      website: row.website || null,
      linkedin: row.linkedin || null,
      logo: row.logo || null,
      statusId: row.active ? 1 : 2
    }));

    this.consultancyService.bulkAdd(payload).subscribe({
      next: (res: any) => {
        this.isUploadingBulk = false;
        const count = res?.value || validRows.length;
        this.toastr.success(`Successfully uploaded ${count} companies!`, 'Bulk Upload Complete');
        this.clearCsvUpload();
        this.loadCompanies();
        this.setTab('view-edit');
      },
      error: (err: any) => {
        this.isUploadingBulk = false;
        const msg = err?.error?.message || err?.message || 'Failed to bulk upload companies.';
        this.toastr.error(msg, 'Upload Failed');
      }
    });
  }

  // ==========================================
  // TAB 2: VIEW / EDIT COMPANIES
  // ==========================================

  loadCompanies() {
    this.isLoadingCompanies = true;
    this.consultancyService.apiConsultancyAllConsultancyGet().subscribe({
      next: (res: any) => {
        this.isLoadingCompanies = false;
        if (res && res.value) {
          this.allCompanies = Array.isArray(res.value) ? res.value : [res.value];
        } else if (Array.isArray(res)) {
          this.allCompanies = res;
        } else {
          this.allCompanies = [];
        }
        this.applyFilters();
      },
      error: (err: any) => {
        this.isLoadingCompanies = false;
        this.toastr.error('Failed to load companies list.', 'Error');
      }
    });
  }

  onSearchChange() {
    this.currentPage = 1;
    this.applyFilters();
  }

  get activeCompaniesCount(): number {
    return this.allCompanies.filter(c => c.active === true || c.active === 1).length;
  }

  get inactiveCompaniesCount(): number {
    return this.allCompanies.filter(c => c.active === false || c.active === 0 || c.active === null).length;
  }

  onStatusFilterChange(filter: 'all' | 'active' | 'inactive') {
    this.statusFilter = filter;
    this.currentPage = 1;
    this.applyFilters();
  }

  applyFilters() {
    let list = [...this.allCompanies];

    // Status filter
    if (this.statusFilter === 'active') {
      list = list.filter(c => c.active === true || c.active === 1);
    } else if (this.statusFilter === 'inactive') {
      list = list.filter(c => c.active === false || c.active === 0 || c.active === null);
    }

    // Search term
    if (this.searchTerm && this.searchTerm.trim()) {
      const term = this.searchTerm.trim().toLowerCase();
      list = list.filter(c =>
        (c.name && c.name.toLowerCase().includes(term)) ||
        (c.website && c.website.toLowerCase().includes(term)) ||
        (c.email && c.email.toLowerCase().includes(term)) ||
        (c.phone && c.phone.toLowerCase().includes(term)) ||
        (c.address && c.address.toLowerCase().includes(term))
      );
    }

    this.filteredCompanies = list;
  }

  // --- Review User Companies Getters & Handlers ---

  get userAddedCompanies(): any[] {
    return this.allCompanies.filter(c => c.isDirectCompany === true || c.isDirectCompany === 1 || c.statusId === 1);
  }

  get reviewActiveCount(): number {
    return this.userAddedCompanies.filter(c => c.active === true || c.active === 1).length;
  }

  get reviewInactiveCount(): number {
    return this.userAddedCompanies.filter(c => c.active === false || c.active === 0 || c.active === null).length;
  }

  get filteredReviewCompanies(): any[] {
    let list = this.userAddedCompanies;

    if (this.reviewStatusFilter === 'active') {
      list = list.filter(c => c.active === true || c.active === 1);
    } else if (this.reviewStatusFilter === 'inactive') {
      list = list.filter(c => c.active === false || c.active === 0 || c.active === null);
    }

    if (this.reviewSearchTerm && this.reviewSearchTerm.trim()) {
      const term = this.reviewSearchTerm.trim().toLowerCase();
      list = list.filter(c =>
        (c.name && c.name.toLowerCase().includes(term)) ||
        (c.website && c.website.toLowerCase().includes(term)) ||
        (c.email && c.email.toLowerCase().includes(term)) ||
        (c.phone && c.phone.toLowerCase().includes(term)) ||
        (c.address && c.address.toLowerCase().includes(term))
      );
    }

    return list;
  }

  get paginatedReviewCompanies(): any[] {
    const start = (this.reviewCurrentPage - 1) * this.reviewPageSize;
    return this.filteredReviewCompanies.slice(start, start + this.reviewPageSize);
  }

  get totalReviewPages(): number {
    return Math.ceil(this.filteredReviewCompanies.length / this.reviewPageSize) || 1;
  }

  onReviewPageChange(page: number) {
    if (page >= 1 && page <= this.totalReviewPages) {
      this.reviewCurrentPage = page;
    }
  }

  onReviewPageSizeChange(size: number) {
    this.reviewPageSize = size;
    this.reviewCurrentPage = 1;
  }

  onReviewStatusFilterChange(filter: 'all' | 'active' | 'inactive') {
    this.reviewStatusFilter = filter;
    this.reviewCurrentPage = 1;
  }

  onReviewSearchChange() {
    this.reviewCurrentPage = 1;
  }

  get paginatedCompanies(): any[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredCompanies.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredCompanies.length / this.pageSize) || 1;
  }

  onPageChange(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }

  onPageSizeChange(size: number) {
    this.pageSize = size;
    this.currentPage = 1;
  }

  toggleActive(company: any, event: Event) {
    event.stopPropagation();
    const newActiveState = !company.active;
    const updateDto = {
      ...company,
      active: newActiveState,
      statusId: newActiveState ? 1 : 2
    };

    this.consultancyService.apiConsultancyUpdatePut(updateDto).subscribe({
      next: () => {
        company.active = newActiveState;
        company.statusId = updateDto.statusId;
        this.toastr.success(`Company ${company.name} is now ${newActiveState ? 'Active' : 'Inactive'}.`, 'Status Updated');
      },
      error: (err: any) => {
        this.toastr.error('Failed to update company status.', 'Update Error');
      }
    });
  }

  openEditModal(company: any) {
    this.editingCompany = {
      id: company.id,
      name: company.name || '',
      email: company.email || '',
      address: company.address || '',
      phone: company.phone || '',
      active: company.active === true || company.active === 1,
      website: company.website || '',
      linkedin: company.linkedin || '',
      logo: company.logo || '',
      cityId: company.cityId || null,
      domainname: company.domainname || null,
      statusId: company.statusId || (company.active ? 1 : 2),
      isDirectCompany: company.isDirectCompany || false
    };
    this.selectedLogoFile = null;
    this.logoPreviewUrl = this.getCompanyLogoUrl(company.logo);
    this.isEditModalOpen = true;
  }

  closeEditModal() {
    this.isEditModalOpen = false;
    this.editingCompany = null;
    this.selectedLogoFile = null;
    this.logoPreviewUrl = null;
  }

  onLogoFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      const allowedTypes = ['image/png', 'image/jpeg', 'image/jpg', 'image/webp', 'image/svg+xml'];
      if (!allowedTypes.includes(file.type)) {
        this.toastr.error('Please select a valid image file (PNG, JPG, WEBP, SVG).', 'Invalid Image');
        return;
      }
      this.selectedLogoFile = file;

      // Local preview
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.logoPreviewUrl = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  uploadLogoAndSaveCompany() {
    if (!this.editingCompany || !this.editingCompany.name.trim()) {
      this.toastr.warning('Company Name is required.', 'Required Field');
      return;
    }

    // If logo file is selected, upload it first
    if (this.selectedLogoFile) {
      this.isUploadingLogo = true;
      const websiteUrl = this.editingCompany.website || this.editingCompany.name;

      this.consultancyService.uploadCompanyLogo(this.selectedLogoFile, websiteUrl).subscribe({
        next: (res: any) => {
          this.isUploadingLogo = false;
          // Result can be string filename or object with value/fileName
          const uploadedLogo = typeof res === 'string' ? res : (res?.fileName || res?.value || res?.logo || res);
          if (uploadedLogo) {
            this.editingCompany.logo = uploadedLogo;
          }
          this.saveCompanyDetails();
        },
        error: (err: any) => {
          this.isUploadingLogo = false;
          this.toastr.error('Failed to upload logo to Azure Storage.', 'Logo Upload Error');
        }
      });
    } else {
      this.saveCompanyDetails();
    }
  }

  saveCompanyDetails() {
    this.isSavingCompany = true;
    const dto = {
      ...this.editingCompany,
      statusId: this.editingCompany.active ? 1 : 2
    };

    this.consultancyService.apiConsultancyUpdatePut(dto).subscribe({
      next: (res: any) => {
        this.isSavingCompany = false;
        this.toastr.success(`Company "${this.editingCompany.name}" updated successfully!`, 'Company Updated');

        // Update in list
        const index = this.allCompanies.findIndex(c => c.id === this.editingCompany.id);
        if (index !== -1) {
          this.allCompanies[index] = { ...this.allCompanies[index], ...dto };
          this.applyFilters();
        }

        this.closeEditModal();
      },
      error: (err: any) => {
        this.isSavingCompany = false;
        const msg = err?.error?.message || err?.message || 'Failed to update company.';
        this.toastr.error(msg, 'Save Error');
      }
    });
  }

  getCompanyLogoUrl(logo: string | null | undefined): string {
    if (!logo) {
      return this.defaultLogo;
    }
    if (logo.startsWith('http://') || logo.startsWith('https://') || logo.startsWith('data:image')) {
      return logo;
    }
    return `${this.picBaseUrl}${logo}`;
  }

  onImageError(event: any) {
    event.target.src = this.defaultLogo;
  }

  // ==========================================
  // TAB 0: DAU & ACTIVITY DASHBOARD
  // ==========================================

  loadDauStats(timeframe?: 'day' | 'week' | 'month') {
    if (timeframe) {
      this.dauTimeframe = timeframe;
    }
    this.isLoadingDau = true;
    const url = `${environment.rootUrl}/api/User/DauStats?timeframe=${this.dauTimeframe}`;

    this.http.get<any>(url).subscribe({
      next: (res: any) => {
        this.isLoadingDau = false;
        const data = res?.value || res?.data || res;
        if (data && data.summary) {
          this.dauSummary = data.summary;
          this.dauTrends = data.trends || [];
          this.allUserActivities = data.userActivities || [];
          this.applyActivityFilters();
        }
      },
      error: (err: any) => {
        this.isLoadingDau = false;
        const msg = err?.error?.message || err?.message || 'Failed to fetch DAU analytics.';
        this.toastr.error(msg, 'Dashboard Error');
      }
    });
  }

  setDauTimeframe(tf: 'day' | 'week' | 'month') {
    if (this.dauTimeframe !== tf) {
      this.dauTimeframe = tf;
      this.activityCurrentPage = 1;
      this.loadDauStats();
    }
  }

  setTrendMetric(metric: 'users' | 'logins') {
    this.trendMetric = metric;
  }

  applyActivityFilters() {
    let result = [...this.allUserActivities];

    // Search filter
    if (this.userActivitySearch && this.userActivitySearch.trim()) {
      const q = this.userActivitySearch.trim().toLowerCase();
      result = result.filter(u =>
        (u.fullName && u.fullName.toLowerCase().includes(q)) ||
        (u.userName && u.userName.toLowerCase().includes(q)) ||
        (u.email && u.email.toLowerCase().includes(q)) ||
        (u.roleName && u.roleName.toLowerCase().includes(q)) ||
        (u.ipAddress && u.ipAddress.toLowerCase().includes(q)) ||
        (u.location && u.location.toLowerCase().includes(q))
      );
    }

    // Role filter
    if (this.roleFilter && this.roleFilter !== 'all') {
      result = result.filter(u => u.roleName && u.roleName.toLowerCase() === this.roleFilter.toLowerCase());
    }

    // Online only filter
    if (this.onlineOnlyFilter) {
      result = result.filter(u => u.isOnline);
    }

    this.filteredUserActivities = result;
    this.activityCurrentPage = 1;
  }

  onActivitySearchChange() {
    this.applyActivityFilters();
  }

  onRoleFilterChange(role: string) {
    this.roleFilter = role;
    this.applyActivityFilters();
  }

  toggleOnlineOnly() {
    this.onlineOnlyFilter = !this.onlineOnlyFilter;
    this.applyActivityFilters();
  }

  get paginatedUserActivities(): any[] {
    const startIndex = (this.activityCurrentPage - 1) * this.activityPageSize;
    return this.filteredUserActivities.slice(startIndex, startIndex + this.activityPageSize);
  }

  get activityTotalPages(): number {
    return Math.ceil(this.filteredUserActivities.length / this.activityPageSize) || 1;
  }

  setActivityPage(page: number) {
    if (page >= 1 && page <= this.activityTotalPages) {
      this.activityCurrentPage = page;
    }
  }

  getMaxTrendValue(): number {
    if (!this.dauTrends || this.dauTrends.length === 0) return 1;
    const max = Math.max(...this.dauTrends.map(t => this.trendMetric === 'users' ? t.uniqueUsers : t.totalLogins));
    return max > 0 ? max : 1;
  }

  getTrendBarHeight(point: any): number {
    const val = this.trendMetric === 'users' ? (point.uniqueUsers || 0) : (point.totalLogins || 0);
    const max = this.getMaxTrendValue();
    if (max === 0) return 4;
    const pct = Math.round((val / max) * 100);
    return Math.max(pct, 4); // Minimum 4% height for visibility
  }

  getUserInitials(name: string): string {
    if (!name || typeof name !== 'string') return 'U';
    const clean = name.replace(/undefined/gi, '').replace(/\s+/g, ' ').trim();
    if (!clean) return 'U';
    const parts = clean.split(' ').filter(p => p.length > 0);
    if (parts.length >= 2) {
      const first = parts[0].charAt(0) || '';
      const second = parts[1].charAt(0) || '';
      return (first + second).toUpperCase() || 'U';
    }
    return clean.slice(0, 2).toUpperCase();
  }

  formatDateDisplay(dateVal: any): string {
    if (!dateVal) return '—';
    try {
      const d = new Date(dateVal);
      return d.toLocaleString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        hour12: true
      });
    } catch {
      return String(dateVal);
    }
  }

  getRelativeTime(dateVal: any): string {
    if (!dateVal) return '—';
    try {
      const now = new Date().getTime();
      const past = new Date(dateVal).getTime();
      const diffMs = now - past;
      const diffMins = Math.floor(diffMs / (1000 * 60));
      const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
      const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

      if (diffMins < 1) return 'Just now';
      if (diffMins < 60) return `${diffMins}m ago`;
      if (diffHours < 24) return `${diffHours}h ago`;
      if (diffDays === 1) return 'Yesterday';
      return `${diffDays}d ago`;
    } catch {
      return '—';
    }
  }

  exportActivityCsv() {
    if (this.filteredUserActivities.length === 0) {
      this.toastr.warning('No activity data to export.', 'Export Warning');
      return;
    }

    const headers = ['User ID', 'Full Name', 'Username', 'Email', 'Role', 'Status', 'Initial Login Time (UTC)', 'Last Active Time (UTC)', 'IP Address', 'Location'];
    const rows = this.filteredUserActivities.map(u => [
      u.userId,
      `"${(u.fullName || '').replace(/"/g, '""')}"`,
      `"${(u.userName || '').replace(/"/g, '""')}"`,
      `"${(u.email || '').replace(/"/g, '""')}"`,
      `"${(u.roleName || '').replace(/"/g, '""')}"`,
      u.isOnline ? 'Online' : 'Offline',
      u.loginTime || '',
      u.lastActiveTime || '',
      `"${(u.ipAddress || '').replace(/"/g, '""')}"`,
      `"${(u.location || '').replace(/"/g, '""')}"`
    ]);

    const csvContent = [headers.join(','), ...rows.map(r => r.join(','))].join('\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', `chathire_user_activity_${this.dauTimeframe}_${new Date().toISOString().slice(0, 10)}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    this.toastr.success(`Exported ${this.filteredUserActivities.length} user activity records.`, 'Export Success');
  }

  // ==========================================
  // TAB 3: USER QUOTAS MANAGEMENT
  // ==========================================

  loadUserQuotas() {
    this.isLoadingQuotas = true;
    const searchParam = encodeURIComponent(this.quotaSearch.trim());
    const filterParam = encodeURIComponent(this.quotaFilter.trim());
    const url = `${environment.rootUrl}/api/Subscription/AllUserQuotas?page=${this.quotaCurrentPage}&pageSize=${this.quotaPageSize}&search=${searchParam}&filter=${filterParam}`;

    this.http.get<any>(url).subscribe({
      next: (res: any) => {
        this.isLoadingQuotas = false;
        const data = res?.value || res?.data || res;
        if (data && data.items) {
          this.userQuotas = data.items;
          this.quotaTotalCount = data.totalCount || 0;
          this.quotaTotalPages = data.totalPages || 1;
        } else if (Array.isArray(data)) {
          this.userQuotas = data;
          this.quotaTotalCount = data.length;
          this.quotaTotalPages = Math.ceil(data.length / this.quotaPageSize) || 1;
        } else {
          this.userQuotas = [];
          this.quotaTotalCount = 0;
          this.quotaTotalPages = 1;
        }
      },
      error: (err: any) => {
        this.isLoadingQuotas = false;
        const msg = err?.error?.message || (err.status === 0 ? 'Unable to connect to backend API server. Please ensure the API is running.' : err?.message || 'Failed to fetch user quotas.');
        this.toastr.error(msg, 'Quota Load Error');
      }
    });
  }

  onQuotaSearchChange() {
    this.quotaCurrentPage = 1;
    this.loadUserQuotas();
  }

  onQuotaFilterChange(filter: string) {
    this.quotaFilter = filter;
    this.quotaCurrentPage = 1;
    this.loadUserQuotas();
  }

  onQuotaPageChange(page: number) {
    if (page >= 1 && page <= this.quotaTotalPages) {
      this.quotaCurrentPage = page;
      this.loadUserQuotas();
    }
  }

  onQuotaPageSizeChange(size: number) {
    this.quotaPageSize = size;
    this.quotaCurrentPage = 1;
    this.loadUserQuotas();
  }

  openEditQuotaModal(user: any) {
    this.editingQuotaUser = user;
    const startD = user.cycleStartDate ? new Date(user.cycleStartDate).toISOString().slice(0, 10) : '';
    const endD = user.cycleEndDate ? new Date(user.cycleEndDate).toISOString().slice(0, 10) : '';

    this.editQuotaForm = {
      userId: user.userId,
      actualJobPosting: user.actualJobPosting ?? 15,
      actualDownloads: user.actualDownloads ?? 10,
      dailyChatLimit: user.dailyChatLimit ?? (user.isFree ? 20 : 500),
      startDate: startD,
      endDate: endD,
      isFree: user.isFree !== false
    };
    this.isEditQuotaModalOpen = true;
  }

  closeEditQuotaModal() {
    this.isEditQuotaModalOpen = false;
    this.editingQuotaUser = null;
  }

  saveUserQuota() {
    if (!this.editingQuotaUser) return;
    this.isSavingQuota = true;

    const payload = {
      userId: this.editQuotaForm.userId,
      actualJobPosting: Number(this.editQuotaForm.actualJobPosting),
      actualDownloads: Number(this.editQuotaForm.actualDownloads),
      dailyChatLimit: Number(this.editQuotaForm.dailyChatLimit),
      startDate: this.editQuotaForm.startDate ? new Date(this.editQuotaForm.startDate).toISOString() : null,
      endDate: this.editQuotaForm.endDate ? new Date(this.editQuotaForm.endDate).toISOString() : null,
      isFree: this.editQuotaForm.isFree
    };

    const url = `${environment.rootUrl}/api/Subscription/UpdateUserQuota`;
    this.http.post<any>(url, payload).subscribe({
      next: (res: any) => {
        this.isSavingQuota = false;
        this.toastr.success(`Quotas updated successfully for ${this.editingQuotaUser.fullName || this.editingQuotaUser.email}!`, 'Quota Saved');
        this.closeEditQuotaModal();
        this.loadUserQuotas();
      },
      error: (err: any) => {
        this.isSavingQuota = false;
        const msg = err?.error?.message || err?.message || 'Failed to update user quota.';
        this.toastr.error(msg, 'Update Error');
      }
    });
  }

  formatDateOnly(dateVal: any): string {
    if (!dateVal) return '—';
    try {
      const d = new Date(dateVal);
      return d.toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric'
      });
    } catch {
      return String(dateVal);
    }
  }

  // ==========================================
  // TAB: CREATE / MANAGE ADMIN USERS (Type ID = 7)
  // ==========================================

  get filteredAdminUsers(): any[] {
    if (!this.adminUserSearch.trim()) return this.adminUsers;
    const term = this.adminUserSearch.trim().toLowerCase();
    return this.adminUsers.filter(u =>
      (u.userName && u.userName.toLowerCase().includes(term)) ||
      (u.email && u.email.toLowerCase().includes(term)) ||
      (u.fname && u.fname.toLowerCase().includes(term)) ||
      (u.lname && u.lname.toLowerCase().includes(term)) ||
      (u.phone && u.phone.toLowerCase().includes(term))
    );
  }

  loadAdminUsers() {
    this.isLoadingAdminUsers = true;
    const url = `${environment.rootUrl}/api/User/GetAdminUsers`;
    this.http.get<any>(url).subscribe({
      next: (res: any) => {
        this.isLoadingAdminUsers = false;
        this.adminUsers = res?.value || res?.data || res || [];
      },
      error: (err: any) => {
        this.isLoadingAdminUsers = false;
        console.error('Failed to load admin users', err);
        this.toastr.error('Failed to load admin users.', 'Error');
      }
    });
  }

  createAdminUser() {
    if (!this.createAdminForm.userName || !this.createAdminForm.userName.trim()) {
      this.toastr.warning('Please enter a username or email.', 'Validation Error');
      return;
    }
    if (!this.createAdminForm.password || !this.createAdminForm.password.trim()) {
      this.toastr.warning('Please enter a password.', 'Validation Error');
      return;
    }

    this.isCreatingAdmin = true;
    const payload = {
      userName: this.createAdminForm.userName.trim(),
      password: this.createAdminForm.password.trim(),
      fname: this.createAdminForm.fname?.trim() || null,
      lname: this.createAdminForm.lname?.trim() || null,
      phone: this.createAdminForm.phone?.trim() || null
    };

    const url = `${environment.rootUrl}/api/User/CreateAdminUser`;
    this.http.post<any>(url, payload).subscribe({
      next: (res: any) => {
        this.isCreatingAdmin = false;
        this.toastr.success(`Admin user "${payload.userName}" (UserTypeId = 7) created / updated successfully!`, 'Admin User Created');
        this.resetCreateAdminForm();
        this.loadAdminUsers();
      },
      error: (err: any) => {
        this.isCreatingAdmin = false;
        const msg = err?.error?.message || err?.message || 'Failed to create admin user.';
        this.toastr.error(msg, 'Creation Error');
      }
    });
  }

  resetCreateAdminForm() {
    this.createAdminForm = {
      userName: '',
      password: '',
      fname: '',
      lname: '',
      phone: ''
    };
    this.showAdminPassword = false;
  }

  toggleAdminPasswordVisibility() {
    this.showAdminPassword = !this.showAdminPassword;
  }

  // ==========================================
  // --- Tab: LinkedIn Company Scraper State & Methods ---
  // ==========================================
  linkedinUrlsInput: string = '';
  isScrapingLinkedIn: boolean = false;
  scrapedResults: any[] = [];
  linkedinFilter: 'all' | 'completed' | 'failed' = 'all';
  linkedinSearchTerm: string = '';
  isDraggingFile: boolean = false;
  uploadedFileName: string = '';
  autoSaveToDb: boolean = true;
  showAddLinkedInModal: boolean = false;
  parsedUrlCount: number = 0;

  openAddLinkedInModal() {
    this.showAddLinkedInModal = true;
  }

  closeAddLinkedInModal() {
    this.showAddLinkedInModal = false;
  }

  onLinkedInUrlInputChanged() {
    const urls = this.extractUrlsFromText(this.linkedinUrlsInput);
    this.parsedUrlCount = urls.length;
  }

  onLinkedInDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDraggingFile = true;
  }

  onLinkedInDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDraggingFile = false;
  }

  onLinkedInDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDraggingFile = false;

    if (event.dataTransfer && event.dataTransfer.files && event.dataTransfer.files.length > 0) {
      const file = event.dataTransfer.files[0];
      this.processLinkedInFile(file);
    }
  }

  onLinkedInFileSelected(event: any) {
    if (event.target.files && event.target.files.length > 0) {
      const file = event.target.files[0];
      this.processLinkedInFile(file);
    }
  }

  processLinkedInFile(file: File) {
    const ext = file.name.split('.').pop()?.toLowerCase();
    if (ext !== 'csv' && ext !== 'txt') {
      this.toastr.warning('Please upload a valid .csv or .txt file.', 'Invalid File Format');
      return;
    }

    this.uploadedFileName = file.name;
    const reader = new FileReader();
    reader.onload = (e: any) => {
      const content = e.target.result;
      const extractedUrls = this.extractUrlsFromText(content);
      if (extractedUrls.length === 0) {
        this.toastr.warning('No valid LinkedIn URLs found in the uploaded file.', 'Empty / Invalid File');
        return;
      }

      // Merge with existing or replace
      if (this.linkedinUrlsInput.trim()) {
        this.linkedinUrlsInput = this.linkedinUrlsInput.trim() + '\n' + extractedUrls.join('\n');
      } else {
        this.linkedinUrlsInput = extractedUrls.join('\n');
      }

      this.onLinkedInUrlInputChanged();
      this.toastr.info(`Loaded ${extractedUrls.length} LinkedIn URL(s) from "${file.name}".`, 'File Processed');
    };
    reader.readAsText(file);
  }

  extractUrlsFromText(text: string): string[] {
    if (!text || !text.trim()) return [];
    // Split by newlines, commas, semicolons or spaces
    const tokens = text.split(/[\r\n,;]+/).map(t => t.trim()).filter(t => t.length > 0);
    const validUrls: string[] = [];
    for (const token of tokens) {
      if (token.toLowerCase().includes('linkedin.com/company') || 
          token.toLowerCase().includes('linkedin.com/school') ||
          token.toLowerCase().startsWith('http://') || 
          token.toLowerCase().startsWith('https://') ||
          token.toLowerCase().startsWith('www.linkedin.com')) {
        validUrls.push(token);
      } else if (token.length > 2 && !token.includes(' ')) {
        // Assume company slug or raw url
        if (!token.startsWith('http')) {
          validUrls.push(`https://www.linkedin.com/company/${token}`);
        } else {
          validUrls.push(token);
        }
      }
    }
    // Return unique URLs
    return Array.from(new Set(validUrls));
  }

  startLinkedInScraping() {
    const urls = this.extractUrlsFromText(this.linkedinUrlsInput);
    if (urls.length === 0) {
      this.toastr.warning('Please enter at least one valid LinkedIn company URL.', 'Validation Error');
      return;
    }

    this.isScrapingLinkedIn = true;
    const payload = {
      urls: urls,
      autoSaveToDb: this.autoSaveToDb
    };

    const endpoint = `${environment.rootUrl}/api/Consultancy/ScrapeAndAddLinkedInCompanies`;
    this.http.post<any>(endpoint, payload).subscribe({
      next: (res: any) => {
        this.isScrapingLinkedIn = false;
        const results = res?.value || res?.data || res || [];
        
        // Merge into current session results (prepend)
        this.scrapedResults = [...results, ...this.scrapedResults];
        
        const successCount = results.filter((r: any) => r.status === 'Completed').length;
        const failCount = results.filter((r: any) => r.status === 'Failed').length;

        if (successCount > 0) {
          this.toastr.success(`Processed ${results.length} company URLs (${successCount} saved to database).`, 'Extraction Complete');
          // Reload all companies so the "Manage Companies" tab is fresh
          this.loadCompanies();
        } else {
          this.toastr.warning(`Completed processing with ${failCount} errors.`, 'Extraction Finished');
        }

        // Close modal if open
        this.closeAddLinkedInModal();
      },
      error: (err: any) => {
        this.isScrapingLinkedIn = false;
        const msg = err?.error?.message || err?.message || 'Failed to extract company data.';
        this.toastr.error(msg, 'Scraper Error');
      }
    });
  }

  clearLinkedInInputs() {
    this.linkedinUrlsInput = '';
    this.uploadedFileName = '';
    this.parsedUrlCount = 0;
  }

  clearScrapedResults() {
    this.scrapedResults = [];
    this.toastr.info('Cleared scraper activity results.', 'Cleared');
  }

  getFilteredScrapedResults(): any[] {
    let list = this.scrapedResults;
    if (this.linkedinFilter === 'completed') {
      list = list.filter(r => r.status === 'Completed');
    } else if (this.linkedinFilter === 'failed') {
      list = list.filter(r => r.status === 'Failed');
    }

    if (this.linkedinSearchTerm.trim()) {
      const term = this.linkedinSearchTerm.toLowerCase().trim();
      list = list.filter(r => 
        (r.companyName && r.companyName.toLowerCase().includes(term)) ||
        (r.domainName && r.domainName.toLowerCase().includes(term)) ||
        (r.website && r.website.toLowerCase().includes(term)) ||
        (r.inputUrl && r.inputUrl.toLowerCase().includes(term))
      );
    }
    return list;
  }

  getScrapedStats() {
    const total = this.scrapedResults.length;
    const completed = this.scrapedResults.filter(r => r.status === 'Completed').length;
    const failed = this.scrapedResults.filter(r => r.status === 'Failed').length;
    const newRecords = this.scrapedResults.filter(r => r.isNewRecord).length;
    return { total, completed, failed, newRecords };
  }

  exportScrapedResults() {
    if (this.scrapedResults.length === 0) {
      this.toastr.warning('No results to export.', 'Export Empty');
      return;
    }

    const headers = ['Company Name', 'Domain', 'Website', 'LinkedIn URL', 'Azure Logo', 'Status', 'Message', 'Consultancy ID'];
    const rows = this.scrapedResults.map(r => [
      `"${(r.companyName || '').replace(/"/g, '""')}"`,
      `"${(r.domainName || '').replace(/"/g, '""')}"`,
      `"${(r.website || '').replace(/"/g, '""')}"`,
      `"${(r.normalizedLinkedinUrl || r.inputUrl || '').replace(/"/g, '""')}"`,
      `"${(r.azureLogoFileName || '').replace(/"/g, '""')}"`,
      `"${(r.status || '').replace(/"/g, '""')}"`,
      `"${(r.statusMessage || '').replace(/"/g, '""')}"`,
      `"${r.consultancyId || ''}"`
    ]);

    const csvContent = 'data:text/csv;charset=utf-8,' + [headers.join(','), ...rows.map(e => e.join(','))].join('\n');
    const encodedUri = encodeURI(csvContent);
    const link = document.createElement('a');
    link.setAttribute('href', encodedUri);
    link.setAttribute('download', `LinkedIn_Companies_${new Date().toISOString().slice(0,10)}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    this.toastr.success('Exported scraper results to CSV.', 'Export Complete');
  }

  // ==========================================
  // TAB: PROMO CODE MANAGEMENT
  // ==========================================
  promoCodes: PromocodeDto[] = [];
  isLoadingPromoCodes: boolean = false;
  isCreatingPromoCode: boolean = false;
  isDeletingPromoCode: boolean = false;
  promoCodeSearch: string = '';
  promoCodeFilter: 'all' | 'active' | 'expired' | 'inactive' = 'all';
  promoCurrentPage: number = 1;
  promoPageSize: number = 25;
  promoPageSizeOptions: number[] = [10, 25, 50, 100];

  isCreatePromoModalOpen: boolean = false;
  isDeletePromoModalOpen: boolean = false;
  selectedPromoForDelete: PromocodeDto | null = null;

  createPromoForm: CreatePromocodeDto = {
    promocode: '',
    description: '',
    startDate: '',
    endDate: '',
    noOfFreeDownloads: null,
    noOfFreeJobPosting: null,
    discountAmount: null,
    dailyChatLimit: null,
    isSingleUse: true,
    maxRedemptions: null,
    active: true
  };
  promoFormErrors: { [key: string]: string } = {};

  loadPromoCodes() {
    this.isLoadingPromoCodes = true;
    this.promocodeService.getAll().subscribe({
      next: (res: any) => {
        this.isLoadingPromoCodes = false;
        const data = res?.value || res?.data || res || [];
        this.promoCodes = Array.isArray(data) ? data : [];
      },
      error: (err: any) => {
        this.isLoadingPromoCodes = false;
        const msg = err?.error?.message || err?.message || 'Failed to load promo codes.';
        this.toastr.error(msg, 'Error');
      }
    });
  }

  get filteredPromoCodes(): PromocodeDto[] {
    let list = [...this.promoCodes];

    if (this.promoCodeFilter === 'active') {
      list = list.filter(p => p.active === true && (!p.endDate || new Date(p.endDate) >= new Date()));
    } else if (this.promoCodeFilter === 'expired') {
      list = list.filter(p => p.endDate && new Date(p.endDate) < new Date());
    } else if (this.promoCodeFilter === 'inactive') {
      list = list.filter(p => p.active === false || p.active === null);
    }

    if (this.promoCodeSearch && this.promoCodeSearch.trim()) {
      const term = this.promoCodeSearch.trim().toLowerCase();
      list = list.filter(p =>
        (p.promocode && p.promocode.toLowerCase().includes(term)) ||
        (p.description && p.description.toLowerCase().includes(term))
      );
    }

    return list;
  }

  get paginatedPromoCodes(): PromocodeDto[] {
    const start = (this.promoCurrentPage - 1) * this.promoPageSize;
    return this.filteredPromoCodes.slice(start, start + this.promoPageSize);
  }

  get totalPromoPages(): number {
    return Math.ceil(this.filteredPromoCodes.length / this.promoPageSize) || 1;
  }

  get promoTotalCount(): number {
    return this.promoCodes.length;
  }

  get promoActiveCount(): number {
    return this.promoCodes.filter(p => p.active === true && (!p.endDate || new Date(p.endDate) >= new Date())).length;
  }

  get promoExpiredCount(): number {
    return this.promoCodes.filter(p => p.endDate && new Date(p.endDate) < new Date()).length;
  }

  get promoTotalRedemptions(): number {
    return this.promoCodes.reduce((sum, p) => sum + (p.redemptionCount || 0), 0);
  }

  onPromoSearchChange() {
    this.promoCurrentPage = 1;
  }

  onPromoFilterChange(filter: 'all' | 'active' | 'expired' | 'inactive') {
    this.promoCodeFilter = filter;
    this.promoCurrentPage = 1;
  }

  onPromoPageChange(page: number) {
    if (page >= 1 && page <= this.totalPromoPages) {
      this.promoCurrentPage = page;
    }
  }

  onPromoPageSizeChange(size: number) {
    this.promoPageSize = size;
    this.promoCurrentPage = 1;
  }

  openCreatePromoModal() {
    const today = new Date().toISOString().slice(0, 10);
    const thirtyDaysLater = new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().slice(0, 10);

    this.createPromoForm = {
      promocode: '',
      description: '',
      startDate: today,
      endDate: thirtyDaysLater,
      noOfFreeDownloads: null,
      noOfFreeJobPosting: null,
      discountAmount: null,
      dailyChatLimit: null,
      isSingleUse: true,
      maxRedemptions: null,
      active: true
    };
    this.promoFormErrors = {};
    this.isCreatePromoModalOpen = true;
  }

  closeCreatePromoModal() {
    this.isCreatePromoModalOpen = false;
    this.promoFormErrors = {};
  }

  generateRandomPromoCode() {
    const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
    let code = 'HIRES-';
    for (let i = 0; i < 6; i++) {
      code += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    this.createPromoForm.promocode = code;
  }

  validatePromoForm(): boolean {
    this.promoFormErrors = {};

    if (!this.createPromoForm.promocode || !this.createPromoForm.promocode.trim()) {
      this.promoFormErrors['promocode'] = 'Promo Code is required.';
    }

    if (!this.createPromoForm.startDate) {
      this.promoFormErrors['startDate'] = 'Start Date is mandatory.';
    }

    if (!this.createPromoForm.endDate) {
      this.promoFormErrors['endDate'] = 'End Date is mandatory.';
    }

    if (this.createPromoForm.startDate && this.createPromoForm.endDate) {
      if (new Date(this.createPromoForm.endDate) < new Date(this.createPromoForm.startDate)) {
        this.promoFormErrors['endDate'] = 'End Date must be on or after Start Date.';
      }
    }

    return Object.keys(this.promoFormErrors).length === 0;
  }

  savePromoCode() {
    if (!this.validatePromoForm()) {
      this.toastr.warning('Please fix the validation errors.', 'Validation Required');
      return;
    }

    this.isCreatingPromoCode = true;

    const payload: CreatePromocodeDto = {
      promocode: this.createPromoForm.promocode.trim().toUpperCase(),
      description: this.createPromoForm.description?.trim() || undefined,
      startDate: new Date(this.createPromoForm.startDate).toISOString(),
      endDate: new Date(this.createPromoForm.endDate).toISOString(),
      noOfFreeDownloads: this.createPromoForm.noOfFreeDownloads != null && this.createPromoForm.noOfFreeDownloads !== ('' as any)
        ? Number(this.createPromoForm.noOfFreeDownloads) : null,
      noOfFreeJobPosting: this.createPromoForm.noOfFreeJobPosting != null && this.createPromoForm.noOfFreeJobPosting !== ('' as any)
        ? Number(this.createPromoForm.noOfFreeJobPosting) : null,
      discountAmount: this.createPromoForm.discountAmount != null && this.createPromoForm.discountAmount !== ('' as any)
        ? Number(this.createPromoForm.discountAmount) : null,
      dailyChatLimit: this.createPromoForm.dailyChatLimit != null && this.createPromoForm.dailyChatLimit !== ('' as any)
        ? Number(this.createPromoForm.dailyChatLimit) : null,
      active: this.createPromoForm.active !== false
    };

    this.promocodeService.create(payload).subscribe({
      next: (res: any) => {
        this.isCreatingPromoCode = false;
        this.toastr.success(`Promo Code "${payload.promocode}" created successfully!`, 'Created');
        this.closeCreatePromoModal();
        this.loadPromoCodes();
      },
      error: (err: any) => {
        this.isCreatingPromoCode = false;
        const msg = err?.error?.message || err?.message || 'Failed to create promo code.';
        this.toastr.error(msg, 'Creation Error');
      }
    });
  }

  openDeletePromoModal(promo: PromocodeDto) {
    this.selectedPromoForDelete = promo;
    this.isDeletePromoModalOpen = true;
  }

  closeDeletePromoModal() {
    this.isDeletePromoModalOpen = false;
    this.selectedPromoForDelete = null;
  }

  confirmDeletePromoCode() {
    if (!this.selectedPromoForDelete) return;

    this.isDeletingPromoCode = true;
    const promoId = this.selectedPromoForDelete.id;
    const codeName = this.selectedPromoForDelete.promocode;

    this.promocodeService.delete(promoId).subscribe({
      next: (res: any) => {
        this.isDeletingPromoCode = false;
        this.toastr.success(`Promo code "${codeName}" removed / deactivated successfully.`, 'Deleted');
        this.closeDeletePromoModal();
        this.loadPromoCodes();
      },
      error: (err: any) => {
        this.isDeletingPromoCode = false;
        const msg = err?.error?.message || err?.message || 'Failed to delete promo code.';
        this.toastr.error(msg, 'Delete Error');
      }
    });
  }

  togglePromoStatus(promo: PromocodeDto, event: Event) {
    event.stopPropagation();
    this.promocodeService.toggleStatus(promo.id).subscribe({
      next: () => {
        promo.active = !promo.active;
        this.toastr.success(`Promo code "${promo.promocode}" is now ${promo.active ? 'Active' : 'Inactive'}.`, 'Status Updated');
      },
      error: (err: any) => {
        const msg = err?.error?.message || err?.message || 'Failed to toggle promo code status.';
        this.toastr.error(msg, 'Update Error');
      }
    });
  }

  copyPromoCode(code: string) {
    if (!code) return;
    navigator.clipboard.writeText(code).then(() => {
      this.toastr.success(`Copied "${code}" to clipboard!`, 'Copied');
    }).catch(() => {
      this.toastr.info(`Code: ${code}`);
    });
  }

  isPromoExpired(promo: PromocodeDto): boolean {
    if (!promo.endDate) return false;
    return new Date(promo.endDate) < new Date();
  }

  // ==========================================
  // TAB 9: JOBPOSTING BY EMAIL
  // ==========================================

  emailJobPostings: EmailJobPostingQueueDto[] = [];
  isLoadingEmailJobs = false;
  emailJobTotalCount = 0;
  emailJobCurrentPage = 1;
  emailJobPageSize = 20;
  emailJobSearch = '';
  emailJobStatusFilter = 'all';
  emailJobStats: EmailJobPostingStatsDto = {
    totalReceived: 0,
    publishedCount: 0,
    parsedCount: 0,
    failedCount: 0,
    quotaExceededCount: 0,
    rejectedCount: 0
  };

  // Simulate Inbound Email Modal State
  isSimulateModalOpen = false;
  isSimulating = false;
  simulateForm: SimulateInboundEmailDto = {
    senderEmail: '',
    senderName: '',
    emailSubject: '',
    emailBody: '',
    autoPublish: true
  };

  // Review & Publish Confirmation Modal State
  isReviewPublishModalOpen = false;
  isApprovingJob = false;
  selectedQueueItem: EmailJobPostingQueueDto | null = null;
  reviewForm: {
    queueId: number;
    name: string;
    description: string;
    jobLocation: string;
    totalExp: number;
    fromAmt: number;
    toAmt: number;
    numberOfOpening: number;
    skillsStr: string;
    employmentTypesStr: string;
    visasStr: string;
  } = {
    queueId: 0,
    name: '',
    description: '',
    jobLocation: 'Remote',
    totalExp: 5,
    fromAmt: 50,
    toAmt: 75,
    numberOfOpening: 1,
    skillsStr: '',
    employmentTypesStr: 'Contract',
    visasStr: 'Any Visa'
  };

  // Raw Email Viewer Modal State
  isRawEmailModalOpen = false;
  viewingRawEmailItem: EmailJobPostingQueueDto | null = null;

  loadEmailJobPostings() {
    this.isLoadingEmailJobs = true;
    this.emailJobPostingService.getAll(
      this.emailJobStatusFilter,
      this.emailJobSearch,
      this.emailJobCurrentPage,
      this.emailJobPageSize
    ).subscribe({
      next: (res: any) => {
        this.emailJobPostings = res?.value || [];
        this.isLoadingEmailJobs = false;
      },
      error: (err: any) => {
        this.isLoadingEmailJobs = false;
        this.toastr.error('Failed to load email job postings.', 'Error');
      }
    });

    this.emailJobPostingService.getCount(this.emailJobStatusFilter, this.emailJobSearch).subscribe({
      next: (res: any) => {
        this.emailJobTotalCount = res?.value || 0;
      }
    });
  }

  loadEmailJobStats() {
    this.emailJobPostingService.getStats().subscribe({
      next: (res: any) => {
        if (res?.value) {
          this.emailJobStats = res.value;
        }
      }
    });
  }

  onEmailJobFilterChange(status: string) {
    this.emailJobStatusFilter = status;
    this.emailJobCurrentPage = 1;
    this.loadEmailJobPostings();
  }

  onEmailJobSearchChange() {
    this.emailJobCurrentPage = 1;
    this.loadEmailJobPostings();
  }

  onEmailJobPageChange(page: number) {
    if (page < 1 || (page - 1) * this.emailJobPageSize >= this.emailJobTotalCount) return;
    this.emailJobCurrentPage = page;
    this.loadEmailJobPostings();
  }

  // --- Simulate Modal Handlers ---
  openSimulateModal() {
    this.simulateForm = {
      senderEmail: '',
      senderName: '',
      emailSubject: '',
      emailBody: '',
      autoPublish: true
    };
    this.isSimulateModalOpen = true;
  }

  closeSimulateModal() {
    this.isSimulateModalOpen = false;
  }

  submitSimulateEmail() {
    if (!this.simulateForm.senderEmail || !this.simulateForm.emailSubject || !this.simulateForm.emailBody) {
      this.toastr.warning('Please enter sender email, subject, and email body.', 'Validation');
      return;
    }

    this.isSimulating = true;
    this.emailJobPostingService.simulate(this.simulateForm).subscribe({
      next: (res: any) => {
        this.isSimulating = false;
        const item = res?.value;
        if (item?.status === 'Published') {
          this.toastr.success(`Job automatically parsed and published! (Job ID: ${item.createdJobOpeningId})`, 'Job Published');
        } else if (item?.status === 'Parsed') {
          this.toastr.info('Email parsed successfully. You can now review and publish.', 'Parsed');
        } else {
          this.toastr.warning(`Processed with status: ${item?.status}. ${item?.errorMessage || ''}`, 'Notice');
        }
        this.closeSimulateModal();
        this.loadEmailJobPostings();
        this.loadEmailJobStats();
      },
      error: (err: any) => {
        this.isSimulating = false;
        const msg = err?.error?.message || err?.message || 'Failed to simulate email job posting.';
        this.toastr.error(msg, 'Simulation Error');
      }
    });
  }

  // --- Review & Publish Modal Handlers ---
  openReviewPublishModal(item: EmailJobPostingQueueDto) {
    this.selectedQueueItem = item;
    const parsed = item.parsedJob;
    this.reviewForm = {
      queueId: item.id,
      name: parsed?.name || item.emailSubject || '',
      description: parsed?.description || item.rawEmailBodyText || '',
      jobLocation: parsed?.jobLocation || 'Remote',
      totalExp: parsed?.totalExp || 5,
      fromAmt: parsed?.fromAmt || 50,
      toAmt: parsed?.toAmt || 75,
      numberOfOpening: parsed?.numberOfOpening || 1,
      skillsStr: (parsed?.skills || []).join(', '),
      employmentTypesStr: (parsed?.employmentTypes || ['Contract']).join(', '),
      visasStr: (parsed?.visas || ['Any Visa']).join(', ')
    };
    this.isReviewPublishModalOpen = true;
  }

  closeReviewPublishModal() {
    this.isReviewPublishModalOpen = false;
    this.selectedQueueItem = null;
  }

  submitApproveJob() {
    if (!this.reviewForm.name || !this.reviewForm.description) {
      this.toastr.warning('Please provide a job title and description.', 'Validation');
      return;
    }

    const skills = this.reviewForm.skillsStr.split(',').map(s => s.trim()).filter(s => !!s);
    const employmentTypes = this.reviewForm.employmentTypesStr.split(',').map(s => s.trim()).filter(s => !!s);
    const visas = this.reviewForm.visasStr.split(',').map(s => s.trim()).filter(s => !!s);

    const dto: ApproveEmailJobPostingDto = {
      queueId: this.reviewForm.queueId,
      name: this.reviewForm.name,
      description: this.reviewForm.description,
      jobLocation: this.reviewForm.jobLocation,
      totalExp: this.reviewForm.totalExp,
      fromAmt: this.reviewForm.fromAmt,
      toAmt: this.reviewForm.toAmt,
      numberOfOpening: this.reviewForm.numberOfOpening,
      skills: skills,
      employmentTypes: employmentTypes,
      visas: visas
    };

    this.isApprovingJob = true;
    this.emailJobPostingService.approve(dto).subscribe({
      next: (res: any) => {
        this.isApprovingJob = false;
        this.toastr.success('Job successfully approved and published as regular job posting!', 'Success');
        this.closeReviewPublishModal();
        this.loadEmailJobPostings();
        this.loadEmailJobStats();
      },
      error: (err: any) => {
        this.isApprovingJob = false;
        const msg = err?.error?.message || err?.message || 'Failed to approve and publish job.';
        this.toastr.error(msg, 'Approval Error');
      }
    });
  }

  // --- Reprocess with AI ---
  reprocessEmailJob(item: EmailJobPostingQueueDto) {
    this.toastr.info(`Reprocessing email from ${item.senderEmail}...`, 'Processing');
    this.emailJobPostingService.process(item.id).subscribe({
      next: (res: any) => {
        this.toastr.success('Email reprocessed and structured data updated.', 'Success');
        this.loadEmailJobPostings();
        this.loadEmailJobStats();
      },
      error: (err: any) => {
        const msg = err?.error?.message || err?.message || 'Failed to reprocess email.';
        this.toastr.error(msg, 'Error');
      }
    });
  }

  // --- Reject Queue Item ---
  rejectEmailJob(item: EmailJobPostingQueueDto) {
    const reason = prompt(`Enter rejection reason for "${item.emailSubject}":`, 'Did not meet posting criteria / incomplete requirements');
    if (reason === null) return;

    this.emailJobPostingService.reject({ queueId: item.id, reason: reason }).subscribe({
      next: () => {
        this.toastr.info('Job posting request rejected.', 'Rejected');
        this.loadEmailJobPostings();
        this.loadEmailJobStats();
      },
      error: (err: any) => {
        const msg = err?.error?.message || err?.message || 'Failed to reject job.';
        this.toastr.error(msg, 'Error');
      }
    });
  }

  // --- Delete Queue Item ---
  deleteEmailJob(item: EmailJobPostingQueueDto) {
    if (!confirm(`Are you sure you want to delete this email queue entry from "${item.senderEmail}"?`)) return;

    this.emailJobPostingService.delete(item.id).subscribe({
      next: () => {
        this.toastr.success('Entry removed successfully.', 'Deleted');
        this.loadEmailJobPostings();
        this.loadEmailJobStats();
      },
      error: (err: any) => {
        const msg = err?.error?.message || err?.message || 'Failed to delete entry.';
        this.toastr.error(msg, 'Delete Error');
      }
    });
  }

  // --- Raw Email Modal ---
  openRawEmailModal(item: EmailJobPostingQueueDto) {
    this.viewingRawEmailItem = item;
    this.isRawEmailModalOpen = true;
  }

  closeRawEmailModal() {
    this.isRawEmailModalOpen = false;
    this.viewingRawEmailItem = null;
  }

  getEmailStatusBadgeClass(status: string): string {
    switch ((status || '').toLowerCase()) {
      case 'published': return 'bg-success text-white';
      case 'parsed': return 'bg-info text-dark';
      case 'received': return 'bg-secondary text-white';
      case 'failed': return 'bg-danger text-white';
      case 'quotaexceeded': return 'bg-warning text-dark';
      case 'rejected': return 'bg-dark text-white';
      default: return 'bg-light text-dark';
    }
  }
}


