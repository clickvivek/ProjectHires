using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Models;

public partial class EFContexts : DbContext
{
    public EFContexts()
    {
    }

    public EFContexts(DbContextOptions<EFContexts> options)
        : base(options)
    {
    }

    public virtual DbSet<Audit> Audits { get; set; }

    public virtual DbSet<AuditGroup> AuditGroups { get; set; }

    public virtual DbSet<Bofunction> Bofunctions { get; set; }

    public virtual DbSet<CandidateAvailability> CandidateAvailabilities { get; set; }

    public virtual DbSet<CandidateConsultingRole> CandidateConsultingRoles { get; set; }

    public virtual DbSet<CandidateDocument> CandidateDocuments { get; set; }

    public virtual DbSet<CandidatePrefJobType> CandidatePrefJobTypes { get; set; }

    public virtual DbSet<CandidatePrefLocation> CandidatePrefLocations { get; set; }

    public virtual DbSet<CandidateProfile> CandidateProfiles { get; set; }

    public virtual DbSet<CandidateProfileDomain> CandidateProfileDomains { get; set; }

    public virtual DbSet<CandidateProfileEmploymentType> CandidateProfileEmploymentTypes { get; set; }

    public virtual DbSet<CandidateProfileMappingStatus> CandidateProfileMappingStatuses { get; set; }

    public virtual DbSet<CandidateProfileSkill> CandidateProfileSkills { get; set; }

