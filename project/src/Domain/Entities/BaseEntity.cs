using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public abstract class BaseEntity
{
    // CreateDate: บันทึกเวลาที่สร้าง
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreateDate { get; set; }

    // UpdateDate: บันทึกเวลาที่อัปเดตล่าสุด
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdateDate { get; set; }
}
