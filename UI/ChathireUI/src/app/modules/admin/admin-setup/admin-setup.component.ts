import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { ConsultancyService } from 'src/app/api/api/consultancy.service';
import { picUrl, defaultProfilePic } from 'src/app/data/various';

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

  activeTab: 'bulk-upload' | 'view-edit' = 'bulk-upload';

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
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadCompanies();
  }

  setTab(tab: 'bulk-upload' | 'view-edit') {
    this.activeTab = tab;
    if (tab === 'view-edit' && this.allCompanies.length === 0) {
      this.loadCompanies();
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
}