    public virtual DbSet<CandidateProfileViewHistory> CandidateProfileViewHistories { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Consultancy> Consultancies { get; set; }

    public virtual DbSet<ConsultancyStatus> ConsultancyStatuses { get; set; }

    public virtual DbSet<ConsultancyUser> ConsultancyUsers { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<Domain> Domains { get; set; }

    public virtual DbSet<EmploymentType> EmploymentTypes { get; set; }

    public virtual DbSet<JobOpening> JobOpenings { get; set; }

    public virtual DbSet<JobOpeningCandidateProfileMap> JobOpeningCandidateProfileMaps { get; set; }

    public virtual DbSet<JobOpeningEmploymentType> JobOpeningEmploymentTypes { get; set; }

    public virtual DbSet<JobOpeningJobType> JobOpeningJobTypes { get; set; }

    public virtual DbSet<JobOpeningLocation> JobOpeningLocations { get; set; }

    public virtual DbSet<JobOpeningProfileConsultancyComment> JobOpeningProfileConsultancyComments { get; set; }

    public virtual DbSet<JobOpeningSkill> JobOpeningSkills { get; set; }

    public virtual DbSet<JobOpeningVisaMap> JobOpeningVisaMaps { get; set; }

    public virtual DbSet<JobType> JobTypes { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<LogLevel> LogLevels { get; set; }

    public virtual DbSet<ProfileStatus> ProfileStatuses { get; set; }

    public virtual DbSet<ProjectStartInWeek> ProjectStartInWeeks { get; set; }

    public virtual DbSet<Promocode> Promocodes { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public virtual DbSet<SubscriptionPlanFeature> SubscriptionPlanFeatures { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAccess> UserAccesses { get; set; }

    public virtual DbSet<UserFavorite> UserFavorites { get; set; }

    public virtual DbSet<UserFavoritesDetail> UserFavoritesDetails { get; set; }

    public virtual DbSet<UserPlanTree> UserPlanTrees { get; set; }

    public virtual DbSet<UserSubscriptionPlan> UserSubscriptionPlans { get; set; }

    public virtual DbSet<UserLogin> UserLogins { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

    public virtual DbSet<Visa> Visas { get; set; }

    // OnConfiguring removed because it's configured in Program.cs via AddDbContext

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Audit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Audit__3214EC07D69E2169");

            entity.ToTable("Audit");

            entity.Property(e => e.AdddlData)
                .HasMaxLength(2000)
                .IsUnicode(false);
            entity.Property(e => e.AuditDate).HasColumnType("datetime");
            entity.Property(e => e.KeyColumnName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.KeyColumnValue)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Message)
                .HasMaxLength(2000)
                .IsUnicode(false);
            entity.Property(e => e.MethodFullName)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.MethodName)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.AuditGroup).WithMany(p => p.Audits)
                .HasForeignKey(d => d.AuditGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Audit__AuditGrou__6B24EA82");
        });

        modelBuilder.Entity<AuditGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AuditGro__3214EC07C3F895D3");

            entity.ToTable("AuditGroup");

            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<Bofunction>(entity =>
        {
            entity.ToTable("BOFunctions");

            entity.HasIndex(e => e.Description, "UC_BOFunctions_Description").IsUnique();

            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<CandidateAvailability>(entity =>
        {
            entity.ToTable("CandidateAvailability");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<CandidateConsultingRole>(entity =>
        {
            entity.ToTable("CandidateConsultingRole");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<CandidateDocument>(entity =>
        {
            entity.ToTable("CandidateDocument");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Doc)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.CandidateProfile).WithMany(p => p.CandidateDocuments)
                .HasForeignKey(d => d.CandidateProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CandidateDocument_CandidateProfiles");

            entity.HasOne(d => d.Document).WithMany(p => p.CandidateDocuments)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CandidateDocument_Document");
        });

        modelBuilder.Entity<CandidatePrefJobType>(entity =>
        {
            entity.ToTable("CandidatePrefJobType");

            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.CandidateProfile).WithMany(p => p.CandidatePrefJobTypes)
                .HasForeignKey(d => d.CandidateProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CandidatePrefJobType_CandidateProfile");

            entity.HasOne(d => d.JobType).WithMany(p => p.CandidatePrefJobTypes)
                .HasForeignKey(d => d.JobTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CandidatePrefJobType_JobType");
        });

        modelBuilder.Entity<CandidatePrefLocation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_CandidateLocation");

            entity.ToTable("CandidatePrefLocation");

            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.Candidate).WithMany(p => p.CandidatePrefLocations)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CandidateLocation_CandidateProfiles");

            entity.HasOne(d => d.City).WithMany(p => p.CandidatePrefLocations)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CandidateLocation_Location");
        });

        modelBuilder.Entity<CandidateProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_CandidateProfiles");

            entity.ToTable("CandidateProfile");

            entity.Property(e => e.CandidateName)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.Comment)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("EMail");
            entity.Property(e => e.EmployerInfo)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.FirstArrivalDate).HasColumnType("date");
            entity.Property(e => e.LinkedIn)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("LinkedIN");
            entity.Property(e => e.Passportno)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.PostedDate).HasColumnType("datetime");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
            entity.Property(e => e.VisaExpiryDate).HasColumnType("date");

            entity.HasOne(d => d.AvailabilityNavigation).WithMany(p => p.CandidateProfiles)
                .HasForeignKey(d => d.Availability)
                .HasConstraintName("FK_CandidateProfiles_Availability");

            entity.HasOne(d => d.City).WithMany(p => p.CandidateProfiles)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK_CandidateProfiles_CityId");

            entity.HasOne(d => d.ConsultancyUser).WithMany(p => p.CandidateProfiles)
                .HasForeignKey(d => d.ConsultancyUserId)
                .HasConstraintName("FK_CandidateProfiles_ConsultancyUser");

            entity.HasOne(d => d.ConsultingRole).WithMany(p => p.CandidateProfiles)
                .HasForeignKey(d => d.ConsultingRoleId)
                .HasConstraintName("FK_CandidateProfile_ConsultingRole");

            entity.HasOne(d => d.Status).WithMany(p => p.CandidateProfiles)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CandidateProfiles_ProfileStatus");

            entity.HasOne(d => d.User).WithMany(p => p.CandidateProfiles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CandidateProfiles_User");

            entity.HasOne(d => d.Visa).WithMany(p => p.CandidateProfiles)
                .HasForeignKey(d => d.VisaId)
                .HasConstraintName("FK_CandidateProfiles_VisaId");
        });

        modelBuilder.Entity<CandidateProfileDomain>(entity =>
        {
            entity.ToTable("CandidateProfileDomain");

            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.CandidateProfile).WithMany(p => p.CandidateProfileDomains)
                .HasForeignKey(d => d.CandidateProfileId)
                .HasConstraintName("FK_CandidateProfileDomain_CandidateProfiles");

            entity.HasOne(d => d.Domain).WithMany(p => p.CandidateProfileDomains)
                .HasForeignKey(d => d.DomainId)
                .HasConstraintName("FK_CandidateProfileDomain_Domain");
        });

