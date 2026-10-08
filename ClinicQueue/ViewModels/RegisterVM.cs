using System.ComponentModel.DataAnnotations;

namespace ClinicQueue.ViewModels;

public class RegisterVM
{
    [Required(ErrorMessage = "الاسم مطلوب")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "يجب أن يتراوح الاسم بين 2 و100 حرف")]
    [RegularExpression(@"^[A-Za-z\u0600-\u06FF\s]+$", ErrorMessage = "يجب أن يحتوي الاسم على أحرف فقط")]
    [Display(Name = "الاسم")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [RegularExpression(@"^09\d{8}$", ErrorMessage = "رقم الهاتف يبدأ بـ 09 ويتكون من 10 أرقام")]
    [Display(Name = "رقم الهاتف")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [MinLength(6, ErrorMessage = "كلمة المرور 6 أحرف على الأقل")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
    [Compare(nameof(Password), ErrorMessage = "كلمتا المرور غير متطابقتين")]
    [DataType(DataType.Password)]
    [Display(Name = "تأكيد كلمة المرور")]
    public string ConfirmPassword { get; set; } = string.Empty;
}