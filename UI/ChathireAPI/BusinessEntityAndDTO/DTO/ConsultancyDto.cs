using BusinessEntityAndDTO.Models;
using System;
using System.Collections.Generic;

namespace BusinessEntityAndDTO.DTO;

public partial class ConsultancyDto
{
    public long Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public bool? Active { get; set; }

    public DateTime? Updated { get; set; }

    public string? Website { get; set; }

    public string? Linkedin { get; set; }

    public string? Logo { get; set; }

    public int? CityId { get; set; }

    public string? Domainname { get; set; }

    public long? UpdatedBy { get; set; }

    public bool? IsDirectCompany { get; set; }

    public short? StatusId { get; set; }

}

public partial class ConsultancyForInsertDtoWithImage
{
    public FileModel? img { get; set; }
    public ConsultancyForInsertDto? consultancyForInsertDto { get; set; }
}
public partial class ConsultancyForInsertDto
{
    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public bool? Active { get; set; }

    public DateTime? Updated { get; set; }

    public string? Website { get; set; }

    public string? Linkedin { get; set; }

    public string? Logo { get; set; }

    public int? CityId { get; set; }

    public string? Domainname { get; set; }

    public long? UpdatedBy { get; set; }

    public bool? IsDirectCompany { get; set; }

    public short? StatusId { get; set; }

}