        modelBuilder.Entity<CandidateProfileEmploymentType>(entity =>
        {
            entity.ToTable("CandidateProfileEmploymentType");

            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.CandidateProfile).WithMany(p => p.CandidateProfileEmploymentTypes)
                .HasForeignKey(d => d.CandidateProfileId)
                .HasConstraintName("FK_CandidateProfileEmploymentType_CandidateProfile");

            entity.HasOne(d => d.EmploymentType).WithMany(p => p.CandidateProfileEmploymentTypes)
                .HasForeignKey(d => d.EmploymentTypeId)
                .HasConstraintName("FK_CandidateProfileEmploymentType_EmploymentType");
        });

        modelBuilder.Entity<CandidateProfileMappingStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ProfileMappingStatus");

            entity.ToTable("CandidateProfileMappingStatus");

            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<CandidateProfileSkill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ProfileKeySkill");

            entity.Property(e => e.Updated)
                .HasColumnType("datetime")
                .HasColumnName("updated");

            entity.HasOne(d => d.CandidateProfile).WithMany(p => p.CandidateProfileSkills)
                .HasForeignKey(d => d.CandidateProfileid)
                .HasConstraintName("FK_CandidateProfileKeySkill_CandidateProfiles");

            entity.HasOne(d => d.Skill).WithMany(p => p.CandidateProfileSkills)
                .HasForeignKey(d => d.SkillId)
                .HasConstraintName("FK_CandidateProfileKeySkill_KeySkill");
        });

        modelBuilder.Entity<CandidateProfileViewHistory>(entity =>
        {
            entity.ToTable("CandidateProfileViewHistory");

            entity.Property(e => e.Updated)
                .HasColumnType("datetime")
                .HasColumnName("updated");

            entity.HasOne(d => d.CandidateProfile).WithMany(p => p.CandidateProfileViewHistories)
                .HasForeignKey(d => d.CandidateProfileid)
                .HasConstraintName("FK_CandidateProfileViewHistory_CandidateProfiles");

            entity.HasOne(d => d.ConsultancyUser).WithMany(p => p.CandidateProfileViewHistories)
                .HasForeignKey(d => d.ConsultancyUserId)
                .HasConstraintName("FK_CandidateProfileViewHistory_ConsultancyUser");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");

            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__US_CITIE__3214EC2743B4205A");

            entity.ToTable("CITIES");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.City1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CITY");
            entity.Property(e => e.IdState).HasColumnName("ID_STATE");
            entity.Property(e => e.IsState).HasDefaultValueSql("((0))");
            entity.Property(e => e.Updated).HasColumnType("datetime");
            entity.Property(e => e.Zip)
                .HasMaxLength(25)
                .IsUnicode(false);

            entity.HasOne(d => d.IdStateNavigation).WithMany(p => p.Cities)
                .HasForeignKey(d => d.IdState)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__US_CITIES__ID_ST__23F3538A");
        });

        modelBuilder.Entity<Consultancy>(entity =>
        {
            entity.ToTable("Consultancy");

            entity.Property(e => e.Address)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Domainname)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsDirectCompany).HasDefaultValueSql("((0))");
            entity.Property(e => e.Linkedin)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Logo).IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
            entity.Property(e => e.Website)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("website");

            entity.HasOne(d => d.City).WithMany(p => p.Consultancies)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK_Consultancy_City");

            entity.HasOne(d => d.Status).WithMany(p => p.Consultancies)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK_Consultancy_Status");
        });

        modelBuilder.Entity<ConsultancyStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_CosultancyStatus");

            entity.ToTable("ConsultancyStatus");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<ConsultancyUser>(entity =>
        {
            entity.ToTable("ConsultancyUser");

            entity.HasIndex(e => new { e.ConsultancyId, e.UserId }, "Uinq_Idx_ConsultancyUser").IsUnique();

            entity.Property(e => e.NoOfViews).HasDefaultValueSql("((0))");
            entity.Property(e => e.PublicProfileUserName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasDefaultValueSql("((1))");

            entity.HasOne(d => d.Consultancy).WithMany(p => p.ConsultancyUsers)
                .HasForeignKey(d => d.ConsultancyId)
                .HasConstraintName("FK_ConsultancyUser_Consultancy");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_ConsultancyUser_ConsultancyUser");

            entity.HasOne(d => d.User).WithMany(p => p.ConsultancyUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ConsultancyUser_User");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.CountryCode);

            entity.ToTable("Country");

            entity.Property(e => e.CountryCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Document__3213E83F58049FFC");

            entity.ToTable("Document");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<Domain>(entity =>
        {
            entity.ToTable("Domain");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<EmploymentType>(entity =>
        {
            entity.ToTable("EmploymentType");

            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<JobOpening>(entity =>
        {
            entity.ToTable("JobOpening");

            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.IsExpired).HasDefaultValueSql("((0))");
            entity.Property(e => e.JobLocation)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LastDate).HasColumnType("date");
            entity.Property(e => e.LocalCandidateOnly).HasDefaultValueSql("((0))");
            entity.Property(e => e.LocalCandidatePref).HasDefaultValueSql("((0))");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NotifyOnCandidateProfileMap).HasDefaultValueSql("((0))");
            entity.Property(e => e.NotifyWithResume).HasDefaultValueSql("((0))");
            entity.Property(e => e.Postalcode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PostedDate).HasColumnType("date");
            entity.Property(e => e.PriorityId).HasColumnName("PriorityID");
            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.JobOpenings)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_JobOpening_Category");

            entity.HasOne(d => d.ConsultancyUser).WithMany(p => p.JobOpenings)
                .HasForeignKey(d => d.ConsultancyUserId)
                .HasConstraintName("FK_JobOpening_ConsultancyUser");

            entity.HasOne(d => d.ProjectStart).WithMany(p => p.JobOpenings)
                .HasForeignKey(d => d.ProjectStartId)
                .HasConstraintName("FK_JobOpening_ProjectStart");
        });

        modelBuilder.Entity<JobOpeningCandidateProfileMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_JobProfiles");

            entity.ToTable("JobOpeningCandidateProfileMap");

            entity.Property(e => e.AppliedDate).HasColumnType("datetime");
            entity.Property(e => e.CandidateName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CandidateProfileMappingStatusId).HasDefaultValueSql("((1))");
            entity.Property(e => e.Comment)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Doc)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LinkedIn)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("LinkedIN");
            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.CandidateProfile).WithMany(p => p.JobOpeningCandidateProfileMaps)
                .HasForeignKey(d => d.CandidateProfileId)
                .HasConstraintName("FK_JobProfiles_CandidateProfiles_ProfileId");

            entity.HasOne(d => d.CandidateProfileMappingStatus).WithMany(p => p.JobOpeningCandidateProfileMaps)
                .HasForeignKey(d => d.CandidateProfileMappingStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JobopeningCandidateProfileMap_ProfileMapSatus");

            entity.HasOne(d => d.CandidateUser).WithMany(p => p.JobOpeningCandidateProfileMaps)
                .HasForeignKey(d => d.CandidateUserId)
                .HasConstraintName("FK_JobOpeningCandidateProfileMap_CandidateUserId");

            entity.HasOne(d => d.ConsultancyUser).WithMany(p => p.JobOpeningCandidateProfileMaps)
                .HasForeignKey(d => d.ConsultancyUserId)
                .HasConstraintName("FK_JobOpeningCandidateProfileMap_ConsultancyUserId");

            entity.HasOne(d => d.CurrentLocationCity).WithMany(p => p.JobOpeningCandidateProfileMaps)
                .HasForeignKey(d => d.CurrentLocationCityid)
                .HasConstraintName("FK_JobOpeningCandidateProfileMap_CityId");

            entity.HasOne(d => d.JobOpening).WithMany(p => p.JobOpeningCandidateProfileMaps)
                .HasForeignKey(d => d.JobOpeningId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JobProfiles__JobOpening_JobId");
        });

        modelBuilder.Entity<JobOpeningEmploymentType>(entity =>
        {
            entity.ToTable("JobOpeningEmploymentType");

            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.EmploymentType).WithMany(p => p.JobOpeningEmploymentTypes)
                .HasForeignKey(d => d.EmploymentTypeId)
                .HasConstraintName("FK_JobOpeningEmploymentType_EmploymentType");

            entity.HasOne(d => d.JobOpening).WithMany(p => p.JobOpeningEmploymentTypes)
                .HasForeignKey(d => d.JobOpeningId)
                .HasConstraintName("FK_JobOpeningEmploymentType_JobOpening");
        });

        modelBuilder.Entity<JobOpeningJobType>(entity =>
        {
            entity.ToTable("JobOpeningJobType");

            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.JobOpening).WithMany(p => p.JobOpeningJobTypes)
                .HasForeignKey(d => d.JobOpeningId)
                .HasConstraintName("FK_JobOpeningJobType_JobOpening");

            entity.HasOne(d => d.JobType).WithMany(p => p.JobOpeningJobTypes)
                .HasForeignKey(d => d.JobTypeId)
                .HasConstraintName("FK_JobOpeningJobType_JobType");
        });

        modelBuilder.Entity<JobOpeningLocation>(entity =>
        {
            entity.ToTable("JobOpeningLocation");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.City).WithMany(p => p.JobOpeningLocations)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK_JobOpeningLocation_CityId");

            entity.HasOne(d => d.JobOpening).WithMany(p => p.JobOpeningLocations)
                .HasForeignKey(d => d.JobOpeningId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JobOpeningLocation_JobOpening");
        });

        modelBuilder.Entity<JobOpeningProfileConsultancyComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_JobopeningProfileConsultancyComment");

            entity.ToTable("JobOpeningProfileConsultancyComment");

            entity.Property(e => e.Comment)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.ConsultancyUser).WithMany(p => p.JobOpeningProfileConsultancyComments)
                .HasForeignKey(d => d.ConsultancyUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JBCPComments_ConsultancyUser");

            entity.HasOne(d => d.JobopeningCandidateProfileMap).WithMany(p => p.JobOpeningProfileConsultancyComments)
                .HasForeignKey(d => d.JobopeningCandidateProfileMapId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JBCPComments_JobopeningCandidateProfileMap");
        });

        modelBuilder.Entity<JobOpeningSkill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_JobOpeningKeySkill");

            entity.Property(e => e.IsMandate).HasColumnName("isMandate");
            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.Job).WithMany(p => p.JobOpeningSkills)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_JobOpeningKeySkill_JobOpening");

            entity.HasOne(d => d.Skill).WithMany(p => p.JobOpeningSkills)
                .HasForeignKey(d => d.SkillId)
                .HasConstraintName("FK_JobOpeningKeySkill_KeySkill");
        });

        modelBuilder.Entity<JobOpeningVisaMap>(entity =>
        {
            entity.ToTable("JobOpeningVisaMap");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.JobOpening).WithMany(p => p.JobOpeningVisaMaps)
                .HasForeignKey(d => d.JobOpeningId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CandidateVisaMap__JobOpening_JobOpeningId");

            entity.HasOne(d => d.Visa).WithMany(p => p.JobOpeningVisaMaps)
                .HasForeignKey(d => d.VisaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JobOpeningVisaMap__Visa_VisaId");
        });

        modelBuilder.Entity<JobType>(entity =>
        {
            entity.ToTable("JobType");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Log__3214EC0749A21822");

            entity.ToTable("Log");

            entity.Property(e => e.AdddlData)
                .HasMaxLength(4000)
                .IsUnicode(false);
            entity.Property(e => e.Message)
                .HasMaxLength(2000)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.LogLevelNavigation).WithMany(p => p.Logs)
                .HasForeignKey(d => d.LogLevel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Log__LogLevel__76969D2E");
        });

        modelBuilder.Entity<LogLevel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LogLevel__3214EC07698F2773");

            entity.ToTable("LogLevel");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<ProfileStatus>(entity =>
        {
            entity.ToTable("ProfileStatus");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<ProjectStartInWeek>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ProjectStart");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<Promocode>(entity =>
        {
            entity.ToTable("Promocode");

            entity.Property(e => e.Active).HasDefaultValueSql("((1))");
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Promocode1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Promocode");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_KeySkill");

            entity.HasIndex(e => e.Name, "UK_Skills").IsUnique();

            entity.Property(e => e.IsUserDefined).HasDefaultValueSql("((0))");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__US_STATE");

            entity.ToTable("STATES");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CountryCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.StateCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("STATE_CODE");
            entity.Property(e => e.StateName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("STATE_NAME");
            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.CountryCodeNavigation).WithMany(p => p.States)
                .HasForeignKey(d => d.CountryCode)
                .HasConstraintName("FK_STATES_CountryCode");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.ToTable("Status");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.ToTable("SubscriptionPlan");

            entity.Property(e => e.Active).HasDefaultValueSql("((1))");
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsFree).HasDefaultValueSql("((0))");
            entity.Property(e => e.NoOfUsers).HasDefaultValueSql("((1))");
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<SubscriptionPlanFeature>(entity =>
        {
            entity.ToTable("SubscriptionPlanFeature");

            entity.Property(e => e.Active).HasDefaultValueSql("((1))");
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.SubscriptionPlan).WithMany(p => p.SubscriptionPlanFeatures)
                .HasForeignKey(d => d.SubscriptionPlanId)
                .HasConstraintName("FK_SubscriptionPlanFeature_SubscriptionPlan");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.HasIndex(e => e.UserName, "UQ__User__C9F28456116211F0").IsUnique();

            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.AltEmailVerified).HasDefaultValueSql("((0))");
            entity.Property(e => e.AlternateEmail)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.EmailVerified).HasDefaultValueSql("((0))");
            entity.Property(e => e.Fname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("FName");
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Hiringforcountry)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("HIRINGFORCOUNTRY");
            entity.Property(e => e.Location)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("LOCATION");
            entity.Property(e => e.Linkedin)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Lname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("LName");
            entity.Property(e => e.OtpaltEmail)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("OTPAltEmail");
            entity.Property(e => e.OtpaltEmailDate)
                .HasColumnType("datetime")
                .HasColumnName("OTPAltEmailDate");
            entity.Property(e => e.Otpemail)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("OTPEmail");
            entity.Property(e => e.OtpemailDate)
                .HasColumnType("datetime")
                .HasColumnName("OTPEmailDate");
            entity.Property(e => e.OtppwdDateTime)
                .HasColumnType("datetime")
                .HasColumnName("OTPPwdDateTime");
            entity.Property(e => e.OtppwdReset)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("OTPPwdReset");
            entity.Property(e => e.Password)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ProfilePic)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.ResetPassword).HasDefaultValueSql("((0))");
            entity.Property(e => e.Updated).HasColumnType("datetime");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.City).WithMany(p => p.Users)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK_User_City");

            entity.HasOne(d => d.UserType).WithMany(p => p.Users)
                .HasForeignKey(d => d.UserTypeId)
                .HasConstraintName("FK_User_UserType");
        });

        modelBuilder.Entity<UserAccess>(entity =>
        {
            entity.ToTable("UserAccess");

            entity.HasIndex(e => new { e.UserTypeId, e.BofunctionId }, "UC_UserAccess_UserType").IsUnique();

            entity.Property(e => e.BofunctionId).HasColumnName("BOFunctionId");
            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.Bofunction).WithMany(p => p.UserAccesses)
                .HasForeignKey(d => d.BofunctionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAccess_BOFunctions");

            entity.HasOne(d => d.UserType).WithMany(p => p.UserAccesses)
                .HasForeignKey(d => d.UserTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAccess_UserType");
        });

        modelBuilder.Entity<UserFavorite>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Active)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.UserFavorites)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserFavorites_UserId");
        });

        modelBuilder.Entity<UserFavoritesDetail>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Active)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.IsJobOpening)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.UserFavorite).WithMany(p => p.UserFavoritesDetails)
                .HasForeignKey(d => d.UserFavoriteId)
                .HasConstraintName("FK_UserFavoritesDetails_UserFavorite");
        });

        modelBuilder.Entity<UserPlanTree>(entity =>
        {
            entity.ToTable("UserPlanTree");

            entity.Property(e => e.Active).HasDefaultValueSql("((1))");
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.AssignedUser).WithMany(p => p.UserPlanTreeAssignedUsers)
                .HasForeignKey(d => d.AssignedUserId)
                .HasConstraintName("FK_UserPlanTree_AssignedUser");

            entity.HasOne(d => d.User).WithMany(p => p.UserPlanTreeUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserPlanTree_User");
        });

        modelBuilder.Entity<UserSubscriptionPlan>(entity =>
        {
            entity.ToTable("UserSubscriptionPlan");

            entity.Property(e => e.Active).HasDefaultValueSql("((1))");
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.IsFree).HasDefaultValueSql("((0))");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.SubscriptionPlan).WithMany(p => p.UserSubscriptionPlans)
                .HasForeignKey(d => d.SubscriptionPlanId)
                .HasConstraintName("FK_UserSubscriptionPlan_SubscriptionPlan");

            entity.HasOne(d => d.User).WithMany(p => p.UserSubscriptionPlans)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserSubscriptionPlan_User");
        });

        modelBuilder.Entity<UserLogin>(entity =>
        {
            entity.ToTable("UserLogins");

            entity.Property(e => e.LoginTime).HasColumnType("datetime");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Location)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.UserLogins)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserLogins_User");
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ConsultancyUserType");

            entity.ToTable("UserType");

            entity.HasIndex(e => e.Name, "UC_ConsultancyUserType_Description").IsUnique();

            entity.Property(e => e.IsInternal).HasDefaultValueSql("((0))");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<Visa>(entity =>
        {
            entity.ToTable("Visa");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
