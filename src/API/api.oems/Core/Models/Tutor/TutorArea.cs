﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.oems.Core.Models.Tutor
{
    public class TutorArea : CommonEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string AreaName { get; set; }

        public int? DistrictId { get; set; }

        [ForeignKey("DistrictId")]
        public TutorDistrict District { get; set; }
    }
}
